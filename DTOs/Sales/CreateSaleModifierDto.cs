namespace AlegriaPosApi.DTOs.Sales
{
    public class CreateSaleModifierDto
    {
        public string ModifierName { get; set; } = string.Empty;
        public string OptionName { get; set; } = string.Empty;
        public decimal PriceAdjustment { get; set; }
    }
}
