using AlegriaPosApi.Data;
using AlegriaPosApi.DTOs.Charts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlegriaPosApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public ReportsController(PosDbContext context)
        {
            _context = context;
        }

        [HttpGet("product-sales")]
        public async Task<ActionResult<List<ProductSalesChartDto>>> GetProductSales(
            DateTime? from,
            DateTime? to)
        {
            var query = _context.Sale
                .Include(x => x.Product)
                .Include(x => x.SalesInvoice)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(x => x.SalesInvoice.InvoiceDate >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.SalesInvoice.InvoiceDate <= to.Value);

            // 1️⃣ Group by PRODUCT + DATE
            var data = await query
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.Product.Name,
                    Date = x.SalesInvoice.InvoiceDate.Date
                })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.Name,
                    g.Key.Date,
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalSales = g.Sum(x => x.TotalPrice)
                })
                .ToListAsync();

            // 2️⃣ Group again by PRODUCT
            var result = data
                .GroupBy(x => new { x.ProductId, x.Name })
                .Select(g => new ProductSalesChartDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    Sales = g
                        .OrderBy(x => x.Date)
                        .Select(x => new ProductSalesByDateDto
                        {
                            Date = x.Date,
                            QuantitySold = x.QuantitySold,
                            TotalSales = x.TotalSales
                        })
                        .ToList()
                })
                .ToList();

            return Ok(result);
        }

    }
}
