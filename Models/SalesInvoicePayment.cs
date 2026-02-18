namespace AlegriaPosApi.Models
{
    public enum PaymentMethod
    {
        Cash = 1,
        GCash = 2,
        Card = 3,
        BankTransfer = 4
    }

    public class SalesInvoicePayment
    {
        public int Id { get; set; }

        public int SalesInvoiceId { get; set; }
        public SalesInvoice SalesInvoice { get; set; } = null!;

        public PaymentMethod PaymentMethod { get; set; }

        public decimal Amount { get; set; }
    }
}
