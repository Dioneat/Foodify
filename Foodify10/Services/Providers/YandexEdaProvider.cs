using Foodify10.Models;
using Foodify10.Models.JsonModels;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Foodify10.Services.Providers
{
    public class YandexEdaProvider : IProductDataProvider
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public YandexEdaProvider(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<ProductData?> TryGetProductAsync(ProductSearchRequest request)
        {
            // 1. Через Barcode-List
            Debug.WriteLine($"[Я.Еда] Поиск через barcode-list...");
            var nameFromList = await GetNameFromBarcodeListAsync(request.Barcode);

            if (!string.IsNullOrEmpty(nameFromList))
            {
                var result = await TryProcessWithNameAsync(nameFromList, request.Barcode);
                if (result != null) return result;
            }

            // 2. Через Disai
            Debug.WriteLine($"[Я.Еда] Поиск через Disai...");
            var nameFromDisai = await GetNameFromBarcodeViaDisaiAsync(request.Barcode);

            if (!string.IsNullOrEmpty(nameFromDisai) && nameFromDisai != "Not found" && nameFromDisai != nameFromList)
            {
                return await TryProcessWithNameAsync(nameFromDisai, request.Barcode);
            }

            return null;
        }

        private async Task<ProductData?> TryProcessWithNameAsync(string rawName, string barcode)
        {
            string normalizedName = NormalizeProductName(rawName);
            var (id, slug) = await SearchInYandexAsync(normalizedName);

            if (id != null && slug != null)
            {
                return await GetProductDetailsAsync(id, slug, barcode, normalizedName);
            }
            return null;
        }

        private async Task<string?> GetNameFromBarcodeListAsync(string barcode)
        {
            try
            {
                string url = $"https://barcode-list.ru/barcode/RU/Поиск.htm?barcode={barcode}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0 Safari/537.36");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                return doc.DocumentNode.SelectSingleNode("//p[@style='margin-top:20px']/a")?.InnerText?.Trim();
            }
            catch { return null; }
        }

        public async Task<string> GetNameFromBarcodeViaDisaiAsync(string barcode)
        {
            try
            {
                string url = "https://ru.disai.org/";
                var formData = new Dictionary<string, string> { { "search_query", barcode } };
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(formData) };
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0 Safari/537.36");

                var response = await _httpClient.SendAsync(request);
                string html = await response.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var productRow = doc.DocumentNode.SelectSingleNode("//table//tr[2]");
                if (productRow != null)
                {
                    var productNameNode = productRow.SelectSingleNode(".//td[1]//font");
                    return productNameNode?.InnerText?.Trim() ?? "Not found";
                }
            }
            catch { }
            return "Not found";
        }

        private string NormalizeProductName(string name)
        {
            
            return name.Replace("проц", "").Trim();
        }

        private async Task<(string? Id, string? Slug)> SearchInYandexAsync(string name)
        {
            var requestData = new
            {
                text = name,
                filters = Array.Empty<object>(),
                location = new { longitude = 43.861339691782256, latitude = 56.25566251192055 }
            };

            var request = CreatePostRequest("https://eda.yandex.ru/eats/v1/full-text-search/v1/search", requestData);
            try
            {
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return (null, null);

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var root = doc.RootElement;

                if (!root.TryGetProperty("blocks", out var blocks)) return (null, null);

                foreach (var block in blocks.EnumerateArray())
                {
                    if (block.GetProperty("type").GetString() == "places")
                    {
                        var payload = block.GetProperty("payload").EnumerateArray().ToList();
                        if (payload.Count == 0) continue;

                        var place = payload[0].GetProperty("slug").GetString() != "yandeks_lavka" || payload.Count == 1
                                    ? payload[0] : payload[1];

                        string slug = place.GetProperty("slug").GetString()!;
                        if (place.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                        {
                            string id = items[0].GetProperty("id").GetString()!;
                            return (id, slug);
                        }
                    }
                }
            }
            catch { }
            return (null, null);
        }

        private async Task<ProductData?> GetProductDetailsAsync(string id, string slug, string barcode, string fallbackName)
        {
            var requestData = new
            {
                place_slug = slug,
                product_public_id = id,
                filters = Array.Empty<object>(),
                location = new { longitude = 43.861339691782256, latitude = 56.25566251192055 }
            };

            var request = CreatePostRequest("https://eda.yandex.ru/api/v2/menu/product?auto_translate=false", requestData);
            try
            {
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var root = doc.RootElement;

                if (!root.TryGetProperty("menu_item", out var menuItem)) return null;

                string finalName = menuItem.TryGetProperty("name", out var n) ? n.GetString() ?? fallbackName : fallbackName;
                string composition = "Состав не найден";
                string kkal = "0", prot = "0", fat = "0", carb = "0";

                if (root.TryGetProperty("detailed_data", out var detailedData))
                {
                    foreach (var detail in detailedData.EnumerateArray())
                    {
                        string type = detail.GetProperty("type").GetString() ?? "";

                        if (type == "energy_values" && detail.TryGetProperty("payload", out var energyPayload))
                        {
                            if (energyPayload.TryGetProperty("energy_values", out var evArray))
                            {
                                foreach (var ev in evArray.EnumerateArray())
                                {
                                    string evName = ev.GetProperty("name").GetString()?.ToLower() ?? "";
                                    string evValue = ev.GetProperty("value").ToString().Split(' ')[0];

                                    if (evName.Contains("ккал")) kkal = evValue;
                                    else if (evName.Contains("белки")) prot = evValue;
                                    else if (evName.Contains("жиры")) fat = evValue;
                                    else if (evName.Contains("углеводы")) carb = evValue;
                                }
                            }
                        }

                        if (type == "product_details" && detail.TryGetProperty("payload", out var detailsPayload))
                        {
                            if (detailsPayload.TryGetProperty("product_descriptions", out var pdArray))
                            {
                                foreach (var pd in pdArray.EnumerateArray())
                                {
                                    string title = pd.GetProperty("title").GetString() ?? "";
                                    if (title == "Состав" || title == "Ingredients")
                                        composition = pd.GetProperty("text").GetString() ?? composition;
                                }
                            }
                        }
                    }
                }
                var nValue = new NutrinitionalValue(kkal, prot, fat, carb);
                //string fullComp = composition + (kkal != "0" ? $"\n\nКБЖУ: {kkal} ккал, Б: {prot}г, Ж: {fat}г, У: {carb}г" : "");
                return new ProductData(barcode, finalName, composition, nValue);
            }
            catch { return null; }
        }

        private HttpRequestMessage CreatePostRequest(string url, object data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-platform", "desktop_web");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0 Safari/537.36");
            string json = JsonSerializer.Serialize(data, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return request;
        }
    }
}
