using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.BAL.master
{
    public class bll_SectionMaster
    {
        private readonly i_SectionMaster _sectionDal;

        public bll_SectionMaster(i_SectionMaster sectionDal)
        {
            _sectionDal = sectionDal;
        }

        public Task<SectionListResult> GetSectionsAsync(SectionSearchRequest request) => _sectionDal.GetSectionsAsync(request);
        public Task<SectionMasterVm?> GetSectionByIdAsync(int sectionId) => _sectionDal.GetSectionByIdAsync(sectionId);
        public Task<(bool Success, string Message)> SaveSectionAsync(SectionMasterVm model, string employeeCode) => _sectionDal.SaveSectionAsync(model, employeeCode);
        public Task<(bool Success, string Message)> DeleteSectionAsync(int sectionId, string employeeCode) => _sectionDal.DeleteSectionAsync(sectionId, employeeCode);
        public Task<List<SectionMasterVm>> GetSectionsForExportAsync(SectionSearchRequest request) => _sectionDal.GetSectionsForExportAsync(request);
        public Task<List<DepartmentLookupVm>> GetDepartmentLookupAsync(string? searchText) => _sectionDal.GetDepartmentLookupAsync(searchText);
    }
}
