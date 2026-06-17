using iTextSharp.text.pdf;
using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Utilities;
using System.Data;
using System.Data.Common;
using System.Drawing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Maruwa_Emgmt.DAL
{
    public class da_tblempmaster : i_tblempmaster
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_tblempmaster> _logger;
        private readonly IFtpService _ftp;
        private readonly IWebHostEnvironment _env;
        private readonly IProjectLocationService _ipls;

        private readonly string _liveDb;
        private readonly string _ERHMDb;

        public da_tblempmaster(ApplicationDbContext context, ILogger<da_tblempmaster> logger, IFtpService ftp, IProjectLocationService ipls, 
            IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure context is not null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));  // Ensure logger is not null
            _ftp = ftp ?? throw new ArgumentNullException(nameof(ftp));  // Ensure ftp is not null;
            _ipls = ipls;
            _liveDb = configuration.GetConnectionString("LiveHRMISConnection");//Running Database
            _ERHMDb = configuration.GetConnectionString("EHRMConnection");//Running Database
        }

        // Fetch all employee data from tblempmaster table
        public IEnumerable<empMaster> GetAllEmployeeData_Old()
        {
            try
            {
                var employees = _context.empmaster.AsNoTracking().Where(e => e.isResigned == "N" ).OrderByDescending(e => e.createdDate).ToList();
                _logger.LogInformation("Fetched {Count} employees from tblempmaster.", employees.Count);
                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching employees from tblempmaster.");
                return Enumerable.Empty<empMaster>();  // Return empty collection in case of error
            }
        }

        // Get the last empcode from tblempmaster
        public string GetLastEmpCode()
        {
            try
            {
                var lastEmpCode = _context.empmaster//tblempmaster
                    .OrderByDescending(e => e.empcode)  // Order by empcode in descending order
                    .Select(e => e.empcode)  // Select only the empcode
                    .FirstOrDefault();  // Get the first result (the last empcode)

                return lastEmpCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching the last employee code.");
                return string.Empty;  // Return empty string in case of error
            }
        }

        // Validate login information using empcode and password
        public empMaster? ValidateLoginInfo(string empcode, string pwd)
        {
            try
            {
                // Log the connection string (for debugging purposes only, remove in production)
                var connectionString = _context.Database.GetDbConnection().ConnectionString;
                //_logger.LogInformation("Connection String: {ConnectionString}", connectionString);

                // Fetch user by em`pcode and password
                var user = _context.empmaster
                    .AsNoTracking().Where(e => e.empcode == empcode && e.pwd == pwd && e.isResigned == "N")
                    .Select(e => new empMaster
                    {
                        empcode = e.empcode,
                        empName = e.empName,
                        designation = e.designation
                        // add other string fieldoths you need here
                    })
                    .FirstOrDefault();
                // Log whether the login is valid or not
                bool isValid = user != null;
                _logger.LogInformation("Login result for EmpCode {EmpCode}: {IsValid}", empcode, isValid);
                return user;  // Return user data if found
            }
            catch (Exception ex)
            {
                // ⚠️ Do NOT log password
                _logger.LogError(ex, "Error validating login for EmpCode: {empcode}", empcode);
                return null;  // Return null if error occurs
            }
        }

        // Method to fetch house numbers from the database
        public List<tblhousenumber> GetHouseNumbersByHostel(string housecode)
        {
            try
            {
                // Fetch house numbers from the database where isActive is true
                var houseNumbers = _context.tblhousenumber
                                            .Where(h => h.housecode == housecode && h.isActive == true)
                                            .Select(h => new tblhousenumber
                                            {
                                                housecode = h.housecode,
                                                housenumber = h.housenumber  // Assuming housenumber is the desired property
                                            })
                                            .ToList();

                return houseNumbers;  // Return the list of house numbers
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching house Code: {ex.Message}", "House Code: {housecode}", housecode);
                return new List<tblhousenumber>();
            }
        }

        #region New Connept
        public IEnumerable<empMaster> GetAllEmployeeData()
        {
            try
            {
                var employees = _context.empmaster.AsNoTracking().Where(e => e.isResigned == "N").OrderByDescending(e => e.createdDate).ToList();
                _logger.LogInformation("Fetched {Count} employees from tblempmaster.", employees.Count);
                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching employees from tblempmaster.");
                return Enumerable.Empty<empMaster>();  // Return empty collection in case of error
            }
        }

        public Task<DataSet> GetAllEmployeeList()
        {
            var ds = new DataSet();
            try
            {
                using (SqlConnection con = new SqlConnection(_context.Database.GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand("getEmployeeList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds); // ✅ direct call
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in GetAllEmployeeList");
                throw;
            }

            return Task.FromResult(ds);
        }

        #endregion
        public async Task<string> SaveEmployeeAsync(EmployeeSaveVM model, IFormFile img, IFormFile icfile, IFormFile passportfile, IFormFile professionaldoc, IFormFile resume, IFormFile academicCertificate, IFormFile otherCertificates)
        {
            SqlConnection con = null;
            SqlTransaction tran = null;
            try
            {
                con = new SqlConnection(_ERHMDb);
                await con.OpenAsync();
                tran = con.BeginTransaction();

                using (SqlCommand cmd = new SqlCommand("sp_SaveEmployee", con, tran))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    #region File Upload and Make URL
                    if (img != null && img.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await img.CopyToAsync(ms);
                        model.empMaster.photo = ms.ToArray();
                    }

                    string filePrefix = model.empMaster.empName?.Length > 10 ? model.empMaster.empName.Substring(0, 10) : model.empMaster.empName ?? "gANDi";

                    if (icfile != null && icfile.Length > 0)
                        model.empMaster.icFilePath = await UploadIfExists(icfile, model.empMaster.empcode, filePrefix);
                    if (passportfile != null && passportfile.Length > 0)
                        model.empMaster.passportfileurl = await UploadIfExists(passportfile, model.empMaster.empcode, filePrefix);
                    if (professionaldoc != null && professionaldoc.Length > 0)
                        model.empMaster.professionalDocurl = await UploadIfExists(professionaldoc, model.empMaster.empcode, filePrefix);
                    if (resume != null && resume.Length > 0)
                        model.empMaster.resumeDocURL = await UploadIfExists(resume, model.empMaster.empcode, filePrefix);
                    if (academicCertificate != null && academicCertificate.Length > 0)
                        model.empMaster.academicCertificateUrl = await UploadIfExists(academicCertificate, model.empMaster.empcode, filePrefix);
                    if (otherCertificates != null && otherCertificates.Length > 0)
                        model.empMaster.otherCertificateUrl = await UploadIfExists(otherCertificates, model.empMaster.empcode, filePrefix);// "ftp://192.168.0.245/c/ehrms/16122025170954058.pdf";
                    #endregion

                    #region -------- EMP MASTER -------- (62-Columns)
                    //5
                    cmd.Parameters.Add("@empcode", SqlDbType.VarChar).Value = model.empMaster.empcode ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@empName", SqlDbType.VarChar).Value = model.empMaster.empName ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@dateOfBirth", SqlDbType.Date).Value = model.empMaster.dateOfBirth ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@gender", SqlDbType.VarChar).Value = model.empMaster.gender ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@dateOfJoin", SqlDbType.Date).Value = model.empMaster.dateOfJoin ?? (object)DBNull.Value;
                    //10
                    cmd.Parameters.Add("@contractperiod", SqlDbType.VarChar).Value = model.empMaster.contractperiod ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@empType", SqlDbType.VarChar).Value = model.empMaster.empType ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@nationality", SqlDbType.VarChar).Value = model.empMaster.nationality ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@isForeign", SqlDbType.VarChar).Value = model.empMaster.isForeign ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@bloodGroup", SqlDbType.VarChar).Value = model.empMaster.bloodGroup ?? (object)DBNull.Value;
                    //15
                    cmd.Parameters.Add("@icNo", SqlDbType.VarChar).Value = model.empMaster.icNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@emailID", SqlDbType.VarChar).Value = model.empMaster.emailID ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@mobileNo", SqlDbType.VarChar).Value = model.empMaster.mobileNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@religion", SqlDbType.VarChar).Value = model.empMaster.religion ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@race", SqlDbType.VarChar).Value = model.empMaster.race ?? (object)DBNull.Value;
                    //20
                    cmd.Parameters.Add("@maritalStatus", SqlDbType.VarChar).Value = model.empMaster.maritalStatus ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@spouseName", SqlDbType.VarChar).Value = model.empMaster.spouseName ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@spouseWorking", SqlDbType.VarChar).Value = model.empMaster.spouseWorking ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@noOfChildren", SqlDbType.VarChar).Value = model.empMaster.noOfChildren ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@permanentAddress", SqlDbType.VarChar).Value = model.empMaster.permanentAddress ?? (object)DBNull.Value;
                    //25
                    cmd.Parameters.Add("@currentAddress", SqlDbType.VarChar).Value = model.empMaster.currentAddress ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@dateOfService", SqlDbType.Date).Value = model.empMaster.dateOfService ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@leaveLevel", SqlDbType.VarChar).Value = model.empMaster.leaveLevel ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@dateOfTerimination", SqlDbType.Date).Value = model.empMaster.dateOfTerimination ?? (object)DBNull.Value;// Wait
                    cmd.Parameters.Add("@epf", SqlDbType.VarChar).Value = model.empMaster.epf ?? (object)DBNull.Value;
                    //30
                    cmd.Parameters.Add("@sosco", SqlDbType.VarChar).Value = model.empMaster.sosco ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@taxNo", SqlDbType.VarChar).Value = model.empMaster.taxNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@workingExp", SqlDbType.VarChar).Value = model.empMaster.workingExp ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@others", SqlDbType.VarChar).Value = model.empMaster.others ?? (object)DBNull.Value;// URL
                    //34
                    cmd.Parameters.Add("@isResigned", SqlDbType.VarChar).Value = "N";
                    #region isOperator Validation
                    if (!string.IsNullOrWhiteSpace(model.empMaster.designation))
                    {
                        if ((model.empMaster.designation == "OPR" || model.empMaster.designation == "L.OPT" || model.empMaster.designation == "Local Oper") ||
                            (model.empMaster.designation == "Operator" || model.empMaster.designation == "LOCAL OPT" || model.empMaster.designation == "Local Operators"))
                        {
                            model.empMaster.isOperator = "Y";
                        }
                        else
                        {
                            model.empMaster.isOperator = "N";
                        }
                    }

                    cmd.Parameters.Add("@isOperator", SqlDbType.VarChar).Value = model.empMaster.isOperator ?? (object)DBNull.Value;
                    #endregion

                    cmd.Parameters.Add("@emergencyContactPerson", SqlDbType.VarChar).Value = model.empMaster.emergencyContactPerson ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@emergencyAddress", SqlDbType.VarChar).Value = model.empMaster.emergencyAddress ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@emergencyContact", SqlDbType.VarChar).Value = model.empMaster.emergencyContact ?? (object)DBNull.Value;
                    //39
                    cmd.Parameters.Add("@relation", SqlDbType.VarChar).Value = model.empMaster.relation ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@department", SqlDbType.VarChar).Value = model.empMaster.department ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@subDepartment", SqlDbType.VarChar).Value = model.empMaster.subDepartment ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@section", SqlDbType.VarChar).Value = model.empMaster.section ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@designation", SqlDbType.VarChar).Value = model.empMaster.designation ?? (object)DBNull.Value;

                    //44
                    cmd.Parameters.Add("@isStayingHostel", SqlDbType.VarChar).Value = model.empMaster.isStayingHostel ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@hostelName", SqlDbType.VarChar).Value = model.empMaster.hostelName ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@houseNo", SqlDbType.VarChar).Value = model.empMaster.houseNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@isTransport", SqlDbType.VarChar).Value = model.empMaster.isTransport ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@route", SqlDbType.VarChar).Value = model.empMaster.route ?? (object)DBNull.Value;
                    //49
                    cmd.Parameters.Add("@isDrivingLicense", SqlDbType.VarChar).Value = model.empMaster.isDrivingLicense ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@License", SqlDbType.VarChar).Value = model.empMaster.License ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@vehicleNo1", SqlDbType.VarChar).Value = model.empMaster.vehicleNo1 ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@vehicleNo2", SqlDbType.VarChar).Value = model.empMaster.vehicleNo2 ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@vehicleNo3", SqlDbType.VarChar).Value = model.empMaster.vehicleNo3 ?? (object)DBNull.Value;
                    //54
                    cmd.Parameters.Add("@bankName", SqlDbType.VarChar).Value = model.empMaster.bankName ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@accountNumber", SqlDbType.VarChar).Value = model.empMaster.accountNumber ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@educationLevel", SqlDbType.VarChar).Value = model.empMaster.educationLevel ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@languageProficiency", SqlDbType.VarChar).Value = model.empMaster.languageProficiency ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@proficiencyLevel", SqlDbType.VarChar).Value = model.empMaster.proficiencyLevel ?? (object)DBNull.Value;
                    //59
                    cmd.Parameters.Add("@fieldOfStudy", SqlDbType.VarChar).Value = model.empMaster.fieldOfStudy ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@otherQualifications", SqlDbType.VarChar).Value = model.empMaster.otherQualifications ?? (object)DBNull.Value;// URL
                    cmd.Parameters.Add("@photo", SqlDbType.VarBinary).Value = model.empMaster.photo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@professionalDocurl", SqlDbType.VarChar).Value = model.empMaster.professionalDocurl ?? (object)DBNull.Value;// URL
                    cmd.Parameters.Add("@resumeDocURL", SqlDbType.VarChar).Value = model.empMaster.resumeDocURL ?? (object)DBNull.Value;// URL
                    //63
                    cmd.Parameters.Add("@academicCertificateUrl", SqlDbType.VarChar).Value = model.empMaster.academicCertificateUrl ?? (object)DBNull.Value;// URL
                    cmd.Parameters.Add("@otherCertificateUrl", SqlDbType.VarChar).Value = model.empMaster.otherCertificateUrl ?? (object)DBNull.Value;// URL
                    cmd.Parameters.Add("@icFilePath", SqlDbType.VarChar).Value = model.empMaster.icFilePath ?? (object)DBNull.Value;// URL
                    cmd.Parameters.Add("@passportNo", SqlDbType.VarChar).Value = model.empMaster.passportNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@createdBy", SqlDbType.VarChar).Value = model.empMaster.createdBy ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@modifiedBy", SqlDbType.VarChar).Value = model.empMaster.modifiedBy ?? (object)DBNull.Value;


                    #endregion

                    #region -------- UNIFORM -------- (16-Columns)
                    cmd.Parameters.Add("@jacketSize", SqlDbType.VarChar).Value = model.uniformInfo.jacketSize ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@jacketqty", SqlDbType.VarChar).Value = model.uniformInfo.jacketqty ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@pantSize", SqlDbType.VarChar).Value = model.uniformInfo.pantSize ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@pantqty", SqlDbType.VarChar).Value = model.uniformInfo.pantqty ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@tShirtSize", SqlDbType.VarChar).Value = model.uniformInfo.tShirtSize ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@tShirtqty", SqlDbType.VarChar).Value = model.uniformInfo.tShirtqty ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@safetyShoes", SqlDbType.VarChar).Value = model.uniformInfo.safetyShoes ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@safetyShoesqty", SqlDbType.VarChar).Value = model.uniformInfo.safetyShoesqty ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@capColor", SqlDbType.VarChar).Value = model.uniformInfo.capColor ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@capColorqty", SqlDbType.VarChar).Value = model.uniformInfo.capColorqty ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@safetyBeltSize", SqlDbType.VarChar).Value = model.uniformInfo.safetyBeltSize ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@safetyBeltqty", SqlDbType.VarChar).Value = model.uniformInfo.safetyBeltqty ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@earMuffqty", SqlDbType.VarChar).Value = model.uniformInfo.earMuffqty ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@safetyHelmetqty", SqlDbType.VarChar).Value = model.uniformInfo.safetyHelmetqty ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@apronQty", SqlDbType.VarChar).Value = model.uniformInfo.apronQty ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@dateOfIssue", SqlDbType.Date).Value = model.uniformInfo.dateOfIssue ?? (object)DBNull.Value;

                    #endregion

                    #region  -------- ACCESSORIES -------- (34-Columns)

                    cmd.Parameters.Add("@cpmHandPhone", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmHandPhone ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmHandphoneBrand", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmHandphoneBrand ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmHandphoneSerialNo", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmHandphoneSerialNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmHandphoneIssuedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmHandphoneIssuedDate ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmHandphoneReturnedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmHandphoneReturnedDate ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@cpmLaptop", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmLaptop ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmLaptopBrand", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmLaptopBrand ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmLaptopSerialNo", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmLaptopSerialNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmLaptopIssuedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmLaptopIssuedDate ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmLaptopReturnedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmLaptopReturnedDate ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@cpmTablet", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmTablet ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmTabletBrand", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmTabletBrand ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmTabletSerialNo", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmTabletSerialNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmTabletIssuedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmTabletIssuedDate ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmTabletReturnedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmTabletReturnedDate ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@cpmSimcard", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmSimcard ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmSimcardBrand", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmSimcardBrand ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmSimcardSerialNo", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmSimcardSerialNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmSimcardIssuedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmSimcardIssuedDate ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmSimcardReturnedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmSimcardReturnedDate ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@cpmWalkieTalkie", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmWalkieTalkie ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmWalkieTalkieBrand", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmWalkieTalkieBrand ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmWalkieTalkieSerialNo", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmWalkieTalkieSerialNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmWalkieTalkieIssuedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmWalkieTalkieIssuedDate ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmWalkieTalkieReturnedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmWalkieTalkieReturnedDate ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@cpmInternetAccess", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmInternetAccess ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmInternetAccessBrand", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmInternetAccessBrand ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmInternetAccessSerialNo", SqlDbType.VarChar).Value = model.accessoriesInfo.cpmInternetAccessSerialNo ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmInternetAccessIssuedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmInternetAccessIssuedDate ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@cpmInternetAccessReturnedDate", SqlDbType.Date).Value = model.accessoriesInfo.cpmInternetAccessReturnedDate ?? (object)DBNull.Value;

                    cmd.Parameters.Add("@windowsLoginID", SqlDbType.VarChar).Value = model.accessoriesInfo.windowsLoginID ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@windowsLoginIDIssuedDate", SqlDbType.VarChar).Value = model.accessoriesInfo.windowsLoginIDIssuedDate ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@companyEmail", SqlDbType.VarChar).Value = model.accessoriesInfo.companyEmail ?? (object)DBNull.Value;
                    cmd.Parameters.Add("@companyEmailIssuedDate", SqlDbType.Date).Value = model.accessoriesInfo.companyEmailIssuedDate ?? (object)DBNull.Value;

                    #endregion


                    // --- OUTPUT parameter ---
                    var finalEmpCodeParam = new SqlParameter("@finalEmpCode", SqlDbType.NVarChar, 20)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(finalEmpCodeParam);

                    int result = await cmd.ExecuteNonQueryAsync();
                    tran.Commit();  // Commit

                    return finalEmpCodeParam.Value.ToString();// --- Return output parameter ---
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while Insert/Update employees EHRMS.dbo.empmaster.");
                if (tran != null) tran.Rollback();// Rollback if anything fails
                throw new Exception("Error saving employee data", ex);  // Re-throw for Controller / BAL
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                    await con.CloseAsync();
            }
        }

        private async Task<string?> UploadIfExists(IFormFile file, string empcode, string filePrefix)
        {
            try
            {
                if (file == null || file.Length == 0) return null;
                //return await _ftp.UploadAsync(file); // FTP Server code (while storing the files in FTP-server throwing unable to Connection
                if (_ipls == null)
                    throw new InvalidOperationException("ProjectLocationService not initialized");
                return await _ipls.UploadAsync(file, empcode, filePrefix);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching house Code: {ex.Message}");
                return null; // or rethrow if needed
            }
        }
        public async Task<EmployeeSaveVM?> GetByEmpCodeAsync(string empcode)
        {
            try
            {
                if (string.IsNullOrEmpty(empcode))
                    return null;

                var vm = new EmployeeSaveVM
                {
                    empMaster = new empMaster(),
                    uniformInfo = new uniformInfo(),
                    accessoriesInfo = new accessoriesInfo()
                };

                using (var conn = _context.Database.GetDbConnection())
                {
                    await conn.OpenAsync();

                    using (var cmd = conn.CreateCommand())
                    {
                        //cmd.CommandText = "SELECT * FROM dbo.getEmpDetailsByID(@empcode)";
                        cmd.CommandText = "SELECT * FROM dbo.getEmpDetailsByID_NEW(@empcode)";
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.Add(new SqlParameter("@empcode", empcode));

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            // If no row returned
                            if (!await reader.ReadAsync())
                                return null;
                            
                            #region // --- Map empMaster fields ---

                            vm.empMaster.empcode = reader["empCode"] as string;
                            vm.empMaster.empName = reader["empName"] as string;
                            vm.empMaster.department = reader["department"] as string;
                            vm.empMaster.subDepartment = reader["subDepartment"] as string;
                            vm.empMaster.section = reader["section"] as string;
                            vm.empMaster.dateOfBirth = reader["dateOfBirth"] as DateTime?;
                            vm.empMaster.gender = reader["gender"] as string;
                            vm.empMaster.dateOfJoin = reader["dateOfJoin"] as DateTime?;
                            vm.empMaster.empType = reader["empType"] as string;
                            vm.empMaster.nationality = reader["nationality"] as string;
                            vm.empMaster.isForeign = reader["isForeign"] as string;
                            vm.empMaster.bloodGroup = reader["bloodGroup"] as string;
                            vm.empMaster.icNo = reader["icNo"] as string;
                            vm.empMaster.emailID = reader["emailID"] as string;
                            vm.empMaster.mobileNo = reader["mobileNo"] as string;
                            vm.empMaster.maritalStatus = reader["maritalStatus"] as string;
                            vm.empMaster.spouseName = reader["spouseName"] as string;
                            vm.empMaster.permanentAddress = reader["permanentAddress"] as string;
                            vm.empMaster.currentAddress = reader["currentAddress"] as string;
                            vm.empMaster.epf = reader["epf"] as string;
                            vm.empMaster.sosco = reader["sosco"] as string;
                            vm.empMaster.taxNo = reader["taxNo"] as string;
                            vm.empMaster.relation = reader["relation"] as string;
                            vm.empMaster.designation = reader["designation"] as string;
                            vm.empMaster.isStayingHostel = reader["isStayingHostel"] as string;
                            vm.empMaster.hostelName = reader["hostelName"] as string;
                            vm.empMaster.houseNo = reader["houseNo"] as string;
                            vm.empMaster.isTransport = reader["isTransport"] as string;
                            vm.empMaster.route = reader["route"] as string;
                            vm.empMaster.isDrivingLicense = reader["isDrivingLicense"] as string;
                            vm.empMaster.bankName = reader["bankName"] as string;
                            vm.empMaster.educationLevel = reader["educationLevel"] as string;
                            vm.empMaster.languageProficiency = reader["languageProficiency"] as string;
                            vm.empMaster.proficiencyLevel = reader["proficiencyLevel"] as string;
                            vm.empMaster.accountNumber = reader["accountNumber"] as string;
                            vm.empMaster.religion = reader["Religion"] as string;
                            vm.empMaster.race = reader["Race"] as string;
                            vm.empMaster.emergencyContactPerson = reader["emergencyContactPerson"] as string;
                            vm.empMaster.emergencyAddress = reader["emergencyAddress"] as string;
                            vm.empMaster.emergencyContact = reader["emergencyContact"] as string;
                            vm.empMaster.fieldOfStudy = reader["fieldOfStudy"] as string;
                            vm.empMaster.others = reader["others"] as string;
                            vm.empMaster.contractperiod = reader["contractperiod"] as string;
                            vm.empMaster.License = reader["License"] as string;
                            vm.empMaster.vehicleNo1 = reader["vehicleNo1"] as string;
                            vm.empMaster.vehicleNo2 = reader["vehicleNo2"] as string;
                            vm.empMaster.vehicleNo3 = reader["vehicleNo3"] as string;
                            vm.empMaster.passportNo = reader["passportNo"] as string;
                            vm.empMaster.photo = reader["photo"] != DBNull.Value ? (byte[])reader["photo"] : null;// Safe conversion from database to byte[]?
                            vm.empMaster.professionalDocurl = reader["professionaldocurl"] as string;
                            vm.empMaster.resumeDocURL = reader["resumeDocURL"] as string;
                            vm.empMaster.academicCertificateUrl = reader["academicCertificateurl"] as string;
                            vm.empMaster.passportfileurl = reader["passportfileurl"] as string;
                            vm.empMaster.icFilePath = reader["icFilePath"] as string;

                            #endregion

                            #region --- Map Uniform Info fields ---
                            
                            vm.uniformInfo.empcode = vm.empMaster.empcode;
                            vm.uniformInfo.jacketSize = reader["jacketSize"] as string;
                            vm.uniformInfo.jacketqty = reader["jacketqty"] as string;
                            vm.uniformInfo.pantSize = reader["pantSize"] as string;
                            vm.uniformInfo.pantqty = reader["pantqty"] as string;
                            vm.uniformInfo.tShirtSize = reader["tShirtSize"] as string;
                            vm.uniformInfo.tShirtqty = reader["tShirtqty"] as string;
                            vm.uniformInfo.safetyShoes = reader["safetyShoes"] as string;
                            vm.uniformInfo.safetyShoesqty = reader["safetyShoesqty"] as string;
                            vm.uniformInfo.capColor = reader["capColor"] as string;
                            vm.uniformInfo.capColorqty = reader["capColorqty"] as string;
                            vm.uniformInfo.safetyBeltSize = reader["safetyBeltSize"] as string;
                            vm.uniformInfo.safetyBeltqty = reader["safetyBeltqty"] as string;
                            vm.uniformInfo.earMuffqty = reader["earMuffqty"] as string;
                            vm.uniformInfo.safetyHelmetqty = reader["safetyHelmetqty"] as string;
                            vm.uniformInfo.apronQty = reader["apronQty"] as string;
                            vm.uniformInfo.dateOfIssue = reader["dateOfIssue"] as DateTime?;

                            #endregion


                            #region // --- Map Accessories Info fields ---


                            vm.accessoriesInfo.empcode = vm.empMaster.empcode;
                            vm.accessoriesInfo.cpmHandPhone = reader["cpmHandPhone"] as string;
                            vm.accessoriesInfo.cpmHandphoneBrand = reader["cpmHandphoneBrand"] as string;

                            vm.accessoriesInfo.cpmLaptop = reader["cpmLaptop"] as string;
                            vm.accessoriesInfo.cpmLaptopIssuedDate =reader["cpmLaptopIssuedDate"] == DBNull.Value? null : Convert.ToDateTime(reader["cpmLaptopIssuedDate"]); 

                            vm.accessoriesInfo.cpmTablet = reader["cpmTablet"] as string;
                            vm.accessoriesInfo.cpmTabletIssuedDate = reader["cpmTabletIssuedDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["cpmTabletIssuedDate"]);

                            vm.accessoriesInfo.cpmSimcard = reader["cpmSimcard"] as string;
                            vm.accessoriesInfo.cpmSimcardIssuedDate = reader["cpmSimcardIssuedDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["cpmSimcardIssuedDate"]);
                           
                            vm.accessoriesInfo.cpmInternetAccess = reader["cpmInternetAccess"] as string;
                            vm.accessoriesInfo.cpmInternetAccessIssuedDate = reader["cpmInternetAccessIssuedDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["cpmInternetAccessIssuedDate"]);

                            vm.accessoriesInfo.cpmWalkieTalkie = reader["cpmWalkieTalkie"] as string;
                            vm.accessoriesInfo.cpmWalkieTalkieIssuedDate = reader["cpmWalkieTalkieIssuedDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["cpmWalkieTalkieIssuedDate"]);

                            vm.accessoriesInfo.cpmHandphoneSerialNo = reader["cpmHandphoneSerialNo"] as string;
                            vm.accessoriesInfo.cpmHandphoneIssuedDate = reader["cpmHandphoneIssuedDate"] as DateTime?;
                            vm.accessoriesInfo.cpmHandphoneReturnedDate = reader["cpmHandphoneReturnedDate"] as DateTime?;

                            vm.accessoriesInfo.windowsLoginID = reader["windowsLoginID"] as string;
                            vm.accessoriesInfo.windowsLoginIDIssuedDate = reader["windowsLoginIDIssuedDate"] as DateTime?;

                            vm.accessoriesInfo.companyEmail = reader["companyEmail"] as string;
                            vm.accessoriesInfo.companyEmailIssuedDate = reader["companyEmailIssuedDate"] as DateTime?;

                            // map other accessories info fields as needed

                            #endregion
                        }
                    }
                }

                return vm;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching employee details with EmpCode: {EmpCode}", empcode);
                throw;
            }
        }

    }
}
