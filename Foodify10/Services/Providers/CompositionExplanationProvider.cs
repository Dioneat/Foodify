using Foodify10.Models.JsonModels;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Foodify10.Services.Providers
{
    public class CompositionExplanationProvider : ICompositionExplanationProvider
    {
        // Словарь для быстрого поиска по E-коду
        private readonly Dictionary<string, AdditiveJsonModel> _additivesByECode = new();
        // Полный список для поиска по названиям и синонимам
        private readonly List<AdditiveJsonModel> _allAdditives = new();

        private readonly string _fileName;
        private bool _isInitialized = false;

        public CompositionExplanationProvider(string fileName = "additives.json")
        {
            _fileName = fileName;
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(_fileName);
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var additives = JsonSerializer.Deserialize<List<AdditiveJsonModel>>(json, options) ?? new();

                foreach (var additive in additives)
                {
                    _allAdditives.Add(additive);

                    if (!string.IsNullOrEmpty(additive.ECode))
                    {
                        string mainKey = NormalizeKey(additive.ECode);
                        _additivesByECode[mainKey] = additive;
                    }
                }

                _isInitialized = true;
                Debug.WriteLine($"[Database] База добавок загружена: {_allAdditives.Count} записей.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error] Не удалось загрузить базу: {ex.Message}");
            }
        }

        public async Task<CompositionExplanation> GetCompositionExplanationAsync(ProductData productData)
        {
            await InitializeAsync();

            if (string.IsNullOrWhiteSpace(productData.Composition))
                return new CompositionExplanation("Состав продукта не указан.");

            if (_allAdditives.Count == 0)
                return new CompositionExplanation("База пищевых добавок пуста или не загрузилась.");

            var foundAdditives = new HashSet<AdditiveJsonModel>();
            string lowerComposition = productData.Composition.ToLower(); 


            var extractedECodes = ExtractENumbers(productData.Composition);
            foreach (var eCode in extractedECodes)
            {
                if (_additivesByECode.TryGetValue(eCode, out var additive))
                {
                    foundAdditives.Add(additive);
                }
            }


            foreach (var additive in _allAdditives)
            {

                if (foundAdditives.Contains(additive)) continue;


                if (!string.IsNullOrEmpty(additive.Name) && lowerComposition.Contains(additive.Name.ToLower()))
                {
                    foundAdditives.Add(additive);
                    continue;
                }


                if (additive.Synonyms != null)
                {
                    foreach (var synonym in additive.Synonyms)
                    {
                        if (lowerComposition.Contains(synonym.ToLower()))
                        {
                            foundAdditives.Add(additive);
                            break; 
                        }
                    }
                }
            }


            if (foundAdditives.Count == 0)
                return new CompositionExplanation("Опасные или специфические пищевые добавки в составе не обнаружены.");

            var sb = new StringBuilder("Анализ состава:\n\n");

            foreach (var additive in foundAdditives)
            {
                string title = !string.IsNullOrEmpty(additive.Title) ? additive.Title : additive.Name;
                string category = !string.IsNullOrEmpty(additive.Category) ? additive.Category : "Неизвестная категория";
                string danger = additive.Danger?.Text ?? "Уровень опасности не указан";
                string description = ExtractDescriptionFromHtml(additive.Content);

                sb.AppendLine($"🔹 {title}");
                sb.AppendLine($"Категория: {category}");
                sb.AppendLine($"Опасность: {danger}");
                sb.AppendLine($"{description}\n");
            }

            return new CompositionExplanation(sb.ToString().TrimEnd());
        }


        private HashSet<string> ExtractENumbers(string composition)
        {
            var results = new HashSet<string>();
            // Ищет Е, E, е, e, за которыми могут идти пробелы или дефисы, а затем цифры
            var regex = new Regex(@"[ЕEеe]\s*-?\s*(\d{3,4}[a-zA-Zа-яА-Я]?)");
            var matches = regex.Matches(composition);

            foreach (Match match in matches)
            {
                results.Add(NormalizeKey(match.Value));
            }
            return results;
        }

        private string NormalizeKey(string input)
        {
            string raw = input.ToLower().Replace(" ", "").Replace("-", "");
            raw = raw.Replace("а", "a").Replace("в", "b").Replace("с", "c").Replace("і", "i");

            if (!raw.StartsWith("e") && !raw.StartsWith("е"))
                raw = "e" + Regex.Replace(raw, @"^[еe]", "");
            else
                raw = "e" + raw.Substring(1);

            return raw;
        }

        private string ExtractDescriptionFromHtml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent)) return "Описание отсутствует.";

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);


            var paragraphs = doc.DocumentNode.SelectNodes("//p")?
                                .Take(2)
                                .Select(p => HtmlEntity.DeEntitize(p.InnerText).Trim());

            return (paragraphs != null && paragraphs.Any())
                ? Regex.Replace(string.Join(" ", paragraphs), @"\s+", " ") 
                : "Описание отсутствует.";
        }
    }
}
