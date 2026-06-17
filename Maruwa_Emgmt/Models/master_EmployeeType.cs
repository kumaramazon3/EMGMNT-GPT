using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("master_EmpType", Schema = "dbo")]
    public class master_EmployeeType
    {
        [Key]
        public string Sno { get; set; }
        public string? Empcode { get; set; }
        public string? EmpType { get; set; }
        public string isEmpActive { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
