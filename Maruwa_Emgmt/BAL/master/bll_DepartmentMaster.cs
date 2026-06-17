using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models.master;

namespace Maruwa_Emgmt.BAL.master
{
    public class bll_DepartmentMaster
    {
        private readonly i_DepartmentMaster _departmentDal;

        public bll_DepartmentMaster(i_DepartmentMaster departmentDal)
        {
            _departmentDal = departmentDal;
        }

        public Task<DepartmentListResult> GetDepartmentsAsync(DepartmentSearchRequest request) => _departmentDal.GetDepartmentsAsync(request);
        public Task<DepartmentMasterVm?> GetDepartmentByIdAsync(int recordNo) => _departmentDal.GetDepartmentByIdAsync(recordNo);
        public Task<(bool Success, string Message)> SaveDepartmentAsync(DepartmentMasterVm model, string employeeCode) => _departmentDal.SaveDepartmentAsync(model, employeeCode);
        public Task<(bool Success, string Message)> DeleteDepartmentAsync(int recordNo, string employeeCode) => _departmentDal.DeleteDepartmentAsync(recordNo, employeeCode);
        public Task<List<DepartmentMasterVm>> GetDepartmentsForExportAsync(DepartmentSearchRequest request) => _departmentDal.GetDepartmentsForExportAsync(request);
    }
}
