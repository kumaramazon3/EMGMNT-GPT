using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.InterFace.master
{
    public interface i_ReasonMaster
    {
        Task<ReasonListResult> GetReasonsAsync(ReasonSearchRequest request);
        Task<ReasonMasterVm?> GetReasonByIdAsync(string reasonId);
        Task<(bool Success, string Message)> SaveReasonAsync(ReasonMasterVm model, string employeeCode);
        Task<(bool Success, string Message)> DeleteReasonAsync(string reasonId, string employeeCode);
        Task<List<ReasonMasterVm>> GetReasonsForExportAsync(ReasonSearchRequest request);
    }
}
