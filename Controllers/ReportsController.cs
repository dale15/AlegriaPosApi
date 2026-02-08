using AlegriaPosApi.Data;
using AlegriaPosApi.DTOs.Charts;
using AlegriaPosApi.Models;
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

        //[HttpGet("product-total-sales")]
        //public async Task<IActionResult> GetTotalProductSales(
        //    [FromQuery] DateTime? from,
        //    [FromQuery] DateTime? to)
        //{
        //    var query = _context.Sale.AsQueryable();

        //    if (from.HasValue)
        //        query = query.Where(s => s.SalesInvoice.InvoiceDate >= from.Value);

        //    if (to.HasValue)
        //        query = query.Where(s => s.SalesInvoice.InvoiceDate <= to.Value);

        //    var totalSales = await query
        //        .SumAsync(s => (decimal?)s.TotalPrice) ?? 0;

        //    return Ok(new
        //    {
        //        ProductId = productId,
        //        ProductName = productName,
        //        TotalSales = totalSales,
        //    });
        //}

        [HttpGet("product-sales")]
        public async Task<ActionResult<List<ProductSalesChartDto>>> GetProductSales(
            int productId,
            DateTime? from,
            DateTime? to)
        {
            var query = _context.Sale
                .Include(x => x.Product)
                .Include(x => x.SalesInvoice)
                .Where(x => x.ProductId == productId)
                .AsQueryable();

            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.SalesInvoice.InvoiceDate >= fromUtc);
            }

            if (to.HasValue)
            {
                var toUtcExclusive = DateTime.SpecifyKind(
                    to.Value.Date.AddDays(1),
                    DateTimeKind.Utc
                );
                query = query.Where(x => x.SalesInvoice.InvoiceDate < toUtcExclusive);
            }

            // 1️⃣ Group by DATE only (product is already filtered)
            var salesByDate = await query
                .GroupBy(x => new
                {
                    Date = x.SalesInvoice.InvoiceDate.Date
                })
                .Select(g => new ProductSalesByDateDto
                {
                    Date = g.Key.Date,
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalSales = g.Sum(x => x.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            if (!salesByDate.Any())
                return NotFound("No sales found for this product.");

            // 2️⃣ Get product info + summary
            var product = await _context.Products
                .Where(p => p.Id == productId)
                .Select(p => new
                {
                    p.Id,
                    p.Name
                })
                .FirstAsync();

            var result = new ProductSalesChartDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                TotalQuantitySold = salesByDate.Sum(x => x.QuantitySold),
                TotalSales = salesByDate.Sum(x => x.TotalSales),
                Sales = salesByDate
            };

            return Ok(result);
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts(
            [FromQuery] int limit = 5,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string orderBy = "sales") // "sales" | "quantity"
        {
            var query = _context.Sale
            .Include(s => s.SalesInvoice)
            .AsQueryable();

            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(
                    from.Value.Date,
                    DateTimeKind.Utc
                );

                query = query.Where(s =>
                    s.SalesInvoice.InvoiceDate >= fromUtc
                );
            }

            if (to.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(
                    to.Value.Date.AddDays(1).AddTicks(-1),
                    DateTimeKind.Utc
                );

                query = query.Where(s =>
                    s.SalesInvoice.InvoiceDate <= toUtc
                );
            }

            var grouped = query
                .GroupBy(s => new
                {
                    s.ProductId,
                    s.Product.Name
                })
                .Select(g => new
                {
                    productId = g.Key.ProductId,
                    productName = g.Key.Name,
                    totalQuantitySold = g.Sum(x => x.Quantity),
                    totalSales = g.Sum(x => x.TotalPrice)
                });

            grouped = orderBy == "quantity"
                ? grouped.OrderByDescending(x => x.totalQuantitySold)
                : grouped.OrderByDescending(x => x.totalSales);

            var result = await grouped
                .ToListAsync();

            return Ok(result);
        }
    }
}
