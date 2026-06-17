using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.InterFace.master
{
    public interface i_LeaveTypeMaster
    {
        Task<LeaveTypeListResult> GetLeaveTypesAsync(LeaveTypeSearchRequest request);
        Task<LeaveTypeMasterVm?> GetLeaveTypeByIdAsync(string leaveId);
        Task<(bool Success, string Message)> SaveLeaveTypeAsync(LeaveTypeMasterVm model, string employeeCode);
        Task<(bool Success, string Message)> DeleteLeaveTypeAsync(string leaveId, string employeeCode);
        Task<List<LeaveTypeMasterVm>> GetLeaveTypesForExportAsync(LeaveTypeSearchRequest request);
    }
}
