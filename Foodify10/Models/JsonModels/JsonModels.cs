namespace Foodify10.Models.JsonModels
{
    public class DangerInfo
    {
        public string Text { get; set; }
        public int DangerLevel { get; set; }
    }

    public class AdditiveJsonModel
    {
        public string Title { get; set; }
        public string ECode { get; set; }
        public string Name { get; set; }
        public DangerInfo Danger { get; set; }
        public string Category { get; set; }
        public List<string> Synonyms { get; set; }
        public string Content { get; set; }
    }

    public class ProductCriterionRatingData
    {
        public string Title { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    public class ProductResearchData
    {
        public bool HasQualityMark { get; set; }
        public List<string> Worth { get; set; } = new();
        public List<ProductCriterionRatingData> CriteriaRatings { get; set; } = new();
        public string? Manufacturer { get; set; }
        public string? CategoryName { get; set; }
        public string? ProductLink { get; set; }
        public string? Thumbnail { get; set; }
    }

    public record ProductSearchRequest(string Barcode, string? Name = null);

    public record ProductData(
        string Barcode,
        string Name,
        string Composition,
        NutrinitionalValue NutrinitionalValue)
    {
        public ProductResearchData? ResearchData { get; init; }
        public string? SourceName { get; init; }
    }

    public record ReviewData(IEnumerable<string> Reviews, double AverageRating);
    public record CompositionExplanation(string Details);

    public record ProductFinalResult(
        ProductData? Product,
        ReviewData? Reviews,
        CompositionExplanation? Explanation);

    public interface IProductDataProvider
    {
        Task<ProductData?> TryGetProductAsync(ProductSearchRequest request);
    }

    public interface IAverageProductDataProvider
    {
        Task<ProductData> GetAverageDataAsync(ProductSearchRequest request);
    }

    public interface IReviewProvider
    {
        Task<ReviewData?> GetReviewsAsync(ProductData productData);
    }

    public interface ICompositionExplanationProvider
    {
        Task<CompositionExplanation> GetCompositionExplanationAsync(ProductData productData);
    }
}