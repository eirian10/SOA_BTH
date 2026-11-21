using Microsoft.EntityFrameworkCore;
using ReportService.Models;

namespace ReportService.Data
{
    public class ReportDbContext : DbContext
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options) { }

        public DbSet<OrderReport> OrderReports { get; set; }
        public DbSet<ProductReport> ProductReports { get; set; }
    }
}