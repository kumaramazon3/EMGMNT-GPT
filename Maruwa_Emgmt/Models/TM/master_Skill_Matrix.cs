using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models.TM
{
    [Table("master_Skill_Matrix", Schema = "dbo")]
    public class master_Skill_Matrix
    {
        [Key]
        public int RowID { get; set; }
        public string? empCode { get; set; }
        public string? empName { get; set; }
        public string? departmentCode { get; set; }
        public string? subDepartmentCode { get; set; }
        public string? sectionCode { get; set; }
        public string? position { get; set; }
        public string? score { get; set; }
        public DateTime? skillDate { get; set; }
        public string? columnNames { get; set; }
        public bool? isActive { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDateTime { get; set; }
        public string? modifyedBy { get; set; }
        public DateTime? modifyedDateTime { get; set; }
    }

    [Keyless]
    public class new_skillMatrixDocumentData
    {
        [Key]
        public int? RowID { get; set; }
        public string? empCode { get; set; }
        public string? empName { get; set; }
        public string? position { get; set; }
        public string? score { get; set; }
        public DateTime? skillDate { get; set; }
        public string? columnNames { get; set; }
        public string? departmentCode { get; set; }// Document No --> columnNames
        public string? subDepartmentCode { get; set; }
        public string? sectionCode { get; set; }
        //public string? empsectionCode { get; set; }
        public string? DocumentName { get; set; }
        public string? DepartmentName { get; set; }
        public int? Rev { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string? modifyedBy { get; set; }
        public DateTime? modifyedDateTime { get; set; }
        //public bool? Status { get; set; }
        public int? Status { get; set; }
    }

    // For Report
    public class skillMatrixReportList
    {
        [Key]
        public int RowID { get; set; }
        public string? columnNames { get; set; }
        public string? documentName { get; set; }
        public string? Department { get; set; }
        public string? sectionCode { get; set; }
        public string? Current_score { get; set; }
        public string? Old_score { get; set; }
        public DateTime? skillDate { get; set; }
        public DateTime? Old_skilldate { get; set; }
        public int? Rev { get; set; }
        public string? empCode { get; set; }
        public string? empName { get; set; }
        public string? position { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string? modifyedBy { get; set; }
        public DateTime? modifyedDateTime { get; set; }
        public DateTime? MaxLogDate { get; set; }
    }

    public class skillMatrixReportByDocNo
    {
        [Key]
        public int RowID { get; set; }
        public string? empCode { get; set; }
        public string? empName { get; set; }
        public string? position { get; set; }
        public string? departmentCode { get; set; }// Document No --> columnNames
        public string? subDepartmentCode { get; set; }
        public string? sectionCode { get; set; }
        public string? score { get; set; }
        public DateTime? skillDate { get; set; }
        public string? columnNames { get; set; }
        public string? DocumentName { get; set; }
        public int? Rev { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string? modifyedBy { get; set; }
        public DateTime? modifyedDateTime { get; set; }
        public DateTime? logDate { get; set; }
        public DateTime? logCreatedDateTime { get; set; }
        public string? Old_score { get; set; }
        public DateTime? Old_skilldate { get; set; }

    }

    [Keyless]
    public class SelectskillMatrixDocumentData
    {
        [Key]
        public int? RowID { get; set; }
        public string? empCode { get; set; }
        public string? empName { get; set; }
        public string? position { get; set; }
        public string? score { get; set; }
        public DateTime? skillDate { get; set; }
        public string? columnNames { get; set; }
        public string? DepartmentName { get; set; }
        public string? DocumentName { get; set; }

    }
}
