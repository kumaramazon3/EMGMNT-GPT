using Maruwa_Emgmt.DAL.Leave;
using Maruwa_Emgmt.InterFace.Leave;
using Maruwa_Emgmt.Models.Leave;
using System.Data;

namespace Maruwa_Emgmt.BAL.Leave
{
    public class bll_leave
    {
        private readonly i_leave _leaveRepo;
        public bll_leave(i_leave leaveRepo)
        {
            _leaveRepo = leaveRepo;
        }

        public async Task<List<leaveform>> GetScheduledLeaveAsync(string empCode,string designation)
        {
            return await _leaveRepo.GetScheduledLeaveAsync(empCode, designation);
        }

        public async Task<List<leaveformHistory>> GetLeaveFormByEmpAsync(string empCode)
        {
            try
            {
                return await _leaveRepo.GetLeaveFormByEmpAsync(empCode);
            }
            catch (Exception)
            {
                throw; // bubble up to controller
            }
        }
        public async Task<int> Approved_SubmitLeavesEmployee(LeaveApprovalModel model)
        {
            return await _leaveRepo.Approved_SubmitLeavesEmployee(model);
        }

        public async Task<bool> UpdateLeaveApprovalsAsync(List<LeaveApprovalUpdate> updates)
        {
            if (updates == null || !updates.Any())
                throw new ArgumentException("No records to update.");

            foreach (var item in updates)
            {
                if (string.IsNullOrEmpty(item.modifiedby))
                    item.modifiedby = "SYSTEM";

                if (item.modifiedtime == default)
                    item.modifiedtime = DateTime.Now;

                await _leaveRepo.SubmitApprovalAsync(item);
            }

            return true;
        }

        public object GetEmployeeLeaveFullSummary(string empCode)
        {
            DataSet ds = _leaveRepo.GetEmployeeLeaveFullSummary(empCode);

            var response = new
            {
                currentYear = ds.Tables["CurrentYearLeave"].Rows.Count > 0
                    ? new
                    {
                        empcode = ds.Tables["CurrentYearLeave"].Rows[0]["empcode"],
                        leavelevel = ds.Tables["CurrentYearLeave"].Rows[0]["leavelevel"],
                        annual = ds.Tables["CurrentYearLeave"].Rows[0]["annual"],
                        med = ds.Tables["CurrentYearLeave"].Rows[0]["med"],
                        balAleave = ds.Tables["CurrentYearLeave"].Rows[0]["balAleave"],
                        balMleave = ds.Tables["CurrentYearLeave"].Rows[0]["balMleave"],
                        probation = ds.Tables["CurrentYearLeave"].Rows[0]["probation"],
                        experience = ds.Tables["CurrentYearLeave"].Rows[0]["experience"]
                    }
                    : null,

                lastYear = ds.Tables.Count > 1 && ds.Tables["LastYearLeave"].Rows.Count > 0
                    ? new
                    {
                        leavelevel = ds.Tables["LastYearLeave"].Rows[0]["leavelevel"],
                        probation = ds.Tables["LastYearLeave"].Rows[0]["probation"],
                        experience = ds.Tables["LastYearLeave"].Rows[0]["experience"],
                        ActualAnnualLeaves = ds.Tables["LastYearLeave"].Rows[0]["ActualAnnualLeaves"],
                        BAL_LastYear_AnnualLeave = ds.Tables["LastYearLeave"].Rows[0]["BAL_LastYear_AnnualLeave"],
                        ActualMedicalLeaves = ds.Tables["LastYearLeave"].Rows[0]["ActualMedicalLeaves"],
                        BAL_LastYear_MedicalLeave = ds.Tables["LastYearLeave"].Rows[0]["BAL_LastYear_MedicalLeave"]
                    }
                    : null
            };

            return response;
        }
        
        public object GetEmployeeLeaveSummary(string empCode)
        {
            DataSet ds = _leaveRepo.GetEmployeeLeaveSummary(empCode);
            var response = new
            {
                currentYear = ds.Tables["CurrentYearLeave"].Rows.Count > 0
                    ? new
                    {
                        empcode = ds.Tables["CurrentYearLeave"].Rows[0]["empCode"],
                        empName = ds.Tables["CurrentYearLeave"].Rows[0]["empName"],
                        designation = ds.Tables["CurrentYearLeave"].Rows[0]["designation"],
                        department = ds.Tables["CurrentYearLeave"].Rows[0]["department"],
                        subDepartment = ds.Tables["CurrentYearLeave"].Rows[0]["subDepartment"],
                        section = ds.Tables["CurrentYearLeave"].Rows[0]["section"],
                        leavelevel = ds.Tables["CurrentYearLeave"].Rows[0]["leaveLevel"],
                        //annual = ds.Tables["CurrentYearLeave"].Rows[0]["CurrentYear_Total_AnnualLeaves"],
                        //UsedLeaves = ds.Tables["CurrentYearLeave"].Rows[0]["UsedLeaves"],
                        //RemainingAnnualLeaves = ds.Tables["CurrentYearLeave"].Rows[0]["RemainingAnnualLeaves"],


                        annual = ds.Tables["CurrentYearLeave"].Rows[0]["CurrentYear_Total_AnnualLeaves"] != DBNull.Value
                        ? Convert.ToInt32(ds.Tables["CurrentYearLeave"].Rows[0]["CurrentYear_Total_AnnualLeaves"]): 0,
                        UsedLeaves = ds.Tables["CurrentYearLeave"].Rows[0]["UsedLeaves"] != DBNull.Value
                        ? Convert.ToInt32(ds.Tables["CurrentYearLeave"].Rows[0]["UsedLeaves"]): 0,
                        RemainingAnnualLeaves = ds.Tables["CurrentYearLeave"].Rows[0]["RemainingAnnualLeaves"] != DBNull.Value
                        ? Convert.ToInt32(ds.Tables["CurrentYearLeave"].Rows[0]["RemainingAnnualLeaves"]): 0,



                        med = ds.Tables["CurrentYearLeave"].Rows[0]["CurrentYear_Total_MedicalLeaves"],
                        MCUsed = ds.Tables["CurrentYearLeave"].Rows[0]["MCUsed"],
                        RemainingMCLeaves = ds.Tables["CurrentYearLeave"].Rows[0]["RemainingMCLeaves"],

                        probation = ds.Tables["CurrentYearLeave"].Rows[0]["probation"],
                        experience = ds.Tables["CurrentYearLeave"].Rows[0]["LeaveExperience"]
                    }
                    : null,

                lastYear = ds.Tables.Count > 1 && ds.Tables["LastYearLeave"].Rows.Count > 0
                    ? new
                    {
                        leavelevel = ds.Tables["LastYearLeave"].Rows[0]["leaveLevel"],
                       // probation = ds.Tables["LastYearLeave"].Rows[0]["probation"],
                        experience = ds.Tables["LastYearLeave"].Rows[0]["MappedExperience"],
                        ActualAnnualLeaves = ds.Tables["LastYearLeave"].Rows[0]["LastYear_Total_AnnualLeaves"],
                        LastYear_Used_AnnualLeave = ds.Tables["LastYearLeave"].Rows[0]["LastYear_Used_AnnualLeaves"],
                        LastYear_Remaining_AnnualLeaves = ds.Tables["LastYearLeave"].Rows[0]["LastYear_Remaining_AnnualLeaves"]
                    }
                    : null
            };
            return response;
        }

        public async Task<string> SubmitLeave(LeaveApplicationSubmitModel model)
        {
            // Business logic validation
            if (model.days <= 0)
                throw new ArgumentException("Invalid leave days");
            try
            {
                // Call the DAL/Repository
                var appNo = await _leaveRepo.SubmitLeave(model);
                return appNo;
            }
            catch (Exception ex)
            {
                // Rethrow the exception to controller
                throw;
            }
        }
    }
}
