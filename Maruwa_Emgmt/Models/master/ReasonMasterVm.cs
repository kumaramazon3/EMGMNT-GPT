using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models.master
{
    public class ReasonMasterVm
    {
        [Required(ErrorMessage = "ReasonID is required")]
        [StringLength(50)]
        public string ReasonID { get; set; } = string.Empty;

        [Required(ErrorMessage = "ReasonType is required")]
        [StringLength(100)]
        public string ReasonType { get; set; } = string.Empty;

        [Required(ErrorMessage = "ReasonDescription is required")]
        [StringLength(500)]
        public string ReasonDescription { get; set; } = string.Empty;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? EditedBy { get; set; }
        public DateTime? EditedOn { get; set; }
        public bool isActive { get; set; } = true;
    }

    public class ReasonSearchRequest
    {
        public string? GlobalSearch { get; set; }
        public string? ReasonID { get; set; }
        public string? ReasonType { get; set; }
        public string? ReasonDescription { get; set; }
        public string? CreatedBy { get; set; }
        public string? EditedBy { get; set; }
        public string? isActive { get; set; }
        public string SortColumn { get; set; } = "ReasonID";
        public string SortDirection { get; set; } = "ASC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ReasonListResult
    {
        public List<ReasonMasterVm> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
