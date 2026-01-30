using AlegriaPosApi.DTOs.ProductModifiers;

namespace AlegriaPosApi.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public List<ProductModifierDto> Modifiers { get; set; } = new();
    }
}
