using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_Nationality", Schema = "dbo")]
    public class master_Nationality
    {
        [Key]
        public string id { get; set; }
        public string? nationCOde { get; set; }
        public string? nationName { get; set; }
        public string isDeleted { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
