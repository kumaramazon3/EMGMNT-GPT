using Maruwa_Emgmt.BAL;
using Maruwa_Emgmt.BAL.master;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.master;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Maruwa_Emgmt.Controllers
{
    public class masterController : Controller
    {
        private readonly bll_Designation _blldesig;
        private readonly bal_tbldropdownData _dropdownBal;
        private readonly bll_DepartmentMaster _departmentBal;

        public masterController(bll_Designation blldesig, bal_tbldropdownData dropdownBal, bll_DepartmentMaster departmentBal)
        {
            _blldesig = blldesig;
            _dropdownBal = dropdownBal;
            _departmentBal = departmentBal;
        }

        public IActionResult DesignationList()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDesignationList()
        {
            try
            {
                var data = await _blldesig.GetAllDesignationAsync();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult DepartmentMaster()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetDepartmentList([FromBody] DepartmentSearchRequest request)
        {
            try
            {
                var data = await _departmentBal.GetDepartmentsAsync(request);
                return Json(new { success = true, data = data.Data, totalCount = data.TotalCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await _departmentBal.GetDepartmentByIdAsync(id);
            return department == null
                ? Json(new { success = false, message = "Department not found" })
                : Json(new { success = true, data = department });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDepartment(DepartmentMasterVm model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("\n", errors) });
            }

            var employeeCode = GetLoggedInEmployeeCode();
            var result = await _departmentBal.SaveDepartmentAsync(model, employeeCode);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var employeeCode = GetLoggedInEmployeeCode();
            var result = await _departmentBal.DeleteDepartmentAsync(id, employeeCode);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> ExportDepartments([FromBody] DepartmentSearchRequest request, string format)
        {
            var departments = await _departmentBal.GetDepartmentsForExportAsync(request);
            format = (format ?? "csv").ToLowerInvariant();
            return format switch
            {
                "xlsx" => File(CreateXlsx(departments), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DepartmentMaster.xlsx"),
                "pdf" => File(CreatePdf(departments), "application/pdf", "DepartmentMaster.pdf"),
                _ => File(CreateCsv(departments), "text/csv", "DepartmentMaster.csv")
            };
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

        private static byte[] CreateCsv(IEnumerable<DepartmentMasterVm> departments)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Department Code,Department Name,Japan Head,Office,Got Section,Prefix,Created By,Created On,Edited By,Edited On,Active Status");
            foreach (var d in departments)
            {
                string Csv(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
                sb.AppendLine(string.Join(',', Csv(d.DepartmentCode), Csv(d.DepartmentName), Csv(d.JapanHead), Csv(d.Office), Csv(d.GotSection), Csv(d.Prefix), Csv(d.CreatedBy), Csv(d.CreatedOn?.ToString("yyyy-MM-dd HH:mm")), Csv(d.EditedBy), Csv(d.EditedOn?.ToString("yyyy-MM-dd HH:mm")), Csv(d.ActiveStatus ? "Active" : "Inactive")));
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static byte[] CreatePdf(IEnumerable<DepartmentMasterVm> departments)
        {
            using var ms = new MemoryStream();
            using var doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();
            doc.Add(new Paragraph("Department Master"));
            doc.Add(new Paragraph(" "));
            var table = new PdfPTable(11) { WidthPercentage = 100 };
            string[] headers = ["Dept Code", "Dept Name", "Japan Head", "Office", "Section", "Prefix", "Created By", "Created On", "Edited By", "Edited On", "Status"];
            foreach (var h in headers) table.AddCell(new Phrase(h));
            foreach (var d in departments)
            {
                table.AddCell(d.DepartmentCode); table.AddCell(d.DepartmentName); table.AddCell(d.JapanHead); table.AddCell(d.Office); table.AddCell(d.GotSection); table.AddCell(d.Prefix);
                table.AddCell(d.CreatedBy ?? ""); table.AddCell(d.CreatedOn?.ToString("yyyy-MM-dd") ?? ""); table.AddCell(d.EditedBy ?? ""); table.AddCell(d.EditedOn?.ToString("yyyy-MM-dd") ?? ""); table.AddCell(d.ActiveStatus ? "Active" : "Inactive");
            }
            doc.Add(table);
            doc.Close();
            return ms.ToArray();
        }

        private static byte[] CreateXlsx(IEnumerable<DepartmentMasterVm> departments)
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                AddZipEntry(archive, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
                AddZipEntry(archive, "_rels/.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
                AddZipEntry(archive, "xl/_rels/workbook.xml.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
                AddZipEntry(archive, "xl/workbook.xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"DepartmentMaster\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                AddZipEntry(archive, "xl/worksheets/sheet1.xml", BuildSheetXml(departments));
            }
            return ms.ToArray();
        }

        private static void AddZipEntry(ZipArchive archive, string path, string content)
        {
            var entry = archive.CreateEntry(path);
            using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
            writer.Write(content);
        }

        private static string BuildSheetXml(IEnumerable<DepartmentMasterVm> departments)
        {
            var rows = new StringBuilder();
            string[] headers = ["Department Code", "Department Name", "Japan Head", "Office", "Got Section", "Prefix", "Created By", "Created On", "Edited By", "Edited On", "Active Status"];
            int rowIndex = 1;
            rows.Append(BuildXlsxRow(rowIndex++, headers));
            foreach (var d in departments)
            {
                rows.Append(BuildXlsxRow(rowIndex++, [d.DepartmentCode, d.DepartmentName, d.JapanHead, d.Office, d.GotSection, d.Prefix, d.CreatedBy ?? "", d.CreatedOn?.ToString("yyyy-MM-dd HH:mm") ?? "", d.EditedBy ?? "", d.EditedOn?.ToString("yyyy-MM-dd HH:mm") ?? "", d.ActiveStatus ? "Active" : "Inactive"]));
            }
            return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>{rows}</sheetData></worksheet>";
        }

        private static string BuildXlsxRow(int rowIndex, string[] values)
        {
            var sb = new StringBuilder($"<row r=\"{rowIndex}\">");
            for (int i = 0; i < values.Length; i++)
            {
                var cellRef = $"{GetColumnName(i + 1)}{rowIndex}";
                sb.Append($"<c r=\"{cellRef}\" t=\"inlineStr\"><is><t>{WebUtility.HtmlEncode(values[i])}</t></is></c>");
            }
            sb.Append("</row>");
            return sb.ToString();
        }

        private static string GetColumnName(int index)
        {
            var name = string.Empty;
            while (index > 0)
            {
                index--;
                name = (char)('A' + index % 26) + name;
                index /= 26;
            }
            return name;
        }
    }
}
