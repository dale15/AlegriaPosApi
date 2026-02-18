using AlegriaPosApi.Models;

namespace AlegriaPosApi.DTOs.Sales
{
    public class CreatePaymentDto
    {
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
    }
}
