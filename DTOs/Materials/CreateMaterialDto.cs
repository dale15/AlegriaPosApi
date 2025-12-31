namespace AlegriaPosApi.DTOs.Materials
{
    public class CreateMaterialDto
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal InitialStock { get; set; }
        public decimal LowStockThreshold { get; set; }
    }
}
