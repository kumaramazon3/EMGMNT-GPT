using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{

    [Table("ProductDailySales", Schema = "dbo")]
    public class DailyProductSales
    {
        [Key]
        public int Id { get; set; }
        public string ProductName { get; set; }
        public DateTime SalesDate { get; set; }
        public int Quantity { get; set; }
    }
}
