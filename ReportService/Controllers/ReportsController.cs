using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using ReportService.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ReportService.Controllers
{
    [Route("reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ReportDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public ReportsController(ReportDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
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

        // 1. GET /reports/products
        [HttpGet("products")]
        public async Task<IActionResult> GetAllProductReports()
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });
            return Ok(await _context.ProductReports.ToListAsync());
        }

        // 2. GET /reports/products/{id}
        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProductReportById(int id)
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });
            var item = await _context.ProductReports.FindAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        // 3. GET /reports/orders
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrderReports()
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });
            return Ok(await _context.OrderReports.ToListAsync());
        }

        // 4. GET /reports/orders/{id}
        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderReportById(int id)
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });
            var item = await _context.OrderReports.FindAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        // 5. POST /reports/orders
        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrderReport([FromBody] CreateOrderReportDto dto)
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });

            var authHeader = Request.Headers["Authorization"].ToString();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

            var orderUrl = $"{_config["ServiceUrls:OrderService"]}/orders/{dto.OrderId}";
            var res = await client.GetAsync(orderUrl);
            if (!res.IsSuccessStatusCode)
                return BadRequest(new { message = $"Không tìm thấy đơn hàng ID {dto.OrderId} bên OrderService" });

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            decimal totalRevenue = root.GetProperty("totalAmount").GetDecimal();
            decimal totalCost = Math.Round(totalRevenue * dto.CostRate, 2);
            decimal totalProfit = totalRevenue - totalCost;

            var report = new OrderReport
            {
                OrderId = dto.OrderId,
                TotalRevenue = totalRevenue,
                TotalCost = totalCost,
                TotalProfit = totalProfit
            };

            _context.OrderReports.Add(report);
            await _context.SaveChangesAsync();
            return Ok(report);
        }

        // 6. POST /reports/products
        [HttpPost("products")]
        public async Task<IActionResult> CreateProductReport([FromBody] CreateProductReportDto dto)
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });

            var client = _httpClientFactory.CreateClient();

            var prodRes = await client.GetAsync($"{_config["ServiceUrls:ProductService"]}/products/{dto.ProductId}");
            if (!prodRes.IsSuccessStatusCode)
                return BadRequest(new { message = $"Sản phẩm ID {dto.ProductId} không tồn tại" });

            var itemsRes = await client.GetAsync($"{_config["ServiceUrls:OrderService"]}/order_items");
            if (!itemsRes.IsSuccessStatusCode)
                return BadRequest(new { message = "Không lấy được dữ liệu từ OrderService" });

            var itemsJson = await itemsRes.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(itemsJson);

            int totalSold = 0;
            decimal revenue = 0;

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (item.GetProperty("productId").GetInt32() == dto.ProductId)
                {
                    totalSold += item.GetProperty("quantity").GetInt32();
                    revenue += item.GetProperty("totalPrice").GetDecimal();
                }
            }

            decimal cost = Math.Round(revenue * dto.CostRate, 2);
            decimal profit = revenue - cost;

            var report = new ProductReport
            {
                OrderReportId = dto.OrderReportId,
                ProductId = dto.ProductId,
                TotalSold = totalSold,
                Revenue = revenue,
                Cost = cost,
                Profit = profit
            };

            _context.ProductReports.Add(report);
            await _context.SaveChangesAsync();
            return Ok(report);
        }

        // 7. DELETE /reports/products/{id}
        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProductReport(int id)
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });
            var item = await _context.ProductReports.FindAsync(id);
            if (item == null) return NotFound();
            _context.ProductReports.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa báo cáo sản phẩm" });
        }

        // 8. DELETE /reports/orders/{id}
        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrderReport(int id)
        {
            if (!await IsAuthenticated()) return Unauthorized(new { message = "Token không hợp lệ" });
            var item = await _context.OrderReports.FindAsync(id);
            if (item == null) return NotFound();
            _context.OrderReports.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa báo cáo đơn hàng" });
        }
    }

    public class CreateOrderReportDto
    {
        public int OrderId { get; set; }
        public decimal CostRate { get; set; } = 0.7m;
    }

    public class CreateProductReportDto
    {
        public int OrderReportId { get; set; } = 1;
        public int ProductId { get; set; }
        public decimal CostRate { get; set; } = 0.7m;
    }
}