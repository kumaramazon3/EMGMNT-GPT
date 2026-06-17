using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_BloodGroup", Schema = "dbo")]
    public class master_BloodGroup
    {
        [Key]
        public string id { get; set; }
        public string? bloodGroupCode { get; set; }
        public string? bloodGroup { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
