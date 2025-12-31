namespace AlegriaPosApi.DTOs.Sales
{
    public class SaleModifierDto
    {
        public string ModifierName { get; set; } = string.Empty;
        public string OptionName { get; set; } = string.Empty;
        public decimal PriceAdjustment { get; set; }
    }
}
