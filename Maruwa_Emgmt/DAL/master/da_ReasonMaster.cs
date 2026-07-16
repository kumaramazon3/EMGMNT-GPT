using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models.master;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Maruwa_Emgmt.DAL.master
{
    public class da_ReasonMaster : i_ReasonMaster
    {
        private readonly string _connectionString;
        private readonly ILogger<da_ReasonMaster> _logger;

        public da_ReasonMaster(IConfiguration configuration, ILogger<da_ReasonMaster> logger)
        {
            _connectionString = configuration.GetConnectionString("EHRMConnection") ?? throw new InvalidOperationException("EHRMConnection is missing in appsettings.json");
            _logger = logger;
        }

        public async Task<ReasonListResult> GetReasonsAsync(ReasonSearchRequest request)
        {
            var result = new ReasonListResult();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_ReasonTypeMaster_GetPaged", con) { CommandType = CommandType.StoredProcedure };
            AddSearchParameters(cmd, request);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            result.Data = ReadReasonList(reader);
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                result.TotalCount = Convert.ToInt32(reader["TotalCount"]);
            }
            return result;
        }

        public async Task<ReasonMasterVm?> GetReasonByIdAsync(string reasonId)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_ReasonTypeMaster_GetById", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ReasonID", reasonId);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapReason(reader) : null;
        }

        public async Task<(bool Success, string Message)> SaveReasonAsync(ReasonMasterVm model, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_ReasonTypeMaster_Save", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@ReasonID", string.IsNullOrWhiteSpace(model.ReasonID) ? DBNull.Value : model.ReasonID.Trim());
                cmd.Parameters.AddWithValue("@ReasonType", model.ReasonType.Trim());
                cmd.Parameters.AddWithValue("@ReasonDescription", model.ReasonDescription.Trim());
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 250) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Reason saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving Reason");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteReasonAsync(string reasonId, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_ReasonTypeMaster_Delete", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@ReasonID", reasonId);
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 250) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Reason deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting Reason");
                return (false, ex.Message);
            }
        }

        public async Task<List<ReasonMasterVm>> GetReasonsForExportAsync(ReasonSearchRequest request)
        {
            request.PageNumber = 1;
            request.PageSize = 0;
            var result = await GetReasonsAsync(request);
            return result.Data;
        }

        private static void AddSearchParameters(SqlCommand cmd, ReasonSearchRequest request)
        {
            cmd.Parameters.AddWithValue("@GlobalSearch", (object?)request.GlobalSearch ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReasonID", (object?)request.ReasonID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReasonType", (object?)request.ReasonType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReasonDescription", (object?)request.ReasonDescription ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)request.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EditedBy", (object?)request.EditedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@isActive", (object?)request.isActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SortColumn", request.SortColumn);
            cmd.Parameters.AddWithValue("@SortDirection", request.SortDirection);
            cmd.Parameters.AddWithValue("@PageNumber", request.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize);
        }

        private static List<ReasonMasterVm> ReadReasonList(SqlDataReader reader)
        {
            var list = new List<ReasonMasterVm>();
            while (reader.Read()) list.Add(MapReason(reader));
            return list;
        }

        private static ReasonMasterVm MapReason(IDataRecord reader)
        {
            return new ReasonMasterVm
            {
                ReasonID = Convert.ToString(reader["ReasonID"]) ?? string.Empty,
                ReasonType = Convert.ToString(reader["ReasonType"]) ?? string.Empty,
                ReasonDescription = Convert.ToString(reader["ReasonDescription"]) ?? string.Empty,
                CreatedBy = Convert.ToString(reader["CreatedBy"]),
                CreatedOn = reader["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["CreatedOn"]),
                EditedBy = Convert.ToString(reader["EditedBy"]),
                EditedOn = reader["EditedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["EditedOn"]),
                isActive = reader["isActive"] != DBNull.Value && Convert.ToBoolean(reader["isActive"])
            };
        }
    }
}
