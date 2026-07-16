using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.BAL.master
{
    public class bll_LeaveTypeMaster
    {
        private readonly i_LeaveTypeMaster _leaveTypeDal;

        public bll_LeaveTypeMaster(i_LeaveTypeMaster leaveTypeDal)
        {
            _leaveTypeDal = leaveTypeDal;
        }

        public Task<LeaveTypeListResult> GetLeaveTypesAsync(LeaveTypeSearchRequest request) => _leaveTypeDal.GetLeaveTypesAsync(request);
        public Task<LeaveTypeMasterVm?> GetLeaveTypeByIdAsync(string leaveId) => _leaveTypeDal.GetLeaveTypeByIdAsync(leaveId);
        public Task<(bool Success, string Message)> SaveLeaveTypeAsync(LeaveTypeMasterVm model, string employeeCode) => _leaveTypeDal.SaveLeaveTypeAsync(model, employeeCode);
        public Task<(bool Success, string Message)> DeleteLeaveTypeAsync(string leaveId, string employeeCode) => _leaveTypeDal.DeleteLeaveTypeAsync(leaveId, employeeCode);
        public Task<List<LeaveTypeMasterVm>> GetLeaveTypesForExportAsync(LeaveTypeSearchRequest request) => _leaveTypeDal.GetLeaveTypesForExportAsync(request);
    }
}
