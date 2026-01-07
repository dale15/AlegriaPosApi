namespace AlegriaPosApi.DTOs.Materials
{
    public class CreateMaterialDto
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal LowStockThreshold { get; set; }
        public decimal CostPerUnit { get; set; }
    }
}
