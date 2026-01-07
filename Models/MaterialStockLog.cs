namespace AlegriaPosApi.Models
{
    public class MaterialStockLog
    {
        public int Id { get; set; }
        public int MaterialId { get; set; }
        public int QuantityChange { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ReferenceType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
