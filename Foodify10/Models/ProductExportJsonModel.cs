namespace Foodify10.Models
{
    public class ProductExportJsonModel
    {
        public string ExportedAt { get; set; } = string.Empty;
        public string AppName { get; set; } = "Foodify10";

        public string Barcode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Composition { get; set; } = string.Empty;
        public string AnalysisText { get; set; } = string.Empty;

        public NutritionExportJsonModel Nutrition { get; set; } = new();
        public ScoresExportJsonModel Scores { get; set; } = new();
        public ResearchExportJsonModel Research { get; set; } = new();
    }

    public class NutritionExportJsonModel
    {
        public string Calories { get; set; } = "0";
        public string Proteins { get; set; } = "0";
        public string Fats { get; set; } = "0";
        public string Carbohydrates { get; set; } = "0";
    }

    public class ScoresExportJsonModel
    {
        public string NutriScore { get; set; } = "-";
        public string NutriExplanation { get; set; } = string.Empty;

        public string NovaScore { get; set; } = "-";
        public string NovaTitle { get; set; } = string.Empty;
        public string NovaExplanation { get; set; } = string.Empty;

        public string TrafficExplanation { get; set; } = string.Empty;
    }

    public class ResearchExportJsonModel
    {
        public string SourceTitle { get; set; } = string.Empty;
        public bool HasQualityMark { get; set; }
        public List<string> WorthItems { get; set; } = new();
        public List<ResearchCriterionExportJsonModel> CriteriaRatings { get; set; } = new();
    }

    public class ResearchCriterionExportJsonModel
    {
        public string Title { get; set; } = string.Empty;
        public double Value { get; set; }
    }
}