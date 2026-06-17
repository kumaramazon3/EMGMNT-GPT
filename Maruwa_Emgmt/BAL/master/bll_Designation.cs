using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.BAL.master
{
    public class bll_Designation
    {
        private readonly i_Designation _da;
        public bll_Designation(i_Designation da) => _da = da;
        public Task<DesignationListResult> GetDesignationsAsync(DesignationSearchRequest request) => _da.GetDesignationsAsync(request);
        public Task<DesignationMasterVm?> GetDesignationByIdAsync(int sno) => _da.GetDesignationByIdAsync(sno);
        public Task<(bool Success, string Message)> SaveDesignationAsync(DesignationMasterVm model, string employeeCode) => _da.SaveDesignationAsync(model, employeeCode);
        public Task<(bool Success, string Message)> DeleteDesignationAsync(int sno, string employeeCode) => _da.DeleteDesignationAsync(sno, employeeCode);
        public Task<List<DesignationMasterVm>> GetDesignationsForExportAsync(DesignationSearchRequest request) => _da.GetDesignationsForExportAsync(request);
        public Task<List<InsuranceCategoryLookupVm>> GetInsuranceCategoryLookupAsync(string? searchText) => _da.GetInsuranceCategoryLookupAsync(searchText);
        public Task<List<ProbationLookupVm>> GetProbationLookupAsync(string? searchText) => _da.GetProbationLookupAsync(searchText);
    }
}
