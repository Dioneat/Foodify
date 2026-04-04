using Foodify10.Models;
using Foodify10.Models.JsonModels;
using System.Diagnostics;
using System.Text.Json;

namespace Foodify10.Services.Providers
{
    public class OpenFoodFactsProvider : IProductDataProvider
    {
        private readonly HttpClient _httpClient;

        public OpenFoodFactsProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductData?> TryGetProductAsync(ProductSearchRequest request)
        {
            string url = $"https://ru.openfoodfacts.org/api/v2/product/{request.Barcode}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("product", out var product)) return null;

            
                if (!product.TryGetProperty("ingredients_text_ru", out var ingRu) ||
                    string.IsNullOrWhiteSpace(ingRu.GetString()))
                {
                    Debug.WriteLine("[OFF] Пропуск: отсутствует состав на русском.");
                    return null;
                }

               
                if (!product.TryGetProperty("nutriments", out var nutriments) || !HasRequiredNutrients(nutriments))
                {
                    Debug.WriteLine("[OFF] Пропуск: отсутствуют данные КБЖУ.");
                    return null;
                }

                // Название
                if (!product.TryGetProperty("product_name_ru", out var nameRu) ||
                    string.IsNullOrWhiteSpace(nameRu.GetString()))
                {
                    return null;
                }

                string kcal = nutriments.TryGetProperty("energy-kcal_100g", out var e) ? e.GetString() : "null";
                string prot = nutriments.TryGetProperty("proteins_100g", out var p) ? p.GetString() : "null";
                string fat = nutriments.TryGetProperty("fat_100g", out var f) ? f.GetString() : "null";
                string carb = nutriments.TryGetProperty("carbohydrates_100g", out var c) ? c.GetString() : "null";
                var nValue = new NutrinitionalValue(kcal, prot, fat, carb);
                //string fullComposition = $"{ingRu.GetString()}\n\nКБЖУ: {kcal} ккал, Б: {prot}г, Ж: {fat}г, У: {carb}г";

                return new ProductData(request.Barcode, nameRu.GetString()!, ingRu.GetString(), nValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OFF] Ошибка: {ex.Message}");
                return null;
            }
        }

        private bool HasRequiredNutrients(JsonElement nutriments)
        {
            return nutriments.TryGetProperty("energy-kcal_100g", out _) &&
                   nutriments.TryGetProperty("proteins_100g", out _) &&
                   nutriments.TryGetProperty("fat_100g", out _) &&
                   nutriments.TryGetProperty("carbohydrates_100g", out _);
        }
    }
}
