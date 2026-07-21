using Maruwa_Emgmt.InterFace.Leave;
using Maruwa_Emgmt.Models.Leave;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Maruwa_Emgmt.DAL.Leave
{
    public class da_LeaveApplication : i_LeaveApplication
    {
        private readonly string _connectionString;
        private readonly ILogger<da_LeaveApplication> _logger;

        public da_LeaveApplication(IConfiguration configuration, ILogger<da_LeaveApplication> logger)
        {
            _connectionString = configuration.GetConnectionString("EHRMConnection") ?? throw new InvalidOperationException("EHRMConnection is missing.");
            _logger = logger;
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<LeaveApplicationPageDataVm> GetLeaveApplicationPageDataAsync(string empCode)
        {
            var page = new LeaveApplicationPageDataVm();
            await using var con = CreateConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("usp_Leave_GetApplicationPageData", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@EmpCode", empCode);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync()) page.Summary = MapSummary(reader);

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync()) page.LeaveTypes.Add(new LeaveLookupVm
                {
                    Id = Convert.ToString(reader["LeaveID"]) ?? string.Empty,
                    Text = Convert.ToString(reader["LeaveType"]) ?? string.Empty,
                    Code = Convert.ToString(reader["LeaveID"]),
                    Remark = Convert.ToString(reader["Remark"])
                });
            }
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync()) page.HalfDayLeaves.Add(new LeaveLookupVm
                {
                    Id = Convert.ToString(reader["HalfDayLeaveID"]) ?? string.Empty,
                    Text = Convert.ToString(reader["TimeText"]) ?? string.Empty
                });
            }
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync()) page.Reasons.Add(new LeaveLookupVm
                {
                    Id = Convert.ToString(reader["ReasonID"]) ?? string.Empty,
                    Text = Convert.ToString(reader["ReasonDescription"]) ?? string.Empty
                });
            }
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync()) page.PersonsInCharge.Add(new LeaveLookupVm
                {
                    Id = Convert.ToString(reader["EmpCode"]) ?? string.Empty,
                    Text = Convert.ToString(reader["EmpDisplay"]) ?? string.Empty
                });
            }
            return page;
        }

        public async Task<List<LeaveLookupVm>> GetLeaveTypesAsync()
        {
            var list = new List<LeaveLookupVm>();
            await using var con = CreateConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("usp_Leave_GetLeaveTypes", con) { CommandType = CommandType.StoredProcedure };
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(new LeaveLookupVm { Id = Convert.ToString(reader["LeaveID"]) ?? string.Empty, Text = Convert.ToString(reader["LeaveType"]) ?? string.Empty, Code = Convert.ToString(reader["LeaveID"]), Remark = Convert.ToString(reader["Remark"]) });
            return list;
        }

        public async Task<List<LeaveLookupVm>> GetHalfDayLeavesAsync()
        {
            var list = new List<LeaveLookupVm>();
            await using var con = CreateConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("usp_Leave_GetHalfDayLeaves", con) { CommandType = CommandType.StoredProcedure };
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(new LeaveLookupVm { Id = Convert.ToString(reader["HalfDayLeaveID"]) ?? string.Empty, Text = Convert.ToString(reader["TimeText"]) ?? string.Empty });
            return list;
        }

        public async Task<List<LeaveLookupVm>> GetReasonsAsync()
        {
            var list = new List<LeaveLookupVm>();
            await using var con = CreateConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("usp_Leave_GetReasons", con) { CommandType = CommandType.StoredProcedure };
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(new LeaveLookupVm { Id = Convert.ToString(reader["ReasonID"]) ?? string.Empty, Text = Convert.ToString(reader["ReasonDescription"]) ?? string.Empty });
            return list;
        }

        public async Task<List<LeaveLookupVm>> GetPersonsInChargeAsync(string searchText)
        {
            var list = new List<LeaveLookupVm>();
            await using var con = CreateConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("usp_Leave_SearchEmployees", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@SearchText", (object?)searchText ?? DBNull.Value);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(new LeaveLookupVm { Id = Convert.ToString(reader["EmpCode"]) ?? string.Empty, Text = Convert.ToString(reader["EmpDisplay"]) ?? string.Empty });
            return list;
        }

        public async Task<LeaveSummaryVm> GetEmployeeLeaveSummaryAsync(string empCode)
        {
            await using var con = CreateConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("usp_Leave_GetEmployeeSummary", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@EmpCode", empCode);
            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapSummary(reader) : new LeaveSummaryVm();
        }

        public async Task<LeaveApplicationSaveResult> ApplyLeaveAsync(LeaveApplicationSaveRequest request, string empCode)
        {
            try
            {
                await using var con = CreateConnection();
                await con.OpenAsync();
                await using var cmd = new SqlCommand("usp_Leave_Apply", con) { CommandType = CommandType.StoredProcedure };
                AddApplyParameters(cmd, request, empCode);
                var appNo = new SqlParameter("@AppNo", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(appNo);
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await cmd.ExecuteNonQueryAsync();
                return new LeaveApplicationSaveResult
                {
                    Success = Convert.ToInt32(status.Value) == 1,
                    Message = Convert.ToString(message.Value) ?? "Leave application processed.",
                    AppNo = appNo.Value == DBNull.Value ? 0 : Convert.ToInt32(appNo.Value)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying leave");
                return new LeaveApplicationSaveResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<LeaveSelfStatusListResult> GetSelfStatusAsync(LeaveSelfStatusSearchRequest request, string empCode)
        {
            var result = new LeaveSelfStatusListResult();
            await using var con = CreateConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("usp_Leave_GetSelfStatus", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@EmpCode", empCode);
            cmd.Parameters.AddWithValue("@GlobalSearch", (object?)request.GlobalSearch ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AppNo", (object?)request.AppNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeaveType", (object?)request.LeaveType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)request.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PageNumber", request.PageNumber <= 0 ? 1 : request.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize <= 0 ? 10 : request.PageSize);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) result.Data.Add(MapStatusRow(reader));
            if (await reader.NextResultAsync() && await reader.ReadAsync()) result.TotalCount = Convert.ToInt32(reader["TotalCount"]);
            return result;
        }

        public async Task<LeaveApplicationSaveResult> CancelLeaveAsync(int appNo, string empCode)
        {
            try
            {
                await using var con = CreateConnection();
                await con.OpenAsync();
                await using var cmd = new SqlCommand("usp_Leave_Cancel", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@AppNo", appNo);
                cmd.Parameters.AddWithValue("@EmpCode", empCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await cmd.ExecuteNonQueryAsync();
                return new LeaveApplicationSaveResult { Success = Convert.ToInt32(status.Value) == 1, Message = Convert.ToString(message.Value) ?? "Leave cancelled." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling leave");
                return new LeaveApplicationSaveResult { Success = false, Message = ex.Message };
            }
        }



        public async Task<LeaveDaysCalculationResult> CalculateLeaveDaysAsync(LeaveDaysCalculationRequest request, string empCode)
        {
            try
            {
                await using var con = CreateConnection();
                await con.OpenAsync();
                await using var cmd = new SqlCommand("usp_Leave_CalculateDays", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@EmpCode", empCode);
                cmd.Parameters.AddWithValue("@FromDate", request.FromDate.HasValue ? (object)request.FromDate.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", request.ToDate.HasValue ? (object)request.ToDate.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@IsHalfDay", request.IsHalfDay);
                var leaveDays = new SqlParameter("@LeaveDays", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 };
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(leaveDays);
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await cmd.ExecuteNonQueryAsync();
                return new LeaveDaysCalculationResult
                {
                    Success = Convert.ToInt32(status.Value) == 1,
                    Message = Convert.ToString(message.Value) ?? string.Empty,
                    LeaveDays = leaveDays.Value == DBNull.Value ? 0 : Convert.ToDecimal(leaveDays.Value)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating leave days");
                return new LeaveDaysCalculationResult { Success = false, Message = ex.Message, LeaveDays = 0 };
            }
        }

        public async Task<LeaveApplicationEditVm?> GetLeaveApplicationForEditAsync(int appNo, string empCode)
        {
            await using var con = CreateConnection();
            await con.OpenAsync();
            await using var cmd = new SqlCommand("usp_Leave_GetForEdit", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@AppNo", appNo);
            cmd.Parameters.AddWithValue("@EmpCode", empCode);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            return new LeaveApplicationEditVm
            {
                AppNo = Convert.ToInt32(reader["AppNo"]),
                LeaveTypeId = Convert.ToString(reader["LeaveTypeID"]) ?? string.Empty,
                LeaveTypeName = Convert.ToString(reader["LeaveTypeName"]) ?? string.Empty,
                FromDate = Convert.ToDateTime(reader["FromDate"]),
                ToDate = Convert.ToDateTime(reader["ToDate"]),
                LeaveDays = ToDecimal(reader["LeaveDays"]),
                IsHalfDay = reader["IsHalfDay"] != DBNull.Value && Convert.ToBoolean(reader["IsHalfDay"]),
                HalfDayLeaveId = reader["HalfDayLeaveID"] == DBNull.Value ? null : Convert.ToInt32(reader["HalfDayLeaveID"]),
                HalfDayLeaveText = Convert.ToString(reader["HalfDayLeaveText"]),
                ReasonId = Convert.ToString(reader["ReasonID"]),
                ReasonText = Convert.ToString(reader["ReasonText"]),
                PersonInCharge1 = Convert.ToString(reader["PersonInCharge1"]),
                PersonInCharge1Name = Convert.ToString(reader["PersonInCharge1Name"]),
                PersonInCharge2 = Convert.ToString(reader["PersonInCharge2"]),
                PersonInCharge2Name = Convert.ToString(reader["PersonInCharge2Name"]),
                Status = Convert.ToString(reader["Status"]) ?? string.Empty
            };
        }

        private static void AddApplyParameters(SqlCommand cmd, LeaveApplicationSaveRequest request, string empCode)
        {
            cmd.Parameters.AddWithValue("@EmpCode", empCode);
            cmd.Parameters.AddWithValue("@LeaveTypeID", (object?)request.LeaveTypeId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeaveTypeName", (object?)request.LeaveTypeName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FromDate", request.FromDate.HasValue ? (object)request.FromDate.Value.Date : DBNull.Value);
            cmd.Parameters.AddWithValue("@ToDate", request.ToDate.HasValue ? (object)request.ToDate.Value.Date : DBNull.Value);
            cmd.Parameters.AddWithValue("@LeaveDays", request.LeaveDays);
            cmd.Parameters.AddWithValue("@IsHalfDay", request.IsHalfDay);
            cmd.Parameters.AddWithValue("@HalfDayLeaveID", (object?)request.HalfDayLeaveId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@HalfDayLeaveText", (object?)request.HalfDayLeaveText ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReasonID", (object?)request.ReasonId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReasonText", (object?)request.ReasonText ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PersonInCharge1", (object?)request.PersonInCharge1 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PersonInCharge1Name", (object?)request.PersonInCharge1Name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PersonInCharge2", (object?)request.PersonInCharge2 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PersonInCharge2Name", (object?)request.PersonInCharge2Name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AnnualBalance", request.AnnualBalance);
            cmd.Parameters.AddWithValue("@MedicalBalance", request.MedicalBalance);
            cmd.Parameters.AddWithValue("@CarryForwardBalance", request.CarryForwardBalance);
            cmd.Parameters.AddWithValue("@CreatedBy", empCode);
            cmd.Parameters.AddWithValue("@AppNoToUpdate", (object?)request.AppNo ?? DBNull.Value);
        }

        private static LeaveSummaryVm MapSummary(IDataRecord r)
        {
            return new LeaveSummaryVm
            {
                Employee = new LeaveEmployeeInfoVm
                {
                    EmpCode = Convert.ToString(r["EmpCode"]) ?? string.Empty,
                    EmpName = Convert.ToString(r["EmpName"]) ?? string.Empty,
                    Designation = Convert.ToString(r["Designation"]),
                    Department = Convert.ToString(r["Department"]),
                    SubDepartment = Convert.ToString(r["SubDepartment"]),
                    Section = Convert.ToString(r["Section"]),
                    DateOfJoin = r["DateOfJoin"] == DBNull.Value ? null : Convert.ToDateTime(r["DateOfJoin"]),
                    IsOperator = HasColumn(r, "IsOperator") && r["IsOperator"] != DBNull.Value && Convert.ToBoolean(r["IsOperator"])
                },
                CarryForwardTotal = ToDecimal(r["CarryForwardTotal"]),
                CarryForwardUtilised = ToDecimal(r["CarryForwardUtilised"]),
                CarryForwardBalance = ToDecimal(r["CarryForwardBalance"]),
                AnnualEntitlement = ToDecimal(r["AnnualEntitlement"]),
                AnnualUtilised = ToDecimal(r["AnnualUtilised"]),
                AnnualBalance = ToDecimal(r["AnnualBalance"]),
                MedicalEntitlement = ToDecimal(r["MedicalEntitlement"]),
                MedicalUtilised = ToDecimal(r["MedicalUtilised"]),
                MedicalBalance = ToDecimal(r["MedicalBalance"]),
                TotalEntitlementBalance = ToDecimal(r["TotalEntitlementBalance"])
            };
        }

        private static LeaveSelfStatusRowVm MapStatusRow(IDataRecord r)
        {
            return new LeaveSelfStatusRowVm
            {
                AppNo = Convert.ToInt32(r["AppNo"]),
                ApplicationDate = Convert.ToDateTime(r["ApplicationDate"]),
                LeaveDays = ToDecimal(r["LeaveDays"]),
                FromDate = Convert.ToDateTime(r["FromDate"]),
                ToDate = Convert.ToDateTime(r["ToDate"]),
                LeaveTypeName = Convert.ToString(r["LeaveTypeName"]) ?? string.Empty,
                ReasonText = Convert.ToString(r["ReasonText"]) ?? string.Empty,
                Status = Convert.ToString(r["Status"]) ?? string.Empty,
                StatusReason = Convert.ToString(r["StatusReason"]),
                ApprovedBy = Convert.ToString(r["ApprovedBy"]),
                ApprovedDate = r["ApprovedDate"] == DBNull.Value ? null : Convert.ToDateTime(r["ApprovedDate"]),
                HalfDayLeaveText = Convert.ToString(r["HalfDayLeaveText"]),
                PersonInCharge1Name = Convert.ToString(r["PersonInCharge1Name"]),
                PersonInCharge2Name = Convert.ToString(r["PersonInCharge2Name"])
            };
        }

        private static decimal ToDecimal(object value) => value == DBNull.Value || value == null ? 0 : Convert.ToDecimal(value);
        private static bool HasColumn(IDataRecord r, string columnName)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (string.Equals(r.GetName(i), columnName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}
