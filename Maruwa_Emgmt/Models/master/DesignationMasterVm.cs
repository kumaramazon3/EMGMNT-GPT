using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models.master
{
    public class DesignationMasterVm
    {
        public int Sno { get; set; }

        [Required(ErrorMessage = "Designation Code is required")]
        [StringLength(50)]
        public string designationcode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Designation Name is required")]
        [StringLength(150)]
        public string designationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Insurance Category is required")]
        [StringLength(10)]
        public string insCatergory { get; set; } = string.Empty;

        [StringLength(20)]
        public string? dlevel { get; set; }

        [Required(ErrorMessage = "Probation is required")]
        [StringLength(10)]
        public string probation { get; set; } = string.Empty;

        [StringLength(20)]
        public string? CTQLevel { get; set; }

        [StringLength(20)]
        public string? kpi { get; set; }

        [StringLength(20)]
        public string? positioned { get; set; }

        [Required(ErrorMessage = "Insurance Amount is required")]
        [Range(0, 999999999, ErrorMessage = "Insurance Amount must be a valid amount")]
        public decimal insamount { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? EditedBy { get; set; }
        public DateTime? EditedOn { get; set; }
        public bool isActive { get; set; } = true;
    }

    public class InsuranceCategoryLookupVm
    {
        public int InsuranceCategoryId { get; set; }
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }

    public class ProbationLookupVm
    {
        public int ProbationId { get; set; }
        public string ProbationCode { get; set; } = string.Empty;
        public string ProbationName { get; set; } = string.Empty;
    }

    public class DesignationSearchRequest
    {
        public string? GlobalSearch { get; set; }
        public string? designationcode { get; set; }
        public string? designationName { get; set; }
        public string? probation { get; set; }
        public string? insCatergory { get; set; }
        public string? insamount { get; set; }
        public string? CreatedBy { get; set; }
        public string? EditedBy { get; set; }
        public string? isActive { get; set; }
        public string SortColumn { get; set; } = "Sno";
        public string SortDirection { get; set; } = "DESC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class DesignationListResult
    {
        public List<DesignationMasterVm> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
