using AlegriaPosApi.Data;
using AlegriaPosApi.DTOs.ProductMaterials;
using AlegriaPosApi.DTOs.ProductModifiers;
using AlegriaPosApi.DTOs.Products;
using AlegriaPosApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AlegriaPosApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public ProductsController(PosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Modifiers)
                    .ThenInclude(m => m.Options)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    SellingPrice = p.SellingPrice,
                    CostPrice = p.CostPrice,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,

                    Modifiers = p.Modifiers.Select(m => new ProductModifierDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        IsRequired = m.IsRequired,
                        IsMultiple = m.IsMultiple,
                        Options = m.Options.Select(o => new ProductModifierOptionDto
                        {
                            Id = o.Id,
                            Name = o.Name,
                            PriceAdjustment = o.PriceAdjustment
                        }).ToList()
                    }).ToList()
                }).ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Modifiers)
                    .ThenInclude(m => m.Options)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return Ok(new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                SellingPrice = product.SellingPrice,
                CostPrice = product.CostPrice,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,

                Modifiers = product.Modifiers.Select(m => new ProductModifierDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    IsRequired = m.IsRequired,
                    IsMultiple = m.IsMultiple,
                    Options = m.Options.Select(o => new ProductModifierOptionDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        PriceAdjustment = o.PriceAdjustment
                    }).ToList()
                }).ToList()
            });
        }

        [HttpPost("addProducts")]
        public async Task<IActionResult> CreateProduct(CreateProductDto dto)
        {
            // optional validation
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return BadRequest("Category does not exist.");

            var product = new Product
            {
                Name = dto.Name,
                SKU = dto.SKU,
                SellingPrice = dto.SellingPrice,
                CostPrice = dto.CostPrice,
                CategoryId = dto.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            // optional validation
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return BadRequest("Category does not exist.");

            product.Name = dto.Name;
            product.SKU = dto.SKU;
            product.CategoryId = dto.CategoryId;
            product.SellingPrice = dto.SellingPrice;
            product.CostPrice = dto.CostPrice;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/materials")]
        public async Task<IActionResult> GetProductMaterial(int id) 
        {             
            var productMaterials = await _context.ProductMaterials
                .Where(pm => pm.ProductId == id)
                .Include(pm => pm.Material)
                .Select(pm => new ProductMaterialViewDto
                {
                    MaterialId = pm.MaterialId,
                    MaterialName = pm.Material.Name,
                    QuantityUsed = pm.QuantityUsed
                })
                .ToListAsync();

            return Ok(productMaterials);
        }

        [HttpPost("{id}/materials")]
        public async Task<IActionResult> SetProductMaterials(int id, List<ProductMaterialDto> dto)
        {
            var product = await _context.Products
                .Include(p => p.ProductMaterials)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            // Remove old recipe
            product.ProductMaterials.Clear();

            foreach (var item in dto)
            {
                // Optional: validate material exists
                var materialExists = await _context.Materials
                    .AnyAsync(m => m.Id == item.MaterialId);

                if (!materialExists)
                    return BadRequest($"Material {item.MaterialId} not found");

                product.ProductMaterials.Add(new ProductMaterial
                {
                    MaterialId = item.MaterialId,
                    QuantityUsed = item.QuantityUsed
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{productId}/materials/{materialId}")]
        public async Task<IActionResult> RemoveMaterialFromProduct(int productId, int materialId)
        {
            var pm = await _context.ProductMaterials
                .FirstOrDefaultAsync(x =>
                    x.ProductId == productId &&
                    x.MaterialId == materialId);

            if (pm == null)
                return NotFound();

            _context.ProductMaterials.Remove(pm);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
