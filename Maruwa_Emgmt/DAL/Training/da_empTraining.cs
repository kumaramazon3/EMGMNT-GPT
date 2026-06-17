using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.InterFace.Training;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.TM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace Maruwa_Emgmt.DAL.Training
{
    public class da_empTraining: i_empTraining
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_tblempmaster> _logger;
        public da_empTraining(ApplicationDbContext context, ILogger<da_tblempmaster> logger, IFtpService ftp, IProjectLocationService ipls,
           IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure context is not null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));  // Ensure logger is not null
        }

        /// <summary>
        /// Get employee by EmpCode from empmaster table
        /// </summary>
        /// <param name="empcode">Employee code</param>
        /// <returns>empMaster object or null if not found</returns>
        public async Task<empMaster?> GetByEmpCodeAsync(string empcode)
        {
            if (string.IsNullOrWhiteSpace(empcode))
            {
                _logger.LogWarning("GetByEmpCodeAsync called with empty empcode.");
                return null;
            }
            try
            {
                using var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_GetEmployeeDetails";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var param = cmd.CreateParameter();
                param.ParameterName = "@EmpCode";
                param.Value = empcode;
                cmd.Parameters.Add(param);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new empMaster
                    {
                        empName = reader["empname"]?.ToString(),
                        //department = reader["department"]?.ToString(),
                        department = reader["departmentName"]?.ToString(),
                        //SubDepartment = reader["subDepartment"]?.ToString(),
                        subDepartment = reader["subDepartmentName"]?.ToString(),
                        dateOfJoin = reader["dateOfJoin"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["dateOfJoin"]),
                        section = reader["sectionName"]?.ToString()
                    };
                }

                _logger.LogInformation("Employee not found or resigned for empcode: {EmpCode}", empcode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee data for empcode: {EmpCode}", empcode);
                throw;
            }
        }

    }
}
