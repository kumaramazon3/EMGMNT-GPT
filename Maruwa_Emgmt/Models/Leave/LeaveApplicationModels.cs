using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models.Leave
{
    public class LeaveLookupVm
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Code { get; set; }
        public decimal? Entitlement { get; set; }
        public string? Remark { get; set; }
    }

    public class LeaveEmployeeInfoVm
    {
        public string EmpCode { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public string? SubDepartment { get; set; }
        public string? Section { get; set; }
        public DateTime? DateOfJoin { get; set; }
        public bool IsOperator { get; set; }
    }

    public class LeaveSummaryVm
    {
        public LeaveEmployeeInfoVm Employee { get; set; } = new();
        public decimal CarryForwardTotal { get; set; }
        public decimal CarryForwardUtilised { get; set; }
        public decimal CarryForwardBalance { get; set; }
        public decimal AnnualEntitlement { get; set; }
        public decimal AnnualUtilised { get; set; }
        public decimal AnnualBalance { get; set; }
        public decimal MedicalEntitlement { get; set; }
        public decimal MedicalUtilised { get; set; }
        public decimal MedicalBalance { get; set; }
        public decimal TotalEntitlementBalance { get; set; }
    }

    public class LeaveApplicationPageDataVm
    {
        public LeaveSummaryVm Summary { get; set; } = new();
        public List<LeaveLookupVm> LeaveTypes { get; set; } = new();
        public List<LeaveLookupVm> HalfDayLeaves { get; set; } = new();
        public List<LeaveLookupVm> Reasons { get; set; } = new();
        public List<LeaveLookupVm> PersonsInCharge { get; set; } = new();
    }

    public class LeaveApplicationSaveRequest
    {
        public string? LeaveTypeId { get; set; }
        public string? LeaveTypeName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal LeaveDays { get; set; }
        public bool IsHalfDay { get; set; }
        public int? HalfDayLeaveId { get; set; }
        public string? HalfDayLeaveText { get; set; }
        public string? ReasonId { get; set; }
        public string? ReasonText { get; set; }
        public string? PersonInCharge1 { get; set; }
        public string? PersonInCharge1Name { get; set; }
        public string? PersonInCharge2 { get; set; }
        public string? PersonInCharge2Name { get; set; }
        public int? AppNo { get; set; }
        public decimal AnnualBalance { get; set; }
        public decimal MedicalBalance { get; set; }
        public decimal CarryForwardBalance { get; set; }
    }

    public class LeaveApplicationSaveResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AppNo { get; set; }
    }



    public class LeaveDaysCalculationRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsHalfDay { get; set; }
    }

    public class LeaveDaysCalculationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal LeaveDays { get; set; }
    }

    public class LeaveApplicationEditVm
    {
        public int AppNo { get; set; }
        public string LeaveTypeId { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal LeaveDays { get; set; }
        public bool IsHalfDay { get; set; }
        public int? HalfDayLeaveId { get; set; }
        public string? HalfDayLeaveText { get; set; }
        public string? ReasonId { get; set; }
        public string? ReasonText { get; set; }
        public string? PersonInCharge1 { get; set; }
        public string? PersonInCharge1Name { get; set; }
        public string? PersonInCharge2 { get; set; }
        public string? PersonInCharge2Name { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LeaveSelfStatusSearchRequest
    {
        public string? GlobalSearch { get; set; }
        public string? AppNo { get; set; }
        public string? LeaveType { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class LeaveSelfStatusRowVm
    {
        public int AppNo { get; set; }
        public DateTime ApplicationDate { get; set; }
        public decimal LeaveDays { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public string ReasonText { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? StatusReason { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? HalfDayLeaveText { get; set; }
        public string? PersonInCharge1Name { get; set; }
        public string? PersonInCharge2Name { get; set; }
    }

    public class LeaveSelfStatusListResult
    {
        public List<LeaveSelfStatusRowVm> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
