using Maruwa_Emgmt.InterFace.Leave;
using Maruwa_Emgmt.Models.Leave;

namespace Maruwa_Emgmt.BAL.Leave
{
    public class bll_LeaveApplication
    {
        private readonly i_LeaveApplication _repo;
        public bll_LeaveApplication(i_LeaveApplication repo)
        {
            _repo = repo;
        }

        public Task<LeaveApplicationPageDataVm> GetLeaveApplicationPageDataAsync(string empCode) => _repo.GetLeaveApplicationPageDataAsync(empCode);
        public Task<List<LeaveLookupVm>> GetLeaveTypesAsync() => _repo.GetLeaveTypesAsync();
        public Task<List<LeaveLookupVm>> GetHalfDayLeavesAsync() => _repo.GetHalfDayLeavesAsync();
        public Task<List<LeaveLookupVm>> GetReasonsAsync() => _repo.GetReasonsAsync();
        public Task<List<LeaveLookupVm>> GetPersonsInChargeAsync(string searchText) => _repo.GetPersonsInChargeAsync(searchText);
        public Task<LeaveSummaryVm> GetEmployeeLeaveSummaryAsync(string empCode) => _repo.GetEmployeeLeaveSummaryAsync(empCode);
        public Task<LeaveSelfStatusListResult> GetSelfStatusAsync(LeaveSelfStatusSearchRequest request, string empCode) => _repo.GetSelfStatusAsync(request, empCode);
        public Task<LeaveApplicationSaveResult> CancelLeaveAsync(int appNo, string empCode) => _repo.CancelLeaveAsync(appNo, empCode);
        public Task<LeaveDaysCalculationResult> CalculateLeaveDaysAsync(LeaveDaysCalculationRequest request, string empCode) => _repo.CalculateLeaveDaysAsync(request, empCode);
        public Task<LeaveApplicationEditVm?> GetLeaveApplicationForEditAsync(int appNo, string empCode) => _repo.GetLeaveApplicationForEditAsync(appNo, empCode);

        public async Task<LeaveApplicationSaveResult> ApplyLeaveAsync(LeaveApplicationSaveRequest request, string empCode)
        {
            if (request == null)
            {
                return new LeaveApplicationSaveResult { Success = false, Message = "Invalid leave request." };
            }
            var errors = Validate(request);
            if (errors.Count > 0)
            {
                return new LeaveApplicationSaveResult { Success = false, Message = string.Join("\n", errors) };
            }
            return await _repo.ApplyLeaveAsync(request, empCode);
        }

        private static List<string> Validate(LeaveApplicationSaveRequest request)
        {
            var errors = new List<string>();
            var leaveTypeName = request.LeaveTypeName ?? string.Empty;
            var isMedical = leaveTypeName.Contains("medical", StringComparison.OrdinalIgnoreCase);
            var isOperator = string.Equals(request.PersonInCharge1Name, "OPERATOR_SKIP", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(request.LeaveTypeId)) errors.Add("Please select Leave Type.");
            if (request.FromDate == null) errors.Add("Please select Leave Period From Date.");
            if (request.ToDate == null) errors.Add("Please select Leave Period To Date.");
            if (request.FromDate != null && request.ToDate != null && request.FromDate.Value.Date > request.ToDate.Value.Date) errors.Add("Please check the selected Leave Period dates.");
            if (request.LeaveDays < 0.5m) errors.Add("Leave should not apply below half day.");
            if ((request.LeaveDays * 10) % 5 != 0) errors.Add("Leave days should be whole day or 0.5 day only.");
            if (request.IsHalfDay && (request.HalfDayLeaveId == null || request.HalfDayLeaveId <= 0)) errors.Add("Please select Half-Day Leave time.");
            if (request.IsHalfDay && request.FromDate != null && request.ToDate != null && request.FromDate.Value.Date != request.ToDate.Value.Date) errors.Add("Half Day Leave Date cannot be more than one day.");

            if (isMedical)
            {
                if (string.IsNullOrWhiteSpace(request.ReasonId)) errors.Add("Please select Reason for Medical Leave.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.ReasonText)) errors.Add("Please enter Reason.");
            }

            if (!isOperator && string.IsNullOrWhiteSpace(request.PersonInCharge1)) errors.Add("Please select Person in Charge 1.");
            if (!isOperator && !string.IsNullOrWhiteSpace(request.PersonInCharge1) &&
                string.Equals(request.PersonInCharge1?.Trim(), request.PersonInCharge2?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Person in Charge 1 and Person in Charge 2 should not be same.");
            }
            return errors;
        }
    }
}
