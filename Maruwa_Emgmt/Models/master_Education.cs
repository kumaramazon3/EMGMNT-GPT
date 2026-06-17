using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_Education", Schema = "dbo")]
    public class master_Education
    {
        [Key]
        public string id { get; set; }
        public string? educationCode { get; set; }
        public string? educationName { get; set; }
        public string iseducationActive { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
