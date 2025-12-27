namespace AlegriaPosApi.Models
{
    public class ProductModifier
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Name { get; set; } = string.Empty; // Size, Add-ons
        public bool IsRequired { get; set; } // Size = true
        public bool IsMultiple { get; set; } // Add-ons = true

        public ICollection<ProductModifierOption> Options { get; set; }
            = new List<ProductModifierOption>();
    }
}
