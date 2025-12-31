namespace AlegriaPosApi.DTOs.Sales
{
    public class SaleDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public List<SaleModifierDto> Modifiers { get; set; } = new();
    }
}
