using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OrderService.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public OrdersController(OrderDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private async Task<bool> IsAuthenticated()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ")) return false;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
            var authUrl = _config["ServiceUrls:AuthService"] + "/auth";

            try
            {
                var res = await client.GetAsync(authUrl);
                return res.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ hoặc hết hạn" });
            return Ok(await _context.Orders.Include(o => o.OrderItems).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Chưa đăng nhập hoặc token sai" });

            var client = _httpClientFactory.CreateClient();
            var productServiceUrl = _config["ServiceUrls:ProductService"];
            decimal totalOrderAmount = 0;
            var validItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var res = await client.GetAsync($"{productServiceUrl}/products/{item.ProductId}");
                if (!res.IsSuccessStatusCode)
                    return BadRequest(new { message = $"Sản phẩm ID {item.ProductId} không tồn tại" });

                var json = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string pName = root.GetProperty("name").GetString() ?? "";
                decimal pPrice = root.GetProperty("price").GetDecimal();
                int stock = root.GetProperty("quantity").GetInt32();

                if (stock < item.Quantity)
                    return BadRequest(new { message = $"Sản phẩm '{pName}' chỉ còn {stock} trong kho, không đủ đặt {item.Quantity}" });

                decimal itemTotal = pPrice * item.Quantity;
                totalOrderAmount += itemTotal;

                validItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = pName,
                    Quantity = item.Quantity,
                    UnitPrice = pPrice,
                    TotalPrice = itemTotal
                });
            }

            var order = new Order
            {
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                TotalAmount = totalOrderAmount,
                Status = "pending",
                OrderItems = validItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa đơn hàng" });
        }
    }

    public class CreateOrderDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public List<OrderItemInput> Items { get; set; } = new();
    }
    public class OrderItemInput { public int ProductId { get; set; } public int Quantity { get; set; } }
    public class UpdateStatusDto { public string Status { get; set; } = "completed"; }
}