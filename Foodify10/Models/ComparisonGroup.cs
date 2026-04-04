namespace Foodify10.Models
{
    public class ComparisonGroup
    {
        public string Name { get; set; } = string.Empty;
        public List<ComparisonProduct> Products { get; set; } = new();
    }
}