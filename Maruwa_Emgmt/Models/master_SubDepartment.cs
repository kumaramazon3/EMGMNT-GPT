using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_SubDepartment", Schema = "dbo")]
    public class master_SubDepartment
    {
        [Key]
        public string id { get; set; }
        public string? subDepartmentCode { get; set; }
        public string? subDepartmentID { get; set; }
        public string departmentCode { get; set; }
        public string? subDepartmentName { get; set; }
        public string? subDepartmentPrefix { get; set; }
        public string? isSubDeptActive { get; set; }
        public string? section { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
