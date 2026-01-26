namespace AlegriaPosApi.DTOs.Revenues
{
    public class RevenueSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public List<RevenuePointDto> RevenuePoints { get; set; } = new();
    }
}
