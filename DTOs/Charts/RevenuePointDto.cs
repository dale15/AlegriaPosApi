using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AlegriaPosApi.DTOs.Revenues
{
    public class RevenuePointDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }

        // optional helper
        public string Label => Date.ToString("yyyy-MM-dd");
    }
}
