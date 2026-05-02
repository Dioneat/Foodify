using Foodify10.Models;
using Foodify10.Models.JsonModels;
using Foodify10.Services.Interfaces;
using System.Reflection;

namespace Foodify10.Services
{
    public class ProductPageStateFactory : IProductPageStateFactory
    {
        public ProductPageState CreateSuccessState(ProductData product, string explanationDetails)
        {
            double cal = ParseDouble(product.NutrinitionalValue?.Calories);
            double prot = ParseDouble(product.NutrinitionalValue?.Proteins);
            double fat = ParseDouble(product.NutrinitionalValue?.Fats);
            double carbs = ParseDouble(product.NutrinitionalValue?.Carbohydrates);

            // Эти поля читаются безопасно: если их нет в NutrinitionalValue, вернётся null
            double? sugars = GetOptionalNutritionValue(product.NutrinitionalValue, "Sugars");
            double? saturatedFats = GetOptionalNutritionValue(product.NutrinitionalValue, "SaturatedFats");
            double? salt = GetOptionalNutritionValue(product.NutrinitionalValue, "Salt");
            double? fiber = GetOptionalNutritionValue(product.NutrinitionalValue, "Fiber");

            string composition = product.Composition ?? string.Empty;
            string normalizedComposition = NormalizeComposition(composition);

            var nova = ScoringService.CalculateNovaEstimate(composition);

            var nutri = ScoringService.CalculateNutriScoreSmart(
                calories: cal,
                proteins: prot,
                composition: composition,
                carbs: carbs,
                fats: fat,
                sugars: sugars,
                saturatedFats: saturatedFats,
                salt: salt,
                fiber: fiber,
                kind: ProductKind.Food);

            // Жиры почти всегда точные
            var tlFat = ScoringService.GetTrafficLight(
                TrafficLightNutrient.Fat,
                fat > 0 ? fat : null,
                ProductKind.Food);

            // Сахара: если есть точные данные — используем их, если нет — аккуратно оцениваем
            var (sugarForTraffic, sugarEstimated) = EstimateSugarForTraffic(
                carbs,
                sugars,
                normalizedComposition);

            var tlSugar = ScoringService.GetTrafficLight(
                TrafficLightNutrient.Sugars,
                sugarForTraffic,
                ProductKind.Food);

            // Соль: если есть точные данные — используем их, если нет — осторожная оценка по составу
            var (saltForTraffic, saltEstimated) = EstimateSaltForTraffic(
                salt,
                normalizedComposition);

            var tlSalt = ScoringService.GetTrafficLight(
                TrafficLightNutrient.Salt,
                saltForTraffic,
                ProductKind.Food);

            return new ProductPageState
            {
                Product = product,
                Title = product.Name ?? "Неизвестный продукт",
                Composition = string.IsNullOrWhiteSpace(product.Composition)
                    ? "Состав не указан"
                    : product.Composition,
                AnalysisText = string.IsNullOrWhiteSpace(explanationDetails)
                    ? "Добавки не найдены"
                    : explanationDetails,

                Calories = SafeNutritionString(product.NutrinitionalValue?.Calories),
                Proteins = SafeNutritionString(product.NutrinitionalValue?.Proteins),
                Fats = SafeNutritionString(product.NutrinitionalValue?.Fats),
                Carbs = SafeNutritionString(product.NutrinitionalValue?.Carbohydrates),

                NutriScore = nutri.DisplayGrade,
                NutriScoreBadgeColor = nutri.BadgeColor,
                NutriExplanation = nutri.Explanation,

                NovaScore = nova.Group > 0 ? nova.Group.ToString() : "?",
                NovaBadgeColor = nova.BadgeColor,
                NovaTitle = $"NOVA: {nova.Title}",
                NovaExplanation = nova.Explanation,

                TrafficFatColor = tlFat.Color,
                TrafficSugarColor = tlSugar.Color,
                TrafficSaltColor = tlSalt.Color,

                TrafficExplanation = BuildTrafficExplanation(
                    fatValue: fat > 0 ? fat : null,
                    fatResult: tlFat,
                    sugarValue: sugarForTraffic,
                    sugarResult: tlSugar,
                    sugarEstimated: sugarEstimated,
                    saltValue: saltForTraffic,
                    saltResult: tlSalt,
                    saltEstimated: saltEstimated),

                IsAnalysisVisible = true
            };
        }

        public ProductPageState CreateNotFoundState()
        {
            return new ProductPageState
            {
                Title = "Товар не найден",
                Composition = "Мы не нашли этот штрих-код в наших базах.",
                AnalysisText = string.Empty,
                IsAnalysisVisible = false
            };
        }

        public ProductPageState CreateErrorState(string message)
        {
            return new ProductPageState
            {
                Title = "Ошибка",
                Composition = message,
                AnalysisText = string.Empty,
                IsAnalysisVisible = false
            };
        }

        public HistoryItem CreateHistoryItem(string barcode, ProductData product)
        {
            return new HistoryItem
            {
                Barcode = barcode,
                Name = product.Name,
                ScanTime = DateTime.Now.ToString("g"),
                Status = "Проверено",
                StatusColor = Colors.Green
            };
        }

        public string BuildShareText(ProductPageState state)
        {
            if (state.Product == null)
                return string.Empty;

            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"🔎 АНАЛИЗ ПРОДУКТА: {state.Title.ToUpper()}");
            sb.AppendLine("------------------------------------");
            sb.AppendLine($"🥗 Nutri-Score: 【 {state.NutriScore} 】");
            sb.AppendLine($"🏭 NOVA: Класс {state.NovaScore} ({state.NovaTitle})");
            sb.AppendLine("------------------------------------");
            sb.AppendLine("🚦 СИСТЕМА «СВЕТОФОР»:");
            sb.AppendLine($"• Жиры: {GetEmojiForColor(state.TrafficFatColor)}");
            sb.AppendLine($"• Сахар: {GetEmojiForColor(state.TrafficSugarColor)}");
            sb.AppendLine($"• Соль: {GetEmojiForColor(state.TrafficSaltColor)}");
            sb.AppendLine("------------------------------------");
            sb.AppendLine("📊 ЭНЕРГЕТИЧЕСКАЯ ЦЕННОСТЬ (на 100г):");
            sb.AppendLine($"🔥 Калории: {state.Calories} ккал");
            sb.AppendLine($"🥩 Белки: {state.Proteins} г");
            sb.AppendLine($"🥑 Жиры: {state.Fats} г");
            sb.AppendLine($"🍬 Углеводы: {state.Carbs} г");

            if (!string.IsNullOrWhiteSpace(state.Composition))
            {
                sb.AppendLine("------------------------------------");
                sb.AppendLine("📝 СОСТАВ:");
                sb.AppendLine(state.Composition);
            }

            if (!string.IsNullOrWhiteSpace(state.NutriExplanation))
            {
                sb.AppendLine("------------------------------------");
                sb.AppendLine("ℹ️ ПОЯСНЕНИЕ:");
                sb.AppendLine(state.NutriExplanation);
            }

            sb.AppendLine("\n📱 Проверено в Foodify10");

            return sb.ToString();
        }

        private static string BuildTrafficExplanation(
            double? fatValue,
            TrafficLightResult fatResult,
            double? sugarValue,
            TrafficLightResult sugarResult,
            bool sugarEstimated,
            double? saltValue,
            TrafficLightResult saltResult,
            bool saltEstimated)
        {
            string fatLine = fatValue.HasValue
                ? $"Жиры: {fatValue.Value:0.##} г ({fatResult.Status.ToLower()})."
                : "Жиры: нет данных.";

            string sugarLine = sugarValue.HasValue
                ? $"Сахара{(sugarEstimated ? " (оценка)" : string.Empty)}: {sugarValue.Value:0.##} г ({sugarResult.Status.ToLower()})."
                : "Сахара: нет данных.";

            string saltLine = saltValue.HasValue
                ? $"Соль{(saltEstimated ? " (оценка)" : string.Empty)}: {saltValue.Value:0.##} г ({saltResult.Status.ToLower()})."
                : "Соль: нет данных.";

            return $"{fatLine}\n{sugarLine}\n{saltLine}";
        }

        private static (double? Value, bool IsEstimated) EstimateSugarForTraffic(
            double carbs,
            double? explicitSugars,
            string normalizedComposition)
        {
            if (explicitSugars.HasValue)
                return (Math.Max(0, explicitSugars.Value), false);

            if (carbs <= 0)
                return (0, true);

            bool hasSugarMarkers = ContainsAny(
                normalizedComposition,
                "сахар",
                "глюкозно-фруктозный сироп",
                "сироп",
                "фруктоза",
                "декстроза",
                "патока",
                "мед");

            bool sugarIsEarly = ContainsEarlyIngredient(
                normalizedComposition,
                "сахар",
                "глюкозно-фруктозный сироп",
                "сироп");

            if (sugarIsEarly)
                return (Math.Min(carbs, carbs * 0.6), true);

            if (hasSugarMarkers)
                return (Math.Min(carbs, carbs * 0.35), true);

            // Если ничего не указывает на сахар, не выдумываем его слишком агрессивно
            return (null, true);
        }

        private static (double? Value, bool IsEstimated) EstimateSaltForTraffic(
            double? explicitSalt,
            string normalizedComposition)
        {
            if (explicitSalt.HasValue)
                return (Math.Max(0, explicitSalt.Value), false);

            bool hasSalt = ContainsAny(normalizedComposition, "соль");
            bool saltIsEarly = ContainsEarlyIngredient(normalizedComposition, "соль");

            if (saltIsEarly)
                return (0.8, true);

            if (hasSalt)
                return (0.3, true);

            return (null, true);
        }

        private static double? GetOptionalNutritionValue(object? nutritionObject, string propertyName)
        {
            if (nutritionObject == null)
                return null;

            PropertyInfo? property = nutritionObject.GetType().GetProperty(propertyName);
            if (property == null)
                return null;

            object? rawValue = property.GetValue(nutritionObject);
            if (rawValue == null)
                return null;

            return ParseNullableDouble(rawValue.ToString());
        }

        private static string SafeNutritionString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "0" : value;
        }

        private static double ParseDouble(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            string cleanInput = input.Replace(",", ".").Trim();

            if (double.TryParse(
                cleanInput,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double result))
            {
                return result;
            }

            return 0;
        }

        private static double? ParseNullableDouble(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            string cleanInput = input.Replace(",", ".").Trim();

            if (double.TryParse(
                cleanInput,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double result))
            {
                return result;
            }

            return null;
        }

        private static string NormalizeComposition(string? input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? string.Empty
                : input.ToLowerInvariant().Replace('ё', 'е');
        }

        private static bool ContainsAny(string source, params string[] markers)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;

            return markers.Any(source.Contains);
        }

        private static bool ContainsEarlyIngredient(string source, params string[] markers)
        {
            if (string.IsNullOrWhiteSpace(source))
                return false;

            var ingredients = source
                .Split(',', ';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(3)
                .ToList();

            return ingredients.Any(i => markers.Any(i.Contains));
        }

        private static string GetEmojiForColor(Color color)
        {
            if (color == Colors.LightGreen || color == Color.FromArgb("#27AE60")) return "🟢 (Низкое)";
            if (color == Colors.Gold || color == Colors.Yellow || color == Color.FromArgb("#F39C12")) return "🟡 (Среднее)";
            if (color == Colors.Red || color == Color.FromArgb("#E74C3C")) return "🔴 (Высокое)";
            return "⚪ (Нет данных)";
        }
    }
}