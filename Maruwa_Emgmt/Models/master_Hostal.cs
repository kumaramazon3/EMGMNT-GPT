using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_Hostal", Schema = "dbo")]
    public class master_Hostal
    {
        [Key]
        public string id { get; set; }
        public string? hostalCode { get; set; }
        public string? hostalName { get; set; }
        public string ishostalActive { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
