using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foodify10.Models.JsonModels
{
    public class RskrfModels
    {
        //Для блока товаров со знаком Роскачества
        public class QualityProductResponse
        {
            public List<QualityProduct> Response { get; set; }
        }

        public class QualityProduct
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Barcode { get; set; }
        }
        //JSON модели
        public class RskrfSearchResponse
        {
            [JsonPropertyName("response")]
            public SearchResult Response { get; set; }

            [JsonPropertyName("message")]
            public JsonElement[] Message { get; set; }
        }

        public class SearchResult
        {
            [JsonPropertyName("total_items")]
            public int TotalItems { get; set; }

            [JsonPropertyName("products")]
            public ProductItem[] Products { get; set; }
        }

        public class ProductItem
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }
        }

        // --- Классы для получения деталей продукта ---
        public class RskrfProductDetailResponse
        {
            [JsonPropertyName("response")]
            public ProductDetail Response { get; set; }

            [JsonPropertyName("message")]
            public JsonElement[] Message { get; set; }
        }

        public class ProductDetail
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("total_rating")]
            public double TotalRating { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("product_link")]
            public string ProductLink { get; set; }

            [JsonPropertyName("category_name")]
            public string CategoryName { get; set; }

            [JsonPropertyName("manufacturer")]
            public string Manufacturer { get; set; }

            [JsonPropertyName("research")]
            public Research Research { get; set; }

            [JsonPropertyName("worth")]
            public string[] Worth { get; set; }

            [JsonPropertyName("disadvantage")]
            public string[] Disadvantage { get; set; }

            [JsonPropertyName("product_info")]
            public ProductInfo[] ProductInfo { get; set; }

            [JsonPropertyName("criteria_ratings")]
            public CriteriaRating[] CriteriaRatings { get; set; }

            [JsonPropertyName("price")]
            public string Price { get; set; }

            [JsonPropertyName("thumbnail")]
            public string Thumbnail { get; set; }

            [JsonPropertyName("product_documents")]
            public ProductDocument[] ProductDocuments { get; set; }

            [JsonPropertyName("is_in_favorites")]
            public bool IsInFavorites { get; set; }
            [JsonPropertyName("has_quality_mark")]
            public bool HasQualityMark { get; set; }
        }

        public class Research
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("product_group")]
            public string ProductGroup { get; set; }

            [JsonPropertyName("image")]
            public string Image { get; set; }

            [JsonPropertyName("date")]
            public string Date { get; set; }
        }

        public class ProductInfo
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("info")]
            public string Info { get; set; }
        }

        public class CriteriaRating
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("value")]
            public double Value { get; set; }
        }

        public class ProductDocument
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("file")]
            public string File { get; set; }
        }


        public class ProductInfoItem
        {
            public string Name { get; set; }
            public string Info { get; set; }
        }

        ///
        public class ApiResponse
        {
            public Product[] Products { get; set; }
        }

        public class Product
        {
            public int Plu { get; set; }
            public string Name { get; set; }
        }

        public class SimpleResponse
        {
            [JsonPropertyName("header")]
            public Header Header { get; set; }

            [JsonPropertyName("blocks")]
            public List<Block> Blocks { get; set; }
        }

        public class Header
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
        }

        public class Block
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("payload")]
            public JsonElement Payload { get; set; }
        }

        public class PlaceItem
        {
            [JsonPropertyName("slug")]
            public string Slug { get; set; }

            [JsonPropertyName("items")]
            public List<MenuItem> Items { get; set; }
        }

        public class MenuItem
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("price")]
            public string Price { get; set; }

            [JsonPropertyName("decimal_price")]
            public string DecimalPrice { get; set; }

            [JsonPropertyName("price_discount_value")]
            public int PriceDiscountValue { get; set; }

            [JsonPropertyName("decimal_old_price")]
            public string DecimalOldPrice { get; set; }

            [JsonPropertyName("weight")]
            public string Weight { get; set; }

            [JsonPropertyName("parent_category_id")]
            public string ParentCategoryId { get; set; }

            [JsonPropertyName("search_type")]
            public string SearchType { get; set; }
        }

        public class ProductResponse
        {
            [JsonPropertyName("menu_item")]
            public Menu_Item MenuItem { get; set; }

            [JsonPropertyName("detailed_data")]
            public List<DetailedData> DetailedData { get; set; }
        }

        public class Menu_Item
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("weight")]
            public string Weight { get; set; }

            [JsonPropertyName("decimalPrice")]
            public string DecimalPrice { get; set; }
        }

        public class DetailedData
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("payload")]
            public JsonElement? Payload { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }
        }

        public class EnergyValuesPayload
        {
            [JsonPropertyName("energy_values")]
            public List<EnergyValue> EnergyValues { get; set; }
        }

        public class EnergyValue
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; }
        }

        public class ProductDetailsPayload
        {
            [JsonPropertyName("product_descriptions")]
            public List<ProductDescription> ProductDescriptions { get; set; }
        }

        public class ProductDescription
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; }
        }
    }
}
