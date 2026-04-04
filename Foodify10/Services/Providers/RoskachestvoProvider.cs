using Foodify10.Models;
using Foodify10.Models.JsonModels;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Foodify10.Models.JsonModels.RskrfModels;

namespace Foodify10.Services.Providers
{
    public class RoskachestvoProvider : IProductDataProvider
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RoskachestvoProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductData?> TryGetProductAsync(ProductSearchRequest request)
        {
            try
            {
                var detail = await GetByBarcodeAsync(request.Barcode);

                if (detail?.Title == null && !string.IsNullOrWhiteSpace(request.Name))
                {
                    detail = await GetByNameAsync(request.Name);
                }

                if (detail?.Title == null)
                    return null;

                string composition = ExtractComposition(detail);
                NutrinitionalValue nutrition = ExtractNutrition(detail);

                return new ProductData(
                    request.Barcode,
                    detail.Title,
                    composition,
                    nutrition)
                {
                    SourceName = "Роскачество",
                    ResearchData = new ProductResearchData
                    {
                        HasQualityMark = detail.HasQualityMark,
                        Worth = detail.Worth?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>(),
                        CriteriaRatings = detail.CriteriaRatings?
                            .Select(x => new ProductCriterionRatingData
                            {
                                Title = x.Title ?? string.Empty,
                                Value = x.Value
                            })
                            .ToList() ?? new List<ProductCriterionRatingData>(),
                        Manufacturer = detail.Manufacturer,
                        CategoryName = detail.CategoryName,
                        ProductLink = detail.ProductLink,
                        Thumbnail = detail.Thumbnail
                    }
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<ProductDetail?> GetByBarcodeAsync(string barcode)
        {
            return (await SendRequestAsync<RskrfProductDetailResponse>(
                $"https://rskrf.ru/rest/1/search/barcode?barcode={barcode}"))?.Response;
        }

        private async Task<ProductDetail?> GetByNameAsync(string name)
        {
            var search = await SendRequestAsync<RskrfSearchResponse>(
                $"https://rskrf.ru/rest/1/search/product?query={Uri.EscapeDataString(name)}&page=1");

            var id = search?.Response?.Products?.FirstOrDefault()?.Id;

            if (id == null)
                return null;

            return (await SendRequestAsync<RskrfProductDetailResponse>(
                $"https://rskrf.ru/rest/1/product/{id}/"))?.Response;
        }

        private async Task<T?> SendRequestAsync<T>(string url) where T : class
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0");
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        private static string ExtractComposition(ProductDetail detail)
        {
            var compositionFromInfo = detail.ProductInfo?
                .FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(x.Name) &&
                    x.Name.Contains("Состав", StringComparison.OrdinalIgnoreCase))
                ?.Info;

            if (!string.IsNullOrWhiteSpace(compositionFromInfo))
                return compositionFromInfo;

            return !string.IsNullOrWhiteSpace(detail.Description)
                ? detail.Description
                : "Состав не указан";
        }

        private static NutrinitionalValue ExtractNutrition(ProductDetail detail)
        {
            var productInfo = detail.ProductInfo ?? Array.Empty<ProductInfo>();

            string fullText = string.Join(" ",
                productInfo
                    .Where(x => !string.IsNullOrWhiteSpace(x.Info))
                    .Select(x => $"{x.Name}: {x.Info}"));

            string calories = ExtractCalories(productInfo, fullText) ?? "0";
            string proteins = ExtractNamedValue(fullText, @"бел(?:ок|ки)\s*[-:]?\s*(\d+(?:[.,]\d+)?)") ?? "0";
            string fats = ExtractNamedValue(fullText, @"жир(?:ы)?\s*[-:]?\s*(\d+(?:[.,]\d+)?)") ?? "0";
            string carbs = ExtractNamedValue(fullText, @"углевод(?:ы)?\s*[-:]?\s*(\d+(?:[.,]\d+)?)") ?? "0";

            return new NutrinitionalValue(calories, proteins, fats, carbs);
        }

        private static string? ExtractCalories(ProductInfo[] productInfo, string fullText)
        {
            var directCalories = productInfo.FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(x.Name) &&
                    x.Name.Contains("Энергетическая ценность", StringComparison.OrdinalIgnoreCase))
                ?.Info;

            if (!string.IsNullOrWhiteSpace(directCalories))
            {
                var normalized = ExtractFirstNumber(directCalories);
                if (!string.IsNullOrWhiteSpace(normalized))
                    return normalized;
            }

            var kcalFromCombined = Regex.Match(
                fullText,
                @"(\d+(?:[.,]\d+)?)\s*(?:ккал|kcal|kkal)",
                RegexOptions.IgnoreCase);

            if (kcalFromCombined.Success)
                return NormalizeNumber(kcalFromCombined.Groups[1].Value);

            var fallback = Regex.Match(
                fullText,
                @"энергетическ(?:ая|ой)\s+ценност[ьи][^0-9]{0,30}(\d+(?:[.,]\d+)?)",
                RegexOptions.IgnoreCase);

            if (fallback.Success)
                return NormalizeNumber(fallback.Groups[1].Value);

            return null;
        }

        private static string? ExtractNamedValue(string source, string pattern)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            var match = Regex.Match(source, pattern, RegexOptions.IgnoreCase);
            return match.Success ? NormalizeNumber(match.Groups[1].Value) : null;
        }

        private static string? ExtractFirstNumber(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            var match = Regex.Match(source, @"(\d+(?:[.,]\d+)?)");
            return match.Success ? NormalizeNumber(match.Groups[1].Value) : null;
        }

        private static string NormalizeNumber(string value)
        {
            return value.Replace(",", ".").Trim();
        }
    }
}