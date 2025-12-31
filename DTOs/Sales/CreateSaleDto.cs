namespace AlegriaPosApi.DTOs.Sales
{
    public class CreateSaleDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public List<CreateSaleModifierDto> Modifiers { get; set; } = new();
    }
}
