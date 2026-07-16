using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models.master;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Maruwa_Emgmt.DAL.master
{
    public class da_Designation : i_Designation
    {
        private readonly string _connectionString;
        private readonly ILogger<da_Designation> _logger;

        public da_Designation(IConfiguration configuration, ILogger<da_Designation> logger)
        {
            _connectionString = configuration.GetConnectionString("EHRMConnection") ?? throw new InvalidOperationException("EHRMConnection is missing in appsettings.json");
            _logger = logger;
        }

        public async Task<DesignationListResult> GetDesignationsAsync(DesignationSearchRequest request)
        {
            var result = new DesignationListResult();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_DesignationMaster_GetPaged", con) { CommandType = CommandType.StoredProcedure };
            AddSearchParameters(cmd, request);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            result.Data = ReadDesignationList(reader);
            if (await reader.NextResultAsync() && await reader.ReadAsync()) result.TotalCount = Convert.ToInt32(reader["TotalCount"]);
            return result;
        }

        public async Task<DesignationMasterVm?> GetDesignationByIdAsync(int sno)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_DesignationMaster_GetById", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Sno", sno);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapDesignation(reader) : null;
        }

        public async Task<(bool Success, string Message)> SaveDesignationAsync(DesignationMasterVm model, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_DesignationMaster_Save", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Sno", model.Sno);
                cmd.Parameters.AddWithValue("@designationcode", model.designationcode.Trim());
                cmd.Parameters.AddWithValue("@designationName", model.designationName.Trim());
                cmd.Parameters.AddWithValue("@insCatergory", model.insCatergory.Trim());
                cmd.Parameters.AddWithValue("@dlevel", (object?)model.dlevel ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@probation", model.probation.Trim());
                cmd.Parameters.AddWithValue("@CTQLevel", (object?)model.CTQLevel ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@kpi", (object?)model.kpi ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@positioned", (object?)model.positioned ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@insamount", model.insamount);
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 250) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status); cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Designation saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving designation");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteDesignationAsync(int sno, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_DesignationMaster_Delete", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Sno", sno);
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 250) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status); cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Designation deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting designation");
                return (false, ex.Message);
            }
        }

        public async Task<List<DesignationMasterVm>> GetDesignationsForExportAsync(DesignationSearchRequest request)
        {
            request.PageNumber = 1;
            request.PageSize = 0;
            return (await GetDesignationsAsync(request)).Data;
        }

        public async Task<List<InsuranceCategoryLookupVm>> GetInsuranceCategoryLookupAsync(string? searchText)
        {
            var list = new List<InsuranceCategoryLookupVm>();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_InsuranceCategoryMaster_Lookup", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@SearchText", (object?)searchText ?? DBNull.Value);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(new InsuranceCategoryLookupVm { InsuranceCategoryId = Convert.ToInt32(reader["InsuranceCategoryId"]), CategoryCode = Convert.ToString(reader["CategoryCode"]) ?? "", CategoryName = Convert.ToString(reader["CategoryName"]) ?? "" });
            return list;
        }

        public async Task<List<ProbationLookupVm>> GetProbationLookupAsync(string? searchText)
        {
            var list = new List<ProbationLookupVm>();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_ProbationMaster_Lookup", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@SearchText", (object?)searchText ?? DBNull.Value);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(new ProbationLookupVm { ProbationId = Convert.ToInt32(reader["ProbationId"]), ProbationCode = Convert.ToString(reader["ProbationCode"]) ?? "", ProbationName = Convert.ToString(reader["ProbationName"]) ?? "" });
            return list;
        }

        private static void AddSearchParameters(SqlCommand cmd, DesignationSearchRequest request)
        {
            cmd.Parameters.AddWithValue("@GlobalSearch", (object?)request.GlobalSearch ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@designationcode", (object?)request.designationcode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@designationName", (object?)request.designationName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@probation", (object?)request.probation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@insCatergory", (object?)request.insCatergory ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@insamount", (object?)request.insamount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)request.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EditedBy", (object?)request.EditedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@isActive", (object?)request.isActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SortColumn", request.SortColumn);
            cmd.Parameters.AddWithValue("@SortDirection", request.SortDirection);
            cmd.Parameters.AddWithValue("@PageNumber", request.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize);
        }

        private static List<DesignationMasterVm> ReadDesignationList(SqlDataReader reader)
        {
            var list = new List<DesignationMasterVm>();
            while (reader.Read()) list.Add(MapDesignation(reader));
            return list;
        }

        private static DesignationMasterVm MapDesignation(IDataRecord reader)
        {
            return new DesignationMasterVm
            {
                Sno = Convert.ToInt32(reader["Sno"]),
                designationcode = Convert.ToString(reader["designationcode"]) ?? string.Empty,
                designationName = Convert.ToString(reader["designationName"]) ?? string.Empty,
                insCatergory = Convert.ToString(reader["insCatergory"]) ?? string.Empty,
                dlevel = Convert.ToString(reader["dlevel"]),
                probation = Convert.ToString(reader["probation"]) ?? string.Empty,
                CTQLevel = Convert.ToString(reader["CTQLevel"]),
                kpi = Convert.ToString(reader["kpi"]),
                positioned = Convert.ToString(reader["positioned"]),
                insamount = reader["insamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["insamount"]),
                CreatedBy = Convert.ToString(reader["CreatedBy"]),
                CreatedOn = reader["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["CreatedOn"]),
                EditedBy = Convert.ToString(reader["EditedBy"]),
                EditedOn = reader["EditedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["EditedOn"]),
                isActive = reader["isActive"] != DBNull.Value && Convert.ToBoolean(reader["isActive"])
            };
        }
    }
}
