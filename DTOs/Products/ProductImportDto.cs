namespace AlegriaPosApi.DTOs.Products
{
    public class ProductImportDto
    {
        public string sku { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string costprice { get; set; } = string.Empty;
        public string sellingprice { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string? modifiers { get; set; }
    }
}
