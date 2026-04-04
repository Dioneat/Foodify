using Foodify10.Models.JsonModels;

namespace Foodify10.Models
{
    public class ProductPageState
    {
        public ProductData? Product { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Composition { get; set; } = string.Empty;
        public string AnalysisText { get; set; } = string.Empty;

        public string Calories { get; set; } = "0";
        public string Proteins { get; set; } = "0";
        public string Fats { get; set; } = "0";
        public string Carbs { get; set; } = "0";

        public string NutriScore { get; set; } = "-";
        public Color NutriScoreBadgeColor { get; set; } = Colors.Gray;
        public string NutriExplanation { get; set; } = string.Empty;

        public string NovaScore { get; set; } = "-";
        public Color NovaBadgeColor { get; set; } = Colors.Gray;
        public string NovaTitle { get; set; } = "NOVA";
        public string NovaExplanation { get; set; } = string.Empty;

        public Color TrafficFatColor { get; set; } = Colors.LightGray;
        public Color TrafficSugarColor { get; set; } = Colors.LightGray;
        public Color TrafficSaltColor { get; set; } = Colors.LightGray;
        public string TrafficExplanation { get; set; } = string.Empty;

        public bool IsAnalysisVisible { get; set; } = true;
    }
}