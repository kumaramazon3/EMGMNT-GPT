using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models.master
{
    [Table("master_Designation", Schema = "dbo")]
    public class master_Designation
    {
        [Key]
        public int Sno { get; set; }
        public string? desigcode { get; set; }
        public string? designationname { get; set; }
        public string? inscategory { get; set; }
        public string? dlevel { get; set; }
        public string? probation { get; set; }
        public string? CTQlevel { get; set; }
        public string? kpi { get; set; }
        public string? positionid { get; set; }
        public string? insamount { get; set; }
        public string? createdby { get; set; }
        public DateTime? createddatetime { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDateTime { get; set; }
        public bool? isActive { get; set; }
    }
}
