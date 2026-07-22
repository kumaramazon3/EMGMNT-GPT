using Maruwa_Emgmt.InterFace.ER;
using Maruwa_Emgmt.Models.ER;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Maruwa_Emgmt.DAL.ER
{
    public class da_NatureOfGrievanceMaster : i_NatureOfGrievanceMaster
    {
        private readonly string _connectionString;
        private readonly ILogger<da_NatureOfGrievanceMaster> _logger;

        public da_NatureOfGrievanceMaster(IConfiguration configuration, ILogger<da_NatureOfGrievanceMaster> logger)
        {
            _connectionString = configuration.GetConnectionString("EHRMConnection") ?? throw new InvalidOperationException("EHRMConnection is missing in appsettings.json");
            _logger = logger;
        }

        public async Task<NatureOfGrievanceListResult> GetNatureOfGrievancesAsync(NatureOfGrievanceSearchRequest request)
        {
            var result = new NatureOfGrievanceListResult();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_NatureOfGrievanceMaster_GetPaged", con) { CommandType = CommandType.StoredProcedure };
            AddSearchParameters(cmd, request);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) result.Data.Add(MapNature(reader));
            if (await reader.NextResultAsync() && await reader.ReadAsync()) result.TotalCount = Convert.ToInt32(reader["TotalCount"]);
            return result;
        }

        public async Task<NatureOfGrievanceMasterVm?> GetNatureOfGrievanceByIdAsync(int natureId)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_NatureOfGrievanceMaster_GetById", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@NatureID", natureId);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapNature(reader) : null;
        }

        public async Task<(bool Success, string Message)> SaveNatureOfGrievanceAsync(NatureOfGrievanceMasterVm model, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_NatureOfGrievanceMaster_Save", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@NatureID", model.NatureID);
                cmd.Parameters.AddWithValue("@NatureName", model.NatureName.Trim());
                cmd.Parameters.AddWithValue("@IsOther", model.IsOther);
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Nature of Grievance saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving Nature of Grievance");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteNatureOfGrievanceAsync(int natureId, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_NatureOfGrievanceMaster_Delete", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@NatureID", natureId);
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Nature of Grievance deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting Nature of Grievance");
                return (false, ex.Message);
            }
        }

        public async Task<List<NatureOfGrievanceMasterVm>> GetNatureOfGrievancesForExportAsync(NatureOfGrievanceSearchRequest request)
        {
            request.PageNumber = 1;
            request.PageSize = 0;
            return (await GetNatureOfGrievancesAsync(request)).Data;
        }

        public async Task<List<NatureOfGrievanceMasterVm>> SearchActiveNatureOfGrievanceLookupAsync(string? searchText)
        {
            var list = new List<NatureOfGrievanceMasterVm>();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_NatureOfGrievanceMaster_SearchActive", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@SearchText", DbValue(searchText));
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(MapNature(reader));
            return list;
        }

        private static void AddSearchParameters(SqlCommand cmd, NatureOfGrievanceSearchRequest request)
        {
            cmd.Parameters.AddWithValue("@GlobalSearch", DbValue(request.GlobalSearch));
            cmd.Parameters.AddWithValue("@NatureName", DbValue(request.NatureName));
            cmd.Parameters.AddWithValue("@IsOther", DbValue(request.IsOther));
            cmd.Parameters.AddWithValue("@CreatedBy", DbValue(request.CreatedBy));
            cmd.Parameters.AddWithValue("@EditedBy", DbValue(request.EditedBy));
            cmd.Parameters.AddWithValue("@isActive", DbValue(request.isActive));
            cmd.Parameters.AddWithValue("@SortColumn", string.IsNullOrWhiteSpace(request.SortColumn) ? "NatureName" : request.SortColumn);
            cmd.Parameters.AddWithValue("@SortDirection", string.Equals(request.SortDirection, "DESC", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC");
            cmd.Parameters.AddWithValue("@PageNumber", request.PageNumber <= 0 ? 1 : request.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize < 0 ? 10 : request.PageSize);
        }

        private static object DbValue(string? value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        private static string Value(IDataRecord reader, string name) => reader[name] == DBNull.Value ? string.Empty : Convert.ToString(reader[name]) ?? string.Empty;
        private static DateTime? NullableDate(IDataRecord reader, string name) => reader[name] == DBNull.Value ? null : Convert.ToDateTime(reader[name]);
        private static bool ToBool(object value) => value != DBNull.Value && Convert.ToBoolean(value);

        private static NatureOfGrievanceMasterVm MapNature(IDataRecord reader) => new()
        {
            NatureID = Convert.ToInt32(reader["NatureID"]),
            NatureName = Value(reader, "NatureName"),
            IsOther = ToBool(reader["IsOther"]),
            CreatedBy = Value(reader, "CreatedBy"),
            CreatedOn = NullableDate(reader, "CreatedOn"),
            EditedBy = Value(reader, "EditedBy"),
            EditedOn = NullableDate(reader, "EditedOn"),
            isActive = ToBool(reader["isActive"])
        };
    }
}
