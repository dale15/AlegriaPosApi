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
                    UnitPrice = product.SellingPrice,
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

            invoice.SubTotal = subTotal;
            invoice.Tax = dto.Tax;
            invoice.Discount = dto.Discount;
            invoice.TotalAmount = subTotal + tax - dto.Discount;

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
                    Discount = i.Discount,
                    TotalAmount = i.TotalAmount,
                })
                .ToListAsync();
            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSalesInvoiceById(int id)
        {
            var invoice = await _context.SalesInvoices
                .Include(i => i.Sales)
                    .ThenInclude(s => s.Product)
                .Include(i => i.Sales)
                    .ThenInclude(s => s.Modifiers)
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
                Discount = invoice.Discount,
                TotalAmount = invoice.TotalAmount,
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
    }
}
