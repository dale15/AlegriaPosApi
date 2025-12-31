namespace AlegriaPosApi.DTOs.Materials
{
    public class AdjustMaterialStockDto
    {
        public decimal Quantity { get; set; } // positive or negative
        public string Reason { get; set; } = string.Empty;
    }
}
