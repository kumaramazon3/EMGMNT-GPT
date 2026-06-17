using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models.master
{
    public class DepartmentMasterVm
    {
        public int RecordNo { get; set; }

        [Required(ErrorMessage = "Department Code is required")]
        [StringLength(20)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(150)]
        public string DepartmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Japan Head is required")]
        public string JapanHead { get; set; } = string.Empty;

        [Required(ErrorMessage = "Office is required")]
        public string Office { get; set; } = string.Empty;

        [Required(ErrorMessage = "Got Section is required")]
        public string GotSection { get; set; } = string.Empty;

        [Required(ErrorMessage = "General Abbreviation is required")]
        [StringLength(20)]
        public string Prefix { get; set; } = string.Empty;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? EditedBy { get; set; }
        public DateTime? EditedOn { get; set; }
        public bool ActiveStatus { get; set; }
    }

    public class DepartmentSearchRequest
    {
        public string? GlobalSearch { get; set; }
        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? JapanHead { get; set; }
        public string? Office { get; set; }
        public string? GotSection { get; set; }
        public string? Prefix { get; set; }
        public string SortColumn { get; set; } = "RecordNo";
        public string SortDirection { get; set; } = "DESC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class DepartmentListResult
    {
        public List<DepartmentMasterVm> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
