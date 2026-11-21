using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportService.Models
{
    [Table("orders_reports")]
    public class OrderReport
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("total_revenue", TypeName = "decimal(18,2)")]
        public decimal TotalRevenue { get; set; }

        [Column("total_cost", TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [Column("total_profit", TypeName = "decimal(18,2)")]
        public decimal TotalProfit { get; set; }
    }
}