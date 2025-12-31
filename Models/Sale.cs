namespace AlegriaPosApi.Models
{
    public class Sale
    {
        public int Id { get; set; }

        public int SalesInvoiceId { get; set; }
        public SalesInvoice SalesInvoice { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public ICollection<SaleModifier> Modifiers { get; set; } = new List<SaleModifier>();
    }
}
