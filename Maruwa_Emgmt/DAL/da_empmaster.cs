using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;
using Microsoft.EntityFrameworkCore;

namespace Maruwa_Emgmt.DAL
{
    public class da_empmaster : i_empmaster
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_empmaster> _logger;
        public da_empmaster(ApplicationDbContext context, ILogger<da_empmaster> logger)
        {
            _context = context;
            _logger = logger;
        }
        public empmaster ValidateLoginInfo(string empcode, string pwd)
        {
            try
            {
                // Print or log the connection string
                _logger.LogInformation("Before: Login ID: {empcode} , Pwd: {pwd}", empcode, pwd);

                // Fetch employee
                var user = _context.empmaster
                    .AsNoTracking() // prevents tracking and reduces open readers
                    .FirstOrDefault(e => e.empcode == empcode && e.pwd == pwd);

                // Log result as true/false
                bool isValid = user != null;
                _logger.LogInformation("After: Login result for EmpCode {EmpCode}: {IsValid}", empcode, isValid);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating login for EmpCode: {EmpCode}", empcode);
                return null;
            }
        }

    }
}
