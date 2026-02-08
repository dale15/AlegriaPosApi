namespace AlegriaPosApi.Models
{
    public class Discount
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;         // "Senior", "Promo", etc.
        public string Type { get; set; } = string.Empty;      // "percent" | "fixed"
        public decimal Value { get; set; }         // 10 = 10% or ₱10
        public bool IsActive { get; set; }

        
    }
}
