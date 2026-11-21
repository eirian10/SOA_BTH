using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Controllers
{
    [Route("order_items")]
    [ApiController]
    public class OrderItemsController : ControllerBase
    {
        private readonly OrderDbContext _context;
        public OrderItemsController(OrderDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.OrderItems.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.OrderItems.FindAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderItem item)
        {
            item.TotalPrice = item.Quantity * item.UnitPrice;
            _context.OrderItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrderItem req)
        {
            var item = await _context.OrderItems.FindAsync(id);
            if (item == null) return NotFound();
            item.ProductName = req.ProductName;
            item.Quantity = req.Quantity;
            item.UnitPrice = req.UnitPrice;
            item.TotalPrice = req.Quantity * req.UnitPrice;
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.OrderItems.FindAsync(id);
            if (item == null) return NotFound();
            _context.OrderItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}