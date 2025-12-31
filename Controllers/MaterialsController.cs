using AlegriaPosApi.Data;
using AlegriaPosApi.DTOs.Materials;
using AlegriaPosApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AlegriaPosApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public MaterialsController(PosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMaterials()
        {
            var materials = await _context.Materials
                 .Select(m => new MaterialDto
                 {
                     Id = m.Id,
                     Name = m.Name,
                     Unit = m.Unit,
                     CurrentStock = m.CurrentStock,
                     LowStockThreshold = m.LowStockThreshold
                 })
                 .ToListAsync();

            return Ok(materials);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var material = await _context.Materials.FindAsync(id);

            if (material == null)
                return NotFound();

            return Ok(new MaterialDto
            {
                Id = material.Id,
                Name = material.Name,
                Unit = material.Unit,
                CurrentStock = material.CurrentStock,
                LowStockThreshold = material.LowStockThreshold
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMaterial(CreateMaterialDto dto)
        {
            var material = new Material
            {
                Name = dto.Name,
                Unit = dto.Unit,
                CurrentStock = dto.InitialStock,
                LowStockThreshold = dto.LowStockThreshold,
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            return Ok(new { material.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateMaterialDto dto)
        {
            var material = await _context.Materials.FindAsync(id);

            if (material == null)
                return NotFound();

            material.Name = dto.Name;
            material.Unit = dto.Unit;
            material.LowStockThreshold = dto.LowStockThreshold;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // For Adjusting of Stocks

        [HttpPost("{id}/adjust")]
        public async Task<IActionResult> AdjustStock(int id, AdjustMaterialStockDto dto)
        {
            var material = await _context.Materials.FindAsync(id);
            if(material == null)
            {
                return NotFound();
            }

            material.CurrentStock += dto.Quantity;

            if(material.CurrentStock < 0)
            {
                return BadRequest("Stock cannot be negative.");
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                material.Id,
                material.CurrentStock
            });
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var materials = await _context.Materials
                .Where(m => m.CurrentStock <= m.LowStockThreshold)
                .Select(m => new MaterialDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Unit = m.Unit,
                    CurrentStock = m.CurrentStock,
                    LowStockThreshold = m.LowStockThreshold
                }).ToListAsync();

            return Ok(materials);
        }


    }
}
