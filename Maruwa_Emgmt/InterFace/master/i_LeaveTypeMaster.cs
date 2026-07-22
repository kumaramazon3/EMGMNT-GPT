using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.InterFace.master
{
    public interface i_LeaveTypeMaster
    {
        Task<LeaveTypeListResult> GetLeaveTypesAsync(LeaveTypeSearchRequest request);
        Task<LeaveTypeMasterVm?> GetLeaveTypeByIdAsync(int seqLeaveID);
        Task<(bool Success, string Message)> SaveLeaveTypeAsync(LeaveTypeMasterVm model, string employeeCode);
        Task<(bool Success, string Message)> DeleteLeaveTypeAsync(int seqLeaveID, string employeeCode);
        Task<List<LeaveTypeMasterVm>> GetLeaveTypesForExportAsync(LeaveTypeSearchRequest request);
    }
}
