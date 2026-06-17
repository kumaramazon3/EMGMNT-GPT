using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models.SkillMatrix
{
    [Table("skillMatrixDocumentData", Schema = "dbo")]
    public class skillMatrixDocumentData
    {
        [Key]
        public int RowID { get; set; }
        public string? columnNames { get; set; }
        public string? DocumentName { get; set; }
        public string? Department { get; set; }
        public string? departmentCode { get; set; }
        public string? subDepartmentCode { get; set; }
        public string? sectionCode { get; set; }
        public string? CreatedBy { get; set; }
        public int? Rev { get; set; }
        public string? empCode { get; set; }
        public string? empName { get; set; }
        public DateTime? position { get; set; }
        public string? score { get; set; }
        public DateTime? skillDate { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string? modifyedBy { get; set; }
        public DateTime? modifyedDateTime { get; set; }
        public bool? Status { get; set; }

        // Hidden Field Values
        public string? standards { get; set; }
        public string? category { get; set; }
        public string? preparedBy { get; set; }
        public string? reviewedBy { get; set; }
        public string? approvedBy { get; set; }
        public string? approvedDate { get; set; }

    }
    public class skillMatrixDocumentData_log
    {
        public int Id { get; set; }             // PK
        public DateTime logDate { get; set; }   // must match DB column type
    }
    public class skillMatrixlog_Years
    {
        public string LogYear { get; set; }
    }

    [Keyless]
    public class Insert_skillMatrix
    {
        public string? columnNames { get; set; }// Document Number
        public string? DocumentName { get; set; }
        public int? Rev { get; set; }
        public string? standards { get; set; }
        public string? Department { get; set; }
        public string? category { get; set; }
        public string? preparedBy { get; set; }
        public string? reviewedBy { get; set; }
        public string? approvedBy { get; set; }
        public DateTime? approvedDate { get; set; }
        public string? sectionCode { get; set; }
        public string? CreatedBy { get; set; }
    }

    [Keyless]
    public class OriginalReport_skillMatrixDocumentData
    {

        public string? ColumnNames { get; set; }
        public string? DocumentName { get; set; }
        public int? Rev { get; set; }
        public string? Standards { get; set; }
        public string? Department { get; set; }
        public string? Category { get; set; }
        public string? PreparedBy { get; set; }
        public string? ReviewedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? SectionCode { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
    }


}
