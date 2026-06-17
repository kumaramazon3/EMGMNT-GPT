using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.InterFace.Training;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.TM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Maruwa_Emgmt.DAL.Training
{
    public class dal_trainingList: i_trainingList
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_tblempmaster> _logger;
        public dal_trainingList(ApplicationDbContext context, ILogger<da_tblempmaster> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure context is not null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));  // Ensure logger is not null
        }

        //public async Task<trainingList?> GettraininglistByTitleAsync(string titlename)
        //{
        //    try
        //    {
        //        // Fetch training details from trainingList table
        //        var training = await _context.traininglist
        //                                     .AsNoTracking()
        //                                     .Where(t => t.titleName == titlename)
        //                                     .Select(t => new trainingList
        //                                     {
        //                                         titleName = t.titleName,
        //                                         programme = t.programme,
        //                                         Method = t.Method,
        //                                         Type = t.Type
        //                                     })
        //                                     .FirstOrDefaultAsync();

        //        if (training == null)
        //        {
        //            _logger.LogInformation("No training found for titleName: {TitleName}", titlename);
        //        }

        //        return training;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception with stack trace
        //        _logger.LogError(ex, "Error fetching training details for titleName: {titlename}", titlename);
        //        return null; // Return null instead of throwing to keep the flow safe
        //    }
        //}

        public async Task<trainingList?> GettraininglistByTitleAsync(string titleName,string Type)
        {
            using var conn = new SqlConnection(_context.Database.GetConnectionString());
            using var cmd = new SqlCommand("usp_GetTrainingEmployees", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@titleName", titleName);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            trainingList result = new();
            // ===== Result Set 1 : trainingList =====
            if (await reader.ReadAsync())
            {
                result.programme = reader["programme"]?.ToString();
                result.Method = reader["Method"]?.ToString();
                result.Type = reader["Type"]?.ToString();
            }

            // ===== Result Set 2 : Trainers =====
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Trainers.Add(new empMaster
                    {
                        empcode = reader["empcode"]?.ToString(),
                        empName = reader["empName"]?.ToString()
                    });
                }
            }

            return result;
        }

        public async Task<bool> InsertOrUpdateEmpTrainingAsync(empTraining model)
        {
            try
            {
                using var conn = new SqlConnection(_context.Database.GetConnectionString());
                using var cmd = new SqlCommand("usp_InsertOrUpdateEmpTraining", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@empCode", model.empCode ?? "");
                cmd.Parameters.AddWithValue("@trainingAttended", model.trainingAttended ?? "");
                cmd.Parameters.AddWithValue("@programme", model.programme ?? "");
                cmd.Parameters.AddWithValue("@remarks", model.remarks ?? "");
                cmd.Parameters.AddWithValue("@markScored", model.markScored ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@dateAttended", model.dateAttended ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@hours", model.hours ?? "");
                cmd.Parameters.AddWithValue("@trainingAttachment", model.trainingAttachmentURL ?? "");
                cmd.Parameters.AddWithValue("@sectionCode", model.sectionCode ?? "");
                cmd.Parameters.AddWithValue("@method", model.method ?? "");
                cmd.Parameters.AddWithValue("@type", model.type ?? "");
                cmd.Parameters.AddWithValue("@trainerCode", model.trainerCode ?? "");
                cmd.Parameters.AddWithValue("@trainerName", model.trainerName ?? "");
                cmd.Parameters.AddWithValue("@createdBy", model.createdBy ?? "");

                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                // Treat -1 (rows affected unknown due to SET NOCOUNT ON) as success
                if (rows == -1) rows = 1;
                return rows > 0;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error while inserting/updating empTraining for EmpCode: {EmpCode}", model.empCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while inserting/updating empTraining for EmpCode: {EmpCode}", model.empCode);
                throw;
            }
        }

    }
}
