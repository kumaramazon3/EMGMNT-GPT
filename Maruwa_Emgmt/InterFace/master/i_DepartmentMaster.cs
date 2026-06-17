using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.InterFace.master
{
    public interface i_DepartmentMaster
    {
        Task<DepartmentListResult> GetDepartmentsAsync(DepartmentSearchRequest request);
        Task<DepartmentMasterVm?> GetDepartmentByIdAsync(int recordNo);
        Task<(bool Success, string Message)> SaveDepartmentAsync(DepartmentMasterVm model, string employeeCode);
        Task<(bool Success, string Message)> DeleteDepartmentAsync(int recordNo, string employeeCode);
        Task<List<DepartmentMasterVm>> GetDepartmentsForExportAsync(DepartmentSearchRequest request);
    }
}
