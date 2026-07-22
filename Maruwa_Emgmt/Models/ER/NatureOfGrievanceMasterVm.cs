using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models.ER
{
    public class NatureOfGrievanceMasterVm
    {
        public int NatureID { get; set; }

        [Required(ErrorMessage = "Nature of Grievance is required")]
        [StringLength(300)]
        public string NatureName { get; set; } = string.Empty;

        public bool IsOther { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? EditedBy { get; set; }
        public DateTime? EditedOn { get; set; }
        public bool isActive { get; set; } = true;
    }

    public class NatureOfGrievanceSearchRequest
    {
        public string? GlobalSearch { get; set; }
        public string? NatureName { get; set; }
        public string? IsOther { get; set; }
        public string? CreatedBy { get; set; }
        public string? EditedBy { get; set; }
        public string? isActive { get; set; }
        public string SortColumn { get; set; } = "NatureName";
        public string SortDirection { get; set; } = "ASC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class NatureOfGrievanceListResult
    {
        public List<NatureOfGrievanceMasterVm> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class EmployeeGrievanceNatureSelectionVm
    {
        public int GrievanceNatureID { get; set; }
        public int GrievanceID { get; set; }
        public int NatureID { get; set; }
        public string NatureName { get; set; } = string.Empty;
        public bool IsOther { get; set; }
        public string? OtherComplaintText { get; set; }
    }
}
