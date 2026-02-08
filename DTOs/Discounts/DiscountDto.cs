namespace AlegriaPosApi.DTOs.Discounts
{
    public class DiscountDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // percent | fixed
        public decimal Value { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
