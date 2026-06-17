using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models.TM
{
    [Table("master_Training", Schema = "dbo")]
    public class trainingMaster
    {
        [Key]
        public string code { get; set; }
        public string? titleName { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
