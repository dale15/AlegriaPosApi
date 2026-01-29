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
        private static readonly TimeZoneInfo ManilaTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

        public RevenuesController(PosDbContext context)
        {
            _context = context;
        }

        private static (DateTime startUtc, DateTime endUtc) GetPhDayUtcRange(DateTime phDate)
        {
            var startPh = DateTime.SpecifyKind(phDate.Date, DateTimeKind.Unspecified);
            var endPh = startPh.AddDays(1);

            return (
                TimeZoneInfo.ConvertTimeToUtc(startPh, ManilaTz),
                TimeZoneInfo.ConvertTimeToUtc(endPh, ManilaTz)
            );
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
                var startOfToday = DateTime.UtcNow.Date;
                var startOfTomorrow = startOfToday.AddDays(1);

                query = query.Where(x =>
                    x.InvoiceDate >= startOfToday &&
                    x.InvoiceDate < startOfTomorrow
                );

                var hours = Enumerable.Range(0, 24);

                data = hours
                    .GroupJoin(
                        await query.ToListAsync(),
                        h => h,
                        x => x.InvoiceDate.Hour,
                        (hour, invoices) => new RevenuePointDto
                        {
                            Date = DateTime.SpecifyKind(
                                startOfToday.AddHours(hour),
                                DateTimeKind.Utc
                            ),
                            Revenue = invoices.Sum(x => x.TotalAmount)
                        }
                    ).ToList();
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
