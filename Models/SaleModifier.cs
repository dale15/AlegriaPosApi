namespace AlegriaPosApi.Models
{
    public class SaleModifier
    {
        public int Id { get; set; }

        public int SaleId { get; set; }
        public Sale Sale { get; set; } = null!;

        public string ModifierName { get; set; } = string.Empty; // e.g. Size
        public string OptionName { get; set; } = string.Empty;   // e.g. Large

        public decimal PriceAdjustment { get; set; }             // +20, +30
    }
}
