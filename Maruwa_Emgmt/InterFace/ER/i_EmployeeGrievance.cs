using Maruwa_Emgmt.Models.ER;

namespace Maruwa_Emgmt.InterFace.ER
{
    public interface i_EmployeeGrievance
    {
        Task<List<EmployeeLookupVm>> SearchEmployeeLookupAsync(string? searchText);
        Task<EmployeeLookupVm?> GetEmployeeByCodeAsync(string empCode);
        Task<EmployeeGrievanceListResult> GetMyGrievancesAsync(EmployeeGrievanceSearchRequest request, string loggedInEmpCode, bool isHrUser = false);
        Task<EmployeeGrievanceFormVm?> GetGrievanceByIdAsync(int grievanceId, string loggedInEmpCode, bool isHrUser = false);
        Task<(bool Success, string Message, int GrievanceID, string ReferenceNo)> SaveComplaintAsync(EmployeeGrievanceFormVm model, string employeeCode);
        Task<(bool Success, string Message)> MarkViewedByHrAsync(int grievanceId, string employeeCode);
        Task<(bool Success, string Message)> UpdateHrRemarksAsync(int grievanceId, string remarks, string employeeCode);
        Task<(bool Success, string Message)> SaveHrActionAsync(EmployeeGrievanceHrActionVm model, string employeeCode);
    }
}
