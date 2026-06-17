using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_License", Schema = "dbo")]
    public class master_License
    {
        [Key]
        public string id { get; set; }
        public string? licenseCode { get; set; }
        public string? licenseName { get; set; }
        public string islicenseActive { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
