using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_Religion", Schema = "dbo")]
    public class master_Religion
    {
        [Key]
        public string id { get; set; }
        public string? religionCode { get; set; }
        public string? religionName { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
