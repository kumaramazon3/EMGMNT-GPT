using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace.Training;
using Maruwa_Emgmt.Models.TM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static iTextSharp.text.pdf.AcroFields;

namespace Maruwa_Emgmt.DAL.Training
{
    public class dal_tna: i_tna
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_tblempmaster> _logger;
        public dal_tna(ApplicationDbContext context, ILogger<da_tblempmaster> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure context is not null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));  // Ensure logger is not null
        }

        public async Task<List<master_Skill_Matrix>> GetSkillMatrixAsync(string departmentCode,string subDepartmentCode,string sectionCode,DateTime? logDate)
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

        public async Task SaveSkillMatrixAsync(List<master_Skill_Matrix> skillMatrix)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in skillMatrix)
                {
                    await _context.Database.ExecuteSqlRawAsync("EXEC dbo.usp_UpdateSkillMatrix @empCode, @columnNames, @skillDate, @score, @modifyedBy",
                        new SqlParameter("@empCode", item.empCode ?? (object)DBNull.Value),
                        new SqlParameter("@columnNames", item.columnNames ?? (object)DBNull.Value),
                        new SqlParameter("@skillDate", item.skillDate ?? (object)DBNull.Value),
                        new SqlParameter("@score", item.score ?? (object)DBNull.Value),
                        new SqlParameter("@modifyedBy", item.modifyedBy ?? (object)DBNull.Value));
                }
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while fetching data from SaveSkillMatrixAsync(): " + ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
