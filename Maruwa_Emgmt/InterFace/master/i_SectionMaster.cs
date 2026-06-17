using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.InterFace.master
{
    public interface i_SectionMaster
    {
        Task<SectionListResult> GetSectionsAsync(SectionSearchRequest request);
        Task<SectionMasterVm?> GetSectionByIdAsync(int sectionId);
        Task<(bool Success, string Message)> SaveSectionAsync(SectionMasterVm model, string employeeCode);
        Task<(bool Success, string Message)> DeleteSectionAsync(int sectionId, string employeeCode);
        Task<List<SectionMasterVm>> GetSectionsForExportAsync(SectionSearchRequest request);
        Task<List<DepartmentLookupVm>> GetDepartmentLookupAsync(string? searchText);
    }
}
