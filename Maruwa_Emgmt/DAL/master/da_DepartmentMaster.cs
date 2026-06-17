using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models.master;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Maruwa_Emgmt.DAL.master
{
    public class da_DepartmentMaster : i_DepartmentMaster
    {
        private readonly string _connectionString;
        private readonly ILogger<da_DepartmentMaster> _logger;

        public da_DepartmentMaster(IConfiguration configuration, ILogger<da_DepartmentMaster> logger)
        {
            _connectionString = configuration.GetConnectionString("EHRMConnection") ?? throw new InvalidOperationException("EHRMConnection is missing in appsettings.json");
            _logger = logger;
        }

        public async Task<DepartmentListResult> GetDepartmentsAsync(DepartmentSearchRequest request)
        {
            var result = new DepartmentListResult();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_DepartmentMaster_GetPaged", con) { CommandType = CommandType.StoredProcedure };
            AddSearchParameters(cmd, request);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            result.Data = ReadDepartmentList(reader);
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                result.TotalCount = Convert.ToInt32(reader["TotalCount"]);
            }
            return result;
        }

        public async Task<DepartmentMasterVm?> GetDepartmentByIdAsync(int recordNo)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_DepartmentMaster_GetById", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@RecordNo", recordNo);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapDepartment(reader) : null;
        }

        public async Task<(bool Success, string Message)> SaveDepartmentAsync(DepartmentMasterVm model, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_DepartmentMaster_Save", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@RecordNo", model.RecordNo);
                cmd.Parameters.AddWithValue("@DepartmentCode", model.DepartmentCode.Trim());
                cmd.Parameters.AddWithValue("@DepartmentName", model.DepartmentName.Trim());
                cmd.Parameters.AddWithValue("@JapanHead", model.JapanHead.Trim());
                cmd.Parameters.AddWithValue("@Office", model.Office.Trim());
                cmd.Parameters.AddWithValue("@GotSection", model.GotSection.Trim());
                cmd.Parameters.AddWithValue("@Prefix", model.Prefix.Trim());
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 250) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Department saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving department");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteDepartmentAsync(int recordNo, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_DepartmentMaster_Delete", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@RecordNo", recordNo);
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 250) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Department deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting department");
                return (false, ex.Message);
            }
        }

        public async Task<List<DepartmentMasterVm>> GetDepartmentsForExportAsync(DepartmentSearchRequest request)
        {
            request.PageNumber = 1;
            request.PageSize = 0;
            var result = await GetDepartmentsAsync(request);
            return result.Data;
        }

        private static void AddSearchParameters(SqlCommand cmd, DepartmentSearchRequest request)
        {
            cmd.Parameters.AddWithValue("@GlobalSearch", (object?)request.GlobalSearch ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DepartmentCode", (object?)request.DepartmentCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DepartmentName", (object?)request.DepartmentName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@JapanHead", (object?)request.JapanHead ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Office", (object?)request.Office ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GotSection", (object?)request.GotSection ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Prefix", (object?)request.Prefix ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SortColumn", request.SortColumn);
            cmd.Parameters.AddWithValue("@SortDirection", request.SortDirection);
            cmd.Parameters.AddWithValue("@PageNumber", request.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize);
        }

        private static List<DepartmentMasterVm> ReadDepartmentList(SqlDataReader reader)
        {
            var list = new List<DepartmentMasterVm>();
            while (reader.Read())
            {
                list.Add(MapDepartment(reader));
            }
            return list;
        }

        private static DepartmentMasterVm MapDepartment(IDataRecord reader)
        {
            return new DepartmentMasterVm
            {
                RecordNo = Convert.ToInt32(reader["RecordNo"]),
                DepartmentCode = Convert.ToString(reader["DepartmentCode"]) ?? string.Empty,
                DepartmentName = Convert.ToString(reader["DepartmentName"]) ?? string.Empty,
                JapanHead = Convert.ToString(reader["JapanHead"]) ?? string.Empty,
                Office = Convert.ToString(reader["Office"]) ?? string.Empty,
                GotSection = Convert.ToString(reader["GotSection"]) ?? string.Empty,
                Prefix = Convert.ToString(reader["Prefix"]) ?? string.Empty,
                CreatedBy = Convert.ToString(reader["CreatedBy"]),
                CreatedOn = reader["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["CreatedOn"]),
                EditedBy = Convert.ToString(reader["EditedBy"]),
                EditedOn = reader["EditedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["EditedOn"]),
                ActiveStatus = reader["ActiveStatus"] != DBNull.Value && Convert.ToBoolean(reader["ActiveStatus"])
            };
        }
    }
}
