namespace AlegriaPosApi.DTOs.Materials
{
    public class AdjustMaterialStockDto
    {
        public int QuantityChange { get; set; } // positive or negative
        public string Reason { get; set; } = string.Empty;
    }
}
