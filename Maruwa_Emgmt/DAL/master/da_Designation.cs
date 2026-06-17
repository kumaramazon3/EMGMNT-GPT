using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models;
using Microsoft.EntityFrameworkCore;

namespace Maruwa_Emgmt.DAL.master
{
    public class da_Designation: i_Designation
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_trainingMaster> _logger;
        public da_Designation(ApplicationDbContext context, ILogger<da_trainingMaster> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure context is injected
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));   // Ensure logger is injected
        }

        public async Task<List<master_Designation>> GetAllDesignationAsync()
        {
            try
            {
                var rntdata = await _context.designation.Where(x => x.isActive).OrderByDescending(x => x.Sno).ToListAsync();
                return rntdata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error in GetAllDesignationAsync");
                throw;
            }
        }
    }
}
