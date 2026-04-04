using Foodify10.Models;
using Foodify10.Models.JsonModels;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Foodify10.Services.Providers
{
    public class NationalCatalogProvider : IProductDataProvider
    {
        private readonly HttpClient _httpClient;

        public NationalCatalogProvider(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<ProductData?> TryGetProductAsync(ProductSearchRequest request)
        {
            string searchUrl = $"https://национальный-каталог.рф/search/?q={request.Barcode}&type=goods";

            try
            {
                if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0 Safari/537.36");

                var searchHtml = await _httpClient.GetStringAsync(searchUrl);
                var searchDoc = new HtmlDocument();
                searchDoc.LoadHtml(searchHtml);

                var linkNode = searchDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'catalog__grid-view__item')]//a[@href]");
                if (linkNode == null) return null;

                string productRelativeUrl = linkNode.GetAttributeValue("href", "");
                string productUrl = productRelativeUrl.StartsWith("http") ? productRelativeUrl : $"https://национальный-каталог.рф{productRelativeUrl}";

                var productHtml = await _httpClient.GetStringAsync(productUrl);
                var productDoc = new HtmlDocument();
                productDoc.LoadHtml(productHtml);

                return ParseProductData(productDoc, request.Barcode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[НацКаталог] Ошибка: {ex.Message}");
                return null;
            }
        }

        private ProductData? ParseProductData(HtmlDocument doc, string barcode)
        {
            string name = GetValue(doc, "Полное наименование товара") ?? "Неизвестный товар";
            string composition = GetValue(doc, "Состав") ?? "Состав не указан";

            string proteins = GetNumeric(doc, "Белки") ?? "0";
            string fats = GetNumeric(doc, "Жиры") ?? "0";
            string carbs = GetNumeric(doc, "Углеводы") ?? "0";
            string calories = GetNumeric(doc, "Энергетическая ценность, ккал") ?? "0";

            string fullComposition = composition;
            //if (calories != "0" || proteins != "0" || fats != "0" || carbs != "0")
            //    fullComposition += $"\n\nКБЖУ: {calories} ккал, Б: {proteins}г, Ж: {fats}г, У: {carbs}г";
            
            var nValue = new NutrinitionalValue(calories, proteins, fats, carbs);

            if (name == "Неизвестный товар" && composition == "Состав не указан") return null;

            return new ProductData(barcode, name, fullComposition, nValue);
        }

        private static string? GetValue(HtmlDocument doc, params string[] keywords)
        {
            foreach (var key in keywords)
            {
                var xpath = $"//th[contains(translate(., 'АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ', 'абвгдеёжзийклмнопрстуфхцчшщъыьэюя'), '{key.ToLower()}')]/following-sibling::td";
                var node = doc.DocumentNode.SelectSingleNode(xpath);
                if (node != null) return HtmlEntity.DeEntitize(node.InnerText).Trim();
            }
            return null;
        }

        private static string? GetNumeric(HtmlDocument doc, string keyword)
        {
            string? rawValue = GetValue(doc, keyword);
            if (string.IsNullOrEmpty(rawValue)) return null;
            var match = Regex.Match(rawValue, @"[0-9.,-]+");
            return match.Success ? match.Value : null;
        }
    }
}
