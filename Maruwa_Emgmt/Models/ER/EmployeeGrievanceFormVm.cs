using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models.ER
{
    public class EmployeeLookupVm
    {
        public string EmpCode { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
    }

    public class EmployeeGrievanceListItemVm
    {
        public int GrievanceID { get; set; }
        public string ReferenceNo { get; set; } = string.Empty;
        public string ComplainantEmpId { get; set; } = string.Empty;
        public string ComplainantName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime? DateOfReport { get; set; }
        public string GrievanceSummary { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string HRRemarks { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; }
    }

    public class EmployeeGrievanceListResult
    {
        public List<EmployeeGrievanceListItemVm> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class EmployeeGrievanceSearchRequest
    {
        public string? GlobalSearch { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Status { get; set; }
        public string SortColumn { get; set; } = "CreatedOn";
        public string SortDirection { get; set; } = "DESC";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class EmployeeGrievanceAttachmentVm
    {
        public int AttachmentID { get; set; }
        public int GrievanceID { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime? UploadedOn { get; set; }
    }

    public class EmployeeGrievanceInvolvedPartyVm
    {
        public int PartyID { get; set; }
        public int GrievanceID { get; set; }
        public string EmployeeID { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class EmployeeGrievanceFormVm
    {
        public int GrievanceID { get; set; }
        public string? ReferenceNo { get; set; }

        [Required(ErrorMessage = "Employee ID is required")]
        public string ComplainantEmpId { get; set; } = string.Empty;
        public string ComplainantName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public DateTime? DateOfReport { get; set; }

        public bool UnfairTreatment { get; set; }
        public bool HarassmentBullying { get; set; }
        public bool WorkLapses { get; set; }
        public bool PolicySopBreach { get; set; }
        public bool OshaConcern { get; set; }
        public bool SupervisorMisconduct { get; set; }
        public bool AbuseOfAuthority { get; set; }
        public bool WorkingHoursIssue { get; set; }
        public bool OtherComplaint { get; set; }
        public string? OtherComplaintText { get; set; }

        public DateTime? ConductDate { get; set; }
        public TimeSpan? ConductTime { get; set; }
        public string? Location { get; set; }
        public string? IncidentDescription { get; set; }
        public string? Witnesses { get; set; }
        public bool SupportingDocumentsAttached { get; set; }
        public string? DesiredOutcome { get; set; }

        public string? EmployeeSignaturePath { get; set; }
        public string? DeclarationEmployeeName { get; set; }
        public string? DeclarationEmployeeId { get; set; }
        public DateTime? DeclarationDate { get; set; }
        public string Status { get; set; } = "InProgress";
        public string? HRRemarks { get; set; }

        public List<EmployeeGrievanceInvolvedPartyVm> InvolvedParties { get; set; } = new();
        public List<EmployeeGrievanceAttachmentVm> Attachments { get; set; } = new();
        public EmployeeGrievanceHrActionVm? HrAction { get; set; }
    }


    public class EmployeeGrievanceHrActionEmployeeVm
    {
        public int HRActionEmployeeID { get; set; }
        public int HRActionID { get; set; }
        public int GrievanceID { get; set; }
        public string EmployeeID { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string PositionTitle { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class EmployeeGrievanceHrActionVm
    {
        public int HRActionID { get; set; }
        public int GrievanceID { get; set; }
        public string? HREmpId { get; set; }
        public string? HRName { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? ActionEmployeeId { get; set; }
        public string? ActionEmployeeName { get; set; }
        public string? InvestigationSummary { get; set; }
        public string? EmployeeExplanation { get; set; }
        public string? Remarks { get; set; }
        public bool OutcomeResolved { get; set; }
        public bool OutcomeReferredToER { get; set; }
        public bool OutcomeReferredToDomesticInquiry { get; set; }
        public bool MinorMisconduct { get; set; }
        public bool MajorMisconduct { get; set; }
        public string? MajorMisconductText { get; set; }
        public string? HRSignaturePath { get; set; }
        public string? Department { get; set; }
        public List<EmployeeGrievanceHrActionEmployeeVm> ActionEmployees { get; set; } = new();
    }
}
