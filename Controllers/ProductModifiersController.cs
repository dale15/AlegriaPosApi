using AlegriaPosApi.Data;
using AlegriaPosApi.DTOs.ProductModifiers;
using AlegriaPosApi.DTOs.Products;
using AlegriaPosApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AlegriaPosApi.Controllers
{

    [Route("api/products/{productId}/modifiers")]
    [ApiController]
    public class ProductModifiersController : ControllerBase
    {
        private readonly PosDbContext _context;

        public ProductModifiersController(PosDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateModifier(
            [FromRoute] int productId,
            [FromBody] CreateProductModifierDto dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Product not found");

            var modifier = new ProductModifier
            {
                ProductId = productId,
                Name = dto.Name,
                IsRequired = dto.IsRequired,
                IsMultiple = dto.IsMultiple,
                Options = dto.Options.Select(o => new ProductModifierOption
                {
                    Name = o.Name,
                    PriceAdjustment = o.PriceAdjustment
                }).ToList()
            };

            _context.ProductModifiers.Add(modifier);
            await _context.SaveChangesAsync();

            return Ok(new ProductModifierDto
            {
                Id = modifier.Id,
                Name = modifier.Name,
                IsRequired = modifier.IsRequired,
                IsMultiple = modifier.IsMultiple,
                Options = modifier.Options.Select(o => new ProductModifierOptionDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    PriceAdjustment = o.PriceAdjustment
                }).ToList()
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteModifiers(int productId)
        {
            var modifiers = await _context.ProductModifiers
                .Where(m => m.ProductId == productId)
                .Include(m => m.Options)
                .ToListAsync();

            _context.ProductModifierOptions.RemoveRange(
                modifiers.SelectMany(m => m.Options)
            );
            _context.ProductModifiers.RemoveRange(modifiers);

            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
