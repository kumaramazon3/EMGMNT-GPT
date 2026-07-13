using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.BAL.master
{
    public class bll_ReasonMaster
    {
        private readonly i_ReasonMaster _reasonDal;

        public bll_ReasonMaster(i_ReasonMaster reasonDal)
        {
            _reasonDal = reasonDal;
        }

        public Task<ReasonListResult> GetReasonsAsync(ReasonSearchRequest request) => _reasonDal.GetReasonsAsync(request);
        public Task<ReasonMasterVm?> GetReasonByIdAsync(string reasonId) => _reasonDal.GetReasonByIdAsync(reasonId);
        public Task<(bool Success, string Message)> SaveReasonAsync(ReasonMasterVm model, string employeeCode) => _reasonDal.SaveReasonAsync(model, employeeCode);
        public Task<(bool Success, string Message)> DeleteReasonAsync(string reasonId, string employeeCode) => _reasonDal.DeleteReasonAsync(reasonId, employeeCode);
        public Task<List<ReasonMasterVm>> GetReasonsForExportAsync(ReasonSearchRequest request) => _reasonDal.GetReasonsForExportAsync(request);
    }
}
