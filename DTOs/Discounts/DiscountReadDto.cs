namespace AlegriaPosApi.DTOs.Discounts
{
    public class DiscountReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public bool IsActive { get; set; }
    }
}
