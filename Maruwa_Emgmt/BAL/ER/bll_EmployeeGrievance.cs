using Maruwa_Emgmt.InterFace.ER;
using Maruwa_Emgmt.Models.ER;

namespace Maruwa_Emgmt.BAL.ER
{
    public class bll_EmployeeGrievance
    {
        private readonly i_EmployeeGrievance _employeeGrievanceDal;

        public bll_EmployeeGrievance(i_EmployeeGrievance employeeGrievanceDal)
        {
            _employeeGrievanceDal = employeeGrievanceDal;
        }

        public Task<List<EmployeeLookupVm>> SearchEmployeeLookupAsync(string? searchText) => _employeeGrievanceDal.SearchEmployeeLookupAsync(searchText);
        public Task<EmployeeLookupVm?> GetEmployeeByCodeAsync(string empCode) => _employeeGrievanceDal.GetEmployeeByCodeAsync(empCode);
        public Task<EmployeeGrievanceListResult> GetMyGrievancesAsync(EmployeeGrievanceSearchRequest request, string loggedInEmpCode, bool isHrUser = false) => _employeeGrievanceDal.GetMyGrievancesAsync(request, loggedInEmpCode, isHrUser);
        public Task<EmployeeGrievanceFormVm?> GetGrievanceByIdAsync(int grievanceId, string loggedInEmpCode, bool isHrUser = false) => _employeeGrievanceDal.GetGrievanceByIdAsync(grievanceId, loggedInEmpCode, isHrUser);
        public Task<(bool Success, string Message, int GrievanceID, string ReferenceNo)> SaveComplaintAsync(EmployeeGrievanceFormVm model, string employeeCode) => _employeeGrievanceDal.SaveComplaintAsync(model, employeeCode);
        public Task<(bool Success, string Message)> SaveHrActionAsync(EmployeeGrievanceHrActionVm model, string employeeCode) => _employeeGrievanceDal.SaveHrActionAsync(model, employeeCode);
    }
}
