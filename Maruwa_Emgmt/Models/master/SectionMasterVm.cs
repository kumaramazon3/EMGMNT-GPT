using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models.master
{
    public class SectionMasterVm
    {
        public int SectionId { get; set; }

        [Required(ErrorMessage = "Section Code is required")]
        [StringLength(20)]
        public string SectionCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section Name is required")]
        [StringLength(150)]
        public string Sectionname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Code is required")]
        [StringLength(20)]
        public string Departmentcode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sub Department Name is required")]
        [StringLength(150)]
        public string SubDepartmentName { get; set; } = string.Empty;

        public bool issectionActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? EditedBy { get; set; }
        public DateTime? EditedOn { get; set; }
    }

    public class DepartmentLookupVm
    {
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class SectionSearchRequest
    {
        public string? GlobalSearch { get; set; }
        public string? SectionCode { get; set; }
        public string? Sectionname { get; set; }
        public string? SectionId { get; set; }
        public string? Departmentcode { get; set; }
        public string? SubDepartmentName { get; set; }
        public string? issectionActive { get; set; }
        public string? CreatedBy { get; set; }
        public string? EditedBy { get; set; }
        public string SortColumn { get; set; } = "SectionId";
        public string SortDirection { get; set; } = "DESC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SectionListResult
    {
        public List<SectionMasterVm> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
