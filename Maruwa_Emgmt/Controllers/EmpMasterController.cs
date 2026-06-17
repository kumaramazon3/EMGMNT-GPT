using iTextSharp.text;
using iTextSharp.text.pdf;
using Maruwa_Emgmt.BAL;
using Maruwa_Emgmt.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using static iText.StyledXmlParser.Css.Font.CssFontFace;
using static System.Collections.Specialized.BitVector32;

namespace Maruwa_Emgmt.Controllers
{
    public class EmpMasterController : Controller
    {
        #region Local Variables
        private readonly bal_tblempmaster _empBal;
        private readonly bal_tbldropdownData _dropdownBal; 
        private readonly IWebHostEnvironment _env;
        public EmpMasterController(bal_tblempmaster empBal, bal_tbldropdownData dropdownBal, IWebHostEnvironment env)
        {
            _empBal = empBal;
            _dropdownBal = dropdownBal;  // Assign the new BAL to the field
            _env = env;
        }
        #endregion

        #region Pages
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult empList()
        {
            return View();
        }
        public IActionResult EmpRegisteration()
        {
            return View();
        }

        #endregion

        #region Get Master Data
        [HttpGet]
        public async Task<IActionResult> Getmaster_Nationality()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_Nationality> empList = await _dropdownBal.Getmaster_Nationality();
            return Json(empList);
        }
        [HttpGet]
        public async Task<IActionResult> Getmaster_BloodGroup()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_BloodGroup> empList = await _dropdownBal.Getmaster_BloodGroup();
            return Json(empList);
        }
        [HttpGet]
        public async Task<IActionResult> Getmaster_Bank()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_Bank> empList = await _dropdownBal.Getmaster_Bank();
            return Json(empList);
        }
        [HttpGet]
        public async Task<IActionResult> Getmaster_Education()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_Education> empList = await _dropdownBal.Getmaster_Education();
            return Json(empList);
        }
        [HttpGet]
        public async Task<IActionResult> Getmaster_Hostal()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_Hostal> empList = await _dropdownBal.Getmaster_Hostal();
            return Json(empList);
        }
        [HttpGet]
        public async Task<IActionResult> Getmaster_Language()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_Language> empList = await _dropdownBal.Getmaster_Language();
            return Json(empList);
        }
        [HttpGet]
        public async Task<IActionResult> Getmaster_Department()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_Department> empList = await _dropdownBal.Getmaster_Department();
            return Json(empList);
        }
        [HttpGet]
        public async Task<IActionResult> Getmaster_Race()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_Race> empList = await _dropdownBal.Getmaster_Race();
            return Json(empList);
        }
        [HttpGet]
        public async Task<IActionResult> Getmaster_Religion()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_Religion> empList = await _dropdownBal.Getmaster_Religion();
            return Json(empList);
        }
        [HttpGet]
        public async Task<IActionResult> Getmaster_Route()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_Route> empList = await _dropdownBal.Getmaster_Route();
            return Json(empList);
        }
        [HttpGet]
        public async Task<IActionResult> Getmaster_Designation()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_Designation> empList = await _dropdownBal.Getmaster_Designation();
            return Json(empList);
        }

        [HttpGet]
        public IActionResult GetSubDepartments(string departmentCode)
        {
            if (string.IsNullOrEmpty(departmentCode))
            {
                return BadRequest("Department code is required.");
            }

            var sections = _dropdownBal.GetSubDepartmentsByDepartment(departmentCode);
            return Json(sections);  // Return sections as JSON
        }

        [HttpGet]
        public IActionResult GetSubDeptSections(string SubDeptCode)
        {
            if (string.IsNullOrEmpty(SubDeptCode))
            {
                return BadRequest("Department code is required.");
            }

            var sections = _dropdownBal.GetSubDeptSections(SubDeptCode);
            return Json(sections);  // Return sections as JSON
        }

        #endregion

        #region Get Data

        [HttpGet]
        public async Task<IActionResult> GetAlldropdownData()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }

            // Assuming _dropdownBal.GetAllEmployeeData() can be made async
            IEnumerable<tbldropdownData> empList = await _dropdownBal.GetAllEmployeeDataAsync();
            return Json(empList);
        }

        [HttpGet]
        public IActionResult GetsubDepartment(string departmentCode)
        {
            if (string.IsNullOrEmpty(departmentCode))
            {
                return BadRequest("Department code is required.");
            }

            var subDepratment = _dropdownBal.GetSubDepartmentByDepartment(departmentCode);
            //return Json(sections); 
            return Json(subDepratment); 
        }
        [HttpGet]
        public IActionResult GetSectionsbySubDept(string SubDeptCode)
        {
            if (string.IsNullOrEmpty(SubDeptCode))
            {
                return BadRequest("Department code is required.");
            }

            var sections = _dropdownBal.GetSubDeptSections(SubDeptCode);//
            return Json(sections);  // Return sections as JSON
        }

        [HttpGet]
        public IActionResult GetSections(string departmentCode)
        {
            if (string.IsNullOrEmpty(departmentCode))
            {
                return BadRequest("Department code is required.");
            }

            // Call the BAL method to get sections for the given department code
            var sections = _dropdownBal.GetSectionsByDepartment(departmentCode);
            return Json(sections);  // Return sections as JSON
        }

        [HttpGet]
        public IActionResult GetLastEmpCode()
        {
            string last = _empBal.GetLastEmpCode();
            return Json(new { lastEmpCode = last });
        }

        [HttpGet]
        public JsonResult GetHouseNumbers(string housecode)
        {
            try
            {
                // Step 4: Call the BAL class to fetch house numbers
                var houseNumbers = _empBal.GetHouseNumbersByHostel(housecode);

                // Return the data as JSON without JsonRequestBehavior
                return new JsonResult(houseNumbers);
            }
            catch (Exception ex)
            {
                // Optionally, handle exceptions and return an error response
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetLicenseTypes()
        {
            try
            {
                var data = _dropdownBal.GetLicenseTypes();
                return Json(new { tableName = "tbllicenses", records = data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Getmaster_EmployeeType()
        {
            var data = await _dropdownBal.Getmaster_EmployeeType();
            return Ok(new { tableName = "master_EmpType", records = data });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployeeList()
        {
            var employeeDetails = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(employeeDetails))
                return Unauthorized("SessionExpired");

            var ds = await _empBal.GetAllEmployeeList();

            var table = ds.Tables[0];

            var list = table.AsEnumerable().Select(row => new
            {
                empcode = row["empcode"]?.ToString(),
                empName = row["empName"]?.ToString(),
                department = row["department"]?.ToString(),
                subDepartment = row["subDepartment"]?.ToString(),
                section = row["section"]?.ToString(),
                dateOfJoin = row["dateOfJoin"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["dateOfJoin"]),
                designation = row["designation"]?.ToString(),
                educationLevel = row["educationLevel"]?.ToString(),
                gender = row["gender"]?.ToString(),
                nationality = row["nationality"]?.ToString(),
                passportno = row.Table.Columns.Contains("passportno") ? row["passportno"]?.ToString() : null,
                newicno = row.Table.Columns.Contains("newicno") ? row["newicno"]?.ToString() : null,
                icNo = row.Table.Columns.Contains("icNo") ? row["icNo"]?.ToString() : null,
                bloodGroup = row.Table.Columns.Contains("bloodGroup") ? row["bloodGroup"]?.ToString() : null
            }).ToList();

            return Json(list); // ✅ JS-friendly JSON
        }

        [HttpGet]
        [Route("EmpMaster/EmpRegisteration/{id?}")]
        public async Task<IActionResult> EmpRegisteration(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var decodedBytes = Convert.FromBase64String(Uri.UnescapeDataString(id));
                    var empcode = Encoding.UTF8.GetString(decodedBytes);
                    ViewBag.EmpCode = empcode;

                    // Fetch employee and serialize JSON for JS
                    var employee = await _empBal.GetByEmpCodeAsync(empcode);

                    ViewBag.EmployeeData = Newtonsoft.Json.JsonConvert.SerializeObject(employee,
                        new Newtonsoft.Json.JsonSerializerSettings
                        {
                            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                        });
                }
                else
                {
                    ViewBag.EmpCode = null;
                    ViewBag.EmployeeData = "{}"; // empty object for new
                }
                return View();
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid employee code");
            }
        }

        [HttpGet]
        public IActionResult GetEmployeeData()
        {
            var EmployeeDetails = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(EmployeeDetails)) { return Unauthorized("SessionExpired"); }

            IEnumerable<empMaster> empList = _empBal.GetAllEmployeeData();
            return Json(empList);
        }

        #endregion

        #region Insert / Update

        [HttpPost]
        public async Task<IActionResult> SaveEmployee([FromForm] EmployeeSaveVM model, [FromForm] IFormFile photo, [FromForm] IFormFile icFilePath, [FromForm] IFormFile passportfileurl,
            [FromForm] IFormFile professionaldocurl, [FromForm] IFormFile resumeDocURL, [FromForm] IFormFile academicCertificateUrl, [FromForm] IFormFile otherCertificateUrl)
        {
            try
            {
                #region Login-ID
                string userJson = HttpContext.Session.GetString("EmployeeDetails");
                if (!string.IsNullOrEmpty(userJson))
                {
                    var employee = JsonSerializer.Deserialize<tblempmaster>(userJson);
                    if (!string.IsNullOrWhiteSpace(employee.empcode))
                    {
                        model.empMaster.createdBy = employee?.empcode ?? "";
                    }
                }
                #endregion

                string empCode = await _empBal.SaveEmployeeAsync(model, photo, icFilePath, passportfileurl, professionaldocurl, resumeDocURL, academicCertificateUrl, otherCertificateUrl);
                //return Json(new { success = true, empCode = empCode });
                return Json(new
                {
                    success = true,
                    empCode = empCode,
                    message = $"Employee saved successfully. empCode: {model.empMaster.empcode}"
                });
            }
            catch (Exception ex)
            {
                // Get original DAL message if exists
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployee([FromForm] EmployeeSaveVM model, [FromForm] IFormFile photo, [FromForm] IFormFile icFilePath, [FromForm] IFormFile passportfileurl,
           [FromForm] IFormFile professionaldocurl, [FromForm] IFormFile resumeDocURL, [FromForm] IFormFile academicCertificateUrl, [FromForm] IFormFile otherCertificateUrl)
        {
            try
            {
                #region Login-ID
                string userJson = HttpContext.Session.GetString("EmployeeDetails");
                if (!string.IsNullOrEmpty(userJson))
                {
                    var employee = JsonSerializer.Deserialize<tblempmaster>(userJson);
                    if (!string.IsNullOrWhiteSpace(employee.empcode))
                    {
                        model.empMaster.modifiedBy = employee?.empcode ?? "";
                    }
                }
                #endregion

                string empCode = await _empBal.SaveEmployeeAsync(model, photo, icFilePath, passportfileurl, professionaldocurl, resumeDocURL, academicCertificateUrl, otherCertificateUrl);
                return Json(new
                {
                    success = true,
                    empCode = empCode,
                    message = $"Employee Updated successfully. empCode: {model.empMaster.empcode}"
                });
            }
            catch (Exception ex)
            {
                // Get original DAL message if exists
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new
                {
                    success = false,
                    message = errorMessage
                });
            }
        }


        #endregion

        #region Print .PDF

        [HttpGet]
        [Route("EmpMaster/PrintEmployeePdf/{empCode?}")]
        public async Task<IActionResult> PrintEmployeePdf(string empCode)
        {
            #region Begin-Page Settings
            using var memoryStream = new MemoryStream();

            Document doc = new Document(PageSize.A4, 25f, 25f, 25f, 25f);
            PdfWriter writer = PdfWriter.GetInstance(doc, memoryStream);
            doc.Open();
            Font fontHeaders = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f);
            Font fontNormals = FontFactory.GetFont(FontFactory.HELVETICA, 9f);

            AddEmployeeHeader(doc, writer, fontHeaders, fontNormals);
            // Fonts
            Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f);
            Font fontSubHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9f);
            Font fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 6f);
            Font fontBold = FontFactory.GetFont(FontFactory.HELVETICA, 6f);

            BaseColor lightGray = new BaseColor(246, 246, 246);
            BaseColor mediumGray = new BaseColor(220, 220, 220);// Div-Title
            BaseColor redLine = BaseColor.RED;

            // ================= HEADER =================
            PdfPTable headerTable = new PdfPTable(3);
            headerTable.WidthPercentage = 100;
            headerTable.SetWidths(new float[] { 2f, 4f, 2f });

            doc.Add(headerTable);

            //doc.Add(new Paragraph("\n"));// Space B/W Table to Table
            #endregion

            #region HELPERS
            PdfPCell Label(string text) =>
                new PdfPCell(new Phrase(text, fontBold))
                {
                    BackgroundColor = lightGray,
                    Padding = 5,
                    Border = Rectangle.BOX
                };

            PdfPCell Data(string text) =>
                new PdfPCell(new Phrase(text, fontNormal))
                {
                    Padding = 5,
                    Border = Rectangle.BOX
                };

            PdfPCell SectionHeader(string text) =>
                new PdfPCell(new Phrase(text, fontSubHeader))
                {
                    Colspan = 2,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    BackgroundColor = mediumGray,
                    Padding = 5,
                    Border = Rectangle.NO_BORDER
                };

            #endregion

            var _eua = await _empBal.GetByEmpCodeAsync(empCode);
            var dsmsdata = await _dropdownBal.GetAllMasterDataAsync();

            #region Div-1 --> EMPLOYEE DETAILS 

            #region Commented Code [Old Code]

            //PdfPTable employeeTable = new PdfPTable(2);
            //employeeTable.WidthPercentage = 100;
            //employeeTable.SetWidths(new float[] { 1.5f, 2.5f });

            //employeeTable.AddCell(SectionHeader("EMPLOYEE DETAILS"));

            //employeeTable.AddCell(Label("EMPLOYEE CODE :")); employeeTable.AddCell(Data(_eua.empMaster.empcode.ToString().Trim()));
            //employeeTable.AddCell(Label("EMPLOYEE NAME :")); employeeTable.AddCell(Data(_eua.empMaster.empName));
            //if (!string.IsNullOrWhiteSpace(_eua.empMaster.department))
            //{
            //    #region 
            //    string deptName = dsmsdata?.Tables["Table2"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.department.ToString())?.Field<string>("Name") ?? "";
            //    if (!string.IsNullOrWhiteSpace(deptName))
            //    {
            //        employeeTable.AddCell(Label("DEPARTMENT :")); employeeTable.AddCell(Data(_eua.empMaster.department.ToString() + " - " + deptName));
            //    }
            //    else
            //    {
            //        employeeTable.AddCell(Label("DEPARTMENT :")); employeeTable.AddCell(Data(_eua.empMaster.department + "~"));
            //    }
            //    #endregion
            //}
            //else
            //{
            //    employeeTable.AddCell(Label("DEPARTMENT :")); employeeTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.department) ? "-" : _eua.empMaster.department));//_emp.departmentcode + " - " + _emp.departmentcode));
            //}

            //if (!string.IsNullOrWhiteSpace(_eua.empMaster.section))
            //{
            //    #region 

            //    string sectName = dsmsdata?.Tables["Table3"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.section.ToString())?.Field<string>("Name") ?? "";
            //    if (!string.IsNullOrWhiteSpace(sectName))
            //    {
            //        employeeTable.AddCell(Label("SECTION :")); employeeTable.AddCell(Data(_eua.empMaster.section + " - " + sectName)); //Should display: "9008  E-Innovation Division"
            //    }
            //    else
            //    {
            //        employeeTable.AddCell(Label("SECTION :")); employeeTable.AddCell(Data(_eua.empMaster.section)); //Should display: "9008  E-Innovation Division"
            //    }
            //    #endregion
            //}
            //else
            //{
            //    employeeTable.AddCell(Label("SECTION :")); employeeTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.section) ? "-" : _eua.empMaster.section));//_emp.sectioncode + " - " + _emp.sectioncode)); //Should display: "9008  E-Innovation Division"
            //}

            //if (!string.IsNullOrWhiteSpace(_eua.empMaster.designation))
            //{
            //    #region
            //    string desigName =
            //                    dsmsdata?.Tables["Table4"]?.AsEnumerable().FirstOrDefault(r =>
            //                        string.Equals(r.Field<string>("Code"), _eua.empMaster.designation, StringComparison.OrdinalIgnoreCase) ||string.Equals(r.Field<string>("Name"), _eua.empMaster.designation, StringComparison.OrdinalIgnoreCase))?.Field<string>("Name") ?? "";

            //    if (!string.IsNullOrWhiteSpace(desigName))
            //    {
            //        employeeTable.AddCell(Label("DESIGNATION :")); employeeTable.AddCell(Data(_eua.empMaster.designation + " - " + desigName));
            //    }
            //    else
            //    {
            //        employeeTable.AddCell(Label("DESIGNATION :")); employeeTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.designation) ? "-" : _eua.empMaster.designation));//_emp.designation + "~"));
            //    }
            //    #endregion
            //}
            //else
            //{
            //    employeeTable.AddCell(Label("DESIGNATION :")); employeeTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.designation) ? "~" : _eua.empMaster.designation));//_emp.designation + " ! " + _emp.designation));
            //}

            //employeeTable.AddCell(Label("DATE OF JOIN :")); employeeTable.AddCell(Data(_eua.empMaster.dateOfJoin.HasValue ? _eua.empMaster.dateOfJoin.Value.ToString("dd/MM/yyyy") : "-"));

            //if (!string.IsNullOrWhiteSpace(_eua.empMaster.empType))
            //{
            //    employeeTable.AddCell(Label("EMPLOYEE TYPE :")); employeeTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.empType) ? "-" : _eua.empMaster.empType));//_emp.emptype + " ~ "));
            //}
            //else
            //{
            //    employeeTable.AddCell(Label("EMPLOYEE TYPE :")); employeeTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.empType) ? "~" : _eua.empMaster.empType));//_emp.emptype + " ! " + _emp.emptype));
            //}

            //employeeTable.AddCell(Label("CONTRACT PERIOD :")); employeeTable.AddCell(Data(_eua.empMaster.contractperiod));

            //doc.Add(employeeTable);

            #endregion

            #region New Code with Photo
            // Create main table with 2 columns (left = details, right = photo)
            PdfPTable mainTable = new PdfPTable(2);
            mainTable.WidthPercentage = 100;
            mainTable.SetWidths(new float[] { 3f, 1f }); // 3:1 ratio for text vs photo

            // Create nested employee details table (left side)
            PdfPTable leftTable = new PdfPTable(2);
            leftTable.WidthPercentage = 100;
            leftTable.SetWidths(new float[] { 1.5f, 2.5f });

            // Add section header with colspan = 2
            PdfPCell sectionHeader = new PdfPCell(new Phrase("EMPLOYEE DETAILS", fontSubHeader));
            sectionHeader.Colspan = 2;
            sectionHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            sectionHeader.BackgroundColor = BaseColor.LIGHT_GRAY;
            sectionHeader.Padding = 4f;
            sectionHeader.Border = Rectangle.BOX;
            leftTable.AddCell(sectionHeader);

            // Add employee code
            leftTable.AddCell(Label("EMPLOYEE CODE :"));
            leftTable.AddCell(Data(_eua.empMaster.empcode.ToString().Trim()));

            // Add employee name
            leftTable.AddCell(Label("EMPLOYEE NAME :"));
            leftTable.AddCell(Data(_eua.empMaster.empName));

            // Department with lookup
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.department))
            {
                string deptName = dsmsdata?.Tables["Table2"]?.AsEnumerable()
                                 .FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.department.ToString())
                                 ?.Field<string>("Name") ?? "";

                leftTable.AddCell(Label("DEPARTMENT :"));
                leftTable.AddCell(Data(!string.IsNullOrWhiteSpace(deptName) ? _eua.empMaster.department + " - " + deptName : _eua.empMaster.department + "~"));
            }
            else
            {
                leftTable.AddCell(Label("DEPARTMENT :"));
                leftTable.AddCell(Data("-"));
            }

            // Section with lookup
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.section))
            {
                string sectName = dsmsdata?.Tables["Table3"]?.AsEnumerable()
                                 .FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.section.ToString())
                                 ?.Field<string>("Name") ?? "";

                leftTable.AddCell(Label("SECTION :"));
                leftTable.AddCell(Data(!string.IsNullOrWhiteSpace(sectName) ? _eua.empMaster.section + " - " + sectName : _eua.empMaster.section));
            }
            else
            {
                leftTable.AddCell(Label("SECTION :"));
                leftTable.AddCell(Data("-"));
            }

            // Designation with lookup
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.designation))
            {
                string desigName = dsmsdata?.Tables["Table4"]?.AsEnumerable()
                                   .FirstOrDefault(r =>
                                      string.Equals(r.Field<string>("Code"), _eua.empMaster.designation, StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(r.Field<string>("Name"), _eua.empMaster.designation, StringComparison.OrdinalIgnoreCase))
                                   ?.Field<string>("Name") ?? "";

                leftTable.AddCell(Label("DESIGNATION :"));
                leftTable.AddCell(Data(!string.IsNullOrWhiteSpace(desigName) ? _eua.empMaster.designation + " - " + desigName : _eua.empMaster.designation));
            }
            else
            {
                leftTable.AddCell(Label("DESIGNATION :"));
                leftTable.AddCell(Data("-"));
            }

            // Date of join
            leftTable.AddCell(Label("DATE OF JOIN :"));
            leftTable.AddCell(Data(_eua.empMaster.dateOfJoin.HasValue ? _eua.empMaster.dateOfJoin.Value.ToString("dd/MM/yyyy") : "-"));

            // Employee type
            leftTable.AddCell(Label("EMPLOYEE TYPE :"));
            leftTable.AddCell(Data(!string.IsNullOrWhiteSpace(_eua.empMaster.empType) ? _eua.empMaster.empType : "-"));

            // Contract period
            leftTable.AddCell(Label("CONTRACT PERIOD :"));
            leftTable.AddCell(Data(_eua.empMaster.contractperiod.ToString()));

            // Add the nested leftTable to mainTable cell
            PdfPCell leftCell = new PdfPCell(leftTable);
            leftCell.Border = Rectangle.NO_BORDER; // Optional: cleaner look
            mainTable.AddCell(leftCell);

            // Prepare photo on right side
            PdfPCell rightCell;

            if (_eua.empMaster.photo != null && _eua.empMaster.photo.Length > 0)
            {
                iTextSharp.text.Image empPhoto = iTextSharp.text.Image.GetInstance(_eua.empMaster.photo);
                empPhoto.ScaleToFit(170f, 150f); // Adjust size as necessary
                empPhoto.Alignment = Element.ALIGN_CENTER;

                rightCell = new PdfPCell(empPhoto);
            }
            else
            {
                // Determine default image file name based on gender
                string defaultImgFile = (string.IsNullOrWhiteSpace(_eua.empMaster.gender) || _eua.empMaster.gender.Trim().ToUpper() != "M") ? "female.jpg" : "male.jpg";
                string defaultImgPath = Path.Combine(_env.WebRootPath, "img", defaultImgFile);

                try
                {
                    iTextSharp.text.Image defaultImg = iTextSharp.text.Image.GetInstance(defaultImgPath);
                    defaultImg.ScaleToFit(170f, 150f);
                    defaultImg.Alignment = Element.ALIGN_CENTER;
                    rightCell = new PdfPCell(defaultImg);
                }
                catch (Exception)
                {
                    rightCell = new PdfPCell(new Phrase("No Photo Available"));
                }
            }

            rightCell.HorizontalAlignment = Element.ALIGN_CENTER;
            rightCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            rightCell.Border = Rectangle.NO_BORDER; // Optional
            mainTable.AddCell(rightCell);

            // Add the mainTable (left + right) to document
            doc.Add(mainTable);
            #endregion


            #endregion

            #region Div-2 --> Employee Particular
            PdfPTable officialTable = new PdfPTable(8);// Number of Columns in Single Row
            officialTable.WidthPercentage = 100;// Table width
            officialTable.SetWidths(new float[] { 2.0f, 1.7f, 2.0f, 1.7f, 2.0f, 1.7f, 2.0f, 1.7f });// Eatch Column(cell) Width from Row 
            officialTable.SpacingBefore = 8f;

            // Section Header
            PdfPCell officialHeader = new PdfPCell(new Phrase("EMPLOYEE PARTICULAR", fontSubHeader));// Table Header: Title
            officialHeader.Colspan = 12;
            officialHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            officialHeader.BackgroundColor = mediumGray;
            officialHeader.Padding = 4f;
            officialHeader.Border = Rectangle.BOX;
            officialTable.AddCell(officialHeader);

            // Row 1
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.nationality))
            {
                #region
                string nationName = dsmsdata?.Tables["Table9"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.nationality.ToString())?.Field<string>("Name") ?? "";
                if (!string.IsNullOrWhiteSpace(nationName))
                {
                    officialTable.AddCell(Label("NATIONALITY :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.nationality) ? "-" : nationName));
                }
                else
                {
                    officialTable.AddCell(Label("NATIONALITY :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.nationality) ? "~" : _eua.empMaster.nationality));
                }
                #endregion
            }
            else
            {
                officialTable.AddCell(Label("NATIONALITY :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.nationality) ? "-" : _eua.empMaster.nationality));
            }

            string _foreignemp = _eua.empMaster.isForeign; // can be null
            officialTable.AddCell(Label("IS FOREIGN? :")); officialTable.AddCell(Data((string.IsNullOrWhiteSpace(_foreignemp) ? "-" : (_foreignemp == "Y" ? "YES" : "NO"))));
            if (!string.IsNullOrWhiteSpace(_foreignemp))
            {
                if (_foreignemp.ToUpper() == "Y")
                {
                    officialTable.AddCell(Label("PASSPORT NO :")); officialTable.AddCell(Data(_eua.empMaster.passportNo));
                    officialTable.AddCell(Label("")); officialTable.AddCell(Data(""));
                }
                else
                {
                    officialTable.AddCell(Label("IC NO :")); officialTable.AddCell(Data(_eua.empMaster.icNo));
                    officialTable.AddCell(Label("")); officialTable.AddCell(Data(""));
                }
            }

            // Row 2
            officialTable.AddCell(Label("DATE OF BIRTH :")); officialTable.AddCell(Data(_eua.empMaster.dateOfBirth.HasValue ? _eua.empMaster.dateOfBirth.Value.ToString("dd/MM/yyyy") : "-"));
            officialTable.AddCell(Label("BLOOD GROUP :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.bloodGroup) ? "-" : _eua.empMaster.bloodGroup));
            officialTable.AddCell(Label("GENDER :")); officialTable.AddCell(Data(_eua.empMaster.gender.ToString() == "M" ? "MALE" : "FEMALE"));
            officialTable.AddCell(Label("EMAIL :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.emailID) ? "-" : _eua.empMaster.emailID));

            // Row 3
            officialTable.AddCell(Label("MOBILE NO :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.mobileNo) ? "-" : _eua.empMaster.mobileNo));
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.religion))
            {
                #region 
                string religionName = dsmsdata?.Tables["Table11"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.religion.ToString())?.Field<string>("Name") ?? "";
                if (!string.IsNullOrWhiteSpace(religionName))
                {
                    officialTable.AddCell(Label("RELIGION. :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(religionName) ? "-" : religionName));
                }
                else
                {
                    officialTable.AddCell(Label("RELIGION. :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.religion) ? "~" : _eua.empMaster.religion));
                }
                #endregion
            }
            else
            {
                officialTable.AddCell(Label("RELIGION. :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.religion) ? "-": _eua.empMaster.religion));
            }

            if (!string.IsNullOrWhiteSpace(_eua.empMaster.race))
            {
                #region 
                string raceName = dsmsdata?.Tables["Table10"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.race.ToString())?.Field<string>("Name") ?? "";
                if (!string.IsNullOrWhiteSpace(raceName))
                {
                    officialTable.AddCell(Label("RACE. :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(raceName) ? "-" : raceName));
                }
                else
                {
                    officialTable.AddCell(Label("RACE. :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.race) ? "~" : _eua.empMaster.race));
                }
                #endregion
            }
            else
            {
                officialTable.AddCell(Label("RACE. :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.race) ? "-" : _eua.empMaster.race));
            }
            officialTable.AddCell(Label("")); officialTable.AddCell(Data(""));
            // Row 4
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.maritalStatus))
            {
                officialTable.AddCell(Label("MARITAL STATUS :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.maritalStatus) ? "~" : _eua.empMaster.maritalStatus));
            }
            else
            {
                officialTable.AddCell(Label("MARITAL STATUS :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.maritalStatus.ToString()) ? "-" : _eua.empMaster.maritalStatus));
            }
            officialTable.AddCell(Label("SPOUSE NAME :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.spouseName) ? "-" : _eua.empMaster.spouseName));
            officialTable.AddCell(Label("SPOUSE WORKING :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.spouseWorking) ? "-" : _eua.empMaster.spouseWorking));
            officialTable.AddCell(Label("NO. OF CHILDREN :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.noOfChildren) ? "-" : _eua.empMaster.noOfChildren));

            //// Row 5
            //officialTable.AddCell(Label("PERMANENT ADDRESS :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.currentAddress) ? "-" : _eua.empMaster.currentAddress));
            //officialTable.AddCell(Label("MAILING ADDRESS :")); officialTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.permanentAddress) ? "-" : _eua.empMaster.permanentAddress));

            officialTable.AddCell(Label("PERMANENT ADDRESS :"));

            PdfPCell permAddr = Data(_eua.empMaster.currentAddress);
            permAddr.Colspan = 3;
            officialTable.AddCell(permAddr);

            officialTable.AddCell(Label("MAILING ADDRESS :"));

            PdfPCell mailAddr = Data(_eua.empMaster.permanentAddress);
            mailAddr.Colspan = 3;
            officialTable.AddCell(mailAddr);


            doc.Add(officialTable);
            #endregion

            #region Div-3 ---> Hostel, Licences, Bank & Tax Details
            PdfPTable hlbtTable = new PdfPTable(6);
            hlbtTable.WidthPercentage = 100;
            hlbtTable.SetWidths(new float[] { 1.3f, 1.7f, 1.3f, 1.7f, 1.3f, 1.7f });
            hlbtTable.SpacingBefore = 8f;

            // Section Header
            PdfPCell hlbtHeader = new PdfPCell(new Phrase("HOSTEL, LICENCES, BANK & TAX DETAILS", fontSubHeader));
            hlbtHeader.Colspan = 12;
            hlbtHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            hlbtHeader.BackgroundColor = mediumGray;
            hlbtHeader.Padding = 4f;
            hlbtHeader.Border = Rectangle.BOX;
            hlbtTable.AddCell(hlbtHeader);

            // Row 1
            hlbtTable.AddCell(Label("STAYING HOSTEL :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.isStayingHostel) ? "-" : _eua.empMaster.isStayingHostel.Trim().ToUpper() == "Y" ? "YES" : "NO"));

            if (!string.IsNullOrWhiteSpace(_eua.empMaster.hostelName))
            {
                string hostelName = dsmsdata?.Tables["Table6"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.hostelName.ToString())?.Field<string>("Name") ?? "";
                if (!string.IsNullOrWhiteSpace(hostelName))
                {
                    hlbtTable.AddCell(Label("HOSTEL NAME :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(hostelName) ? "-" : hostelName));
                }
                else
                {
                    hlbtTable.AddCell(Label("HOSTEL NAME :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.hostelName) ? "~" : _eua.empMaster.hostelName));
                }
            }
            else
            {
                hlbtTable.AddCell(Label("HOSTEL NAME :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.hostelName) ? "-" : _eua.empMaster.hostelName));
            }

            hlbtTable.AddCell(Label("HOUSE NO :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.houseNo) ? "-" : _eua.empMaster.houseNo));

            // Row 2
            hlbtTable.AddCell(Label("TRANSPORT NEED? :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.isTransport) ? "-" : _eua.empMaster.isTransport.Trim().ToUpper() == "Y" ? "YES" : "NO"));
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.route))
            {
                string routeName = dsmsdata?.Tables["Table12"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.route.ToString())?.Field<string>("Name") ?? "";
                if (!string.IsNullOrWhiteSpace(routeName))
                {
                    hlbtTable.AddCell(Label("ROUTE :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(routeName) ? "-" : routeName));
                }
                else
                {
                    hlbtTable.AddCell(Label("ROUTE :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.route) ? "~" : _eua.empMaster.route));
                }
            }
            else
            {
                hlbtTable.AddCell(Label("ROUTE :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.route) ? "-" : _eua.empMaster.route));
            }

            //hlbtTable.AddCell(Label("DRIVING LICENSE :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.isDrivingLicense) ? "-" : _eua.empMaster.isDrivingLicense));
            var drivingLicense = _eua.empMaster.isDrivingLicense;
            string displayValue = string.IsNullOrWhiteSpace(drivingLicense)? "-": drivingLicense == "Y" ? "YES": drivingLicense == "N" ? "NO": drivingLicense;
            hlbtTable.AddCell(Label("DRIVING LICENSE :"));
            hlbtTable.AddCell(Data(displayValue));

            // Row 3
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.License))
            {
                string licenName = dsmsdata?.Tables["Table8"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.License.ToString())?.Field<string>("Name") ?? "";
                if (!string.IsNullOrWhiteSpace(licenName))
                {
                    hlbtTable.AddCell(Label("LICENSE TYPE:")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(licenName) ? "-" : licenName));
                }
                else
                {
                    hlbtTable.AddCell(Label("LICENSE TYPE:")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.License) ? "~" : _eua.empMaster.License));
                }
            }
            else
            {
                hlbtTable.AddCell(Label("LICENSE TYPE:")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.License) ? "-" : _eua.empMaster.License));
            }

            hlbtTable.AddCell(Label("VEHICLE NO :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.vehicleNo1) ? "-" : _eua.empMaster.vehicleNo1));
            hlbtTable.AddCell(Label("VEHICLE NO2 :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.vehicleNo2) ? "-" : _eua.empMaster.vehicleNo2));

            // Row 4
            hlbtTable.AddCell(Label("VEHICLE NO3 :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.vehicleNo3) ? "-" : _eua.empMaster.vehicleNo3));
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.bankName))
            {
                string bankName = dsmsdata?.Tables["Table"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.bankName.ToString())?.Field<string>("Name") ?? "";
                if (!string.IsNullOrWhiteSpace(bankName))
                {
                    hlbtTable.AddCell(Label("BANK NAME :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(bankName) ? "-" : bankName));
                }
                else
                {
                    hlbtTable.AddCell(Label("BANK NAME :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.bankName) ? "~" : _eua.empMaster.bankName));
                }
            }
            else
            {
                hlbtTable.AddCell(Label("BANK NAME :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.bankName) ? "-" : _eua.empMaster.bankName));
            }

            hlbtTable.AddCell(Label("ACCOUNT NO :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.accountNumber) ? "-" : _eua.empMaster.accountNumber));

            // Row 5
            hlbtTable.AddCell(Label("TAX :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.taxNo) ? "-" : _eua.empMaster.taxNo));
            hlbtTable.AddCell(Label("EPF NO :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.epf) ? "-" : _eua.empMaster.epf));
            hlbtTable.AddCell(Label("SOSCO NO :")); hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.sosco) ? "-" : _eua.empMaster.sosco));

            // Row 6: Emergency Contact Name, Contact No, Relation
            hlbtTable.AddCell(Label("EMERGENCY CONTACT NAME :"));
            hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.emergencyContactPerson) ? "-" : _eua.empMaster.emergencyContactPerson));

            hlbtTable.AddCell(Label("EMERGENCY CONTACT NO :"));
            hlbtTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.emergencyContact) ? "-" : _eua.empMaster.emergencyContact));

            string relationName = dsmsdata?.Tables["Table14"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.relation)?.Field<string>("Name") ?? "";
            hlbtTable.AddCell(Label("RELATION :"));
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.relation))
            {
                if (!string.IsNullOrWhiteSpace(relationName))
                {
                    hlbtTable.AddCell(Data(relationName));
                }
                else
                {
                    hlbtTable.AddCell(Data(_eua.empMaster.relation + "~"));
                }
            }
            else
            {
                hlbtTable.AddCell(Data("-"));
            }

            // Row 7: Emergency Address spanning remaining 5 columns
            hlbtTable.AddCell(Label("EMERGENCY ADDRESS :"));

            PdfPCell emergencyAddrCell = Data(string.IsNullOrWhiteSpace(_eua.empMaster.emergencyAddress) ? "-" : _eua.empMaster.emergencyAddress);
            emergencyAddrCell.Colspan = 5;

            hlbtTable.AddCell(emergencyAddrCell);


            doc.Add(hlbtTable);
            #endregion

            #region Div-4 ---> Education Details
            PdfPTable eduTable = new PdfPTable(6);
            eduTable.WidthPercentage = 100;
            eduTable.SetWidths(new float[] { 1.3f, 1.7f, 1.3f, 1.7f, 1.3f, 1.7f });
            eduTable.SpacingBefore = 8f;

            // Section Header
            PdfPCell eduHeader = new PdfPCell(new Phrase("EDUCATION DETAILS", fontSubHeader));
            eduHeader.Colspan = 12;
            eduHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            eduHeader.BackgroundColor = mediumGray;
            eduHeader.Padding = 4f;
            eduHeader.Border = Rectangle.BOX;
            eduTable.AddCell(eduHeader);

            // Row 1
            if (!string.IsNullOrWhiteSpace(_eua.empMaster.educationLevel))
            {
                string eduName = dsmsdata?.Tables["Table5"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.educationLevel.ToString())?.Field<string>("Name") ?? "";
                if (!string.IsNullOrWhiteSpace(eduName))
                {
                    eduTable.AddCell(Label("EDUCATION LEVEL :")); eduTable.AddCell(Data(string.IsNullOrWhiteSpace(eduName) ? "-" : eduName));
                }
                else
                {
                    eduTable.AddCell(Label("EDUCATION LEVEL :")); eduTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.educationLevel.ToUpper()) ? "~" : _eua.empMaster.educationLevel.ToUpper()));
                }
            }
            else { 
                eduTable.AddCell(Label("EDUCATION LEVEL :")); eduTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.educationLevel) ? "-" : _eua.empMaster.educationLevel));
            }

            if (!string.IsNullOrWhiteSpace(_eua.empMaster.languageProficiency))
            {
                #region 
                string ProficiencyName = dsmsdata?.Tables["Table16"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.languageProficiency.ToString())?.Field<string>("Name") ?? "";
                if (!string.IsNullOrWhiteSpace(ProficiencyName))
                {
                    eduTable.AddCell(Label("LANGUAGE PROFICIENCY :")); eduTable.AddCell(Data(ProficiencyName));
                }
                else
                {
                    eduTable.AddCell(Label("LANGUAGE PROFICIENCY :")); eduTable.AddCell(Data(_eua.empMaster.languageProficiency + "~"));
                }
                #endregion
            }
            else
            {
                eduTable.AddCell(Label("LANGUAGE PROFICIENCY :")); eduTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.languageProficiency) ? "-" : _eua.empMaster.languageProficiency));
            }

            if (!string.IsNullOrWhiteSpace(_eua.empMaster.proficiencyLevel))
            {
                #region 
                string ProficiencyName = dsmsdata?.Tables["Table15"]?.AsEnumerable().FirstOrDefault(r => r.Field<string>("Code") == _eua.empMaster.proficiencyLevel.ToString())?.Field<string>("Name") ?? "";
                if (!string.IsNullOrWhiteSpace(ProficiencyName))
                {
                    eduTable.AddCell(Label("PROFICIENCY LEVEL :")); eduTable.AddCell(Data(ProficiencyName));
                }
                else
                {
                    eduTable.AddCell(Label("PROFICIENCY LEVEL :")); eduTable.AddCell(Data(_eua.empMaster.proficiencyLevel + "~"));
                }
                #endregion
            }
            else
            {
                eduTable.AddCell(Label("PROFICIENCY LEVEL :")); eduTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.proficiencyLevel) ? "-" : _eua.empMaster.proficiencyLevel));
            }


            // Row 2
            //eduTable.AddCell(Label("FIELD OF STUDY :")); eduTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.fieldOfStudy) ? "-" : _eua.empMaster.fieldOfStudy));
            //eduTable.AddCell(Label("OTHER QUALIFICATION :")); eduTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.empMaster.others) ? "-" : _eua.empMaster.others));
            //eduTable.AddCell(Label("")); eduTable.AddCell(Data(""));
            //eduTable.AddCell(Label("")); eduTable.AddCell(Data(""));
            // FIELD OF STUDY (spans 2 cells for label and 2 cells for data)


            PdfPCell fieldLabel = Label("FIELD OF STUDY:");
            fieldLabel.Colspan = 1;  // Merge 2 cells for label
            eduTable.AddCell(fieldLabel);

            PdfPCell fieldData = Data(string.IsNullOrWhiteSpace(_eua.empMaster.fieldOfStudy) ? "-" : _eua.empMaster.fieldOfStudy);
            fieldData.Colspan = 2;   // Merge 2 cells for data
            eduTable.AddCell(fieldData);

            // OTHER QUALIFICATION (spans 2 cells for label and 2 cells for data)
            PdfPCell qualLabel = Label("OTHER QUALIFICATION:");
            qualLabel.Colspan = 1;  // Merge 2 cells for label
            eduTable.AddCell(qualLabel);

            PdfPCell qualData = Data(string.IsNullOrWhiteSpace(_eua.empMaster.others) ? "-" : _eua.empMaster.others);
            qualData.Colspan = 2;   // Merge 2 cells for data
            eduTable.AddCell(qualData);

            doc.Add(eduTable);
            #endregion

            #region Div-5 ---> Uniform and PPE Details
            PdfPTable updTable = new PdfPTable(8);
            updTable.WidthPercentage = 100;
            updTable.SetWidths(new float[] { 2.0f, 1.7f, 1.3f, 1.7f, 2.0f, 1.7f, 2.0f, 1.7f });
            updTable.SpacingBefore = 8f;

            // Section Header
            PdfPCell updHeader = new PdfPCell(new Phrase("UNIFORM AND PPE DETAILS", fontSubHeader));
            updHeader.Colspan = 12;
            updHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            updHeader.BackgroundColor = mediumGray;
            updHeader.Padding = 4f;
            updHeader.Border = Rectangle.BOX;
            updTable.AddCell(updHeader);

            // Row 1
            updTable.AddCell(Label("JACKET SIZE :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.jacketSize) ? "-" : _eua.uniformInfo.jacketSize));
            updTable.AddCell(Label("QTY :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.jacketqty) ? "-" : _eua.uniformInfo.jacketqty));
            updTable.AddCell(Label("T-Shirt Size :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.tShirtSize) ? "-" : _eua.uniformInfo.tShirtSize));

            updTable.AddCell(Label("QTY :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.tShirtqty) ? "-" : _eua.uniformInfo.tShirtqty));

            // Row 2
            updTable.AddCell(Label("PANT SIZE :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.pantSize) ? "-" : _eua.uniformInfo.pantSize));
            updTable.AddCell(Label("QTY :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.pantqty) ? "-" : _eua.uniformInfo.pantqty));
            updTable.AddCell(Label("SAFETY SHOES :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.safetyShoes) ? "-" : _eua.uniformInfo.safetyShoes));
            updTable.AddCell(Label("QTY :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.safetyShoesqty) ? "-" : _eua.uniformInfo.safetyShoesqty));

            // Row 3
            updTable.AddCell(Label("CAP COLOR :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.capColor) ? "-" : _eua.uniformInfo.capColor));
            updTable.AddCell(Label("QTY :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.capColorqty) ? "-" : _eua.uniformInfo.capColorqty));
            updTable.AddCell(Label("SAFETY BELT SIZE :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.safetyBeltSize) ? "-" : _eua.uniformInfo.safetyBeltSize));
            updTable.AddCell(Label("BELT QTY:")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.safetyBeltqty) ? "-" : _eua.uniformInfo.safetyBeltqty));

            // Row 4
            updTable.AddCell(Label("EARMUFF (QTY) :")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.earMuffqty) ? "-" : _eua.uniformInfo.earMuffqty));
            updTable.AddCell(Label("APRON (QTY)")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.apronQty) ? "-" : _eua.uniformInfo.apronQty));
            updTable.AddCell(Label("SAFETY HELMET(QTY)")); updTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.uniformInfo.safetyBeltSize) ? "-" : _eua.uniformInfo.safetyBeltSize));
            updTable.AddCell(Label("DATE OF ISSUED:")); updTable.AddCell(Data(_eua.uniformInfo.dateOfIssue.HasValue ? _eua.uniformInfo.dateOfIssue.Value.ToString("dd/MM/yyyy") : "-"));


            doc.Add(updTable);
            #endregion

            #region Div-6 --> ACCESS
            PdfPTable accessTable = new PdfPTable(4);
            accessTable.WidthPercentage = 100;
            accessTable.SetWidths(new float[] { 1.5f, 1.5f, 1.8f, 1.5f });
            accessTable.SpacingBefore = 8f;

            // Section header
            PdfPCell accessHeader = new PdfPCell(new Phrase("ACCESSORIES", fontSubHeader));
            accessHeader.Colspan = 12;
            accessHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            accessHeader.BackgroundColor = mediumGray;
            accessHeader.Padding = 4f;
            accessHeader.Border = Rectangle.BOX;
            accessTable.AddCell(accessHeader);

            // Rows
            accessTable.AddCell(Label("HAND PHONE :")); accessTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.accessoriesInfo.cpmHandPhone) ? "-" : (_eua.accessoriesInfo.cpmHandPhone.Trim().ToUpper() == "Y" ? "YES" : "NO")));
            accessTable.AddCell(Label("DATE OF ISSUED :")); accessTable.AddCell(Data(_eua.accessoriesInfo.cpmHandphoneIssuedDate.HasValue ? _eua.accessoriesInfo.cpmHandphoneIssuedDate.Value.ToString("dd/MM/yyyy") : "-"));

            accessTable.AddCell(Label("LAPTOP :")); accessTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.accessoriesInfo.cpmLaptop) ? "-" : _eua.accessoriesInfo.cpmLaptop.Trim().ToUpper() == "Y" ? "YES" : "NO"));
            accessTable.AddCell(Label("DATE OF ISSUED :")); accessTable.AddCell(Data(_eua.accessoriesInfo.cpmLaptopIssuedDate.HasValue ? _eua.accessoriesInfo.cpmLaptopIssuedDate.Value.ToString("dd/MM/yyyy") : "-"));

            accessTable.AddCell(Label("TABLET :")); accessTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.accessoriesInfo.cpmTablet) ? "-" : _eua.accessoriesInfo.cpmTablet.Trim().ToUpper() == "Y" ? "YES" : "NO"));
            accessTable.AddCell(Label("DATE OF ISSUED :")); accessTable.AddCell(Data(_eua.accessoriesInfo.cpmTabletIssuedDate.HasValue ? _eua.accessoriesInfo.cpmTabletIssuedDate.Value.ToString("dd/MM/yyyy") : "-"));

            accessTable.AddCell(Label("COMPANY SIM :")); accessTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.accessoriesInfo.cpmSimcard) ? "-" : _eua.accessoriesInfo.cpmSimcard.Trim().ToUpper() == "Y" ? "YES" : "NO"));
            accessTable.AddCell(Label("DATE OF ISSUED :")); accessTable.AddCell(Data(_eua.accessoriesInfo.cpmSimcardIssuedDate.HasValue ? _eua.accessoriesInfo.cpmSimcardIssuedDate.Value.ToString("dd/MM/yyyy") : "-"));

            accessTable.AddCell(Label("WALKIE TALKIE :")); accessTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.accessoriesInfo.cpmWalkieTalkie) ? "-" : _eua.accessoriesInfo.cpmWalkieTalkie.Trim().ToUpper() == "Y" ? "YES" : "NO"));
            accessTable.AddCell(Label("DATE OF ISSUED :")); accessTable.AddCell(Data(_eua.accessoriesInfo.cpmWalkieTalkieIssuedDate.HasValue ? _eua.accessoriesInfo.cpmWalkieTalkieIssuedDate.Value.ToString("dd/MM/yyyy") : "-"));


            accessTable.AddCell(Label("INTERNET ACCESS :")); accessTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.accessoriesInfo.cpmInternetAccess) ? "-" : _eua.accessoriesInfo.cpmInternetAccess.Trim().ToUpper() == "Y" ? "YES" : "NO"));
            accessTable.AddCell(Label("DATE OF ISSUED :")); accessTable.AddCell(Data(_eua.accessoriesInfo.cpmInternetAccessIssuedDate.HasValue ? _eua.accessoriesInfo.cpmInternetAccessIssuedDate.Value.ToString("dd/MM/yyyy") : "-"));

            accessTable.AddCell(Label("WINDOWS LOGIN ID :")); accessTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.accessoriesInfo.windowsLoginID) ? "-" : _eua.accessoriesInfo.windowsLoginID.Trim().ToUpper() == "Y" ? "YES" : "NO"));
            accessTable.AddCell(Label("DATE OF ISSUED :")); accessTable.AddCell(Data(_eua.accessoriesInfo.windowsLoginIDIssuedDate.HasValue ? _eua.accessoriesInfo.windowsLoginIDIssuedDate.Value.ToString("dd/MM/yyyy") : "-"));

            accessTable.AddCell(Label("COMPANY EMAIL :")); accessTable.AddCell(Data(string.IsNullOrWhiteSpace(_eua.accessoriesInfo.companyEmail) ? "-" : _eua.accessoriesInfo.companyEmail));
            accessTable.AddCell(Label("DATE OF ISSUED :")); accessTable.AddCell(Data(_eua.accessoriesInfo.companyEmailIssuedDate.HasValue ? _eua.accessoriesInfo.companyEmailIssuedDate.Value.ToString("dd/MM/yyyy") : "-"));

            doc.Add(accessTable);
            #endregion

            // ================= PAGE-1 ---> END =================

            doc.Add(new Paragraph("\n"));
            doc.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"Employee_{empCode}.pdf");
        }
        private void AddEmployeeHeader(Document doc, PdfWriter writer, Font fontHeader, Font fontNormal)
        {
            PdfPTable headerTable = new PdfPTable(3);
            headerTable.WidthPercentage = 100;
            headerTable.SetWidths(new float[] { 2f, 4f, 2f });
            headerTable.SpacingAfter = 5f;

            float headerHeight = 40f; // 🔴 controls header compactness

            // LOGO
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "Logo.jpeg");
            Image logo = Image.GetInstance(logoPath);
            logo.ScaleToFit(100f, 18f); // 🔴 smaller & cleaner

            PdfPCell logoCell = new PdfPCell(logo)
            {
                Border = Rectangle.NO_BORDER,
                FixedHeight = headerHeight,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5
            };
            headerTable.AddCell(logoCell);

            // TITLE
            PdfPCell titleCell = new PdfPCell(new Phrase("EMPLOYEE DETAILS", fontHeader))
            {
                Border = Rectangle.NO_BORDER,
                FixedHeight = headerHeight,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 0
            };
            headerTable.AddCell(titleCell);

            // DATE
            PdfPCell dateCell = new PdfPCell(
                new Phrase(DateTime.Now.ToString("dd/MM/yyyy"), fontNormal))
            {
                Border = Rectangle.NO_BORDER,
                FixedHeight = headerHeight,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5
            };
            headerTable.AddCell(dateCell);

            // ADD TABLE
            doc.Add(headerTable);

            // 🔴 RED BORDER (AUTO-ALIGNED TO HEADER)
            PdfContentByte cb = writer.DirectContent;
            cb.SetColorStroke(BaseColor.RED);
            cb.SetLineWidth(1.2f);

            float y = writer.GetVerticalPosition(true) + headerHeight + 5;

            cb.Rectangle(
                doc.LeftMargin,
                y - headerHeight,
                doc.PageSize.Width - doc.LeftMargin - doc.RightMargin,
                headerHeight
            );
            cb.Stroke();
        }
        private void AddEmployeeHeader_Old(Document doc, PdfWriter writer, Font fontHeader, Font fontNormal)
        {
            PdfPTable headerTable = new PdfPTable(3);
            headerTable.WidthPercentage = 100;
            headerTable.SetWidths(new float[] { 2f, 4f, 2f });

            // 🔴 LOGO IMAGE (root/Images/LeftLogo.jpeg)
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","img","Logo.jpeg");

            Image logo = Image.GetInstance(logoPath);
            logo.ScaleAbsolute(115f, 20f);    // adjust size if needed

            PdfPCell logoCell = new PdfPCell(logo)
            {
                Border = Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT,
                PaddingLeft = 5,
                PaddingTop = 18,        // 🔴 moves logo DOWN
                                        // PaddingBottom = 2
            };
            headerTable.AddCell(logoCell);

            // 🔴 TITLE
            PdfPCell titleCell = new PdfPCell(new Phrase("EMPLOYEE DETAILS", fontHeader))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            headerTable.AddCell(titleCell);

            // 🔴 DATE
            PdfPCell dateCell = new PdfPCell(
                new Phrase(DateTime.Now.ToString("dd/MM/yyyy"), fontNormal)
            )
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            headerTable.AddCell(dateCell);

            doc.Add(headerTable);

            // 🔴 RED BORDER BOX
            PdfContentByte cb = writer.DirectContent;
            cb.SetColorStroke(BaseColor.RED);
            cb.SetLineWidth(1.5f);
            cb.Rectangle(doc.LeftMargin - 5,
                doc.PageSize.Height - 90,
                doc.PageSize.Width - doc.LeftMargin - doc.RightMargin + 10,55);
            cb.Stroke();

            //doc.Add(new Paragraph("\n"));
        }
        #endregion

        #region Commented Code

        //[HttpPost]
        //public async Task<IActionResult> SaveEmployee([FromBody] EmployeeSaveVM model)
        //{
        //    try
        //    {
        //        string empCode = await _empBal.SaveEmployeeAsync(model.empMaster,model.uniformInfo,model.accessoriesInfo);

        //        return Json(new { success = true });
        //        //return Ok(new { message = "New Employee Code: " + empCode + " inserted successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> Insert([FromForm] tblempmaster model, [FromForm] IFormFile img, [FromForm] IFormFile icfileurl, [FromForm] IFormFile passportfileurl,
        //     [FromForm] IFormFile professionaldocurl, [FromForm] IFormFile resumeurl, [FromForm] IFormFile academicCertificateurl, [FromForm] IFormFile othercertificatesurl)
        //{
        //    try
        //    {
        //        #region Login-ID
        //        string userJson = HttpContext.Session.GetString("EmployeeDetails");
        //        if (!string.IsNullOrEmpty(userJson))
        //        {
        //            var employee = JsonSerializer.Deserialize<tblempmaster>(userJson);
        //            if (!string.IsNullOrWhiteSpace(employee.empcode))
        //            {
        //                model.createdby = employee?.empcode ?? "";
        //            }
        //        }
        //        #endregion

        //        string empCode = await _empBal.InsertEmployeeAsync(model, img, icfileurl, passportfileurl, professionaldocurl, resumeurl, academicCertificateurl, othercertificatesurl);
        //        //return Ok(new { message = "Employee inserted successfully" });
        //        //return Ok(new { message = "Employee Code: ", empCode });
        //        return Ok(new { message = "New Employee Code: " + empCode + " inserted successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> Update([FromForm] tblempmaster model, [FromForm] IFormFile img, [FromForm] IFormFile icfileurl, [FromForm] IFormFile passportfileurl,
        // [FromForm] IFormFile professionaldocurl, [FromForm] IFormFile resumeurl, [FromForm] IFormFile academicCertificateurl, [FromForm] IFormFile othercertificatesurl)
        //{
        //    try
        //    {
        //        #region Login-ID
        //        string userJson = HttpContext.Session.GetString("EmployeeDetails");
        //        if (!string.IsNullOrEmpty(userJson))
        //        {
        //            var employee = JsonSerializer.Deserialize<tblempmaster>(userJson);
        //            if (!string.IsNullOrWhiteSpace(employee.empcode))
        //            {
        //                model.modifiedby = employee?.empcode ?? "gANDi";
        //            }
        //        }
        //        #endregion

        //        await _empBal.updateEmployeeAsync(model, img, icfileurl, passportfileurl, professionaldocurl, resumeurl, academicCertificateurl, othercertificatesurl);
        //        return Ok(new { message = "Updated Successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        #endregion

    }
}