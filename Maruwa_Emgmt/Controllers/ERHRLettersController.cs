using Maruwa_Emgmt.BAL.ER;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.ER;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Text;
using System.IO.Compression;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Maruwa_Emgmt.Controllers
{
    public class ERHRLettersController : Controller
    {
        private readonly bll_EmployeeGrievance _grievanceBal;
        private readonly bll_NatureOfGrievanceMaster _natureBal;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ERHRLettersController> _logger;

        public ERHRLettersController(bll_EmployeeGrievance grievanceBal, bll_NatureOfGrievanceMaster natureBal, IWebHostEnvironment environment, IConfiguration configuration, ILogger<ERHRLettersController> logger)
        {
            _grievanceBal = grievanceBal;
            _natureBal = natureBal;
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult EmployeeGrievanceForm()
        {
            return View();
        }

        public IActionResult NatureOfGrievanceMaster()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetNatureOfGrievanceList([FromBody] NatureOfGrievanceSearchRequest request)
        {
            try
            {
                var data = await _natureBal.GetNatureOfGrievancesAsync(request);
                return Json(new { success = true, data = data.Data, totalCount = data.TotalCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Nature of Grievance list");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNatureOfGrievance(int id)
        {
            var nature = await _natureBal.GetNatureOfGrievanceByIdAsync(id);
            return nature == null
                ? Json(new { success = false, message = "Nature of Grievance not found" })
                : Json(new { success = true, data = nature });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNatureOfGrievance(NatureOfGrievanceMasterVm model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("\n", errors) });
            }

            if (string.IsNullOrWhiteSpace(model.NatureName))
                return Json(new { success = false, message = "Nature of Grievance is required." });

            var result = await _natureBal.SaveNatureOfGrievanceAsync(model, GetLoggedInEmployeeCode());
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNatureOfGrievance(int id)
        {
            var result = await _natureBal.DeleteNatureOfGrievanceAsync(id, GetLoggedInEmployeeCode());
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetNatureOfGrievanceLookup(string? searchText)
        {
            var data = await _natureBal.SearchActiveNatureOfGrievanceLookupAsync(searchText);
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> ExportNatureOfGrievances([FromBody] NatureOfGrievanceSearchRequest request, string format)
        {
            var natures = await _natureBal.GetNatureOfGrievancesForExportAsync(request);
            format = (format ?? "csv").ToLowerInvariant();
            return format switch
            {
                "xlsx" => File(CreateNatureXlsx(natures), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "NatureOfGrievanceMaster.xlsx"),
                "pdf" => File(CreateNaturePdf(natures), "application/pdf", "NatureOfGrievanceMaster.pdf"),
                _ => File(CreateNatureCsv(natures), "text/csv", "NatureOfGrievanceMaster.csv")
            };
        }

        private static byte[] CreateNatureCsv(IEnumerable<NatureOfGrievanceMasterVm> natures)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Nature of Grievance,Is Other,Created By,Created On,Edited By,Edited On,Status");
            foreach (var d in natures)
            {
                string Csv(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
                sb.AppendLine(string.Join(',', Csv(d.NatureName), Csv(d.IsOther ? "Yes" : "No"), Csv(d.CreatedBy), Csv(d.CreatedOn?.ToString("yyyy-MM-dd HH:mm")), Csv(d.EditedBy), Csv(d.EditedOn?.ToString("yyyy-MM-dd HH:mm")), Csv(d.isActive ? "Active" : "Inactive")));
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static byte[] CreateNaturePdf(IEnumerable<NatureOfGrievanceMasterVm> natures)
        {
            using var ms = new MemoryStream();
            using var doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();
            doc.Add(new Paragraph("Nature of Grievance / Complaint Master"));
            doc.Add(new Paragraph(" "));
            var table = new PdfPTable(7) { WidthPercentage = 100 };
            string[] headers = ["Nature of Grievance", "Is Other", "Created By", "Created On", "Edited By", "Edited On", "Status"];
            foreach (var h in headers) table.AddCell(new Phrase(h));
            foreach (var d in natures)
            {
                table.AddCell(d.NatureName);
                table.AddCell(d.IsOther ? "Yes" : "No");
                table.AddCell(d.CreatedBy ?? "");
                table.AddCell(d.CreatedOn?.ToString("yyyy-MM-dd") ?? "");
                table.AddCell(d.EditedBy ?? "");
                table.AddCell(d.EditedOn?.ToString("yyyy-MM-dd") ?? "");
                table.AddCell(d.isActive ? "Active" : "Inactive");
            }
            doc.Add(table);
            doc.Close();
            return ms.ToArray();
        }

        private static byte[] CreateNatureXlsx(IEnumerable<NatureOfGrievanceMasterVm> natures)
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                AddZipEntry(archive, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
                AddZipEntry(archive, "_rels/.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
                AddZipEntry(archive, "xl/_rels/workbook.xml.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
                AddZipEntry(archive, "xl/workbook.xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"NatureMaster\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                AddZipEntry(archive, "xl/worksheets/sheet1.xml", BuildNatureSheetXml(natures));
            }
            return ms.ToArray();
        }

        private static string BuildNatureSheetXml(IEnumerable<NatureOfGrievanceMasterVm> natures)
        {
            var rows = new StringBuilder();
            string[] headers = ["Nature of Grievance", "Is Other", "Created By", "Created On", "Edited By", "Edited On", "Status"];
            int rowIndex = 1;
            rows.Append(BuildXlsxRow(rowIndex++, headers));
            foreach (var d in natures)
            {
                rows.Append(BuildXlsxRow(rowIndex++, [d.NatureName, d.IsOther ? "Yes" : "No", d.CreatedBy ?? "", d.CreatedOn?.ToString("yyyy-MM-dd HH:mm") ?? "", d.EditedBy ?? "", d.EditedOn?.ToString("yyyy-MM-dd HH:mm") ?? "", d.isActive ? "Active" : "Inactive"]));
            }
            return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>{rows}</sheetData></worksheet>";
        }

        private static string BuildXlsxRow(int rowIndex, IEnumerable<string> values)
        {
            var cells = new StringBuilder();
            int col = 1;
            foreach (var value in values)
            {
                cells.Append($"<c r=\"{GetExcelColumnName(col++)}{rowIndex}\" t=\"inlineStr\"><is><t>{WebUtility.HtmlEncode(value ?? string.Empty)}</t></is></c>");
            }
            return $"<row r=\"{rowIndex}\">{cells}</row>";
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            var dividend = columnNumber;
            var columnName = string.Empty;
            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }
            return columnName;
        }

        private static void AddZipEntry(ZipArchive archive, string entryName, string content)
        {
            var entry = archive.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
            writer.Write(content);
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
        public async Task<IActionResult> SaveEmployeeGrievance([FromForm] EmployeeGrievanceFormVm model, [FromForm] string? involvedPartiesJson, [FromForm] string? natureOfGrievanceJson, [FromForm] string? employeeSignatureData, [FromForm] List<IFormFile>? supportingFiles)
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

                if (!string.IsNullOrWhiteSpace(natureOfGrievanceJson))
                {
                    model.SelectedNatures = JsonSerializer.Deserialize<List<EmployeeGrievanceNatureSelectionVm>>(natureOfGrievanceJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }

                ApplyLegacyNatureFlags(model);

                if (!HasAnyGrievanceNature(model))
                    return Json(new { success = false, message = "Please select at least one Nature of Grievance / Complaint." });

                if (model.SelectedNatures.Any(n => n.IsOther || IsOtherNatureName(n.NatureName)) && string.IsNullOrWhiteSpace(model.OtherComplaintText))
                    return Json(new { success = false, message = "Please enter complaint details for Other nature of grievance." });

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
            return (model.SelectedNatures != null && model.SelectedNatures.Count > 0) ||
                   model.UnfairTreatment || model.HarassmentBullying || model.WorkLapses || model.PolicySopBreach ||
                   model.OshaConcern || model.SupervisorMisconduct || model.AbuseOfAuthority || model.WorkingHoursIssue || model.OtherComplaint;
        }

        private static void ApplyLegacyNatureFlags(EmployeeGrievanceFormVm model)
        {
            if (model.SelectedNatures == null || model.SelectedNatures.Count == 0) return;

            foreach (var nature in model.SelectedNatures)
            {
                var text = (nature.NatureName ?? string.Empty).Trim().ToUpperInvariant();
                if (text.Contains("UNFAIR") || text.Contains("DISCRIMINATION")) model.UnfairTreatment = true;
                if (text.Contains("HARASSMENT") || text.Contains("BULLYING")) model.HarassmentBullying = true;
                if (text.Contains("WORK LAPSES")) model.WorkLapses = true;
                if (text.Contains("BREACH") || text.Contains("SOP")) model.PolicySopBreach = true;
                if (text.Contains("SAFETY") || text.Contains("OSH") || text.Contains("HEALTH") || text.Contains("ENVIRONMENT")) model.OshaConcern = true;
                if (text.Contains("MISCONDUCT BY SUPERVISOR") || text.Contains("MISCONDUCT BY") || text.Contains("COLLEAGUE")) model.SupervisorMisconduct = true;
                if (text.Contains("ABUSE OF AUTHORITY") || text.Contains("ABUSE OF") || text.Contains("POWER")) model.AbuseOfAuthority = true;
                if (text.Contains("WORKING HOURS") || text.Contains("SHIFT SCHEDULING")) model.WorkingHoursIssue = true;
                if (nature.IsOther || IsOtherNatureName(nature.NatureName)) model.OtherComplaint = true;
            }
        }

        private static bool IsOtherNatureName(string? natureName)
        {
            return !string.IsNullOrWhiteSpace(natureName) && natureName.Trim().StartsWith("Other", StringComparison.OrdinalIgnoreCase);
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
