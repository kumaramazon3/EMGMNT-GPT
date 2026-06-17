using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_Route", Schema = "dbo")]
    public class master_Route
    {
        [Key]
        public string id { get; set; }
        public string? routeCode { get; set; }
        public string? routeName { get; set; }
        public string isRouteActive { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
