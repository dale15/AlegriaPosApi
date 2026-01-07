namespace AlegriaPosApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<ProductModifier> Modifiers { get; set; }
        = new List<ProductModifier>();

        public ICollection<ProductMaterial> ProductMaterials { get; set; }
        = new List<ProductMaterial>();
    }
}
