using Maruwa_Emgmt.Models.Leave;

namespace Maruwa_Emgmt.InterFace.Leave
{
    public interface i_LeaveApplication
    {
        Task<LeaveApplicationPageDataVm> GetLeaveApplicationPageDataAsync(string empCode);
        Task<List<LeaveLookupVm>> GetLeaveTypesAsync();
        Task<List<LeaveLookupVm>> GetHalfDayLeavesAsync();
        Task<List<LeaveLookupVm>> GetReasonsAsync();
        Task<List<LeaveLookupVm>> GetPersonsInChargeAsync(string searchText);
        Task<LeaveSummaryVm> GetEmployeeLeaveSummaryAsync(string empCode);
        Task<LeaveApplicationSaveResult> ApplyLeaveAsync(LeaveApplicationSaveRequest request, string empCode);
        Task<LeaveSelfStatusListResult> GetSelfStatusAsync(LeaveSelfStatusSearchRequest request, string empCode);
        Task<LeaveApplicationSaveResult> CancelLeaveAsync(int appNo, string empCode);
        Task<LeaveDaysCalculationResult> CalculateLeaveDaysAsync(LeaveDaysCalculationRequest request, string empCode);
        Task<LeaveApplicationEditVm?> GetLeaveApplicationForEditAsync(int appNo, string empCode);
    }
}
