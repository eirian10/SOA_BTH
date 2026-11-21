using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportService.Models
{
    [Table("product_reports")]
    public class ProductReport
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("order_report_id")]
        public int OrderReportId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("total_sold")]
        public int TotalSold { get; set; }

        [Column("revenue", TypeName = "decimal(18,2)")]
        public decimal Revenue { get; set; }

        [Column("cost", TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        [Column("profit", TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; }
    }
}