using Maruwa_Emgmt.Models.Leave;
using System.Data;

namespace Maruwa_Emgmt.InterFace.Leave
{
    public interface i_leave
    {
        Task<List<leaveform>> GetScheduledLeaveAsync(string empCode, string designation);
        Task<List<leaveformHistory>> GetLeaveFormByEmpAsync(string empCode);
        Task<int> Approved_SubmitLeavesEmployee(LeaveApprovalModel model);
        Task<int> SubmitApprovalAsync(LeaveApprovalUpdate model);
        DataSet GetEmployeeLeaveFullSummary(string empCode);
        DataSet GetEmployeeLeaveSummary(string empCode);
        Task<string> SubmitLeave(LeaveApplicationSubmitModel model);

    }
}
