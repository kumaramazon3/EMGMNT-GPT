using Maruwa_Emgmt.InterFace.ER;
using Maruwa_Emgmt.Models.ER;

namespace Maruwa_Emgmt.BAL.ER
{
    public class bll_NatureOfGrievanceMaster
    {
        private readonly i_NatureOfGrievanceMaster _natureDal;

        public bll_NatureOfGrievanceMaster(i_NatureOfGrievanceMaster natureDal)
        {
            _natureDal = natureDal;
        }

        public Task<NatureOfGrievanceListResult> GetNatureOfGrievancesAsync(NatureOfGrievanceSearchRequest request) => _natureDal.GetNatureOfGrievancesAsync(request);
        public Task<NatureOfGrievanceMasterVm?> GetNatureOfGrievanceByIdAsync(int natureId) => _natureDal.GetNatureOfGrievanceByIdAsync(natureId);
        public Task<(bool Success, string Message)> SaveNatureOfGrievanceAsync(NatureOfGrievanceMasterVm model, string employeeCode) => _natureDal.SaveNatureOfGrievanceAsync(model, employeeCode);
        public Task<(bool Success, string Message)> DeleteNatureOfGrievanceAsync(int natureId, string employeeCode) => _natureDal.DeleteNatureOfGrievanceAsync(natureId, employeeCode);
        public Task<List<NatureOfGrievanceMasterVm>> GetNatureOfGrievancesForExportAsync(NatureOfGrievanceSearchRequest request) => _natureDal.GetNatureOfGrievancesForExportAsync(request);
        public Task<List<NatureOfGrievanceMasterVm>> SearchActiveNatureOfGrievanceLookupAsync(string? searchText) => _natureDal.SearchActiveNatureOfGrievanceLookupAsync(searchText);
    }
}
