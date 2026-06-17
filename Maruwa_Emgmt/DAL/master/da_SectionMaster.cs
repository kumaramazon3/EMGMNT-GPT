using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models.master;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Maruwa_Emgmt.DAL.master
{
    public class da_SectionMaster : i_SectionMaster
    {
        private readonly string _connectionString;
        private readonly ILogger<da_SectionMaster> _logger;

        public da_SectionMaster(IConfiguration configuration, ILogger<da_SectionMaster> logger)
        {
            _connectionString = configuration.GetConnectionString("EHRMConnection") ?? throw new InvalidOperationException("EHRMConnection is missing in appsettings.json");
            _logger = logger;
        }

        public async Task<SectionListResult> GetSectionsAsync(SectionSearchRequest request)
        {
            var result = new SectionListResult();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_SectionMaster_GetPaged", con) { CommandType = CommandType.StoredProcedure };
            AddSearchParameters(cmd, request);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            result.Data = ReadSectionList(reader);
            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                result.TotalCount = Convert.ToInt32(reader["TotalCount"]);
            }
            return result;
        }

        public async Task<SectionMasterVm?> GetSectionByIdAsync(int sectionId)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_SectionMaster_GetById", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@SectionId", sectionId);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapSection(reader) : null;
        }

        public async Task<(bool Success, string Message)> SaveSectionAsync(SectionMasterVm model, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_SectionMaster_Save", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@SectionId", model.SectionId);
                cmd.Parameters.AddWithValue("@SectionCode", model.SectionCode.Trim());
                cmd.Parameters.AddWithValue("@Sectionname", model.Sectionname.Trim());
                cmd.Parameters.AddWithValue("@Departmentcode", model.Departmentcode.Trim());
                cmd.Parameters.AddWithValue("@SubDepartmentName", model.SubDepartmentName.Trim());
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 250) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Section saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving section");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteSectionAsync(int sectionId, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_SectionMaster_Delete", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@SectionId", sectionId);
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 250) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Section deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting section");
                return (false, ex.Message);
            }
        }

        public async Task<List<SectionMasterVm>> GetSectionsForExportAsync(SectionSearchRequest request)
        {
            request.PageNumber = 1;
            request.PageSize = 0;
            var result = await GetSectionsAsync(request);
            return result.Data;
        }

        public async Task<List<DepartmentLookupVm>> GetDepartmentLookupAsync(string? searchText)
        {
            var list = new List<DepartmentLookupVm>();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_SectionMaster_DepartmentLookup", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@SearchText", (object?)searchText ?? DBNull.Value);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new DepartmentLookupVm
                {
                    DepartmentCode = Convert.ToString(reader["DepartmentCode"]) ?? string.Empty,
                    DepartmentName = Convert.ToString(reader["DepartmentName"]) ?? string.Empty
                });
            }
            return list;
        }

        private static void AddSearchParameters(SqlCommand cmd, SectionSearchRequest request)
        {
            cmd.Parameters.AddWithValue("@GlobalSearch", (object?)request.GlobalSearch ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SectionCode", (object?)request.SectionCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Sectionname", (object?)request.Sectionname ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SectionId", (object?)request.SectionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Departmentcode", (object?)request.Departmentcode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SubDepartmentName", (object?)request.SubDepartmentName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@issectionActive", (object?)request.issectionActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)request.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EditedBy", (object?)request.EditedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SortColumn", request.SortColumn);
            cmd.Parameters.AddWithValue("@SortDirection", request.SortDirection);
            cmd.Parameters.AddWithValue("@PageNumber", request.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize);
        }

        private static List<SectionMasterVm> ReadSectionList(SqlDataReader reader)
        {
            var list = new List<SectionMasterVm>();
            while (reader.Read()) list.Add(MapSection(reader));
            return list;
        }

        private static SectionMasterVm MapSection(IDataRecord reader)
        {
            return new SectionMasterVm
            {
                SectionId = Convert.ToInt32(reader["SectionId"]),
                SectionCode = Convert.ToString(reader["SectionCode"]) ?? string.Empty,
                Sectionname = Convert.ToString(reader["Sectionname"]) ?? string.Empty,
                Departmentcode = Convert.ToString(reader["Departmentcode"]) ?? string.Empty,
                SubDepartmentName = Convert.ToString(reader["SubDepartmentName"]) ?? string.Empty,
                issectionActive = reader["issectionActive"] != DBNull.Value && Convert.ToBoolean(reader["issectionActive"]),
                CreatedBy = Convert.ToString(reader["CreatedBy"]),
                CreatedOn = reader["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["CreatedOn"]),
                EditedBy = Convert.ToString(reader["EditedBy"]),
                EditedOn = reader["EditedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["EditedOn"])
            };
        }
    }
}
