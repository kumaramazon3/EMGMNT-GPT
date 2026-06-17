using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models.master
{
    public class LeaveTypeMasterVm
    {
        [Required(ErrorMessage = "LeaveID is required")]
        [StringLength(50)]
        public string LeaveID { get; set; } = string.Empty;

        [Required(ErrorMessage = "LeaveType is required")]
        [StringLength(100)]
        public string LeaveType { get; set; } = string.Empty;

        [Required(ErrorMessage = "LeaveDescription is required")]
        [StringLength(500)]
        public string LeaveDescription { get; set; } = string.Empty;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? EditedBy { get; set; }
        public DateTime? EditedOn { get; set; }
        public bool isActive { get; set; } = true;
    }

    public class LeaveTypeSearchRequest
    {
        public string? GlobalSearch { get; set; }
        public string? LeaveID { get; set; }
        public string? LeaveType { get; set; }
        public string? LeaveDescription { get; set; }
        public string? CreatedBy { get; set; }
        public string? EditedBy { get; set; }
        public string? isActive { get; set; }
        public string SortColumn { get; set; } = "LeaveID";
        public string SortDirection { get; set; } = "ASC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class LeaveTypeListResult
    {
        public List<LeaveTypeMasterVm> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
