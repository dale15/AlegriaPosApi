namespace AlegriaPosApi.DTOs.Sales
{
    public class SalesInvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }

        public List<SaleDto> Items { get; set; } = new();
    }
}
