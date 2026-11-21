using Microsoft.AspNetCore.Mvc;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers
{
    [Route("products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public ProductsController(ProductDbContext context)
        {
            _context = context;
            if (!_context.Products.Any())
            {
                _context.Products.AddRange(
                    new Product { Name = "Laptop Dell", Description = "Core i7", Price = 15000000, Quantity = 10 },
                    new Product { Name = "Chuột Logitech", Description = "Không dây", Price = 500000, Quantity = 50 }
                );
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_context.Products.ToList());

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var p = _context.Products.Find(id);
            return p == null ? NotFound() : Ok(p);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Product p)
        {
            _context.Products.Add(p);
            _context.SaveChanges();
            return Ok(p);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Product req)
        {
            var p = _context.Products.Find(id);
            if (p == null) return NotFound();
            p.Name = req.Name;
            p.Description = req.Description;
            p.Price = req.Price;
            p.Quantity = req.Quantity;
            p.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
            return Ok(p);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var p = _context.Products.Find(id);
            if (p == null) return NotFound();
            _context.Products.Remove(p);
            _context.SaveChanges();
            return Ok();
        }
    }
}