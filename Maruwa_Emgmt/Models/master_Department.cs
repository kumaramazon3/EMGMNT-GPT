using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_Department", Schema = "dbo")]
    public class master_Department
    {
        [Key]
        public string id { get; set; }
        public string? departmentCode { get; set; }
        public string? departmentID { get; set; }
        public string departmentName { get; set; }
        public string departmentPrefix { get; set; }
        public string subDepartment { get; set; }
        public string idDeptActive { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
