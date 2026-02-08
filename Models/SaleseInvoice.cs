using AlegriaPosApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AlegriaPosApi.Models
{
    public class SalesInvoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public decimal SubTotal { get; set; }
        public decimal? Tax { get; set; }

        public int? DiscountId { get; set; }
        public Discount? Discount { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<Sale> Sales { get; set; } = new List<Sale>();


    }
}
