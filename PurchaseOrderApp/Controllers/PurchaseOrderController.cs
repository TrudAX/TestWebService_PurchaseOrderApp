// Controllers/PurchaseOrderController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Data;
using PurchaseOrderApp.Models;
using PurchaseOrderApp.Services;
using PurchaseOrderApp.Exceptions;

namespace PurchaseOrderApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly PurchaseOrderService _service;
        private readonly ApplicationDbContext _context;

        public PurchaseOrderController(PurchaseOrderService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }
        /*
        [HttpPost]
        public async Task<ActionResult<PurchaseOrder>> CreateOrUpdatePurchaseOrder(PurchaseOrder purchaseOrder)
        {
            var result = await _service.UpsertPurchaseOrderAsync(purchaseOrder);
            return CreatedAtAction(nameof(GetPurchaseOrder), new { id = result.ID }, result);
        }
        */
        [HttpPost]
        public async Task<ActionResult<PurchaseOrder>> CreateOrUpdatePurchaseOrder(PurchaseOrder purchaseOrder)
        {
            try
            {
                var result = await _service.UpsertPurchaseOrderAsync(purchaseOrder);
                return CreatedAtAction(nameof(GetPurchaseOrder), new { id = result.ID }, result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Errors = ex.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while processing your request.", Message = ex.Message });
            }
        }
        /*
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseOrder>>> GetPurchaseOrders()
        {
            return await _service.GetAllPurchaseOrdersAsync();
        }
        */
        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrders([FromQuery] DateTime? updatedAt = null)
        {
            var orders = await _service.GetPurchaseOrdersAsync(updatedAt);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseOrder>> GetPurchaseOrder(int id)
        {
            var purchaseOrder = await _service.GetPurchaseOrderAsync(id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }
            return purchaseOrder;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurchaseOrder(int id, PurchaseOrder purchaseOrder)
        {
            if (id != purchaseOrder.ID)
            {
                return BadRequest();
            }
            await _service.UpdatePurchaseOrderAsync(purchaseOrder);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseOrder(int id)
        {
            await _service.DeletePurchaseOrderAsync(id);
            return NoContent();
        }
        [HttpPost("reset")]
        public async Task<IActionResult> ResetDatabase()
        {
            // Clear all existing data
            _context.PurchaseOrderLines.RemoveRange(_context.PurchaseOrderLines);
            _context.PurchaseOrders.RemoveRange(_context.PurchaseOrders);
            await _context.SaveChangesAsync();

            // Reseed the database
            _service.SeedTestData();

            return Ok("Database reset and reseeded successfully.");
        }
    }
}