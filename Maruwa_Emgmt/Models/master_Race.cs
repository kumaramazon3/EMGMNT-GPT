using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_Race", Schema = "dbo")]
    public class master_Race
    {
        [Key]
        public string id { get; set; }
        public string? raceCode { get; set; }
        public string? raceName { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
