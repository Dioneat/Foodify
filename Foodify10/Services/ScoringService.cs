using System.Text.RegularExpressions;

namespace Foodify10.Services
{
    public static class ScoringService
    {
        public static (int Score, Color BadgeColor, string Title, string Explanation) CalculateNova(string composition)
        {
            if (string.IsNullOrWhiteSpace(composition))
                return (0, Colors.Gray, "Нет данных", "Состав не указан, невозможно оценить степень обработки.");

            string compLower = composition.ToLower();

            
            string[] nova4Markers = { "ароматизатор", "краситель", "маргарин", "гидрогенизирован", "усилитель", "консервант", "заменитель", "подсластитель" };
            string[] nova3Markers = { "соль", "сахар", "масло", "уксус", "сироп" };

            bool hasEAdditives = Regex.IsMatch(composition, @"[ЕEеe]\s*-?\d{3,4}");

            if (hasEAdditives || nova4Markers.Any(m => compLower.Contains(m)))
            {
               
                string trigger = nova4Markers.FirstOrDefault(m => compLower.Contains(m)) ?? "Е-добавки";
                return (4, Color.FromArgb("#E74C3C"), "Ультра-обработанный", $"Содержит промышленные ингредиенты ({trigger}). Это продукт глубокой переработки.");
            }

            if (nova3Markers.Any(m => compLower.Contains(m)))
                return (3, Color.FromArgb("#F39C12"), "Обработанный", "Продукт прошел кулинарную обработку (добавлены соль, сахар или масло).");

            if (composition.Split(',').Length > 3)
                return (2, Color.FromArgb("#F1C40F"), "Мин. обработка", "Простая физическая обработка без добавления сложной химии.");

            return (1, Color.FromArgb("#27AE60"), "Необработанный", "Цельный продукт без добавок.");
        }
        // Пока что примитивный расчет Nutri-Score, в дальнейшем необходимо дорабоать, т.к. доля сахара и соли берется примерная
        public static (string Score, Color BadgeColor, string Explanation) CalculateNutriScore(double calories, double sugar, double satFats, double sodium, double proteins)
        {
            int negativePoints = 0;
            if (calories > 800) negativePoints += 3; else if (calories > 300) negativePoints += 1;
            if (sugar > 20) negativePoints += 3; else if (sugar > 9) negativePoints += 1;
            if (satFats > 10) negativePoints += 3;

            int totalScore = negativePoints - (proteins > 8 ? 2 : 0);

            string expl = $"Учтено: Ккал ({calories}), Сахар ({sugar}г), Насыщ. жиры ({satFats}г). Баллы снижены за избыток углеводов/жиров и компенсированы белком ({proteins}г).";

            if (totalScore <= 0) return ("A", Color.FromArgb("#008137"), expl);
            if (totalScore <= 2) return ("B", Color.FromArgb("#85BB2F"), expl);
            if (totalScore <= 5) return ("C", Color.FromArgb("#FECB02"), expl);
            if (totalScore <= 8) return ("D", Color.FromArgb("#EE8100"), expl);
            return ("E", Color.FromArgb("#E63E11"), expl);
        }

        // Аналогично Nutri-Score, т.к. нет точных данных по сахару и соли из данных, которые парсятся, беру только примерные
        public static (Color Color, string Status) GetTrafficLight(double value, double limitGreen, double limitRed)
        {
            if (value <= limitGreen) return (Colors.LightGreen, "Норма");
            if (value <= limitRed) return (Colors.Gold, "Средне");
            return (Colors.Red, "Много");
        }
    }
}
