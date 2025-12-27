namespace AlegriaPosApi.DTOs.ProductModifiers
{
    public class ProductModifierOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PriceAdjustment { get; set; }
    }
}
