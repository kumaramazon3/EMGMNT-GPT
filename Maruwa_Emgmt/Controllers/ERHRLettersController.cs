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

        [HttpGet]
        public async Task<IActionResult> GetGrievanceLoginInfo()
        {
            var employeeCode = GetLoggedInEmployeeCode();
            var employee = await _grievanceBal.GetEmployeeByCodeAsync(employeeCode);
            return Json(new
            {
                success = true,
                isHrUser = IsHrUser(),
                empCode = employee?.EmpCode ?? employeeCode,
                empName = employee?.EmpName ?? GetLoggedInEmployeeName(),
                department = employee?.Department ?? string.Empty,
                designation = GetLoggedInDesignation(),
                positionTitle = employee?.PositionTitle ?? GetLoggedInDesignation()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetMyGrievances([FromBody] EmployeeGrievanceSearchRequest request)
        {
            try
            {
                var employeeCode = GetLoggedInEmployeeCode();
                var result = await _grievanceBal.GetMyGrievancesAsync(request, employeeCode, IsHrUser());
                return Json(new { success = true, data = result.Data, totalCount = result.TotalCount, isHrUser = IsHrUser() });
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
            if (employee != null) return Json(new { success = true, data = employee, isHrUser = IsHrUser() });

            return Json(new
            {
                success = true,
                isHrUser = IsHrUser(),
                data = new EmployeeLookupVm
                {
                    EmpCode = empCode,
                    EmpName = GetLoggedInEmployeeName(),
                    Department = string.Empty,
                    PositionTitle = GetLoggedInDesignation()
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetGrievanceForm(int id)
        {
            var employeeCode = GetLoggedInEmployeeCode();
            var isHr = IsHrUser();

            if (isHr)
            {
                await _grievanceBal.MarkViewedByHrAsync(id, employeeCode);
            }

            var data = await _grievanceBal.GetGrievanceByIdAsync(id, employeeCode, isHr);
            return data == null
                ? Json(new { success = false, message = "Employee grievance form not found" })
                : Json(new { success = true, data, isHrUser = isHr });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEmployeeGrievance([FromForm] EmployeeGrievanceFormVm model, [FromForm] string? involvedPartiesJson, [FromForm] string? employeeSignatureData, [FromForm] List<IFormFile>? supportingFiles)
        {
            try
            {
                if (IsHrUser())
                    return Json(new { success = false, message = "HR login can view and update HR action only. Complaint registration is allowed for employee login." });

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

                if (string.IsNullOrWhiteSpace(employeeSignatureData) && string.IsNullOrWhiteSpace(model.EmployeeSignaturePath))
                    return Json(new { success = false, message = "Employee signature is required. Please draw and save the signature inside the signature box." });

                if (!string.IsNullOrWhiteSpace(employeeSignatureData))
                {
                    model.EmployeeSignaturePath = await SaveBase64SignatureAsync(employeeSignatureData, "employee-signature.png");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHrRemarks(int grievanceId, string? remarks)
        {
            try
            {
                if (!IsHrUser()) return Json(new { success = false, message = "Only HR users can update remarks." });
                if (grievanceId <= 0) return Json(new { success = false, message = "Invalid grievance reference." });

                var result = await _grievanceBal.UpdateHrRemarksAsync(grievanceId, remarks ?? string.Empty, GetLoggedInEmployeeCode());
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating HR remarks");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHrAction([FromForm] EmployeeGrievanceHrActionVm model, [FromForm] string? hrSignatureData, [FromForm] string? hrActionEmployeesJson)
        {
            try
            {
                if (!IsHrUser()) return Json(new { success = false, message = "Only HR users can submit HR action." });
                if (model.GrievanceID <= 0) return Json(new { success = false, message = "Invalid grievance reference." });

                var employeeCode = GetLoggedInEmployeeCode();
                var hrEmployee = await _grievanceBal.GetEmployeeByCodeAsync(employeeCode);
                model.HREmpId = employeeCode;
                model.HRName = hrEmployee?.EmpName ?? GetLoggedInEmployeeName();
                model.Department = string.IsNullOrWhiteSpace(hrEmployee?.Department) ? "HUMAN RESOURCE" : hrEmployee.Department;
                model.ActionDate ??= DateTime.Now;

                if (!string.IsNullOrWhiteSpace(hrActionEmployeesJson))
                {
                    model.ActionEmployees = JsonSerializer.Deserialize<List<EmployeeGrievanceHrActionEmployeeVm>>(hrActionEmployeesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }

                if (model.ActionEmployees.Count == 0 && !string.IsNullOrWhiteSpace(model.ActionEmployeeId))
                {
                    model.ActionEmployees.Add(new EmployeeGrievanceHrActionEmployeeVm
                    {
                        EmployeeID = model.ActionEmployeeId,
                        EmployeeName = model.ActionEmployeeName ?? string.Empty
                    });
                }

                if (model.ActionEmployees.Count == 0)
                    return Json(new { success = false, message = "Please add at least one employee in HR action section." });

                foreach (var actionEmployee in model.ActionEmployees)
                {
                    if (string.IsNullOrWhiteSpace(actionEmployee.EmployeeID))
                        return Json(new { success = false, message = "Invalid employee row in HR action section." });

                    if (string.IsNullOrWhiteSpace(actionEmployee.EmployeeSignatureData) && string.IsNullOrWhiteSpace(actionEmployee.EmployeeSignaturePath))
                        return Json(new { success = false, message = $"Employee signature is required for {actionEmployee.EmployeeID}." });

                    if (!string.IsNullOrWhiteSpace(actionEmployee.EmployeeSignatureData))
                    {
                        var suffix = $"hr-action-employee-{actionEmployee.EmployeeID}-signature.png";
                        actionEmployee.EmployeeSignaturePath = await SaveBase64SignatureAsync(actionEmployee.EmployeeSignatureData, suffix);
                        actionEmployee.EmployeeSignatureData = null;
                    }
                }

                var firstEmployee = model.ActionEmployees[0];
                model.ActionEmployeeId = firstEmployee.EmployeeID;
                model.ActionEmployeeName = firstEmployee.EmployeeName;

                if (string.IsNullOrWhiteSpace(hrSignatureData) && string.IsNullOrWhiteSpace(model.HRSignaturePath))
                    return Json(new { success = false, message = "HR signature is required. Please draw and save the signature before submitting." });

                if (!string.IsNullOrWhiteSpace(hrSignatureData))
                {
                    model.HRSignaturePath = await SaveBase64SignatureAsync(hrSignatureData, "hr-signature.png");
                }

                var result = await _grievanceBal.SaveHrActionAsync(model, employeeCode);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving HR action");
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
            if (string.IsNullOrWhiteSpace(webRoot)) webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var dateFolder = DateTime.Now.ToString("yyyyMMdd");
            var folder = Path.Combine(webRoot, "uploads", "employee-grievance", folderName, dateFolder);
            Directory.CreateDirectory(folder);

            var safeFileName = Path.GetFileName(file.FileName);
            var storedFileName = $"{Guid.NewGuid():N}_{safeFileName}";
            var physicalPath = Path.Combine(folder, storedFileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/employee-grievance/{folderName}/{dateFolder}/{storedFileName}";
        }

        private async Task<string> SaveBase64SignatureAsync(string signatureData, string suffixFileName)
        {
            if (string.IsNullOrWhiteSpace(signatureData))
                throw new InvalidOperationException("Signature is required.");

            var base64Data = signatureData.Trim();
            var commaIndex = base64Data.IndexOf(',');
            if (commaIndex >= 0) base64Data = base64Data[(commaIndex + 1)..];

            byte[] signatureBytes;
            try
            {
                signatureBytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Invalid signature format.");
            }

            if (signatureBytes.Length == 0) throw new InvalidOperationException("Signature is required.");

            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot)) webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var dateFolder = DateTime.Now.ToString("yyyyMMdd");
            var folder = Path.Combine(webRoot, "uploads", "employee-grievance", "signatures", dateFolder);
            Directory.CreateDirectory(folder);

            var storedFileName = $"{Guid.NewGuid():N}_{suffixFileName}";
            var physicalPath = Path.Combine(folder, storedFileName);
            await System.IO.File.WriteAllBytesAsync(physicalPath, signatureBytes);

            return $"/uploads/employee-grievance/signatures/{dateFolder}/{storedFileName}";
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
                message.Body = $"Employee Grievance Form has been submitted.\n\nReference No: {referenceNo}\nEmployee: {model.ComplainantName} ({model.ComplainantEmpId})\nDepartment: {model.Department}\nStatus: InProgress\nForm ID: {grievanceId}\n\nPlease login to E-Management application and review the pending action.";

                using var client = new SmtpClient(smtpHost, smtpPort) { EnableSsl = enableSsl };
                if (!string.IsNullOrWhiteSpace(smtpUser)) client.Credentials = new NetworkCredential(smtpUser, smtpPassword);
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

        private string GetLoggedInDesignation()
        {
            var employeeDetails = HttpContext.Session.GetString("EmployeeDetails");
            if (!string.IsNullOrWhiteSpace(employeeDetails))
            {
                try
                {
                    var employee = JsonSerializer.Deserialize<tblempmaster>(employeeDetails);
                    if (!string.IsNullOrWhiteSpace(employee?.designation)) return employee.designation;
                }
                catch { }
            }
            return HttpContext.Session.GetString("designation") ?? HttpContext.Session.GetString("Designation") ?? HttpContext.Session.GetString("Role") ?? string.Empty;
        }

        private bool IsHrUser()
        {
            var role = HttpContext.Session.GetString("Role") ?? string.Empty;
            var designation = GetLoggedInDesignation();
            var combined = $"{role} {designation}".Trim().ToUpperInvariant();

            return combined == "HR" ||
                   combined.Contains(" HR") ||
                   combined.Contains("HR ") ||
                   combined.Contains("HUMAN RESOURCE") ||
                   combined.Contains("EMPLOYEE RELATIONS") ||
                   role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
        }
    }
}
