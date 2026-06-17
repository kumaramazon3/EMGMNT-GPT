using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models.master;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Maruwa_Emgmt.DAL.master
{
    public class da_LeaveTypeMaster : i_LeaveTypeMaster
    {
        private readonly string _connectionString;
        private readonly ILogger<da_LeaveTypeMaster> _logger;

        public da_LeaveTypeMaster(IConfiguration configuration, ILogger<da_LeaveTypeMaster> logger)
        {
            _connectionString = configuration.GetConnectionString("EHRMConnection") ?? throw new InvalidOperationException("EHRMConnection is missing in appsettings.json");
            _logger = logger;
        }

        public async Task<LeaveTypeListResult> GetLeaveTypesAsync(LeaveTypeSearchRequest request)
        {
            var result = new LeaveTypeListResult();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_LeaveTypeMaster_GetPaged", con) { CommandType = CommandType.StoredProcedure };
            AddSearchParameters(cmd, request);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            result.Data = ReadLeaveTypeList(reader);
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                result.TotalCount = Convert.ToInt32(reader["TotalCount"]);
            }
            return result;
        }

        public async Task<LeaveTypeMasterVm?> GetLeaveTypeByIdAsync(string leaveId)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_LeaveTypeMaster_GetById", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@LeaveID", leaveId);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapLeaveType(reader) : null;
        }

        public async Task<(bool Success, string Message)> SaveLeaveTypeAsync(LeaveTypeMasterVm model, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_LeaveTypeMaster_Save", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@LeaveID", model.LeaveID.Trim());
                cmd.Parameters.AddWithValue("@LeaveType", model.LeaveType.Trim());
                cmd.Parameters.AddWithValue("@LeaveDescription", model.LeaveDescription.Trim());
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 250) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "LeaveType saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving LeaveType");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteLeaveTypeAsync(string leaveId, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_LeaveTypeMaster_Delete", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 250) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "LeaveType deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting LeaveType");
                return (false, ex.Message);
            }
        }

        public async Task<List<LeaveTypeMasterVm>> GetLeaveTypesForExportAsync(LeaveTypeSearchRequest request)
        {
            request.PageNumber = 1;
            request.PageSize = 0;
            var result = await GetLeaveTypesAsync(request);
            return result.Data;
        }

        private static void AddSearchParameters(SqlCommand cmd, LeaveTypeSearchRequest request)
        {
            cmd.Parameters.AddWithValue("@GlobalSearch", (object?)request.GlobalSearch ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeaveID", (object?)request.LeaveID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeaveType", (object?)request.LeaveType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LeaveDescription", (object?)request.LeaveDescription ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)request.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EditedBy", (object?)request.EditedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@isActive", (object?)request.isActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SortColumn", request.SortColumn);
            cmd.Parameters.AddWithValue("@SortDirection", request.SortDirection);
            cmd.Parameters.AddWithValue("@PageNumber", request.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize);
        }

        private static List<LeaveTypeMasterVm> ReadLeaveTypeList(SqlDataReader reader)
        {
            var list = new List<LeaveTypeMasterVm>();
            while (reader.Read()) list.Add(MapLeaveType(reader));
            return list;
        }

        private static LeaveTypeMasterVm MapLeaveType(IDataRecord reader)
        {
            return new LeaveTypeMasterVm
            {
                LeaveID = Convert.ToString(reader["LeaveID"]) ?? string.Empty,
                LeaveType = Convert.ToString(reader["LeaveType"]) ?? string.Empty,
                LeaveDescription = Convert.ToString(reader["LeaveDescription"]) ?? string.Empty,
                CreatedBy = Convert.ToString(reader["CreatedBy"]),
                CreatedOn = reader["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["CreatedOn"]),
                EditedBy = Convert.ToString(reader["EditedBy"]),
                EditedOn = reader["EditedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["EditedOn"]),
                isActive = reader["isActive"] != DBNull.Value && Convert.ToBoolean(reader["isActive"])
            };
        }
    }
}
