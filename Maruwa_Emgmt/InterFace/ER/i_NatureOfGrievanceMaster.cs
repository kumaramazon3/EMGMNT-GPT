using Maruwa_Emgmt.Models.ER;

namespace Maruwa_Emgmt.InterFace.ER
{
    public interface i_NatureOfGrievanceMaster
    {
        Task<NatureOfGrievanceListResult> GetNatureOfGrievancesAsync(NatureOfGrievanceSearchRequest request);
        Task<NatureOfGrievanceMasterVm?> GetNatureOfGrievanceByIdAsync(int natureId);
        Task<(bool Success, string Message)> SaveNatureOfGrievanceAsync(NatureOfGrievanceMasterVm model, string employeeCode);
        Task<(bool Success, string Message)> DeleteNatureOfGrievanceAsync(int natureId, string employeeCode);
        Task<List<NatureOfGrievanceMasterVm>> GetNatureOfGrievancesForExportAsync(NatureOfGrievanceSearchRequest request);
        Task<List<NatureOfGrievanceMasterVm>> SearchActiveNatureOfGrievanceLookupAsync(string? searchText);
    }
}
