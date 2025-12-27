namespace AlegriaPosApi.Models
{
    public class ProductModifierOption
    {
        public int Id { get; set; }

        public int ProductModifierId { get; set; }
        public ProductModifier ProductModifier { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public decimal PriceAdjustment { get; set; } // +10, +20, +0
    }
}
