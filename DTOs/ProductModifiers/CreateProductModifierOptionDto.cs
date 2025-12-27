namespace AlegriaPosApi.DTOs.ProductModifiers
{
    public class CreateProductModifierOptionDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal PriceAdjustment { get; set; }
    }
}
