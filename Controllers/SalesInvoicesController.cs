using AlegriaPosApi.Data;
using AlegriaPosApi.DTOs.Sales;
using AlegriaPosApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AlegriaPosApi.Controllers
{
    [Route("api/sales-invoices")]
    [ApiController]
    public class SalesInvoicesController : ControllerBase
    {
        private readonly PosDbContext _context;

        public SalesInvoicesController(PosDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice(CreateSalesInvoiceDto dto)
        {
            if (dto.Items.Count == 0)
            {
                return BadRequest("Invoice must contain at least one item.");
            }

            var invoice = new SalesInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow.Ticks}",
            };

            decimal subTotal = 0;

            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    return BadRequest($"Product with ID {item.ProductId} not found.");
                }

                decimal modifierTotal = item.Modifiers.Sum(m => m.PriceAdjustment);
                decimal unitPriceWithModifiers = product.SellingPrice + modifierTotal;
                var totalPrice = unitPriceWithModifiers * item.Quantity;

                var sale = new Sale
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = unitPriceWithModifiers,
                    TotalPrice = totalPrice
                };

                foreach (var mod in item.Modifiers)
                {
                    sale.Modifiers.Add(new SaleModifier
                    {
                        ModifierName = mod.ModifierName,
                        OptionName = mod.OptionName,
                        PriceAdjustment = mod.PriceAdjustment
                    });
                }


                invoice.Sales.Add(sale);
                subTotal += totalPrice;
            }

            var tax = dto.Tax ?? 0m;
            decimal discountAmount = 0m;

            if (dto.DiscountId != null)
            {
                var discount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.Id == dto.DiscountId && d.IsActive);

                if (discount == null)
                {
                    return BadRequest("Invalid or inactive discount.");
                }

                discountAmount = discount.Type == "percent" ? subTotal * (discount.Value / 100) : discount.Value;

                if (discountAmount > subTotal)
                {
                    discountAmount = subTotal;
                }

                invoice.DiscountId = discount.Id;
            }

            invoice.SubTotal = subTotal;
            invoice.Tax = dto.Tax;
            invoice.DiscountAmount = discountAmount;
            invoice.TotalAmount = subTotal + tax - discountAmount;

            // 🔥 Validate Payments
            if (dto.Payments == null || dto.Payments.Count == 0)
            {
                return BadRequest("At least one payment is required.");
            }

            var totalPaid = dto.Payments.Sum(p => p.Amount);

            if (totalPaid < invoice.TotalAmount)
            {
                return BadRequest("Total payment is less than invoice total.");
            }

            // 🔥 Save Payments
            foreach (var payment in dto.Payments)
            {
                invoice.Payments.Add(new SalesInvoicePayment
                {
                    PaymentMethod = payment.PaymentMethod,
                    Amount = payment.Amount
                });
            }

            if (invoice.TotalAmount < 0)
            {
                invoice.TotalAmount = 0;
            }

            _context.SalesInvoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(new { invoice.Id, invoice.InvoiceNumber, invoice.InvoiceDate });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesInvoice()
        {
            var invoices = await _context.SalesInvoices
                .Select(i => new SalesInvoiceDto
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    InvoiceDate = i.InvoiceDate,
                    SubTotal = i.SubTotal,
                    Tax = i.Tax ?? 0m,
                    Discount = i.DiscountAmount,
                    TotalAmount = i.TotalAmount,

                    Payments = i.Payments.Select(p => new SalesInvoicePaymentDto
                    {
                        PaymentType = p.PaymentMethod.ToString(), // 🔥 IMPORTANT
                        Amount = p.Amount
                    }).ToList()
                })
                .ToListAsync();

                return Ok(invoices);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSalesInvoiceById(int id)
        {
            var invoice = await _context.SalesInvoices
                .Include(i => i.Sales)
                    .ThenInclude(s => s.Product)
                .Include(i => i.Sales)
                    .ThenInclude(s => s.Modifiers)
                .Include(i => i.Payments) // 👈 ADD THIS
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(new SalesInvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                SubTotal = invoice.SubTotal,
                Tax = invoice.Tax ?? 0m,
                Discount = invoice.DiscountAmount,
                TotalAmount = invoice.TotalAmount,

                Payments = invoice.Payments.Select(p => new SalesInvoicePaymentDto
                {
                    PaymentType = p.PaymentMethod.ToString(),
                    Amount = p.Amount
                }).ToList(),

                Items = invoice.Sales.Select(s => new SaleDto
                {
                    ProductId = s.ProductId,
                    ProductName = s.Product.Name,
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    TotalPrice = s.TotalPrice,
                    Modifiers = s.Modifiers.Select(m => new SaleModifierDto
                    {
                        ModifierName = m.ModifierName,
                        OptionName = m.OptionName,
                        PriceAdjustment = m.PriceAdjustment
                    }).ToList()
                }).ToList()
            });

        }

        [HttpGet("daily-report")]
        public async Task<IActionResult> GetDailyReport()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var invoices = await _context.SalesInvoices
               .Include(i => i.Payments)
               .Include(i => i.Sales)
                   .ThenInclude(ii => ii.Product)
               .AsNoTracking()
               .Where(i => i.InvoiceDate >= today && i.InvoiceDate < tomorrow)
               .ToListAsync();

            var totalSales = invoices.Sum(i => i.SubTotal);
            var totalTax = invoices.Sum(i => i.Tax ?? 0);
            var totalDiscount = invoices.Sum(i => i.DiscountAmount);
            var totalTransactions = invoices.Count;

            var paymentBreakdown = invoices
                .SelectMany(i => i.Payments.Select(p => new
                {
                    p.PaymentMethod,
                    InvoiceTotal = i.TotalAmount
                }))
                .GroupBy(x => x.PaymentMethod)
                .Select(g => new
                {
                    PaymentMethod = g.Key.ToString(),
                    Total = g.Sum(x => x.InvoiceTotal)
                })
                .ToList();

            var topProducts = invoices
                .SelectMany(i => i.Sales)
                .GroupBy(ii => new
                {
                    ii.ProductId,
                    ii.Product.Name
                })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalSales = g.Sum(x => x.Quantity * x.UnitPrice) // or x.Quantity * x.UnitPrice
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5) // top 5 products
                .ToList();

            return Ok(new
            {
                totalSales,
                totalTax,
                totalDiscount,
                totalTransactions,
                paymentBreakdown,
                topProducts
            });
        }

        [HttpGet("version-check")]
        public IActionResult VersionCheck()
        {
            return Ok("BUILD 2026-02-18 11:45PM");
        }
    }
}
