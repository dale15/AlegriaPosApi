using AlegriaPosApi.Data;
using AlegriaPosApi.DTOs.ProductMaterials;
using AlegriaPosApi.DTOs.ProductModifiers;
using AlegriaPosApi.DTOs.Products;
using AlegriaPosApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text;

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
                    ImageUrl = p.ImageUrl,

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
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto dto)
        {
            // optional validation
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return BadRequest("Category does not exist.");

            string? imageUrl = null;

            if (dto.Image != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/products");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.Image.CopyToAsync(stream);

                imageUrl = $"/uploads/products/{fileName}";
            }

            var product = new Product
            {
                Name = dto.Name,
                SKU = dto.SKU,
                SellingPrice = dto.SellingPrice,
                CostPrice = dto.CostPrice,
                CategoryId = dto.CategoryId,
                ImageUrl = imageUrl,
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

        // Import and Export of products data
        [HttpGet("export")]
        public async Task<IActionResult> ExportProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Modifiers)
                    .ThenInclude(m => m.Options)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("sku,name,costprice,sellingprice,category,modifiers");

            foreach (var p in products)
            {
                var modifierGroups = new List<string>();

                foreach (var modifier in p.Modifiers)
                {
                    var requiredText = modifier.IsRequired ? "required" : "optional";
                    var multipleText = modifier.IsMultiple ? "multiple" : "single";

                    var optionParts = modifier.Options
                        .Select(o => $"{o.Name}|{o.PriceAdjustment}");

                    var optionsString = string.Join("|", optionParts);

                    var modifierString =
                        $"{modifier.Name}|{requiredText}|{multipleText}:{optionsString}";

                    modifierGroups.Add(modifierString);
                }

                var modifiersFinal = string.Join(";", modifierGroups);

                csv.AppendLine(
                    $"{p.SKU},{p.Name},{p.CostPrice},{p.SellingPrice},{p.Category.Name},{modifiersFinal}"
                );
            }

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "products.csv"
            );
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportProducts(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // Disable change tracking for speed
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            using var reader = new StreamReader(file.OpenReadStream());

            // Skip header
            await reader.ReadLineAsync();

            int rowNumber = 1;
            int success = 0;

            var errors = new List<object>();
            var toInsert = new List<Product>();
            var toUpdate = new List<Product>();

            // Load existing products ONCE
            var existingProducts = await _context.Products
                .AsNoTracking()
                .ToDictionaryAsync(p => p.SKU);

            var categories = await _context.Categories
            .AsNoTracking()
            .ToDictionaryAsync(
                c => c.Name.Trim().ToLower(),
                c => c.Id
            );

            while (!reader.EndOfStream)
            {
                rowNumber++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = line.Split(',');

                if (cols.Length < 6)
                {
                    errors.Add(new { row = rowNumber, message = "Invalid column count" });
                    continue;
                }

                try
                {
                    var sku = cols[0].Trim();
                    var name = cols[1].Trim();
                    var categoryName = cols[4].Trim();
                    var modifierData = cols[5].Trim().Trim('"');

                    if (string.IsNullOrEmpty(sku))
                        throw new Exception("SKU is required");

                    if (!decimal.TryParse(cols[2], out var costPrice))
                        throw new Exception("Invalid cost price");

                    if (!decimal.TryParse(cols[3], out var sellingPrice))
                        throw new Exception("Invalid selling price");

                    if (string.IsNullOrEmpty(categoryName))
                        throw new Exception("Category is required");

                    if (!categories.TryGetValue(categoryName.ToLower(), out var categoryId))
                    {
                        var newCategory = new Category { Name = categoryName };
                        _context.Categories.Add(newCategory);
                        await _context.SaveChangesAsync();

                        categoryId = newCategory.Id;
                        categories[categoryName.ToLower()] = categoryId;
                    }

                    if (existingProducts.TryGetValue(sku, out var existing))
                    {
                        existing.Name = name;
                        existing.CostPrice = costPrice;
                        existing.SellingPrice = sellingPrice;
                        existing.CategoryId = categoryId;

                        // Remove old modifiers
                        var oldModifiers = _context.ProductModifiers
                            .Where(pm => pm.ProductId == existing.Id)
                            .ToList();

                        _context.ProductModifiers.RemoveRange(oldModifiers);

                        existing.Modifiers.Clear();

                        // Rebuild modifiers
                        if (!string.IsNullOrWhiteSpace(modifierData))
                        {
                            var modifierGroups = modifierData.Split(';');

                            foreach (var group in modifierGroups)
                            {
                                var split = group.Split(':');
                                if (split.Length != 2) continue;

                                var headerParts = split[0].Split('|');
                                if (headerParts.Length != 3) continue;

                                var modifierName = headerParts[0].Trim();
                                var isRequired = headerParts[1].Trim().ToLower() == "required";
                                var isMultiple = headerParts[2].Trim().ToLower() == "multiple";

                                var productModifier = new ProductModifier
                                {
                                    ProductId = existing.Id,
                                    Name = modifierName,
                                    IsRequired = isRequired,
                                    IsMultiple = isMultiple
                                };

                                var optionParts = split[1].Split('|');

                                for (int i = 0; i < optionParts.Length; i += 2)
                                {
                                    var optionName = optionParts[i].Trim();

                                    decimal priceAdjustment = 0;

                                    if (i + 1 < optionParts.Length)
                                        decimal.TryParse(optionParts[i + 1].Trim(), out priceAdjustment);

                                    productModifier.Options.Add(new ProductModifierOption
                                    {
                                        Name = optionName,
                                        PriceAdjustment = priceAdjustment
                                    });
                                }

                                existing.Modifiers.Add(productModifier);
                            }
                        }

                        toUpdate.Add(existing);
                    }
                    else
                    {
                        var newProduct = new Product
                        {
                            SKU = sku,
                            Name = name,
                            CostPrice = costPrice,
                            SellingPrice = sellingPrice,
                            CategoryId = categoryId
                        };

                        if (!string.IsNullOrWhiteSpace(modifierData))
                        {
                            var modifierGroups = modifierData.Split(';');

                            foreach (var group in modifierGroups)
                            {
                                var split = group.Split(':');
                                if (split.Length != 2) continue;

                                var headerParts = split[0].Split('|');
                                if (headerParts.Length != 3) continue;

                                var modifierName = headerParts[0].Trim();
                                var isRequired = headerParts[1].Trim().ToLower() == "required";
                                var isMultiple = headerParts[2].Trim().ToLower() == "multiple";

                                var productModifier = new ProductModifier
                                {
                                    Name = modifierName,
                                    IsRequired = isRequired,
                                    IsMultiple = isMultiple
                                };

                                var optionParts = split[1].Split('|');

                                for (int i = 0; i < optionParts.Length; i += 2)
                                {
                                    var optionName = optionParts[i].Trim();

                                    decimal priceAdjustment = 0;

                                    if (i + 1 < optionParts.Length)
                                        decimal.TryParse(optionParts[i + 1].Trim(), out priceAdjustment);

                                    productModifier.Options.Add(new ProductModifierOption
                                    {
                                        Name = optionName,
                                        PriceAdjustment = priceAdjustment
                                    });
                                }

                                newProduct.Modifiers.Add(productModifier);
                            }
                        }

                        toInsert.Add(newProduct);
                    }

                    success++;
                }
                catch (Exception ex)
                {
                    errors.Add(new { row = rowNumber, message = ex.Message });
                }
            }

            // Bulk save
            if (toInsert.Any())
                _context.Products.AddRange(toInsert);

            if (toUpdate.Any())
                _context.Products.UpdateRange(toUpdate);

            await _context.SaveChangesAsync();

            // Re-enable tracking
            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return Ok(new
            {
                success,
                inserted = toInsert.Count,
                updated = toUpdate.Count,
                failed = errors.Count,
                errors
            });
        }
    }
}
