using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models
{
    public class T_tblProductSales
    {
        [Key]
        public int ID { get; set; }              // Primary Key
        public string ProductName { get; set; }  // Apple, Orange, Watermelon
        public int SaleMonth { get; set; }       // 1 - 12
        public int SaleYear { get; set; }        // Example: 2024
        public int Quantity { get; set; }        // Units Sold
    }
}
