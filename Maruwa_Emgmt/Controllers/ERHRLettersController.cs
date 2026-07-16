using Maruwa_Emgmt.BAL.ER;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.ER;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace Maruwa_Emgmt.Controllers
{
    public class ERHRLettersController : Controller
    {
        private readonly bll_EmployeeGrievance _grievanceBal;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ERHRLettersController> _logger;

        public ERHRLettersController(bll_EmployeeGrievance grievanceBal, IWebHostEnvironment environment, IConfiguration configuration, ILogger<ERHRLettersController> logger)
        {
            _grievanceBal = grievanceBal;
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult EmployeeGrievanceForm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetMyGrievances([FromBody] EmployeeGrievanceSearchRequest request)
        {
            try
            {
                var employeeCode = GetLoggedInEmployeeCode();
                var result = await _grievanceBal.GetMyGrievancesAsync(request, employeeCode, IsHrUser());
                return Json(new { success = true, data = result.Data, totalCount = result.TotalCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee grievance list");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchEmployeeLookup(string? searchText)
        {
            var data = await _grievanceBal.SearchEmployeeLookupAsync(searchText);
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeByCode(string empCode)
        {
            var employee = await _grievanceBal.GetEmployeeByCodeAsync(empCode);
            return employee == null
                ? Json(new { success = false, message = "Employee not found" })
                : Json(new { success = true, data = employee });
        }

        [HttpGet]
        public async Task<IActionResult> GetLoggedInEmployee()
        {
            var empCode = GetLoggedInEmployeeCode();
            var employee = await _grievanceBal.GetEmployeeByCodeAsync(empCode);
            if (employee != null) return Json(new { success = true, data = employee });

            return Json(new
            {
                success = true,
                data = new EmployeeLookupVm
                {
                    EmpCode = empCode,
                    EmpName = GetLoggedInEmployeeName(),
                    Department = string.Empty,
                    PositionTitle = string.Empty
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetGrievanceForm(int id)
        {
            var employeeCode = GetLoggedInEmployeeCode();
            var data = await _grievanceBal.GetGrievanceByIdAsync(id, employeeCode, IsHrUser());
            return data == null
                ? Json(new { success = false, message = "Employee grievance form not found" })
                : Json(new { success = true, data });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEmployeeGrievance([FromForm] EmployeeGrievanceFormVm model, [FromForm] string? involvedPartiesJson, [FromForm] IFormFile? employeeSignatureFile, [FromForm] List<IFormFile>? supportingFiles)
        {
            try
            {
                var employeeCode = GetLoggedInEmployeeCode();
                var loggedInEmployee = await _grievanceBal.GetEmployeeByCodeAsync(employeeCode);

                if (string.IsNullOrWhiteSpace(model.ComplainantEmpId))
                    model.ComplainantEmpId = loggedInEmployee?.EmpCode ?? employeeCode;
                if (string.IsNullOrWhiteSpace(model.ComplainantName))
                    model.ComplainantName = loggedInEmployee?.EmpName ?? GetLoggedInEmployeeName();
                if (string.IsNullOrWhiteSpace(model.Department))
                    model.Department = loggedInEmployee?.Department ?? string.Empty;
                if (string.IsNullOrWhiteSpace(model.PositionTitle))
                    model.PositionTitle = loggedInEmployee?.PositionTitle ?? string.Empty;

                model.DateOfReport ??= DateTime.Now;
                model.DeclarationEmployeeId = model.ComplainantEmpId;
                model.DeclarationEmployeeName = model.ComplainantName;
                model.DeclarationDate ??= DateTime.Now;

                if (string.IsNullOrWhiteSpace(model.ComplainantEmpId))
                    return Json(new { success = false, message = "Employee ID is required." });

                if (!HasAnyGrievanceNature(model))
                    return Json(new { success = false, message = "Please select at least one Nature of Grievance / Complaint." });

                if (string.IsNullOrWhiteSpace(model.IncidentDescription))
                    return Json(new { success = false, message = "Please describe the grievance incident details." });

                if (!string.IsNullOrWhiteSpace(involvedPartiesJson))
                {
                    model.InvolvedParties = JsonSerializer.Deserialize<List<EmployeeGrievanceInvolvedPartyVm>>(involvedPartiesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }

                if (employeeSignatureFile != null && employeeSignatureFile.Length > 0)
                {
                    model.EmployeeSignaturePath = await SaveFileAsync(employeeSignatureFile, "signatures");
                }

                model.Attachments = new List<EmployeeGrievanceAttachmentVm>();
                if (supportingFiles != null && supportingFiles.Count > 0)
                {
                    model.SupportingDocumentsAttached = true;
                    foreach (var file in supportingFiles.Where(f => f.Length > 0))
                    {
                        var path = await SaveFileAsync(file, "attachments");
                        model.Attachments.Add(new EmployeeGrievanceAttachmentVm
                        {
                            OriginalFileName = file.FileName,
                            StoredFileName = Path.GetFileName(path),
                            FilePath = path,
                            ContentType = file.ContentType ?? string.Empty,
                            SizeBytes = file.Length
                        });
                    }
                }

                var result = await _grievanceBal.SaveComplaintAsync(model, employeeCode);
                if (result.Success)
                {
                    await TrySendHrNotificationAsync(model, result.ReferenceNo, result.GrievanceID);
                }

                return Json(new { success = result.Success, message = result.Message, grievanceID = result.GrievanceID, referenceNo = result.ReferenceNo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Employee Grievance Form");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static bool HasAnyGrievanceNature(EmployeeGrievanceFormVm model)
        {
            return model.UnfairTreatment || model.HarassmentBullying || model.WorkLapses || model.PolicySopBreach ||
                   model.OshaConcern || model.SupervisorMisconduct || model.AbuseOfAuthority || model.WorkingHoursIssue || model.OtherComplaint;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var folder = Path.Combine(webRoot, "uploads", "employee-grievance", folderName, DateTime.Now.ToString("yyyyMMdd"));
            Directory.CreateDirectory(folder);

            var safeFileName = Path.GetFileName(file.FileName);
            var storedFileName = $"{Guid.NewGuid():N}_{safeFileName}";
            var physicalPath = Path.Combine(folder, storedFileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/employee-grievance/{folderName}/{DateTime.Now:yyyyMMdd}/{storedFileName}";
        }

        private async Task TrySendHrNotificationAsync(EmployeeGrievanceFormVm model, string referenceNo, int grievanceId)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var hrToEmail = _configuration["EmailSettings:HRToEmail"];
                if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(hrToEmail))
                {
                    _logger.LogInformation("Employee grievance HR email skipped because EmailSettings:SmtpHost or EmailSettings:HRToEmail is not configured.");
                    return;
                }

                var smtpPort = int.TryParse(_configuration["EmailSettings:SmtpPort"], out var port) ? port : 25;
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var enableSsl = bool.TryParse(_configuration["EmailSettings:EnableSsl"], out var ssl) && ssl;
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? smtpUser ?? hrToEmail;

                using var message = new MailMessage();
                message.From = new MailAddress(fromEmail);
                message.To.Add(hrToEmail);
                message.Subject = $"Employee Grievance Form Submitted - {referenceNo}";
                message.Body = $"Employee Grievance Form has been submitted.\n\nReference No: {referenceNo}\nEmployee: {model.ComplainantName} ({model.ComplainantEmpId})\nDepartment: {model.Department}\nStatus: Submitted\nForm ID: {grievanceId}\n\nPlease login to E-Management application and review the pending action.";

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl
                };

                if (!string.IsNullOrWhiteSpace(smtpUser))
                {
                    client.Credentials = new NetworkCredential(smtpUser, smtpPassword);
                }

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Employee grievance HR email notification failed");
            }
        }

        private string GetLoggedInEmployeeCode()
        {
            var employeeDetails = HttpContext.Session.GetString("EmployeeDetails");
            if (!string.IsNullOrWhiteSpace(employeeDetails))
            {
                try
                {
                    var employee = JsonSerializer.Deserialize<tblempmaster>(employeeDetails);
                    if (!string.IsNullOrWhiteSpace(employee?.empcode)) return employee.empcode;
                }
                catch { }
            }
            return HttpContext.Session.GetString("empcode") ?? "SYSTEM";
        }

        private string GetLoggedInEmployeeName()
        {
            var employeeDetails = HttpContext.Session.GetString("EmployeeDetails");
            if (!string.IsNullOrWhiteSpace(employeeDetails))
            {
                try
                {
                    var employee = JsonSerializer.Deserialize<tblempmaster>(employeeDetails);
                    if (!string.IsNullOrWhiteSpace(employee?.empName)) return employee.empName;
                }
                catch { }
            }
            return HttpContext.Session.GetString("empName") ?? string.Empty;
        }

        private bool IsHrUser()
        {
            var role = HttpContext.Session.GetString("Role") ?? string.Empty;
            return role.Equals("HR", StringComparison.OrdinalIgnoreCase) ||
                   role.Equals("HUMAN RESOURCE", StringComparison.OrdinalIgnoreCase) ||
                   role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
        }
    }
}
