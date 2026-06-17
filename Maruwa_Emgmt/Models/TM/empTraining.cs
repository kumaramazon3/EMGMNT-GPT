using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models.TM
{
    [Table("empTraining", Schema = "dbo")]
    public class empTraining
    {
        [Key]
        public int UID { get; set; }
        public string? empCode { get; set; }
        public string? trainingAttended { get; set; }
        public string? programme { get; set; }
        public string? remarks { get; set; }
        public string? markScored { get; set; }
        public DateTime? dateAttended { get; set; }
        public string? hours { get; set; }
        public string? trainingAttachmentURL { get; set; }
        public string? sectionCode { get; set; }
        public string? method { get; set; }
        public string? type { get; set; }
        public string? trainerCode { get; set; }
        public string? trainerName { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
        public bool? isActive { get; set; }
    }
    
    
    [Table("trainingProgram", Schema = "dbo")]
    public class trainingProgram
    {
        [Key]
        public int UID { get; set; }
        public string? deptCode { get; set; }
        public string? deptName { get; set; }
        public string? subDeptCode { get; set; }
        public string? subDeptName { get; set; }
        public string? sectionCode { get; set; }
        public string? sectionName { get; set; }
        public string? Method { get; set; }
        public string? Type { get; set; }
        public string? trainingCode { get; set; }
        public string? program { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
        public bool? isActive { get; set; }
    }


}
