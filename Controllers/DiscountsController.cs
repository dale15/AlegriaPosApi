using AlegriaPosApi.Data;
using AlegriaPosApi.DTOs.Discounts;
using AlegriaPosApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlegriaPosApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public DiscountsController(PosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDiscounts()
        {
            var discounts = await _context.Discounts
                .OrderBy(d => d.Name)
                .Select(d => new DiscountReadDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Type = d.Type,
                    Value = d.Value,
                    IsActive = d.IsActive
                })
                .ToListAsync();

            return Ok(discounts);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveDiscount()
        {
            var discounts = await _context.Discounts
                .Where(d => d.IsActive)
                .Select(d => new DiscountReadDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Type = d.Type,
                    Value = d.Value,
                    IsActive = d.IsActive
                })
                .ToListAsync();

            return Ok(discounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiscountById(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return NotFound();

            return Ok(new DiscountReadDto
            {
                Id = discount.Id,
                Name = discount.Name,
                Type = discount.Type,
                Value = discount.Value,
                IsActive = discount.IsActive
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(DiscountDto dto)
        {
            if (dto.Type != "percent" && dto.Type != "fixed")
                return BadRequest("Discount type must be 'percent' or 'fixed'.");

            if (dto.Type == "percent" && (dto.Value <= 0 || dto.Value > 100))
                return BadRequest("Percent discount must be between 1 and 100.");

            if (dto.Type == "fixed" && dto.Value <= 0)
                return BadRequest("Fixed discount must be greater than 0.");

            var discount = new Discount
            {
                Name = dto.Name,
                Type = dto.Type,
                Value = dto.Value,
                IsActive = dto.IsActive
            };

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDiscountById), new { id = discount.Id }, discount);
        }

        [HttpPut("update-discount/{id}")]
        public async Task<IActionResult> Update(int id, DiscountDto dto)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return NotFound();

            if (dto.Type != "percent" && dto.Type != "fixed")
                return BadRequest("Discount type must be 'percent' or 'fixed'.");

            discount.Name = dto.Name;
            discount.Type = dto.Type;
            discount.Value = dto.Value;
            discount.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("delete-discount/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
                return NotFound();

            // Soft-delete behavior
            discount.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
