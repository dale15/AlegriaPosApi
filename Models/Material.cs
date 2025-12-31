namespace AlegriaPosApi.Models
{
    public class Material
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // g, ml, pcs, etc.
        public string Unit { get; set; } = string.Empty;

        public decimal CurrentStock { get; set; }

        public decimal LowStockThreshold { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
