namespace AlegriaPosApi.DTOs.Sales
{
    public class CreateSalesInvoiceDto
    {
        public decimal? Tax { get; set; }
        public decimal Discount { get; set; }

        public List<CreateSaleDto> Items { get; set; } = new();
    }
}
