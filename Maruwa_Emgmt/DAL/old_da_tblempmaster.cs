using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Maruwa_Emgmt.DAL
{
    public class old_da_tblempmaster : i_tblempmaster
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<old_da_tblempmaster> _logger;

        public old_da_tblempmaster(ApplicationDbContext context, ILogger<old_da_tblempmaster> logger)
        {
            _context = context;

            _logger = logger;
        }

        public IEnumerable<tblempmaster> GetAllEmployeeData()
        {
            try
            {
                var employees = _context.tblempmaster
                    .AsNoTracking()
                    .ToList();

                _logger.LogInformation("Fetched {Count} employees from tblempmaster.", employees.Count);

                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while fetching employees: " + ex);
                return Enumerable.Empty<tblempmaster>();
            }
        }

        public List<tblhousenumber> GetHouseNumbersByHostel(string hostelId)
        {
            throw new NotImplementedException();
        }

        public string GetLastEmpCode()
        {
            // assuming empcode is string but sortable; or numeric — adjust accordingly
            var last = _context.tblempmaster
                       .OrderByDescending(e => e.empcode)
                       .Select(e => e.empcode)
                       .FirstOrDefault();
            return last;
        }

        public tblempmaster ValidateLoginInfo(string empcode, string pwd)
        {
            try
            {
                // Log the connection string
                var connectionString = _context.Database.GetDbConnection().ConnectionString;

                // Log the connection string for debugging purposes
                _logger.LogInformation("Connection String: {ConnectionString}", connectionString);


                // Fetch employee
                var user = _context.loginempmaster
                    .AsNoTracking() // prevents tracking and reduces open readers
                    .FirstOrDefault(e => e.empcode == empcode && e.pwd == pwd);

                // Log result as true/false
                bool isValid = user != null;
                _logger.LogInformation("After: Login result for EmpCode {EmpCode}: {IsValid}", empcode, isValid);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating login for EmpCode: {empcode} , Pwd: {pwd}", empcode, pwd);
                return null;
            }
        }
    
    
    }
}
