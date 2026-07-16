using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.InterFace.master
{
    public interface i_Designation
    {
        Task<DesignationListResult> GetDesignationsAsync(DesignationSearchRequest request);
        Task<DesignationMasterVm?> GetDesignationByIdAsync(int sno);
        Task<(bool Success, string Message)> SaveDesignationAsync(DesignationMasterVm model, string employeeCode);
        Task<(bool Success, string Message)> DeleteDesignationAsync(int sno, string employeeCode);
        Task<List<DesignationMasterVm>> GetDesignationsForExportAsync(DesignationSearchRequest request);
        Task<List<InsuranceCategoryLookupVm>> GetInsuranceCategoryLookupAsync(string? searchText);
        Task<List<ProbationLookupVm>> GetProbationLookupAsync(string? searchText);
    }
}
