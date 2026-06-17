using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace.Leave;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.Leave;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text.RegularExpressions;

namespace Maruwa_Emgmt.DAL.Leave
{
    public class da_leave: i_leave
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_trainingMaster> _logger;
        private readonly EmpDesignationSettings _desigSettings;
        public da_leave(ApplicationDbContext context, ILogger<da_trainingMaster> logger, IOptions<EmpDesignationSettings> desigSettings)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure context is injected
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));   // Ensure logger is injected
            _desigSettings = desigSettings.Value;
        }

        public async Task<List<leaveform>> GetScheduledLeaveAsync(string empCode,string designation)
        {
            try
            {
                string normalized = designation.Replace(" ", "");
                if (_desigSettings.Allowed.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                {
                    var param = new SqlParameter("@stat", designation ?? (object)DBNull.Value);
                    var data = await _context.Set<leaveform>().FromSqlRaw("EXEC LeaveApproval @stat", param).ToListAsync();
                    return data;
                }
                else
                {
                    var param = new SqlParameter("@empCode", empCode ?? (object)DBNull.Value);
                    //var data = await _context.Set<leaveform>().FromSqlRaw("EXEC sp_GetScheduledLeave @empCode", param).ToListAsync();
                    var data = await _context.Set<leaveform>().FromSqlRaw("EXEC sp_GetScheduledLeaveByEmp @empCode", param).ToListAsync();
                    return data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching scheduled leave");
                throw;
            }
        }
        public async Task<List<leaveformHistory>> GetLeaveFormByEmpAsync(string empCode)
        {
            var leaveList = new List<leaveformHistory>();
            try
            {
                using var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "GetLeaveFormByEmp";
                cmd.CommandType = CommandType.StoredProcedure;

                var param = cmd.CreateParameter();
                param.ParameterName = "@empCode";
                param.Value = empCode;
                cmd.Parameters.Add(param);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    _logger.LogInformation("No leave records found for empCode: {EmpCode}", empCode);
                    return leaveList;
                }

                while (await reader.ReadAsync())
                {
                    leaveList.Add(new leaveformHistory
                    {
                        status = reader["status"]?.ToString(),
                        applicationdate = reader["applicationdate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["applicationdate"]),
                        EmpCode = reader["empno"]?.ToString(),
                        workfor = reader["workfor"] == DBNull.Value ? 0 : Convert.ToDouble(reader["workfor"]),

                        fromdate = reader["fromdate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["fromdate"]),
                        todate = reader["todate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["todate"]),
                        leavetype = reader["leavetype"]?.ToString(),
                        reason = reader["reason"]?.ToString(),
                        backdate = reader["backdate"]?.ToString(),
                        carryfwd = reader["carryfwd"]?.ToString(),
                        nocf = reader["nocf"] == DBNull.Value ? 0 : Convert.ToDouble(reader["nocf"])
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leave records for empCode: {EmpCode}", empCode);
                throw;
            }

            return leaveList;
        }
   
        public async Task<int> SubmitApprovalAsync(LeaveApprovalUpdate model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    @"EXEC dbo.sp_leave_approval 
                    @AppNo,@Empno,@approvalType_Status,@Remarks,@modifiedby,@modifiedtime",
                    new SqlParameter("@AppNo", model.AppNo),
                    new SqlParameter("@Empno", model.EmpCode ?? (object)DBNull.Value),
                    new SqlParameter("@approvalType_Status", model.Status ?? (object)DBNull.Value),
                    new SqlParameter("@Remarks", model.Remarks ?? (object)DBNull.Value),
                    new SqlParameter("@modifiedby", model.modifiedby ?? (object)DBNull.Value),
                    new SqlParameter("@modifiedtime", model.modifiedtime)
                );

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while submitting leave approval: " + ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public DataSet GetEmployeeLeaveFullSummary(string empCode)
        {
            DataSet ds = new DataSet();
            try
            {
                // Cast to SqlConnection from DbConnection to ensure native SQL Server behavior
                using (var sqlConnection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();

                    // First SP: Get current year leave summary
                    using (var command1 = new SqlCommand("sp_Get_CurrentYear_Balance_leavesByempCode", sqlConnection))
                    {
                        command1.CommandType = CommandType.StoredProcedure;
                        command1.Parameters.AddWithValue("@empCode", empCode);

                        using (var reader1 = command1.ExecuteReader())
                        {
                            DataTable dtCurrentYear = new DataTable();
                            dtCurrentYear.Load(reader1);
                            dtCurrentYear.TableName = "CurrentYearLeave"; // Optional: give it a meaningful name
                            ds.Tables.Add(dtCurrentYear);
                            _logger.LogInformation($"Current year leave table loaded with {dtCurrentYear.Rows.Count} rows");
                        }
                    }

                    // Second SP: Get last year balance leaves
                    using (var command2 = new SqlCommand("sp_Get_LastYear_Balance_leavesByempCode", sqlConnection))
                    {
                        command2.CommandType = CommandType.StoredProcedure;
                        command2.Parameters.AddWithValue("@empCode", empCode);

                        using (var reader2 = command2.ExecuteReader())
                        {
                            DataTable dtLastYear = new DataTable();
                            dtLastYear.Load(reader2);
                            dtLastYear.TableName = "LastYearLeave"; // Optional: give it a meaningful name
                            ds.Tables.Add(dtLastYear);
                            _logger.LogInformation($"Last year leave table loaded with {dtLastYear.Rows.Count} rows");
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError("Database error while fetching leave summary: " + sqlEx.Message, sqlEx);
                throw new Exception("Database error while fetching leave summary: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while fetching leave summary: " + ex.Message, ex);
                throw new Exception("An error occurred while fetching leave summary: " + ex.Message, ex);
            }

            return ds;
        }

        public DataSet GetEmployeeLeaveSummary(string empCode)
        {
            DataSet ds = new DataSet();
            try
            {
                // Cast to SqlConnection from DbConnection to ensure native SQL Server behavior
                using (var sqlConnection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();

                    // First SP: Get current year leave summary
                    using (var command1 = new SqlCommand("sp_GetEmployee_CurrentYear_LeaveEntitlement", sqlConnection))
                    {
                        command1.CommandType = CommandType.StoredProcedure;
                        command1.Parameters.AddWithValue("@empCode", empCode);

                        using (var reader1 = command1.ExecuteReader())
                        {
                            DataTable dtCurrentYear = new DataTable();
                            dtCurrentYear.Load(reader1);
                            dtCurrentYear.TableName = "CurrentYearLeave"; // Optional: give it a meaningful name
                            ds.Tables.Add(dtCurrentYear);
                            _logger.LogInformation($"Current year leave table loaded with {dtCurrentYear.Rows.Count} rows");
                        }
                    }

                    // Second SP: Get last year balance leaves
                    using (var command2 = new SqlCommand("sp_GetEmployee_LastYear_LeaveEntitlement", sqlConnection))
                    {
                        command2.CommandType = CommandType.StoredProcedure;
                        command2.Parameters.AddWithValue("@empCode", empCode);

                        using (var reader2 = command2.ExecuteReader())
                        {
                            DataTable dtLastYear = new DataTable();
                            dtLastYear.Load(reader2);
                            dtLastYear.TableName = "LastYearLeave"; // Optional: give it a meaningful name
                            ds.Tables.Add(dtLastYear);
                            _logger.LogInformation($"Last year leave table loaded with {dtLastYear.Rows.Count} rows");
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError("Database error while fetching leave summary: " + sqlEx.Message, sqlEx);
                throw new Exception("Database error while fetching leave summary: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while fetching leave summary: " + ex.Message, ex);
                throw new Exception("An error occurred while fetching leave summary: " + ex.Message, ex);
            }

            return ds;
        }


        #region Leave Calculation Part

        //public async Task<int> FindAnnual_Medical_LeavesByempCode(string empCode)
        //{
        //    try
        //    {

        //        return 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception while FindAnnual_Medical_LeavesByempCode(): " + ex);
        //        throw;
        //    }
        //}
        #endregion

        public async Task<string> SubmitLeave(LeaveApplicationSubmitModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appNoParam = new SqlParameter("@AppNo", SqlDbType.VarChar, 50)
                {
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(@"EXEC dbo.sp_Insert_New_LeaveForm 
                                        @EmpCode,@Days,@Fromdate,@Todate,@Leavetype,
                                        @LeaveTypeID,@Reason,@Leavetime,@Backdate,@Createdby,
                                        @Pic1,@Pic11,@Pic2,@Pic22,@AnnualBal,
                                        @MedicalBal,@AppNo OUTPUT",

                    new SqlParameter("@EmpCode", model.EmpCode ?? (object)DBNull.Value),
                    new SqlParameter("@Days", model.days),
                    new SqlParameter("@Fromdate", model.fromdate),
                    new SqlParameter("@Todate", model.todate),
                    new SqlParameter("@Leavetype", model.leavetype ?? (object)DBNull.Value),

                    new SqlParameter("@LeaveTypeID", model.leavetype1 ?? (object)DBNull.Value),
                    new SqlParameter("@Reason", model.reason ?? (object)DBNull.Value),
                    new SqlParameter("@Leavetime", model.leavetime ?? (object)DBNull.Value),
                    new SqlParameter("@Backdate", model.backdate),
                    new SqlParameter("@Createdby", model.createdby ?? (object)DBNull.Value),

                    new SqlParameter("@Pic1", model.pic1 ?? (object)DBNull.Value),
                    new SqlParameter("@Pic11", model.pic11 ?? (object)DBNull.Value),
                    new SqlParameter("@Pic2", model.pic2 ?? (object)DBNull.Value),
                    new SqlParameter("@Pic22", model.pic22 ?? (object)DBNull.Value),
                    new SqlParameter("@AnnualBal", model.AnnualBal),
                    new SqlParameter("@MedicalBal", model.MedicalBal),
                    appNoParam
                );

                await transaction.CommitAsync();
                return appNoParam.Value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while submitting leave: " + ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> Approved_SubmitLeavesEmployee(LeaveApprovalModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    @"EXEC dbo.sp_leave_approval @AppNo,@LeaveEmpCode,@Status,@Nocf,@WorkFor,
                                                 @LeaveType,@Remarks,@Modifiedby,@Carryfwd",

                    new SqlParameter("@AppNo", model.AppNo),
                    new SqlParameter("@LeaveEmpCode", model.LeaveempCode ?? (object)DBNull.Value),
                    new SqlParameter("@Status", model.Status ?? (object)DBNull.Value),
                    new SqlParameter("@Nocf", model.Nocf),
                    new SqlParameter("@WorkFor", model.WorkFor),
                    new SqlParameter("@LeaveType", model.LeavetypeID ?? (object)DBNull.Value),
                    new SqlParameter("@Remarks", model.Remarks ?? (object)DBNull.Value),
                    new SqlParameter("@Modifiedby", model.Modifiedby ?? (object)DBNull.Value),
                    new SqlParameter("@Carryfwd", model.carryfwd)
                );

                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while submitting leave approval: " + ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
