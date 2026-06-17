using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("tbldropdownData", Schema = "dbo")]
    public class tbldropdownData
    {
        [Key]
        public int Sno { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? TableName { get; set; }
        public DateTime? CreatedDatetime { get; set; } // Make DateTime nullable if it can be null
        public string? CreatedBy { get; set; }
        public bool? isActive { get; set; }
    }
}
