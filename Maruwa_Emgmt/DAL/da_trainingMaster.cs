using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models.TM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Maruwa_Emgmt.DAL
{
    public class da_trainingMaster: i_trainingMaster
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_trainingMaster> _logger;
        public da_trainingMaster(ApplicationDbContext context, ILogger<da_trainingMaster> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure context is injected
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));   // Ensure logger is injected
        }

        private async Task LogErrorAsync(string method, Exception ex)
        {
            _logger.LogError(ex, "{Method} | {Message}", method, ex.Message);
            await Task.CompletedTask;
        }
      
        //public async Task<int> GetNextTrainingCodeAsync_Old()
        //{
        //    try
        //    {
        //        int maxCode = await _context.Set<trainingMaster>()
        //                                    .MaxAsync(x => (int?)x.code) ?? 0;
        //        return maxCode + 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogErrorAsync(nameof(GetNextTrainingCodeAsync), ex);
        //        return 1; // first record fallback
        //    }
        //}
        public async Task<string> GetNextTrainingCodeAsync()
        {
            try
            {
                var codes = await _context.Set<trainingMaster>()
                                          .Where(x => x.code.StartsWith("TM"))
                                          .Select(x => x.code)
                                          .ToListAsync();

                int maxNumber = codes
                    .Select(c =>
                    {
                        var numberPart = c.Substring(2); // remove "TM"
                        return int.TryParse(numberPart, out int n) ? n : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                return $"TM{maxNumber + 1}";
            }
            catch (Exception ex)
            {
                await LogErrorAsync(nameof(GetNextTrainingCodeAsync), ex);
                return "TM1";
            }
        }

        public async Task<List<trainingMaster>> GetTrainingListAsync()
        {
            try
            {
                //var connectionString = _context.Database.GetDbConnection().ConnectionString;
                return await _context.Set<trainingMaster>().AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                await LogErrorAsync(nameof(GetTrainingListAsync), ex);
                return new List<trainingMaster>();
            }
        }

        public async Task<trainingMaster> GetTrainingByIdAsync(string code)
        {
            try
            {
                return await _context.Set<trainingMaster>().FirstOrDefaultAsync(x => x.code == code);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(nameof(GetTrainingByIdAsync), ex);
                return null;
            }
        }
      
        public async Task<bool> SaveTrainingAsync(trainingMaster model)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC dbo.sp_Training_Insert @code, @titleName, @createdBy",
                    new SqlParameter("@code", model.code),
                    new SqlParameter("@titleName", model.titleName ?? (object)DBNull.Value),
                    new SqlParameter("@createdBy", model.createdBy ?? (object)DBNull.Value)
                );
                // If SP executes without exception, return true
                return true;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(nameof(SaveTrainingAsync), ex);
                throw;
            }
        }

        public async Task<bool> UpdateTrainingAsync(trainingMaster model)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_Training_Update @code, @titleName, @modifiedBy",
                    new SqlParameter("@code", model.code),
                    new SqlParameter("@titleName", model.titleName ?? (object)DBNull.Value),
                    new SqlParameter("@modifiedBy", model.modifiedBy ?? (object)DBNull.Value)
                );
                return true;// If it executes without exception, consider it success
            }
            catch (Exception ex)
            {
                await LogErrorAsync(nameof(UpdateTrainingAsync), ex);
                throw;
            }
        }
        public async Task<bool> DeleteTrainingAsync(string code)
        {
            try
            {
                var result = await _context.Database.ExecuteSqlInterpolatedAsync($@"EXEC dbo.sp_Training_Delete @code = {code}");
                return result > 0;// Only if SET NOCOUNT is OFF in the SP
            }
            catch (Exception ex)
            {
                await LogErrorAsync(nameof(DeleteTrainingAsync), ex);
                throw;
            }
        }


        #region Commeted Code (EF)
        //public async Task SaveTrainingAsync(trainingMaster model)
        //{
        //    try
        //    {
        //        _context.Set<trainingMaster>().Add(model);
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogErrorAsync(nameof(SaveTrainingAsync_Old), ex);
        //        throw; // Rethrow to let controller handle error if needed
        //    }
        //}

        //public async Task<bool> UpdateTrainingAsync(trainingMaster model)
        //{
        //    try
        //    {
        //        var existing = await _context.Set<trainingMaster>().FirstOrDefaultAsync(x => x.code == model.code);
        //        if (existing == null)return false;

        //        // Map fields manually (recommended)
        //        existing.titleName = model.titleName;
        //        existing.modifiedDate = model.modifiedDate;
        //        existing.modifiedBy = model.modifiedBy;

        //        await _context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogErrorAsync(nameof(UpdateTrainingAsync), ex);
        //        throw;
        //    }
        //}

        //public async Task<bool> DeleteTrainingAsync(string code)
        //{
        //    try
        //    {
        //        var entity = await GetTrainingByIdAsync(code);

        //        if (entity == null)
        //            return false;

        //        _context.Set<trainingMaster>().Remove(entity);
        //        await _context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return false;// Already deleted by another request
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogErrorAsync(nameof(DeleteTrainingAsync), ex);
        //        throw; // unexpected error → controller handles it
        //    }
        //}


        #endregion
    }
}
