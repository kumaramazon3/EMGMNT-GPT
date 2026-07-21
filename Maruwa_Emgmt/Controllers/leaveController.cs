using Maruwa_Emgmt.BAL.Leave;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.Leave;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Maruwa_Emgmt.Controllers
{
    public class leaveController : Controller
    {
        private readonly bll_LeaveApplication _leaveBLL;

        public leaveController(bll_LeaveApplication leaveBLL)
        {
            _leaveBLL = leaveBLL;
        }

        public IActionResult LeaveApplication()
        {
            return View();
        }

        public IActionResult LeaveSelfStatus()
        {
            return View();
        }

        public IActionResult leaveFrm()
        {
            return RedirectToAction(nameof(LeaveApplication));
        }

        public IActionResult Selfstatus()
        {
            return RedirectToAction(nameof(LeaveSelfStatus));
        }

        public IActionResult leavelst()
        {
            return RedirectToAction(nameof(LeaveSelfStatus));
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaveApplicationPageData()
        {
            var empCode = GetLoggedInEmpCode();
            if (string.IsNullOrWhiteSpace(empCode)) return Json(new { success = false, message = "Session expired. Please login again." });
            var data = await _leaveBLL.GetLeaveApplicationPageDataAsync(empCode);
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaveTypes()
        {
            return Json(new { success = true, data = await _leaveBLL.GetLeaveTypesAsync() });
        }

        [HttpGet]
        public async Task<IActionResult> GetHalfDayLeaves()
        {
            return Json(new { success = true, data = await _leaveBLL.GetHalfDayLeavesAsync() });
        }

        [HttpGet]
        public async Task<IActionResult> GetReasons()
        {
            return Json(new { success = true, data = await _leaveBLL.GetReasonsAsync() });
        }

        [HttpGet]
        public async Task<IActionResult> SearchPersonsInCharge(string? searchText)
        {
            return Json(new { success = true, data = await _leaveBLL.GetPersonsInChargeAsync(searchText ?? string.Empty) });
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeLeaveSummary()
        {
            var empCode = GetLoggedInEmpCode();
            if (string.IsNullOrWhiteSpace(empCode)) return Json(new { success = false, message = "Session expired. Please login again." });
            return Json(new { success = true, data = await _leaveBLL.GetEmployeeLeaveSummaryAsync(empCode) });
        }



        [HttpPost]
        public async Task<IActionResult> CalculateLeaveDays([FromBody] LeaveDaysCalculationRequest request)
        {
            var empCode = GetLoggedInEmpCode();
            if (string.IsNullOrWhiteSpace(empCode)) return Json(new { success = false, message = "Session expired. Please login again.", leaveDays = 0 });
            var result = await _leaveBLL.CalculateLeaveDaysAsync(request ?? new LeaveDaysCalculationRequest(), empCode);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaveApplicationForEdit(int appNo)
        {
            var empCode = GetLoggedInEmpCode();
            if (string.IsNullOrWhiteSpace(empCode)) return Json(new { success = false, message = "Session expired. Please login again." });
            var data = await _leaveBLL.GetLeaveApplicationForEditAsync(appNo, empCode);
            if (data == null) return Json(new { success = false, message = "Only Scheduled leave applications can be edited." });
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> ApplyLeave([FromBody] LeaveApplicationSaveRequest request)
        {
            var empCode = GetLoggedInEmpCode();
            if (string.IsNullOrWhiteSpace(empCode)) return Json(new { success = false, message = "Session expired. Please login again." });
            var result = await _leaveBLL.ApplyLeaveAsync(request, empCode);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetSelfStatus([FromBody] LeaveSelfStatusSearchRequest request)
        {
            var empCode = GetLoggedInEmpCode();
            if (string.IsNullOrWhiteSpace(empCode)) return Json(new { success = false, message = "Session expired. Please login again.", data = Array.Empty<object>(), totalCount = 0 });
            var result = await _leaveBLL.GetSelfStatusAsync(request ?? new LeaveSelfStatusSearchRequest(), empCode);
            return Json(new { success = true, data = result.Data, totalCount = result.TotalCount });
        }

        [HttpPost]
        public async Task<IActionResult> CancelLeave(int appNo)
        {
            var empCode = GetLoggedInEmpCode();
            if (string.IsNullOrWhiteSpace(empCode)) return Json(new { success = false, message = "Session expired. Please login again." });
            var result = await _leaveBLL.CancelLeaveAsync(appNo, empCode);
            return Json(result);
        }

        private string GetLoggedInEmpCode()
        {
            var empCode = HttpContext.Session.GetString("EmpCode");
            if (!string.IsNullOrWhiteSpace(empCode)) return empCode;

            var json = HttpContext.Session.GetString("EmployeeDetails");
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var emp = JsonSerializer.Deserialize<empMaster>(json);
                    if (!string.IsNullOrWhiteSpace(emp?.empcode)) return emp.empcode;
                }
                catch { }
            }
            return string.Empty;
        }
    }
}
