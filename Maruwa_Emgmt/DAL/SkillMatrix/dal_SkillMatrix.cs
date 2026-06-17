using iText.StyledXmlParser.Css.Selector.Item;
using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace.SkillMatrix;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.SkillMatrix;
using Maruwa_Emgmt.Models.TM;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Maruwa_Emgmt.DAL.SkillMatrix
{
    public class dal_SkillMatrix : i_SkillMatrix
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_trainingMaster> _logger;
        public dal_SkillMatrix(ApplicationDbContext context, ILogger<da_trainingMaster> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure context is injected
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));   // Ensure logger is injected
        }

        private async Task LogErrorAsync(string method, Exception ex)
        {
            _logger.LogError(ex, "{Method} | {Message}", method, ex.Message);
            await Task.CompletedTask;
        }
       
        #region New Skill Matrix

        public async Task<bool> InsertSkillMatrixListAsync(List<Insert_skillMatrix> models)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Move existing data from Master to log table AND Update Old Table Status =0 (In Master & Log Tables).
                //await _context.Database.ExecuteSqlRawAsync("EXEC dbo.sp_MoveSkillMatrixToLogs11");
                //await _context.Database.ExecuteSqlRawAsync("EXEC dbo.sp_MoveSkillMatrixToLogs12");


                // 1.  Move Master Data to Log Table with Status= 0.
                await _context.Database.ExecuteSqlRawAsync("EXEC dbo.sp_MoveSkillMatrix_To_Logs");
                // 2. Insert new records into Master Table.
                foreach (var model in models)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        //@"EXEC dbo.sp_SkillMatrixDocumentData_Insert
                        //@"EXEC dbo.sp_SkillMatrixDocumentData_Insert_T2
                        //@"EXEC dbo.sp_SkillMatrixDocumentData_Insert_T3
                        @"EXEC dbo.sp_SkillMatrixFileData_IU
                          @ColumnNames,@DocumentName,@Rev,@Standards,@Department,@Category,
                          @PreparedBy,@ReviewedBy,@ApprovedBy,@ApprovedDate,@SectionCode,@CreatedBy",

                        new SqlParameter("@ColumnNames", (object?)model.columnNames ?? DBNull.Value),
                        new SqlParameter("@DocumentName", (object?)model.DocumentName ?? DBNull.Value),
                        new SqlParameter("@Rev", (object?)model.Rev ?? DBNull.Value),
                        new SqlParameter("@Standards", (object?)model.standards ?? DBNull.Value),
                        new SqlParameter("@Department", (object?)model.Department ?? DBNull.Value),
                        new SqlParameter("@Category", (object?)model.category ?? DBNull.Value),
                        new SqlParameter("@PreparedBy", (object?)model.preparedBy ?? DBNull.Value),
                        new SqlParameter("@ReviewedBy", (object?)model.reviewedBy ?? DBNull.Value),
                        new SqlParameter("@ApprovedBy", (object?)model.approvedBy ?? DBNull.Value),
                        new SqlParameter("@ApprovedDate", (object?)model.approvedDate ?? DBNull.Value),
                        new SqlParameter("@SectionCode", (object?)model.sectionCode ?? DBNull.Value),
                        new SqlParameter("@CreatedBy", (object?)model.CreatedBy ?? DBNull.Value)
                    );
                }

                // 3. Delete old records when Status=0 from Master Table.
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM EHRM.dbo.SkillMatrixDocumentData WHERE  STATUS=0;");
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await LogErrorAsync(nameof(InsertSkillMatrixListAsync), ex);
                throw;
            }
        }

        public async Task<bool> UpdateLogTableStatusAsync()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Soft delete old records
                //await _context.Database.ExecuteSqlRawAsync("UPDATE EHRM.dbo.SkillMatrixDocumentData_Logs SET newRecordStatus = 3");
                // Move Master dat to Log Table
                //await _context.Database.ExecuteSqlRawAsync("EXEC dbo.sp_MoveSkillMatrix_To_Logs");

                // Move Master Data to Log Table with Status= 0.
                await _context.Database.ExecuteSqlRawAsync("EXEC dbo.sp_MoveSkillMatrix_To_Logs");
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await LogErrorAsync(nameof(InsertSkillMatrixListAsync), ex);
                throw;
            }
        }

        public async Task<List<master_Skill_Matrix>> GetSkillMatrixAsync(string departmentCode, string subDepartmentCode, string sectionCode, DateTime? logDate)
        {
            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@departmentCode", departmentCode ?? (object)DBNull.Value),
                    new SqlParameter("@subDepartmentCode", subDepartmentCode ?? (object)DBNull.Value),
                    new SqlParameter("@sectionCode", sectionCode ?? (object)DBNull.Value),
                    new SqlParameter("@logDate", logDate.HasValue ? logDate.Value : (object)DBNull.Value)
                };
                var rtndata = await _context.masterskillmatrix
                    .FromSqlRaw("EXEC dbo.GetSkillMatrix @departmentCode, @subDepartmentCode, @sectionCode, @logDate", parameters.ToArray())
                    .ToListAsync();

                return rtndata;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while fetching data from GetSkillMatrixAsync(): " + ex);
                throw;
            }
        }


        #endregion

        #region Update SKM functions
        public async Task<List<new_skillMatrixDocumentData>> new_GetSkillMatrixAsync(string departmentCode, string subDepartmentCode, string sectionCode, string aliasName, string? logDate)
        {
            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@Department",string.IsNullOrEmpty(departmentCode) ? (object)DBNull.Value : departmentCode),
                    new SqlParameter("@SubDepartment",string.IsNullOrEmpty(subDepartmentCode) ? (object)DBNull.Value : subDepartmentCode),
                    new SqlParameter("@aliasName",string.IsNullOrEmpty(aliasName) ? (object)DBNull.Value : aliasName),
                    new SqlParameter("@SectionCode",string.IsNullOrEmpty(sectionCode) ? (object)DBNull.Value : sectionCode)
                };

                List<new_skillMatrixDocumentData> rtndata;
                if (!string.IsNullOrEmpty(logDate))
                {
                    parameters.Add(new SqlParameter("@LogDate", logDate));
                    //rtndata = await _context.new_masterskillmatrix.FromSqlRaw("EXEC dbo.usp_GetSkillMatrixEmployeeDetails_Log " +
                    rtndata = await _context.new_masterskillmatrix.FromSqlRaw("EXEC dbo.usp_GetskmList_Log " +
                        "@Department, @SectionCode, @LogDate", parameters.ToArray()).ToListAsync();
                }
                else
                {
                    // GOOD --> One to One
                    //string sql= "EXEC dbo.usp_GetskmList @Department, @SectionCode";
                    //var result = await _context.new_masterskillmatrix.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();

                    // Not GOOD code
                    //string sql = "EXEC dbo.usp_GetskmList_T1 @Department, @SectionCode,@EmpSection";
                    //var result = await _context.new_masterskillmatrix.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();

                    string sql = "EXEC dbo.usp_GetSkillMatrixByDepartmentSection @Department,@SubDepartment, @SectionCode,@aliasName";
                    var result = await _context.new_masterskillmatrix.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();

                    return result;
                }
                return rtndata;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while fetching data from new_GetSkillMatrixAsync(): " + ex);
                throw;
            }
        }
        public async Task<List<SelectskillMatrixDocumentData>> GetSkillMatrixAsync(string departmentCode, string subDepartmentCode, string sectionCode, string aliasName, string? logDate)
        {
            try
            {
                if (!string.IsNullOrEmpty(aliasName) && (aliasName.StartsWith("cv") || aliasName.StartsWith("mg")))
                {
                    aliasName = aliasName.Substring(2);
                }
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@Department",string.IsNullOrEmpty(departmentCode) ? (object)DBNull.Value : departmentCode),
                    new SqlParameter("@SubDepartment",string.IsNullOrEmpty(subDepartmentCode) ? (object)DBNull.Value : subDepartmentCode),
                    new SqlParameter("@aliasName",string.IsNullOrEmpty(aliasName) ? (object)DBNull.Value : aliasName),
                    new SqlParameter("@SectionCode",string.IsNullOrEmpty(sectionCode) ? (object)DBNull.Value : sectionCode)
                };

                List<SelectskillMatrixDocumentData> rtndata;
                if (!string.IsNullOrEmpty(logDate))
                {
                    parameters.Add(new SqlParameter("@LogYear", logDate));
                    //rtndata = await _context.getskillmatrixasync.FromSqlRaw("EXEC dbo.usp_GetskmList_Log " +
                    //    "@Department, @SectionCode, @LogDate", parameters.ToArray()).ToListAsync();

                    rtndata = await _context.getskillmatrixasync.FromSqlRaw("EXEC dbo.usp_GetSkillMatrixDocumentLog @Department, @SectionCode,@aliasName,@LogYear ", parameters.ToArray()).ToListAsync();
                }
                else
                {
                    //string sql = "EXEC dbo.usp_GetSkillMatrixByDepartmentSection @Department,@SubDepartment, @SectionCode,@aliasName";// One to One (working)
                    //string sql = "EXEC dbo.usp_GetSkillMatrixByDepartmentSection_One_TO_Many @Department,@SubDepartment, @SectionCode,@aliasName"; //--> One to many from eHRMS (working)
                    string sql = "EXEC dbo.usp_GetSKMDS_One_TO_Many @Department,@SubDepartment, @SectionCode,@aliasName"; //--> One to many  from Hrmis (working)
                    var result = await _context.getskillmatrixasync.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();

                    return result;
                }
                return rtndata;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while fetching data from new_GetSkillMatrixAsync(): " + ex);
                throw;
            }
        }

        public async Task new_SaveSkillMatrixAsync(List<new_skillMatrixDocumentData> skillMatrix)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var connectionString = _context.Database.GetDbConnection().ConnectionString;
            try
            {
                foreach (var item in skillMatrix)
                {
                    // Skip if both sectionCode and skillDate are empty/null
                    if (!string.IsNullOrWhiteSpace(item.score) || item.skillDate.HasValue)
                    {
                        string? newSectionCode = null;
                        if (!string.IsNullOrWhiteSpace(item.columnNames))
                        {
                            var parts = item.columnNames.Split('-');

                            if (parts.Length > 3)
                            {
                                newSectionCode = parts[3];
                                // Compare existing sectionCode with new sectionCode

                                string msg = "new_SaveSkillMatrixAsync() --> sectionCode: " + item.sectionCode + "newSectionCode: " + newSectionCode;
                                _logger.LogError(msg, ", Document Number: " + item.columnNames);

                                if (item.sectionCode == newSectionCode)
                                {
                                    item.sectionCode = newSectionCode;
                                }
                                else
                                {
                                    item.sectionCode = newSectionCode;
                                }
                            }
                        }

                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC dbo.usp_UpdateSkillMatrixDocumentData @columnNames,@empCode,@empName,@position,@departmentCode,@subDepartmentCode,@sectionCode,@score, @skillDate, @modifyedBy",
                            new SqlParameter("@columnNames", item.columnNames ?? (object)DBNull.Value),
                            new SqlParameter("@empCode", item.empCode ?? (object)DBNull.Value),
                            new SqlParameter("@empName", item.empName ?? (object)DBNull.Value),
                            new SqlParameter("@position", item.position ?? (object)DBNull.Value),
                            new SqlParameter("@departmentCode", item.departmentCode ?? (object)DBNull.Value),
                            new SqlParameter("@subDepartmentCode", item.subDepartmentCode ?? (object)DBNull.Value),
                            new SqlParameter("@sectionCode", item.sectionCode ?? (object)DBNull.Value),
                            new SqlParameter("@score", item.score ?? (object)DBNull.Value),
                            new SqlParameter("@skillDate", item.skillDate ?? (object)DBNull.Value),
                            new SqlParameter("@modifyedBy", item.modifyedBy ?? (object)DBNull.Value)
                        );
                    }
                }
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while saving skill matrix: " + ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public List<skillMatrixlog_Years> GetLogYears()
        {
            try
            {
                int currentYear = DateTime.Now.Year;

                var years = _context.skillmatrixlog_years
                            .Where(x => x.logDate.Year != currentYear)
                            .Select(x => x.logDate.Year).Distinct().OrderByDescending(y => y).ToList()
                            .Select(y => new skillMatrixlog_Years { LogYear = y.ToString() }).ToList();

                return years;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetLogYears: " + ex.Message);
                _logger.LogError("Exception while GetLogYears(): " + ex);
                return new List<skillMatrixlog_Years>();
            }
        }

        public List<SectionCodeModel> GetAllSectionList()
        {
            try
            {
                var data = _context.Set<SectionCodeModel>()
                                   .FromSqlRaw("EXEC usp_GetActiveSectionCodes")
                                   .AsEnumerable()
                                   .ToList();

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllSectionList: " + ex.Message);
                _logger.LogError("Exception while GetAllSectionList(): " + ex);
                return new List<SectionCodeModel>();
            }
        }

        #endregion

        #region Report List


        public async Task<List<OriginalReport_skillMatrixDocumentData>> GetSkillMatrixReportAsync(string departmentCode, string subDepartmentCode, string sectionCode, string Reporttype, string aliasName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(departmentCode))
                {
                    Reporttype = "";
                }
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@Department", string.IsNullOrEmpty(departmentCode) ? (object)DBNull.Value : departmentCode),
                    new SqlParameter("@SectionCode", string.IsNullOrEmpty(sectionCode) ? (object)DBNull.Value : sectionCode),
                    new SqlParameter("@Reporttype", !string.IsNullOrEmpty(aliasName)? (object)DBNull.Value: (string.IsNullOrEmpty(Reporttype) ? (object)DBNull.Value : Reporttype)),
                    new SqlParameter("@SubDepartment", string.IsNullOrEmpty(sectionCode) ? (object)DBNull.Value : ""),
                    new SqlParameter("@aliasName", string.IsNullOrEmpty(sectionCode) ? (object)DBNull.Value : aliasName),
                };

                List<OriginalReport_skillMatrixDocumentData> rtndata;
                if (Reporttype == "1")
                {

                    rtndata = await _context.original_masterskillmatrix.FromSqlRaw("EXEC dbo.usp_GetSKMDS_One_TO_Many_Report @Department, @SubDepartment, @SectionCode,@aliasName,@Reporttype", parameters.ToArray()).AsNoTracking().ToListAsync();
                }
                //else 
                //{ 
                //    rtndata = await _context.original_masterskillmatrix.FromSqlRaw("EXEC dbo.usp_GetSKMDS_One_TO_Many_Report @Department, @SubDepartment, @SectionCode,@aliasName,@Reporttype", parameters.ToArray()).AsNoTracking().ToListAsync();
                //}
                else // Reporttype == "0"
                {
                    rtndata = await _context.original_masterskillmatrix.FromSqlRaw("EXEC dbo.GetSkillMatrixDifferencesREV @Department, @sectionCode", parameters.ToArray()).AsNoTracking().ToListAsync();
                }

                return rtndata;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while fetching data from GetSkillMatrixReportAsync(): " + ex);
                throw;
            }
        }

        //public async Task<List<OriginalReport_skillMatrixDocumentData>> GetSkillMatrixReportAsync(string departmentCode, string subDepartmentCode, string sectionCode, string Reporttype)
        //{
        //    try
        //    {
        //        var parameters = new List<SqlParameter>
        //        {
        //            new SqlParameter("@Department", string.IsNullOrEmpty(departmentCode) ? (object)DBNull.Value : departmentCode),
        //            new SqlParameter("@SectionCode", string.IsNullOrEmpty(sectionCode) ? (object)DBNull.Value : sectionCode)
        //        };

        //        List<OriginalReport_skillMatrixDocumentData> rtndata;
        //        if (Reporttype == "1")
        //        {
        //            //parameters.Insert(1, new SqlParameter("@Reporttype",string.IsNullOrEmpty(Reporttype) ? (object)DBNull.Value : Reporttype));

        //            //rtndata = await _context.original_masterskillmatrix.FromSqlRaw("EXEC dbo.sp_GetSkillMatrixDocumentData_Original @Department, @Reporttype, @SectionCode", parameters.ToArray()).AsNoTracking().ToListAsync();
        //        }
        //        else // Reporttype == "0"
        //        {
        //            rtndata = await _context.original_masterskillmatrix
        //                //.FromSqlRaw("EXEC dbo.GetSkillMatrixDifferences @Department, @SectionCode", parameters.ToArray())
        //                .FromSqlRaw("EXEC dbo.GetSkillMatrixDifferencesREV @Department, @sectionCode", parameters.ToArray())
        //                .AsNoTracking().ToListAsync();
        //        }

        //        return rtndata;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception while fetching data from GetSkillMatrixReportAsync(): " + ex);
        //        throw;
        //    }
        //}

        public async Task<List<skillMatrixReportList>> GetSkillMatrixReportListAsync(string departmentCode, string sectionCode)
        {
            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@departmentCode", departmentCode ?? (object)DBNull.Value),
                    new SqlParameter("@sectionCode", sectionCode ?? (object)DBNull.Value),
                };
                var rtndata = await _context.masterskillmatrixReportList
                    .FromSqlRaw("EXEC dbo.GetSkillMatrixReportList @departmentCode, @sectionCode", parameters.ToArray())
                    .ToListAsync();

                return rtndata;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while fetching data from GetSkillMatrixReportListAsync(): " + ex);
                throw;
            }
        }

        public async Task<List<skillMatrixReportByDocNo>> GetSkillMatrixReportByDocNo(string DocNo, string Dept, string secCode)
        {
            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@docNo",string.IsNullOrEmpty(DocNo) ? (object)DBNull.Value : DocNo),
                    new SqlParameter("@dept",string.IsNullOrEmpty(Dept) ? (object)DBNull.Value : Dept),
                    new SqlParameter("@secCode",string.IsNullOrEmpty(secCode) ? (object)DBNull.Value : secCode)
                };

                List<skillMatrixReportByDocNo> rtndata;
                rtndata = await _context.skillMatrixReportByDocNo.FromSqlRaw("EXEC dbo.usp_GetSkillMatrixEmployeeDetails_ReportByDocNo @docNo, @dept, @secCode",
                    parameters.ToArray()).ToListAsync();

                return rtndata;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while fetching data from new_GetSkillMatrixAsync(): " + ex);
                throw;
            }
        }

        #endregion

        #region Commented Code

        //public async Task SaveSkillMatrixAsync_OLD(List<master_Skill_Matrix> skillMatrix)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        foreach (var item in skillMatrix)
        //        {
        //            await _context.Database.ExecuteSqlRawAsync("EXEC dbo.usp_UpdateSkillMatrix @empCode, @columnNames, @skillDate, @score, @modifyedBy",
        //                new SqlParameter("@empCode", item.empCode ?? (object)DBNull.Value),
        //                new SqlParameter("@columnNames", item.columnNames ?? (object)DBNull.Value),
        //                new SqlParameter("@skillDate", item.skillDate ?? (object)DBNull.Value),
        //                new SqlParameter("@score", item.score ?? (object)DBNull.Value),
        //                new SqlParameter("@modifyedBy", item.modifyedBy ?? (object)DBNull.Value));
        //        }
        //        await transaction.CommitAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception while fetching data from SaveSkillMatrixAsync(): " + ex);
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}


        //public async Task new_SaveSkillMatrixAsync_Old(List<new_skillMatrixDocumentData> skillMatrix)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        foreach (var item in skillMatrix)
        //        {
        //            if (!string.IsNullOrWhiteSpace(item.score) || item.skillDate.HasValue)
        //            {
        //                await _context.Database.ExecuteSqlRawAsync(
        //                    "EXEC dbo.usp_UpdateSkillMatrixDocumentData @columnNames,@empCode,@empName,@position,@departmentCode,@subDepartmentCode,@sectionCode,@score, @skillDate, @modifyedBy",
        //                    new SqlParameter("@columnNames", item.columnNames ?? (object)DBNull.Value),
        //                    new SqlParameter("@empCode", item.empCode ?? (object)DBNull.Value),
        //                    new SqlParameter("@empName", item.empName ?? (object)DBNull.Value),
        //                    new SqlParameter("@position", item.position ?? (object)DBNull.Value),
        //                    new SqlParameter("@departmentCode", item.departmentCode ?? (object)DBNull.Value),
        //                    new SqlParameter("@subDepartmentCode", item.subDepartmentCode ?? (object)DBNull.Value),
        //                    new SqlParameter("@sectionCode", item.sectionCode ?? (object)DBNull.Value),
        //                    new SqlParameter("@score", item.score ?? (object)DBNull.Value),
        //                    new SqlParameter("@skillDate", item.skillDate ?? (object)DBNull.Value),
        //                    new SqlParameter("@modifyedBy", item.modifyedBy ?? (object)DBNull.Value)
        //                );
        //            }
        //        }
        //        await transaction.CommitAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception while saving skill matrix: " + ex);
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}


        //public async Task<bool> InsertSkillMatrixListAsync_Old(List<skillMatrixDocumentData> models)
        //{
        //    try
        //    {

        //        // ✅ Step 1: Check if records exist
        //        var count = await _context.skillmatrixdocumentdata.CountAsync();

        //        if (count > 0)
        //        {
        //            // ✅ Step 2: Update all records Status = 0
        //            await _context.Database.ExecuteSqlRawAsync("UPDATE skillMatrixDocumentData SET Status = 0");
        //        }

        //        foreach (var model in models)
        //        {
        //            await _context.Database.ExecuteSqlRawAsync(
        //                "EXEC dbo.sp_InsertSkillMatrixDocument @ColumnNames, @DocumentName, @Department, " +
        //                "@DepartmentCode, @SubDepartmentCode, @SectionCode, @CreatedBy, @Rev",
        //                new SqlParameter("@ColumnNames", model.columnNames ?? (object)DBNull.Value),
        //                new SqlParameter("@DocumentName", model.DocumentName ?? (object)DBNull.Value),
        //                new SqlParameter("@Department", model.Department ?? (object)DBNull.Value),
        //                new SqlParameter("@DepartmentCode", model.departmentCode ?? (object)DBNull.Value),
        //                new SqlParameter("@SubDepartmentCode", model.subDepartmentCode ?? (object)DBNull.Value),
        //                new SqlParameter("@SectionCode", model.sectionCode ?? (object)DBNull.Value),
        //                new SqlParameter("@CreatedBy", model.CreatedBy ?? (object)DBNull.Value),
        //                new SqlParameter("@Rev", model.Rev ?? (object)DBNull.Value)
        //            );
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogErrorAsync(nameof(InsertSkillMatrixListAsync_Old), ex);
        //        _logger.LogError(ex, "Exception while inserting list.");
        //        throw;
        //    }
        //}


        //public async Task<bool> InsertSkillMatrixAsync(skillMatrixDocumentData model)
        //{
        //    try
        //    {
        //        await _context.Database.ExecuteSqlRawAsync("EXEC dbo.sp_InsertSkillMatrixDocument @ColumnNames, @DocumentName, @Department, " +
        //            "@DepartmentCode, @SubDepartmentCode, @SectionCode, @CreatedBy,@Rev",
        //            new SqlParameter("@ColumnNames", model.columnNames ?? (object)DBNull.Value),
        //            new SqlParameter("@DocumentName", model.DocumentName ?? (object)DBNull.Value),
        //            new SqlParameter("@Department", model.Department ?? (object)DBNull.Value),
        //            new SqlParameter("@DepartmentCode", model.departmentCode ?? (object)DBNull.Value),
        //            new SqlParameter("@SubDepartmentCode", model.subDepartmentCode ?? (object)DBNull.Value),
        //            new SqlParameter("@SectionCode", model.sectionCode ?? (object)DBNull.Value),
        //            new SqlParameter("@CreatedBy", model.CreatedBy ?? (object)DBNull.Value),
        //            new SqlParameter("@Rev", model.Rev ?? (object)DBNull.Value)
        //        );
        //        return true;// If SP executes without exception, return true
        //    }
        //    catch (Exception ex)
        //    {
        //        // Call your existing logger function
        //        await LogErrorAsync(nameof(InsertSkillMatrixAsync), ex);
        //        _logger.LogError(ex, "Exception while Inserting from InsertSkillMatrixAsync().");
        //        // Throw the exception so controller can catch it
        //        throw;
        //    }
        //}

        //public async Task<List<OriginalReport_skillMatrixDocumentData>> GetSkillMatrixReportAsync(string departmentCode, string subDepartmentCode, string sectionCode,string Reporttype)
        //{
        //    try
        //    {
        //        var parameters = new List<SqlParameter>
        //        {
        //            new SqlParameter("@Department",string.IsNullOrEmpty(departmentCode) ? (object)DBNull.Value : departmentCode),
        //            new SqlParameter("@Reporttype",string.IsNullOrEmpty(Reporttype) ? (object)DBNull.Value : Reporttype),
        //            new SqlParameter("@SectionCode",string.IsNullOrEmpty(sectionCode) ? (object)DBNull.Value : sectionCode)
        //        };

        //        List<OriginalReport_skillMatrixDocumentData> rtndata;
        //        //rtndata = await _context.original_masterskillmatrix.FromSqlRaw("EXEC dbo.usp_GetSkillMatrixReport @Department, @SectionCode", parameters.ToArray()).AsNoTracking().ToListAsync();
        //        rtndata = await _context.original_masterskillmatrix.FromSqlRaw("EXEC dbo.sp_GetSkillMatrixDocumentData_Original @Department, @Reporttype, @SectionCode", parameters.ToArray()).AsNoTracking().ToListAsync();


        //        return rtndata;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Exception while fetching data from new_GetSkillMatrixAsync(): " + ex);
        //        throw;
        //    }
        //}


        //public async Task DeleteAllAsync()
        //{
        //    try
        //    {
        //        //// Remove all records
        //        if (await _context.skillmatrixdocumentdata.AnyAsync())
        //        {
        //            await _context.skillmatrixdocumentdata.ExecuteDeleteAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Exception while Inserting from DeleteAllAsync().");
        //        throw;
        //    }
        //}

        #endregion
    }

    public class SectionResolver
    {
        // Dictionary
        private static readonly Dictionary<string, ProcessGroup> processGroups
            = new Dictionary<string, ProcessGroup>
        {
            {
                "Press",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7120", "7420", "7520", "7620"
                    },

                    Values = "7120,7620,7520,7420"
                }
            },

            {
                "Casting",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7510B", "7510-20", "7110B",
                        "7410B", "7410-20", "7110-20"
                    },

                    Values = "7110B,7110-20,7510B,7710-20,7410B,7410-20"
                }
            },

            {
                "Honing",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7140", "7440", "7540"
                    },

                    Values = "7140,7640,7540,7440"
                }
            },

            {
                "Laser",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "T7150", "7450", "7550"
                    },

                    Values = "7150,7550,7450"
                }
            },

            {
                "Packing",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7190", "7590", "7490"
                    },

                    Values = "7190,7590,7490"
                }
            },

            {
                "MainFiring",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7130", "7430", "7530"
                    },

                    Values = "7130,7530,7430"
                }
            },

            {
                "SecondFiring",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7160", "7460", "7560"
                    },

                    Values = "7160,7560,7460"
                }
            },

            {
                "Inspection",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7170", "7470", "7670"
                    },

                    Values = "7170,7670,7570,7470"
                }
            },

            {
                "Engineering",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7601-12A",
                        "7500-20",
                        "7100-12A",
                        "7400-20",
                        "7401-12A",
                        "7100-20"
                    },

                    Values = "7100-12A,7100-20,7601-12A,7500-20,7400-20,7401-12A"
                }
            },

            {
                "BallMill",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7510A",
                        "7510-10",
                        "7110A",
                        "7410A",
                        "7410-10",
                        "7110-10"
                    },

                    Values = "7110A,7110-10,7510-10,7410A,7410-10"
                }
            },

            {
                "Arraying",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "2130A",
                        "7130A",
                        "7730A"
                    },

                    Values = "7120"
                }
            },

            {
                "General",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7601",
                        "7101",
                        "7501",
                        "7401"
                    },

                    Values = "7100,7101,7601,7501,7401"
                }
            },

            {
                "QA",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7100-10",
                        "7500-10",
                        "7400-10",
                        "7110B-10"
                    },

                    Values = "9010,7100-10,7110B-10,7500-10,7400-10"
                }
            },

            {
                "Catron",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7170A",
                        "7470A"
                    },

                    Values = "7120,7170A,7470A"
                }
            },

            {
                "Maintenance",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7102",
                        "7103",
                        "7403"
                    },

                    Values = "1234,7102,7103,7403"
                }
            },

            {
                "Safety",
                new ProcessGroup
                {
                    Items = new List<string>
                    {
                        "7105",
                        "7405"
                    },

                    Values = "9005,7105,7405"
                }
            }
        };
        public static string ResolveSectionName(string selectedCode)
        {
            string normalizedSelected = selectedCode.Trim().ToLower();
            foreach (var group in processGroups)
            {
                bool matched = group.Value.Items.Any(item => item.Trim().ToLower() == normalizedSelected);

                if (matched)
                {
                    Console.WriteLine($"Matched Group: {group.Key}");
                    return group.Value.Values;
                }
            }

            return "";
        }
    }
} 
// Model Class
public class ProcessGroup
{
    public List<string> Items { get; set; }
    public string Values { get; set; }
}


