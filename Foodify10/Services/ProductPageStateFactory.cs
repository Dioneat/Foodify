using Foodify10.Models;
using Foodify10.Models.JsonModels;
using Foodify10.Services.Interfaces;

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

            string compLower = product.Composition?.ToLower() ?? string.Empty;

            double sugarApprox = 0;
            double sodiumApprox = 0;
            double satFatApprox = fat / 2;

            if (compLower.Contains("сахар") || compLower.Contains("сироп"))
                sugarApprox = carbs > 0 ? carbs : 10;

            if (compLower.Contains("соль"))
                sodiumApprox = 1.2;

            var nova = ScoringService.CalculateNova(product.Composition);
            var nutri = ScoringService.CalculateNutriScore(cal, sugarApprox, satFatApprox, sodiumApprox, prot);

            var tlFat = ScoringService.GetTrafficLight(fat, 3, 20);
            var tlSugar = ScoringService.GetTrafficLight(sugarApprox, 5, 22.5);
            var tlSalt = ScoringService.GetTrafficLight(sodiumApprox, 0.3, 1.5);

            return new ProductPageState
            {
                Product = product,
                Title = product.Name ?? "Неизвестный продукт",
                Composition = product.Composition ?? "Состав не указан",
                AnalysisText = explanationDetails ?? "Добавки не найдены",

                Calories = product.NutrinitionalValue?.Calories?.ToString() ?? "0",
                Proteins = product.NutrinitionalValue?.Proteins?.ToString() ?? "0",
                Fats = product.NutrinitionalValue?.Fats?.ToString() ?? "0",
                Carbs = product.NutrinitionalValue?.Carbohydrates?.ToString() ?? "0",

                NutriScore = nutri.Score,
                NutriScoreBadgeColor = nutri.BadgeColor,
                NutriExplanation = nutri.Explanation,

                NovaScore = nova.Score > 0 ? nova.Score.ToString() : "?",
                NovaBadgeColor = nova.BadgeColor,
                NovaTitle = $"NOVA: {nova.Title}",
                NovaExplanation = nova.Explanation,

                TrafficFatColor = tlFat.Color,
                TrafficSugarColor = tlSugar.Color,
                TrafficSaltColor = sodiumApprox > 0 ? tlSalt.Color : Colors.LightGray,

                TrafficExplanation =
                    $"Жиры: {fat}г ({tlFat.Status}).\n" +
                    $"Сахар (расчетно): {sugarApprox}г ({tlSugar.Status}).\n" +
                    (sodiumApprox > 0
                        ? $"Соль (расчетно): ~{sodiumApprox}г ({tlSalt.Status})."
                        : "Соль не найдена."),

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
            sb.AppendLine($"🥗 Nutri-Score:  【 {state.NutriScore} 】");
            sb.AppendLine($"🏭 NOVA:  Класс {state.NovaScore} ({state.NovaTitle})");
            sb.AppendLine("------------------------------------");
            sb.AppendLine("🚦 СИСТЕМА «СВЕТОФОР»:");
            sb.AppendLine($"• Жиры: {GetEmojiForColor(state.TrafficFatColor)}");
            sb.AppendLine($"• Сахар: {GetEmojiForColor(state.TrafficSugarColor)} (расчетно)");
            sb.AppendLine($"• Соль: {GetEmojiForColor(state.TrafficSaltColor)} (расчетно)");
            sb.AppendLine("------------------------------------");
            sb.AppendLine("📊 ЭНЕРГЕТИЧЕСКАЯ ЦЕННОСТЬ (на 100г):");
            sb.AppendLine($"🔥 Калории: {state.Calories} ккал");
            sb.AppendLine($"🥩 Белки: {state.Proteins}г");
            sb.AppendLine($"🥑 Жиры: {state.Fats}г");
            sb.AppendLine($"🍬 Углеводы: {state.Carbs}г");

            if (!string.IsNullOrWhiteSpace(state.Composition))
            {
                sb.AppendLine("------------------------------------");
                sb.AppendLine("📝 СОСТАВ:");
                sb.AppendLine(state.Composition);
            }

            sb.AppendLine("\n📱 Проверено в Foodify10");

            return sb.ToString();
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

        private static string GetEmojiForColor(Color color)
        {
            if (color == Colors.LightGreen || color == Color.FromArgb("#27AE60")) return "🟢 (Норма)";
            if (color == Colors.Gold || color == Colors.Yellow) return "🟡 (Средне)";
            if (color == Colors.Red || color == Color.FromArgb("#E74C3C")) return "🔴 (Много)";
            return "⚪ (Нет данных)";
        }
    }
}