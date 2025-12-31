namespace AlegriaPosApi.DTOs.Materials
{
    public class UpdateMaterialDto
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal LowStockThreshold { get; set; }
    }
}
