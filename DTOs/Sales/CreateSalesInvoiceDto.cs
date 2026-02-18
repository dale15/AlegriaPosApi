namespace AlegriaPosApi.DTOs.Sales
{
    public class CreateSalesInvoiceDto
    {
        public int? DiscountId { get; set; }
        public decimal? Tax { get; set; }
        public decimal Discount { get; set; }

        public List<CreateSaleDto> Items { get; set; } = new();

        public List<CreatePaymentDto> Payments { get; set; } = new();
    }
}
