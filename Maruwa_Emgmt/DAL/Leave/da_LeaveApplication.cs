using Maruwa_Emgmt.InterFace.Leave;
using Maruwa_Emgmt.Models.Leave;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Maruwa_Emgmt.DAL.Leave
{
    public class da_LeaveApplication : i_LeaveApplication
    {
        private readonly string _connectionString;
        private readonly string _liveHrmisConnectionString;
        private readonly ILogger<da_LeaveApplication> _logger;

        public da_LeaveApplication(IConfiguration configuration, ILogger<da_LeaveApplication> logger)
        {
            _connectionString = configuration.GetConnectionString("EHRMConnection") ?? throw new InvalidOperationException("EHRMConnection is missing.");
            _liveHrmisConnectionString = configuration.GetConnectionString("LiveHRMISConnection") ?? throw new InvalidOperationException("LiveHRMISConnection is missing.");
            _logger = logger;
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);
        private SqlConnection CreateLiveHrmisConnection() => new SqlConnection(_liveHrmisConnectionString);

        public async Task<LeaveApplicationPageDataVm> GetLeaveApplicationPageDataAsync(string empCode)
        {
            return new LeaveApplicationPageDataVm
            {
                Summary = await GetEmployeeLeaveSummaryAsync(empCode),
                LeaveTypes = await GetLeaveTypesAsync(),
                HalfDayLeaves = await GetHalfDayLeavesAsync(),
                Reasons = await GetReasonsAsync(),
                PersonsInCharge = await GetPersonsInChargeAsync(string.Empty)
            };
        }

        public async Task<List<LeaveLookupVm>> GetLeaveTypesAsync()
        {
            var list = new List<LeaveLookupVm>();
            var table = await ExecuteTableAsync("usp_Leave_GetLeaveTypes");
            foreach (DataRow row in table.Rows)
            {
                list.Add(new LeaveLookupVm
                {
                    Id = FieldString(row, "LeaveID", "LeaveTypeID", "Value"),
                    Text = FieldString(row, "LeaveType", "Text", "LeaveTypeName"),
                    Code = FieldString(row, "LeaveID", "LeaveTypeID", "Value"),
                    Remark = FieldString(row, "Remark")
                });
            }
            return list;
        }

        public async Task<List<LeaveLookupVm>> GetHalfDayLeavesAsync()
        {
            var list = new List<LeaveLookupVm>();
            var table = await ExecuteTableAsync("usp_Leave_GetHalfDayLeaves");
            foreach (DataRow row in table.Rows)
            {
                list.Add(new LeaveLookupVm
                {
                    Id = FieldString(row, "HalfDayLeaveID", "Id"),
                    Text = FieldString(row, "TimeText", "Text")
                });
            }
            return list;
        }

        public async Task<List<LeaveLookupVm>> GetReasonsAsync()
        {
            var list = new List<LeaveLookupVm>();
            var table = await ExecuteTableAsync("usp_Leave_GetReasons");
            foreach (DataRow row in table.Rows)
            {
                list.Add(new LeaveLookupVm
                {
                    Id = FieldString(row, "ReasonID", "Id"),
                    Text = FieldString(row, "ReasonDescription", "Reason", "Text")
                });
            }
            return list;
        }

        public async Task<List<LeaveLookupVm>> GetPersonsInChargeAsync(string searchText)
        {
            var list = new List<LeaveLookupVm>();
            var table = await ExecuteLiveHrmisTableAsync("dbo.usp_Leave_SearchEmployees", new SqlParameter("@SearchText", (object?)searchText ?? DBNull.Value));
            foreach (DataRow row in table.Rows)
            {
                list.Add(new LeaveLookupVm
                {
                    Id = FieldString(row, "EmpCode", "empCode"),
                    Text = FieldString(row, "EmpDisplay", "EmpName", "empName")
                });
            }
            return list;
        }

        public async Task<LeaveSummaryVm> GetEmployeeLeaveSummaryAsync(string empCode)
        {
            try
            {
                var employee = await GetEmployeeInfoAsync(empCode);
                var summary = new LeaveSummaryVm { Employee = employee };
                var today = DateTime.Today;
                var fiscalStart = new DateTime(today.Year, 1, 1);
                var fiscalEnd = new DateTime(today.Year, 12, 31);
                var doj = employee.DateOfJoin ?? today;

                decimal expDays = (decimal)(today - doj).TotalDays;
                decimal expMonths = expDays / 30m;
                decimal expYearsRounded = Math.Round(expMonths / 12m, 1);
                int expLevel = GetLegacyExperienceLevel(expYearsRounded);

                var level = await GetLeaveLevelAsync(empCode, expLevel);
                decimal annual = level.Annual;
                decimal medical = level.Medical;
                decimal probation = level.Probation;
                decimal tmpAnnual = annual;

                // Existing Leaveform.aspx.vb logic:
                // If fiscal year start is after DOJ and employee has less than 365 days on fiscal start,
                // prorate annual entitlement; otherwise if DOJ is after fiscal start annual is 0.
                if (fiscalStart > doj)
                {
                    if ((fiscalStart - doj).TotalDays < 365)
                    {
                        annual = Math.Round((((decimal)(fiscalStart - doj).TotalDays) / 365m) * annual, 0);
                    }
                }
                else
                {
                    annual = 0;
                }

                decimal annualTaken = await GetAnnualTakenUsingLegacyProcedureAsync(fiscalStart, fiscalEnd, empCode);
                decimal medicalTaken = await GetMedicalTakenUsingLegacyProcedureAsync(fiscalStart, fiscalEnd, empCode);

                decimal annualBalance = annual - annualTaken;
                decimal medicalBalance = medical - medicalTaken;

                var carryForward = await GetCarryForwardAsync(empCode, today.Year, fiscalStart, fiscalEnd, doj, tmpAnnual);
                decimal carryTotal = carryForward.Total;
                decimal carryBalance = carryForward.Balance;
                decimal carryUtilised = carryTotal - carryBalance;

                decimal prorate;
                if (expMonths >= probation)
                {
                    if (doj > fiscalStart)
                    {
                        _ = ((decimal)(today - doj).TotalDays / 365m) * annual;
                        prorate = Math.Round(annual - annualTaken, 1) + carryBalance;
                    }
                    else
                    {
                        _ = ((decimal)(today - fiscalStart).TotalDays / 365m) * annual;
                        prorate = Math.Round(annual - annualTaken, 1) + carryBalance;
                    }
                }
                else
                {
                    prorate = 0;
                    carryTotal = 0;
                    carryBalance = 0;
                    carryUtilised = 0;
                }

                summary.CarryForwardTotal = NormalizeDecimal(carryTotal);
                summary.CarryForwardUtilised = NormalizeDecimal(carryUtilised);
                summary.CarryForwardBalance = NormalizeDecimal(carryBalance);
                summary.AnnualEntitlement = NormalizeDecimal(annual);
                summary.AnnualUtilised = NormalizeDecimal(annual - annualBalance);
                summary.AnnualBalance = NormalizeDecimal(annualBalance);
                summary.MedicalEntitlement = NormalizeDecimal(medical);
                summary.MedicalUtilised = NormalizeDecimal(medical - medicalBalance);
                summary.MedicalBalance = NormalizeDecimal(medicalBalance);
                summary.TotalEntitlementBalance = NormalizeDecimal(prorate);
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating legacy leave summary for {EmpCode}", empCode);
                return new LeaveSummaryVm { Employee = await GetEmployeeInfoAsync(empCode) };
            }
        }

        public async Task<LeaveApplicationSaveResult> ApplyLeaveAsync(LeaveApplicationSaveRequest request, string empCode)
        {
            try
            {
                var summary = await GetEmployeeLeaveSummaryAsync(empCode);
                var leaveType = NormalizeLegacyLeaveType(request.LeaveTypeName ?? request.LeaveTypeId ?? string.Empty);
                var fromDate = request.FromDate?.Date ?? DateTime.MinValue;
                var toDate = request.ToDate?.Date ?? DateTime.MinValue;
                var today = DateTime.Today;
                var timeline = (fromDate - today).Days;
                var backDate = fromDate < today ? "Y" : "N";
                var leaveDays = request.LeaveDays;
                var leaveTime = request.IsHalfDay ? (request.HalfDayLeaveText ?? string.Empty) : string.Empty;
                var reasonText = request.ReasonText ?? string.Empty;

                if (string.Equals(leaveType, "Medical", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(reasonText) && !string.IsNullOrWhiteSpace(request.ReasonId))
                {
                    reasonText = (await GetReasonsAsync()).FirstOrDefault(x => string.Equals(x.Id, request.ReasonId, StringComparison.OrdinalIgnoreCase))?.Text ?? string.Empty;
                }

                var validation = ValidateUsingLegacyRules(request, summary, leaveType, timeline, leaveTime);
                if (!string.IsNullOrWhiteSpace(validation))
                {
                    return new LeaveApplicationSaveResult { Success = false, Message = validation };
                }

                var appNo = request.AppNo ?? await GetNextAppNoAsync();
                var workFor = leaveDays;
                var noCarryForwardDeduction = 0m;
                var carryForwardFlag = "N";
                decimal balanceCarryForwardAfterSave = summary.CarryForwardBalance;

                if (IsAnnualBucket(leaveType))
                {
                    carryForwardFlag = "Y"; // Existing page sets Session(cancfwd) = Y for leave year.
                    if (leaveDays >= summary.CarryForwardBalance)
                    {
                        workFor = leaveDays - summary.CarryForwardBalance;
                        noCarryForwardDeduction = summary.CarryForwardBalance;
                        balanceCarryForwardAfterSave = 0;
                    }
                    else
                    {
                        workFor = 0;
                        noCarryForwardDeduction = leaveDays;
                        balanceCarryForwardAfterSave = summary.CarryForwardBalance - leaveDays;
                    }
                }

                await TryDisableLegacySmsLinkTriggersAsync();

                if (request.AppNo.HasValue)
                {
                    await ExecuteLiveHrmisNonQueryAsync("dbo.HRMIS_UpdLeave_new",
                        new SqlParameter("@appno", appNo),
                        new SqlParameter("@ecode", empCode),
                        new SqlParameter("@adate", DateTime.Now),
                        new SqlParameter("@totdays", Convert.ToDouble(leaveDays)),
                        new SqlParameter("@days", Convert.ToDouble(workFor)),
                        new SqlParameter("@fdate", fromDate),
                        new SqlParameter("@tdate", toDate),
                        new SqlParameter("@ltype", leaveType),
                        new SqlParameter("@reason", reasonText),
                        new SqlParameter("@ltime", (object?)leaveTime ?? DBNull.Value),
                        new SqlParameter("@carry", carryForwardFlag),
                        new SqlParameter("@nocf", Convert.ToDouble(noCarryForwardDeduction)),
                        new SqlParameter("@stat", "SCHEDULED"),
                        new SqlParameter("@bk", backDate)
                    );
                }
                else
                {
                    await ExecuteLiveHrmisNonQueryAsync("dbo.HRMIS_InsLeavenew",
                        new SqlParameter("@ecode", empCode),
                        new SqlParameter("@adate", DateTime.Now),
                        new SqlParameter("@totdays", Convert.ToDouble(leaveDays)),
                        new SqlParameter("@days", Convert.ToDouble(workFor)),
                        new SqlParameter("@fdate", fromDate),
                        new SqlParameter("@tdate", toDate),
                        new SqlParameter("@ltype", leaveType),
                        new SqlParameter("@reason", reasonText),
                        new SqlParameter("@ltime", (object?)leaveTime ?? DBNull.Value),
                        new SqlParameter("@carry", carryForwardFlag),
                        new SqlParameter("@nocf", Convert.ToDouble(noCarryForwardDeduction)),
                        new SqlParameter("@backdate", backDate),
                        new SqlParameter("@appno", appNo),
                        new SqlParameter("@sts", "SCHEDULED"),
                        new SqlParameter("@PIC1", (object?)request.PersonInCharge1 ?? DBNull.Value),
                        new SqlParameter("@PIC11", (object?)request.PersonInCharge1Name ?? DBNull.Value),
                        new SqlParameter("@PIC2", (object?)request.PersonInCharge2 ?? DBNull.Value),
                        new SqlParameter("@PIC22", (object?)request.PersonInCharge2Name ?? DBNull.Value),
                        new SqlParameter("@AnnualBal", summary.AnnualBalance.ToString(CultureInfo.InvariantCulture)),
                        new SqlParameter("@MedicalBal", summary.MedicalBalance.ToString(CultureInfo.InvariantCulture))
                    );
                }

                if (IsAnnualBucket(leaveType) && string.Equals(carryForwardFlag, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    await ExecuteLiveHrmisNonQueryAsync("dbo.usp_Leave_UpdateCarryForwardLegacy",
                        new SqlParameter("@EmpCode", empCode),
                        new SqlParameter("@ForTheYear", DateTime.Today.Year),
                        new SqlParameter("@Remain", balanceCarryForwardAfterSave));
                }

                return new LeaveApplicationSaveResult
                {
                    Success = true,
                    AppNo = appNo,
                    Message = request.AppNo.HasValue ? "Leave has been updated successfully." : $"Leave has been scheduled successfully. App No: {appNo}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving leave application using legacy Leaveform tables");
                return new LeaveApplicationSaveResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<LeaveSelfStatusListResult> GetSelfStatusAsync(LeaveSelfStatusSearchRequest request, string empCode)
        {
            var result = new LeaveSelfStatusListResult();
            var data = await ExecuteLiveHrmisDataSetAsync("dbo.usp_Leave_GetSelfStatus",
                new SqlParameter("@EmpCode", empCode),
                new SqlParameter("@GlobalSearch", (object?)request.GlobalSearch ?? DBNull.Value),
                new SqlParameter("@AppNo", (object?)request.AppNo ?? DBNull.Value),
                new SqlParameter("@LeaveType", (object?)request.LeaveType ?? DBNull.Value),
                new SqlParameter("@Status", (object?)request.Status ?? DBNull.Value),
                new SqlParameter("@PageNumber", request.PageNumber <= 0 ? 1 : request.PageNumber),
                new SqlParameter("@PageSize", request.PageSize <= 0 ? 10 : request.PageSize));

            if (data.Tables.Count > 0)
            {
                foreach (DataRow row in data.Tables[0].Rows)
                {
                    result.Data.Add(new LeaveSelfStatusRowVm
                    {
                        AppNo = FieldInt(row, "AppNo", "appno"),
                        ApplicationDate = FieldDate(row, "ApplicationDate", "applicationdate") ?? DateTime.MinValue,
                        LeaveDays = FieldDecimal(row, "LeaveDays", "days"),
                        FromDate = FieldDate(row, "FromDate", "fromdate") ?? DateTime.MinValue,
                        ToDate = FieldDate(row, "ToDate", "todate") ?? DateTime.MinValue,
                        LeaveTypeName = FieldString(row, "LeaveTypeName", "leavetype"),
                        ReasonText = FieldString(row, "ReasonText", "reason"),
                        Status = FieldString(row, "Status", "status"),
                        StatusReason = FieldString(row, "StatusReason", "statusreason"),
                        ApprovedBy = FieldString(row, "ApprovedBy", "approvedby"),
                        ApprovedDate = FieldDate(row, "ApprovedDate", "approveddate"),
                        HalfDayLeaveText = FieldString(row, "HalfDayLeaveText", "leavetime"),
                        PersonInCharge1Name = FieldString(row, "PersonInCharge1Name", "pic11"),
                        PersonInCharge2Name = FieldString(row, "PersonInCharge2Name", "pic22")
                    });
                }
            }
            if (data.Tables.Count > 1 && data.Tables[1].Rows.Count > 0)
            {
                result.TotalCount = FieldInt(data.Tables[1].Rows[0], "TotalCount");
            }
            return result;
        }

        public async Task<LeaveApplicationSaveResult> CancelLeaveAsync(int appNo, string empCode)
        {
            try
            {
                await ExecuteLiveHrmisNonQueryAsync("dbo.usp_Leave_Cancel", new SqlParameter("@AppNo", appNo), new SqlParameter("@EmpCode", empCode));
                return new LeaveApplicationSaveResult { Success = true, Message = "Leave cancelled successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling legacy leave application");
                return new LeaveApplicationSaveResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<LeaveDaysCalculationResult> CalculateLeaveDaysAsync(LeaveDaysCalculationRequest request, string empCode)
        {
            try
            {
                if (!request.FromDate.HasValue || !request.ToDate.HasValue)
                    return new LeaveDaysCalculationResult { Success = false, Message = "Please select Leave Period.", LeaveDays = 0 };
                if (request.FromDate.Value.Date > request.ToDate.Value.Date)
                    return new LeaveDaysCalculationResult { Success = false, Message = "Please check the selected Leave Period dates.", LeaveDays = 0 };
                if (request.IsHalfDay && request.FromDate.Value.Date != request.ToDate.Value.Date)
                    return new LeaveDaysCalculationResult { Success = false, Message = "Half Day Leave Date cannot be more than one day.", LeaveDays = 0 };

                var table = await ExecuteLiveHrmisTableAsync("dbo.usp_Leave_CalculateDaysLegacy",
                    new SqlParameter("@EmpCode", empCode),
                    new SqlParameter("@FromDate", request.FromDate.Value.Date),
                    new SqlParameter("@ToDate", request.ToDate.Value.Date),
                    new SqlParameter("@IsHalfDay", request.IsHalfDay));

                var days = table.Rows.Count > 0 ? FieldDecimal(table.Rows[0], "LeaveDays") : (request.IsHalfDay ? 0.5m : (decimal)(request.ToDate.Value.Date - request.FromDate.Value.Date).TotalDays + 1);
                return new LeaveDaysCalculationResult { Success = days > 0, Message = days > 0 ? "Leave days calculated." : "Selected leave period contains no payable leave day.", LeaveDays = days };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating leave days");
                return new LeaveDaysCalculationResult { Success = false, Message = ex.Message, LeaveDays = 0 };
            }
        }

        public async Task<LeaveApplicationEditVm?> GetLeaveApplicationForEditAsync(int appNo, string empCode)
        {
            var table = await ExecuteLiveHrmisTableAsync("dbo.usp_Leave_GetForEdit", new SqlParameter("@AppNo", appNo), new SqlParameter("@EmpCode", empCode));
            if (table.Rows.Count == 0) return null;
            var row = table.Rows[0];
            var leaveTypeName = FieldString(row, "LeaveTypeName", "leavetype");
            var halfDayText = FieldString(row, "HalfDayLeaveText", "leavetime");
            var isHalfDay = FieldDecimal(row, "LeaveDays", "days") == 0.5m || !string.IsNullOrWhiteSpace(halfDayText);
            var leaveTypes = await GetLeaveTypesAsync();
            var reasons = await GetReasonsAsync();
            return new LeaveApplicationEditVm
            {
                AppNo = FieldInt(row, "AppNo", "appno"),
                LeaveTypeId = leaveTypes.FirstOrDefault(x => NormalizeLegacyLeaveType(x.Text) == NormalizeLegacyLeaveType(leaveTypeName))?.Id ?? leaveTypeName,
                LeaveTypeName = leaveTypeName,
                FromDate = FieldDate(row, "FromDate", "fromdate") ?? DateTime.MinValue,
                ToDate = FieldDate(row, "ToDate", "todate") ?? DateTime.MinValue,
                LeaveDays = FieldDecimal(row, "LeaveDays", "days"),
                IsHalfDay = isHalfDay,
                HalfDayLeaveId = GetHalfDayIdByText(await GetHalfDayLeavesAsync(), halfDayText),
                HalfDayLeaveText = halfDayText,
                ReasonId = reasons.FirstOrDefault(x => string.Equals(x.Text, FieldString(row, "ReasonText", "reason"), StringComparison.OrdinalIgnoreCase))?.Id,
                ReasonText = FieldString(row, "ReasonText", "reason"),
                PersonInCharge1 = FieldString(row, "PersonInCharge1", "pic1"),
                PersonInCharge1Name = FieldString(row, "PersonInCharge1Name", "pic11"),
                PersonInCharge2 = FieldString(row, "PersonInCharge2", "pic2"),
                PersonInCharge2Name = FieldString(row, "PersonInCharge2Name", "pic22"),
                Status = FieldString(row, "Status", "status")
            };
        }

        private async Task<LeaveEmployeeInfoVm> GetEmployeeInfoAsync(string empCode)
        {
            var table = await ExecuteLiveHrmisTableAsync("dbo.usp_Leave_GetEmployeeBasicLegacy", new SqlParameter("@EmpCode", empCode));
            if (table.Rows.Count == 0) return new LeaveEmployeeInfoVm { EmpCode = empCode };
            var row = table.Rows[0];
            var designation = FieldString(row, "Designation", "designation");
            return new LeaveEmployeeInfoVm
            {
                EmpCode = FieldString(row, "EmpCode", "empCode") == string.Empty ? empCode : FieldString(row, "EmpCode", "empCode"),
                EmpName = FieldString(row, "EmpName", "empName"),
                Designation = designation,
                Department = FieldString(row, "Department", "department", "departmentcode"),
                SubDepartment = FieldString(row, "SubDepartment", "subDepartment", "subdepartmentcode"),
                Section = FieldString(row, "Section", "section", "sectioncode"),
                DateOfJoin = FieldDate(row, "DateOfService", "dateOfService") ?? FieldDate(row, "DateOfJoin", "dateOfJoin"),
                IsOperator = string.Equals(designation?.Trim(), "Operator", StringComparison.OrdinalIgnoreCase) || FieldString(row, "IsOperator", "isOperator").Equals("Y", StringComparison.OrdinalIgnoreCase)
            };
        }

        private async Task<(decimal Total, decimal Balance)> GetCarryForwardAsync(string empCode, int currentYear, DateTime fiscalStart, DateTime fiscalEnd, DateTime doj, decimal tmpAnnual)
        {
            var cfTable = await ExecuteLiveHrmisTableAsync("dbo.usp_Leave_GetCarryForwardLegacy", new SqlParameter("@EmpCode", empCode), new SqlParameter("@ForTheYear", currentYear));
            if (cfTable.Rows.Count > 0)
            {
                var row = cfTable.Rows[0];
                var total = FieldDecimal(row, "cfwd", "leaveremain");
                var balance = FieldDecimal(row, "remain");
                if (total < 0) total = 0;
                if (balance < 0) balance = 0;
                return (total, balance);
            }

            var previousStart = fiscalStart.AddYears(-1);
            var previousEnd = fiscalEnd.AddYears(-1);
            decimal exp = (decimal)(previousEnd - doj).TotalDays;
            exp = exp / 30m;
            exp = Math.Round(exp / 12m, 1);
            int expLevel = GetLegacyExperienceLevel(exp);
            var previousLevel = await GetLeaveLevelAsync(empCode, expLevel);
            decimal lastYearAnnualFull = previousLevel.Annual1 > 0 ? previousLevel.Annual1 : (previousLevel.Annual > 0 ? previousLevel.Annual : tmpAnnual);
            decimal utilised;
            var totalLeaveTable = await ExecuteLiveHrmisTableAsync("dbo.HRMIS_GetTotalLeave", new SqlParameter("@ecode", empCode), new SqlParameter("@from", previousStart), new SqlParameter("@to", previousEnd));
            if (totalLeaveTable.Rows.Count > 0)
            {
                var value = GetRaw(totalLeaveTable.Rows[0], "leavetaken", "LeaveTaken");
                utilised = value == null || value == DBNull.Value ? lastYearAnnualFull : ToDecimal(value);
            }
            else
            {
                utilised = 0;
            }

            decimal balanceLeave = lastYearAnnualFull - utilised;
            if (balanceLeave <= 0) balanceLeave = 0;
            if ((previousEnd - doj).TotalDays < 365) balanceLeave = 0;

            await ExecuteLiveHrmisNonQueryAsync("dbo.usp_Leave_InsertCarryForwardLegacy",
                new SqlParameter("@EmpCode", empCode),
                new SqlParameter("@LeaveRemain", balanceLeave),
                new SqlParameter("@ForTheYear", currentYear),
                new SqlParameter("@Remain", balanceLeave),
                new SqlParameter("@Cfwd", balanceLeave));

            return (balanceLeave, balanceLeave);
        }

        private async Task<LegacyLeaveLevel> GetLeaveLevelAsync(string empCode, int expLevel)
        {
            var table = await ExecuteLiveHrmisTableAsync("dbo.HRMIS_getleavelevel", new SqlParameter("@ecode", empCode), new SqlParameter("@exp", expLevel));
            if (table.Rows.Count == 0) return new LegacyLeaveLevel();
            var row = table.Rows[0];
            return new LegacyLeaveLevel
            {
                Annual = FieldDecimal(row, "annual", "Annual"),
                Annual1 = FieldDecimal(row, "annual1", "Annual1", "annual"),
                Medical = FieldDecimal(row, "medical", "Medical"),
                Probation = FieldDecimal(row, "probation", "Probation")
            };
        }

        private async Task<decimal> GetAnnualTakenUsingLegacyProcedureAsync(DateTime fiscalStart, DateTime fiscalEnd, string empCode)
        {
            var table = await ExecuteLiveHrmisTableAsync("dbo.HRMIS_GetAnnual",
                new SqlParameter("@from", fiscalStart),
                new SqlParameter("@to", fiscalEnd),
                new SqlParameter("@ecode", empCode));
            if (table.Rows.Count == 0) return 0;
            return FieldDecimal(table.Rows[0], "annual", "Annual");
        }

        private async Task<decimal> GetMedicalTakenUsingLegacyProcedureAsync(DateTime fiscalStart, DateTime fiscalEnd, string empCode)
        {
            var table = await ExecuteLiveHrmisTableAsync("dbo.HRMIS_GetMedical",
                new SqlParameter("@from", fiscalStart),
                new SqlParameter("@to", fiscalEnd),
                new SqlParameter("@ecode", empCode));
            if (table.Rows.Count == 0) return 0;
            return FieldDecimal(table.Rows[0], "medical", "Medical");
        }

        private async Task<int> GetNextAppNoAsync()
        {
            var table = await ExecuteLiveHrmisTableAsync("dbo.usp_Leave_GetNextAppNoLegacy");
            if (table.Rows.Count == 0) return 1;
            var maxApp = FieldInt(table.Rows[0], "NextAppNo");
            return maxApp <= 0 ? 1 : maxApp;
        }

        private static string ValidateUsingLegacyRules(LeaveApplicationSaveRequest request, LeaveSummaryVm summary, string leaveType, int timeline, string leaveTime)
        {
            if (string.IsNullOrWhiteSpace(leaveType)) return "Please Select Valid Leave Type!";
            if (!request.FromDate.HasValue) return "Please Select Leave (From)Date!";
            if (!request.ToDate.HasValue) return "Please Select Leave (To)Date!";
            if (request.FromDate.Value.Date > request.ToDate.Value.Date) return "Please Check the date selected.";
            if (request.LeaveDays < 0.5m) return leaveType == "Annual" ? "Leave should not apply below half day" : "Leave should not below half day";
            if ((request.LeaveDays * 10) % 5 != 0) return "Pls. Check No. Of days applied";
            if (request.LeaveDays == 0.5m && request.FromDate.Value.Date != request.ToDate.Value.Date) return "Half Day Leave Date cannot be more than one day";
            if (request.LeaveDays == 0.5m && string.IsNullOrWhiteSpace(leaveTime)) return "Please Select Leave timing";

            if (leaveType == "Annual")
            {
                if (request.LeaveDays > summary.TotalEntitlementBalance) return "No.of Days applied is greater than total entitlement balance";
                if (request.LeaveDays >= 0.5m && request.LeaveDays < 1m && timeline < 1) return "Leave should apply before one day";
                if (request.LeaveDays == 1m && timeline < 3) return "Leave should apply before three day";
                if (request.LeaveDays > 1m && timeline < 7) return "Leave should apply before seven day";
            }
            else if (leaveType == "Medical")
            {
                if (request.LeaveDays > summary.MedicalBalance || request.LeaveDays > summary.MedicalEntitlement) return "Cannot Apply!!Leave Applied is more than available Medical Leave";
            }
            else if (leaveType == "Emergency" || leaveType == "PlanEmergency")
            {
                if (request.LeaveDays > summary.TotalEntitlementBalance) return "No.of Days applied is greater than  total entitlement balance";
            }
            return string.Empty;
        }


        private async Task TryDisableLegacySmsLinkTriggersAsync()
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.leaveform') IS NOT NULL
BEGIN
    DECLARE @sql NVARCHAR(MAX) = N'';

    SELECT @sql = @sql + N'DISABLE TRIGGER ' + QUOTENAME(t.name) + N' ON dbo.leaveform;' + CHAR(13) + CHAR(10)
    FROM sys.triggers t
    INNER JOIN sys.sql_modules m ON t.object_id = m.object_id
    WHERE t.parent_id = OBJECT_ID(N'dbo.leaveform')
      AND t.is_disabled = 0
      AND (
            m.definition LIKE N'%sms.dbo.SMS_LINK%'
         OR m.definition LIKE N'%[sms].[dbo].[SMS_LINK]%'
         OR m.definition LIKE N'%SMS_LINK%'
      );

    IF (@sql <> N'')
        EXEC sys.sp_executesql @sql;
END";

            try
            {
                await using var con = CreateLiveHrmisConnection();
                await con.OpenAsync();
                await using var cmd = new SqlCommand(sql, con) { CommandType = CommandType.Text, CommandTimeout = 300 };
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to disable legacy SMS_LINK triggers before saving leave. Continuing with HRMIS stored procedure call.");
            }
        }

        private async Task<DataTable> ExecuteLiveHrmisTableAsync(string storedProcedure, params SqlParameter[] parameters)
        {
            var ds = await ExecuteLiveHrmisDataSetAsync(storedProcedure, parameters);
            return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
        }

        private async Task<DataSet> ExecuteLiveHrmisDataSetAsync(string storedProcedure, params SqlParameter[] parameters)
        {
            var ds = new DataSet();
            await using var con = CreateLiveHrmisConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(storedProcedure, con) { CommandType = CommandType.StoredProcedure, CommandTimeout = 300 };
            if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);

            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(ds);
            return ds;
        }

        private async Task ExecuteLiveHrmisNonQueryAsync(string storedProcedure, params SqlParameter[] parameters)
        {
            await using var con = CreateLiveHrmisConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(storedProcedure, con) { CommandType = CommandType.StoredProcedure, CommandTimeout = 300 };
            if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<DataTable> ExecuteTableAsync(string storedProcedure, params SqlParameter[] parameters)
        {
            var ds = await ExecuteDataSetAsync(storedProcedure, parameters);
            return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
        }

        private async Task<DataSet> ExecuteDataSetAsync(string storedProcedure, params SqlParameter[] parameters)
        {
            var ds = new DataSet();
            await using var con = CreateConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(storedProcedure, con) { CommandType = CommandType.StoredProcedure, CommandTimeout = 300 };
            if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);

            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(ds);
            return ds;
        }

        private async Task ExecuteNonQueryAsync(string storedProcedure, params SqlParameter[] parameters)
        {
            await using var con = CreateConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand(storedProcedure, con) { CommandType = CommandType.StoredProcedure, CommandTimeout = 300 };
            if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
            await cmd.ExecuteNonQueryAsync();
        }

        private static int GetLegacyExperienceLevel(decimal expYearsRounded)
        {
            if (expYearsRounded < 2m) return 2;
            if (expYearsRounded >= 2m && expYearsRounded < 5m) return 3;
            return 5;
        }

        private static bool IsAnnualBucket(string leaveType) => leaveType == "Annual" || leaveType == "Emergency" || leaveType == "PlanEmergency";

        private static string NormalizeLegacyLeaveType(string leaveType)
        {
            var t = (leaveType ?? string.Empty).Trim();
            var lower = t.ToLowerInvariant().Replace(" ", string.Empty).Replace("-", string.Empty);
            if (lower.Contains("planemergency") && lower.Contains("unpaid")) return "PlanEmergencyUP";
            if (lower.Contains("planemergency")) return "PlanEmergency";
            if (lower.Contains("emergency") && lower.Contains("unpaid")) return "EmergencyUP";
            if (lower.Contains("emergency") && lower.Contains("annual")) return "Emergency";
            if (lower.Contains("medical")) return "Medical";
            if (lower.Contains("annual")) return "Annual";
            if (lower.Contains("companyholiday")) return "CompanyHoliday";
            if (lower.Contains("hospitalization")) return "Hospitalization";
            if (lower.Contains("unpaid")) return "Unpaid";
            return t;
        }

        private static int? GetHalfDayIdByText(List<LeaveLookupVm> rows, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var match = rows.FirstOrDefault(x => string.Equals(x.Text, text, StringComparison.OrdinalIgnoreCase));
            return match != null && int.TryParse(match.Id, out var id) ? id : null;
        }

        private static decimal NormalizeDecimal(decimal value) => value < 0 ? 0 : value;
        private static decimal ToDecimal(object? value) => value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(Convert.ToString(value)) ? 0 : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        private static int ToInt(object? value) => value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(Convert.ToString(value)) ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);

        private static object? GetRaw(DataRow row, params string[] names)
        {
            foreach (var name in names)
            {
                if (row.Table.Columns.Contains(name)) return row[name];
            }
            return null;
        }

        private static string FieldString(DataRow row, params string[] names)
        {
            var value = GetRaw(row, names);
            return value == null || value == DBNull.Value ? string.Empty : Convert.ToString(value) ?? string.Empty;
        }

        private static decimal FieldDecimal(DataRow row, params string[] names) => ToDecimal(GetRaw(row, names));
        private static int FieldInt(DataRow row, params string[] names) => ToInt(GetRaw(row, names));
        private static DateTime? FieldDate(DataRow row, params string[] names)
        {
            var value = GetRaw(row, names);
            if (value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(Convert.ToString(value))) return null;
            return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        private sealed class LegacyLeaveLevel
        {
            public decimal Annual { get; set; }
            public decimal Annual1 { get; set; }
            public decimal Medical { get; set; }
            public decimal Probation { get; set; }
        }
    }
}
