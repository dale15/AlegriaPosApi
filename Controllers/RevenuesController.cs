using AlegriaPosApi.Data;
using AlegriaPosApi.DTOs.Revenues;
using AlegriaPosApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlegriaPosApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenuesController : ControllerBase
    {
        private readonly PosDbContext _context;

        public RevenuesController(PosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenues(
            [FromQuery] string range = "today", 
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var now = DateTime.UtcNow;
            IQueryable<SalesInvoice> query = _context.SalesInvoices;

            // 🔑 Normalize dates
            if (from.HasValue)
                from = DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc);

            if (to.HasValue)
                to = DateTime.SpecifyKind(to.Value.Date.AddDays(1), DateTimeKind.Utc);

            // 🔑 Apply explicit date range
            if (from.HasValue)
                query = query.Where(x => x.InvoiceDate >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.InvoiceDate < to.Value);

            List<RevenuePointDto> data;

            if (from.HasValue && to.HasValue)
            {
                data = await query
                    .GroupBy(x => x.InvoiceDate.Date)
                    .Select(g => new RevenuePointDto
                    {
                        Date = DateTime.SpecifyKind(g.Key, DateTimeKind.Utc),
                        Revenue = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();
            }
            else if (range == "today")
            {
                data = await query
                    .GroupBy(x => x.InvoiceDate.Hour)
                    .Select(g => new RevenuePointDto
                    {
                        Date = DateTime.SpecifyKind(
                            DateTime.UtcNow.Date.AddHours(g.Key),
                            DateTimeKind.Utc
                        ),
                        Revenue = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();
            }
            else if (range == "year")
            {
                data = await query
                    .GroupBy(x => new { x.InvoiceDate.Year, x.InvoiceDate.Month })
                    .Select(g => new RevenuePointDto
                    {
                        Date = new DateTime(
                            g.Key.Year,
                            g.Key.Month,
                            1,
                            0, 0, 0,
                            DateTimeKind.Utc   // 🔑 THIS FIXES IT
                        ),
                        Revenue = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();
            }
            else
            {
                data = await query
                    .GroupBy(x => x.InvoiceDate.Date)
                    .Select(g => new RevenuePointDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(x => x.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();
            }

            return Ok(new RevenueSummaryDto
            {
                TotalRevenue = data.Sum(x => x.Revenue),
                RevenuePoints = data
            });
        }

    }
}
