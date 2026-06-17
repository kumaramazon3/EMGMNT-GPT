using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_Language", Schema = "dbo")]
    public class master_Language
    {
        [Key]
        public string id { get; set; }
        public string? languageCode { get; set; }
        public string? languageName { get; set; }
        public string islanguageActive { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
