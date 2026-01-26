namespace AlegriaPosApi.DTOs.Charts
{
    public class ProductSalesChartDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public List<ProductSalesByDateDto> Sales { get; set; } = new();
    }

    public class ProductSalesByDateDto
    {
        public DateTime Date { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalSales { get; set; }
    }
}
