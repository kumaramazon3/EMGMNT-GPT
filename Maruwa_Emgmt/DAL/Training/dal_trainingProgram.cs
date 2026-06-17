using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.InterFace.Training;
using Maruwa_Emgmt.Models.TM;
using Microsoft.EntityFrameworkCore;

namespace Maruwa_Emgmt.DAL.Training
{
    public class dal_trainingProgram: i_trainingProgram
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_tblempmaster> _logger;
        public dal_trainingProgram(ApplicationDbContext context, ILogger<da_tblempmaster> logger, IFtpService ftp, IProjectLocationService ipls,
           IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure context is not null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));  // Ensure logger is not null
        }


        public async Task<IEnumerable<trainingProgram>> GetAlltrainingProgramAsync()
        {
            try
            {
                return await _context.trainingprogram .Where(t => t.isActive == true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching training programs: {ex.Message}");
                throw;
            }
        }

        // Explicit method to implement interface
        public async Task<trainingProgram?> GetTrainingByIdAsync(int uid)
        {
            try
            {
                return await _context.trainingprogram.FirstOrDefaultAsync(t => t.UID == uid);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTrainingByIdAsync: {ex.Message}");
                return null;
            }
        }

    }
}
