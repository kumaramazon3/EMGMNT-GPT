using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_DepartmentSection", Schema = "dbo")]
    public class master_DepartmentSection
    {
        [Key]
        public string id { get; set; }
        public string? sectionCode { get; set; }
        public string? sectionId { get; set; }
        public string departmentCode { get; set; }
        public string subDepartmentCode { get; set; }
        public string sectionName { get; set; }
        public string issectionActive { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
