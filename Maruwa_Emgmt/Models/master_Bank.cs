using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_Bank", Schema = "dbo")]
    public class master_Bank
    {
        [Key]
        public string id { get; set; }
        public string? bankCode { get; set; }
        public string? bankName { get; set; }
        public string isbankActive { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
