namespace AlegriaPosApi.DTOs.Products
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = "";
        public string SKU { get; set; } = "";
        public int CategoryId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
    }
}
