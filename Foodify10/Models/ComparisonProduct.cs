namespace Foodify10.Models
{
    public class ComparisonProduct
    {
        public string Name { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string Composition { get; set; } = string.Empty;

        public string Calories { get; set; } = "0";
        public string Proteins { get; set; } = "0";
        public string Fats { get; set; } = "0";
        public string Carbohydrates { get; set; } = "0";
    }
}