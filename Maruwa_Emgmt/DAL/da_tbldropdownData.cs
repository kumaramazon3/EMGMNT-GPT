using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.master;
using Maruwa_Emgmt.Models.TM;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static System.Collections.Specialized.BitVector32;

namespace Maruwa_Emgmt.DAL
{
    public class da_tbldropdownData : i_tbldropdownData
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_tbldropdownData> _logger;

        // Constructor that accepts ApplicationDbContext and ILogger<da_tbldropdownData> via DI
        public da_tbldropdownData(ApplicationDbContext context, ILogger<da_tbldropdownData> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure context is injected
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));   // Ensure logger is injected
        }

        // Fetch all active dropdown data
        public IEnumerable<tbldropdownData> GetAllDropdownData_Old()
        {
            try
            {
                var dropdownData = _context.tbldropdownData
                    .Where(x => x.isActive == true)  // Filter active records
                    .ToList();

                _logger.LogInformation("Fetched {Count} records from tbldropdownData.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception while fetching data from tbldropdownData: " + ex);
                return Enumerable.Empty<tbldropdownData>(); // Return an empty collection on error
            }
        }

        public async Task<IEnumerable<tbldropdownData>> GetAllDropdownDataAsync()
        {
            try
            {
                // Use ToListAsync() for async EF Core query
                var dropdownData = await _context.tbldropdownData
                    .Where(x => x.isActive == true)  // Filter active records
                    .ToListAsync();

                _logger.LogInformation("Fetched {Count} records from tbldropdownData.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from tbldropdownData.");
                return Enumerable.Empty<tbldropdownData>(); // Return empty collection on error
            }
        }

        public IEnumerable<tblsections> GetSectionsBySubDept(string SubDeptCode)
        {
            try
            {
                var _tblsections = _context.tblsections
                                       .Where(s => s.aqiscode == SubDeptCode && s.isActive == true)
                                       .OrderBy(s => s.sectionname)  // Order by section name (ASC)
                                       .Select(s => new tblsections
                                       {
                                           sectioncode = s.sectioncode,
                                           sectionname = s.sectionname
                                       })
                                       .ToList();

                return _tblsections;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching sections for department code " + SubDeptCode, ex);
                return Enumerable.Empty<tblsections>();  // Return an empty list in case of error
            }
        }
        public IEnumerable<tblsections> GetSectionsByDepartment(string departmentCode)
        {
            try
            {
                // Query the database for sections related to the given department code
                var _tblsections = _context.tblsections
                                       .Where(s => s.departmentcode == departmentCode && s.isActive == true)
                                       .OrderBy(s => s.sectionname)  // Order by section name (ASC)
                                       .Select(s => new tblsections
                                       {
                                           sectioncode = s.sectioncode,
                                           sectionname = s.sectionname
                                       })
                                       .ToList();

                return _tblsections;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching sections for department code " + departmentCode, ex);
                return Enumerable.Empty<tblsections>();  // Return an empty list in case of error
            }
        }

        public IEnumerable<master_SubDepartment> GetSubDepartmentByDepartment(string departmentCode)
        {
            try
            {
                var _subdepts = _context.master_subdepartment
                                       .Where(s => s.departmentCode == departmentCode && s.isSubDeptActive == "1")
                                       .OrderBy(s => s.subDepartmentName) 
                                       .Select(s => new master_SubDepartment
                                       {
                                           subDepartmentCode = s.subDepartmentCode,
                                           subDepartmentName = s.subDepartmentName
                                       })
                                       .ToList();

                return _subdepts;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching sections for department code " + departmentCode, ex);
                return Enumerable.Empty<master_SubDepartment>();  // Return an empty list in case of error
            }
        }

        public List<object> GetLicenseTypes()
        {
            try
            {
                // Fetch only active records for tableName='tbllicenses'
                var licenses = _context.tbllicensetypes.Where(x => x.TableName == "tbllicenses" && x.isActive == true)
                    .Select(x => new
                    {
                        value = x.Code,        // code column as value
                        text = x.Description   // description column as text
                    })
                    .ToList<object>();

                return licenses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching license Types.");
                return new List<object>();// Return an empty list in case of error
            }
        }

        public async Task<List<tbldropdownData>> GetAlldropdownlistAsync()
        {
            try
            {
                var _drplst= await _context.tbldropdownData.Where(s => s.isActive == true)
                    .Select(x => new tbldropdownData
                    {
                        Code = x.Code,
                        Description = x.Description,
                        TableName = x.TableName
                    })
                    .ToListAsync();

                return _drplst;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching (From GetAlldropdownlistAsync()) sections.", ex);
                throw; // preserves stack trace
            }
        }

        #region All Drop Down Get Data
        public async Task<List<master_Nationality>> Getmaster_Nationality()
        {
            try
            {
                //var dropdownData = await _context.master_nationality.ToListAsync();
                var dropdownData = await _context.master_nationality.AsNoTracking()
                                  .Select(n => new master_Nationality
                                  {
                                      nationCOde = n.nationCOde,
                                      nationName = n.nationName
                                  }).OrderBy(n => n.nationName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_nationality.",dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Exception while fetching data from master_nationality.");
                return new List<master_Nationality>(); // return empty list
            }
        }
        public async Task<List<master_BloodGroup>> Getmaster_BloodGroup()
        {
            try
            {
                //var dropdownData = await _context.master_nationality.ToListAsync();
                var dropdownData = await _context.master_bloodgroup.AsNoTracking()
                                  .Select(n => new master_BloodGroup
                                  {
                                      bloodGroupCode = n.bloodGroupCode,
                                      bloodGroup = n.bloodGroup
                                  }).OrderBy(n => n.bloodGroup).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from Getmaster_BloodGroup.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from Getmaster_BloodGroup.");
                return new List<master_BloodGroup>(); // return empty list
            }
        }
        public async Task<List<master_Bank>> Getmaster_Bank()
        {
            try
            {
                var dropdownData = await _context.master_bank.AsNoTracking()
                                  .Select(n => new master_Bank
                                  {
                                      bankCode = n.bankCode,
                                      bankName = n.bankName
                                  }).OrderBy(n => n.bankName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_Bank.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from master_Bank.");
                return new List<master_Bank>(); // return empty list
            }
        }
        public async Task<List<master_Department>> Getmaster_Department()
        {
            try
            {
                var dropdownData = await _context.master_department.AsNoTracking()
                                  .Select(n => new master_Department
                                  {
                                      departmentCode = n.departmentCode,
                                      departmentName = n.departmentName
                                  }).OrderBy(n => n.departmentName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_Department.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from master_Department.");
                return new List<master_Department>(); // return empty list
            }
        }
        public async Task<List<master_Education>> Getmaster_Education()
        {
            try
            {
                var dropdownData = await _context.master_education.AsNoTracking()
                                  .Select(n => new master_Education
                                  {
                                      educationCode = n.educationCode,
                                      educationName = n.educationName
                                  }).OrderBy(n => n.educationName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_Education.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from master_Education.");
                return new List<master_Education>(); // return empty list
            }
        }
        public async Task<List<master_Hostal>> Getmaster_Hostal()
        {
            try
            {
                var dropdownData = await _context.master_hostal.AsNoTracking()
                                  .Select(n => new master_Hostal
                                  {
                                      hostalCode = n.hostalCode,
                                      hostalName = n.hostalName
                                  }).OrderBy(n => n.hostalName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_Hostal.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from master_Hostal.");
                return new List<master_Hostal>(); // return empty list
            }
        }
        public async Task<List<master_Language>> Getmaster_Language()
        {
            try
            {
                var dropdownData = await _context.master_language.AsNoTracking()
                                  .Select(n => new master_Language
                                  {
                                      languageCode = n.languageCode,
                                      languageName = n.languageName
                                  }).OrderBy(n => n.languageName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_Language.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from master_Language.");
                return new List<master_Language>(); // return empty list
            }
        }
        public async Task<List<master_License>> Getmaster_License()
        {
            try
            {
                var dropdownData = await _context.master_license.AsNoTracking()
                                  .Select(n => new master_License
                                  {
                                      licenseCode = n.licenseCode,
                                      licenseName = n.licenseName
                                  }).OrderBy(n => n.licenseName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_License.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from master_License.");
                return new List<master_License>(); // return empty list
            }
        }
        public async Task<List<master_Race>> Getmaster_Race()
        {
            try
            {
                var dropdownData = await _context.master_race.AsNoTracking()
                                  .Select(n => new master_Race
                                  {
                                      raceCode = n.raceCode,
                                      raceName = n.raceName
                                  }).OrderBy(n => n.raceName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_Race.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from master_Race.");
                return new List<master_Race>(); // return empty list
            }
        }
        public async Task<List<master_Religion>> Getmaster_Religion()
        {
            try
            {
                var dropdownData = await _context.master_religion.AsNoTracking()
                                  .Select(n => new master_Religion
                                  {
                                      religionCode = n.religionCode,
                                      religionName = n.religionName
                                  }).OrderBy(n => n.religionName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from religionName.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from religionName.");
                return new List<master_Religion>(); // return empty list
            }
        }
        public async Task<List<master_Route>> Getmaster_Route()
        {
            try
            {
                var dropdownData = await _context.master_route.AsNoTracking()
                                  .Select(n => new master_Route
                                  {
                                      routeCode = n.routeCode,
                                      routeName = n.routeName
                                  }).OrderBy(n => n.routeName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_Route.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from master_Route.");
                return new List<master_Route>(); // return empty list
            }
        }
        public async Task<List<master_SubDepartment>> Getmaster_SubDepartment()
        {
            try
            {
                var dropdownData = await _context.master_subdepartment.AsNoTracking()
                                  .Select(n => new master_SubDepartment
                                  {
                                      subDepartmentCode = n.subDepartmentCode,
                                      subDepartmentName = n.subDepartmentName
                                  }).OrderBy(n => n.subDepartmentName).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_SubDepartment.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from master_SubDepartment.");
                return new List<master_SubDepartment>(); // return empty list
            }
        }
        public async Task<List<Models.master_Designation>> Getmaster_Designation()
        {
            try
            {
                var dropdownData = await _context.master_designation.AsNoTracking().Where(n => n.isActive == true)
                                  .Select(n => new Models.master_Designation
                                  {
                                      desigcode = n.desigcode,
                                      designationname = n.designationname
                                  }).OrderBy(n => n.designationname).ToListAsync();

                _logger.LogInformation("Fetched {Count} records from master_SubDmaster_Designationepartment.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from master_Designation.");
                return new List<Models.master_Designation>(); // return empty list
            }
        }
        public IEnumerable<master_SubDepartment> GetSubDepartmentsByDepartment(string departmentCode)
        {
            try
            {
                var _subDepartments = _context.master_subdepartment
                                       .Where(s => s.departmentCode == departmentCode)// && s.issectionActive == "1")
                                       .OrderBy(s => s.subDepartmentName)  // Order by section name (ASC)
                                       .Select(s => new master_SubDepartment
                                       {
                                           subDepartmentCode = s.subDepartmentCode,
                                           subDepartmentName = s.subDepartmentName
                                       })
                                       .ToList();

                return _subDepartments;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching sections for subDepartment code " + departmentCode, ex);
                return Enumerable.Empty<master_SubDepartment>();  // Return an empty list in case of error
            }
        }
        public IEnumerable<master_DepartmentSection> GetSubDeptSections(string SubDeptCode)
        {
            try
            {
                var _subDeptSections = _context.master_departmentsection
                                       .Where(s => s.subDepartmentCode == SubDeptCode)
                                       .OrderBy(s => s.sectionName)  // Order by section name (ASC)
                                       .Select(s => new master_DepartmentSection
                                       {
                                           sectionCode = s.sectionCode,
                                           sectionName = s.sectionName
                                       })
                                       .ToList();

                return _subDeptSections;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching sections for master_DepartmentSection code " + SubDeptCode, ex);
                return Enumerable.Empty<master_DepartmentSection>();  // Return an empty list in case of error
            }
        }
        public IEnumerable<master_DepartmentSection> GetSubDeptSectionsFromSkillMatrix(string SubDeptCode)
        {
            try
            {
                //var _subDeptSections = _context.master_departmentsection
                //         .Where(m => _context.skillmatrixdocumentdata
                //             .Any(s => s.sectionCode == m.sectionCode))
                //         .Select(m => new master_DepartmentSection
                //         {
                //             sectionCode = m.sectionCode,
                //             sectionName = m.sectionName
                //         })
                //         .Distinct()
                //         .OrderBy(m => m.sectionName)
                //         .ToList();
                var _subDeptSections = _context.master_departmentsection
                                      .Where(s => s.subDepartmentCode == SubDeptCode)
                                      .OrderBy(s => s.sectionName)  // Order by section name (ASC)
                                      .Select(s => new master_DepartmentSection
                                      {
                                          sectionCode = s.sectionCode,
                                          sectionName = s.sectionName
                                      })
                                      .ToList();

                return _subDeptSections;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching sections for master_DepartmentSection code " + SubDeptCode, ex);
                return Enumerable.Empty<master_DepartmentSection>();  // Return an empty list in case of error
            }
        }

        public async Task<DataSet> GetAllMasterDataAsync()
        {
            var ds = new DataSet();
            try
            {
                using (SqlConnection con = new SqlConnection(_context.Database.GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand("GetAllMasterData", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        await Task.Run(() => da.Fill(ds));
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while executing GetAllMasterData.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in GetAllMasterDataAsync.");
                throw;
            }
            return ds;
        }

        public async Task<IEnumerable<master_EmployeeType>> Getmaster_EmployeeType()
        {
            try
            {
                var dropdownData = await _context.master_employeetype
                    .AsNoTracking()
                    //.Where(n => n.isEmpActive == "1")
                    .Select(n => new master_EmployeeType
                    {
                        Empcode = n.Empcode,
                        EmpType = n.EmpType
                    })
                    .OrderBy(n => n.EmpType)
                    .ToListAsync();

                _logger.LogInformation(
                    "Fetched {Count} records from master_EmployeeType.",
                    dropdownData.Count
                );

                return dropdownData; // List<T> is OK here
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception while fetching data from master_EmployeeType."
                );

                return Enumerable.Empty<master_EmployeeType>();
            }
        }

        public async Task<List<trainingMaster>> GetTrainingMasterlistAsync()
        {
            try
            {
                var dropdownData = await _context.trainingmaster.AsNoTracking()
                                  .Select(n => new trainingMaster
                                  {
                                      titleName = n.titleName,
                                      code = n.code
                                  }).OrderBy(n => n.titleName).ToListAsync();

                //_logger.LogInformation("Fetched {Count} records from trainingMaster.", dropdownData.Count);
                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from trainingMaster.");
                return new List<trainingMaster>(); // return empty list
            }
        }

        public async Task<List<master_LeaveType>> GetLeaveTypeAsync()
        {
            try
            {
                var dropdownData = await _context.leavetype
                             .AsNoTracking()
                                  .Select(n => new master_LeaveType
                                  {
                                      LeaveType = n.LeaveType,
                                      LTCode = n.LTCode
                                  }).OrderBy(n => n.LeaveType).ToListAsync();

                return dropdownData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching data from GetAllLeaveTypesAsync().");
                return new List<master_LeaveType>(); // return empty list
            }
        }

        public async Task<List<LeaveTimingDto>> GetLeaveTimingsAsync()
        {
            try
            {
                var timings = await _context.LeaveTime
                    .AsNoTracking()
                    .OrderBy(n => n.FromTime)
                    .ToListAsync();

                return timings.Select(n => new LeaveTimingDto
                {
                    uid = n.uid,
                    FromTime = DateTime.Today.Add(n.FromTime).ToString("hh:mm tt"),
                    ToTime = DateTime.Today.Add(n.ToTime).ToString("hh:mm tt")
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching LeaveTimings.");
                throw; // 🔥 temporarily throw to see real error in browser
            }
        }

        public async Task<List<inChargeEmpMasterDto>> GetallEmployesAsync()
        {
            try
            {
                var timings = await _context.empmaster
                                .AsNoTracking()
                                 .Where(n => n.isResigned == "N")
                                .OrderBy(n => n.empName)
                                .Select(n => new inChargeEmpMasterDto
                                {
                                    empCode = n.empcode,
                                    empName = n.empName
                                }).ToListAsync();

                return timings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching GetallEmployesAsync.");
                throw; // 🔥 temporarily throw to see real error in browser
            }
        }

        #endregion

    }
}
