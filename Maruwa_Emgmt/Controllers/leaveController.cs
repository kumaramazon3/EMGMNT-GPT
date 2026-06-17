using Maruwa_Emgmt.BAL;
using Maruwa_Emgmt.BAL.Leave;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.Leave;
using Maruwa_Emgmt.Models.master;
using Microsoft.AspNetCore.Mvc;

namespace Maruwa_Emgmt.Controllers
{
    public class leaveController : Controller
    {
        private readonly bll_leave _leaveBLL;
        private readonly bal_tbldropdownData _dropdownBal;
        public leaveController(bll_leave leaveBLL, bal_tbldropdownData dropdownBal)
        {
            _leaveBLL = leaveBLL;
            _dropdownBal = dropdownBal;
        }

        public IActionResult leavelst()
        {
            return View();
        }
     
        #region Page Load Data

        [HttpGet]
        public async Task<IActionResult> Getmaster_Department()
        {
            IEnumerable<master_LeaveType> empList = await _dropdownBal.GetLeaveTypeAsync();
            return Json(empList);
        }
        
        [HttpGet]
        public async Task<IActionResult> Getmaster_LeaveTypes()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<master_LeaveType> empList = await _dropdownBal.GetLeaveTypeAsync();
            return Json(empList);
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaveTimings()
        {
            try
            {
                var timings = await _dropdownBal.GetLeaveTimingsAsync();
                return Ok(timings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetallEmployes()
        {
            try
            {
                var timings = await _dropdownBal.GetallEmployesAsync();
                return Ok(timings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetScheduledLeave()
        {
            string designation = "";
            string empCode = HttpContext.Session.GetString("EmpCode");
            string empmasterJson = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(empCode))
            {
                return Json(new { success = false, message = "Session is expired. Please login again." });// Session expired
            }
            if (!string.IsNullOrEmpty(empmasterJson))
            {
                var emp = System.Text.Json.JsonSerializer.Deserialize<empMaster>(empmasterJson);
                 designation = emp?.designation;
            }


            var data = await _leaveBLL.GetScheduledLeaveAsync(empCode, designation);
            return Json(data);
        }

        #endregion

        #region Leave Form Approval

        [HttpGet]
        public async Task<IActionResult> GetLeaveHistory(string empCode)
        {
            try
            {
                var result = await _leaveBLL.GetLeaveFormByEmpAsync(empCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetEmployeeLeaveFullSummary(string empCode)
        {
            var result = _leaveBLL.GetEmployeeLeaveFullSummary(empCode);
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetEmployeeLeaveSummary(string empCode)
        {
             empCode = HttpContext.Session.GetString("EmpCode");
            if (string.IsNullOrEmpty(empCode))
            {
                return Json(new { success = false, message = "Session is expired. Please login again." });// Session expired
            }
            var result = _leaveBLL.GetEmployeeLeaveSummary(empCode);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitApproval([FromBody] List<LeaveApprovalModel> models)
        {
            try
            {
                if (models == null || !models.Any())
                    return Json(new { success = false, message = "No leave approvals received." });

                string empCode = HttpContext.Session.GetString("EmpCode");
                if (string.IsNullOrEmpty(empCode))
                    return Json(new { success = false, message = "Session is expired. Please login again." });

                foreach (var model in models)
                {
                    model.Modifiedby = empCode;
                    await _leaveBLL.Approved_SubmitLeavesEmployee(model);
                }

                return Json(new { success = true, message = "All records updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLeaveStatus([FromBody] List<LeaveApprovalUpdate> updates)
        {
            if (updates == null || !updates.Any())
                return BadRequest("No records provided.");

            string empCode = HttpContext.Session.GetString("EmpCode");
            if (string.IsNullOrEmpty(empCode))
                return Json(new { success = false, message = "Session expired. Please login again." });

            // Set modified info
            foreach (var item in updates)
            {
                item.modifiedby = empCode;
                item.modifiedtime = DateTime.Now;
            }

            try
            {
                await _leaveBLL.UpdateLeaveApprovalsAsync(updates);
                return Ok(new { message = "Leave approvals updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #endregion

        #region Create a New Leave Request
        public IActionResult leaveFrm()
        {
            return View();
        }
        #endregion

        #region Submit Button

        //[HttpPost]
        //public IActionResult SubmitLeave([FromBody] LeaveApplicationSubmitModel model)
        //{
        //    string empCode = HttpContext.Session.GetString("EmpCode");
        //    if (model == null)
        //    {
        //        if (string.IsNullOrEmpty(empCode))
        //            return Json(new { success = false, message = "Session expired. Please login again." });

        //        return BadRequest(new { message = "Invalid data" });
        //    }
        //    else
        //    {
        //        model.EmpCode = empCode;
        //        model.createdby = empCode;
        //    }

        //    var result = _leaveBLL.SubmitLeave(model);

        //    return Ok(new { message = result });
        //}

        [HttpPost]
        public async Task<IActionResult> SubmitLeave([FromBody] LeaveApplicationSubmitModel model)
        {
            string empCode = HttpContext.Session.GetString("EmpCode");

            if (model == null)
            {
                if (string.IsNullOrEmpty(empCode))
                    return Json(new { success = false, message = "Session expired. Please login again." });
                    return BadRequest(new { success = false, message = "Invalid data" });
            }
            model.EmpCode = empCode;
            model.createdby = empCode;
            try
            {
                // Await the async BLL call
                var result = await _leaveBLL.SubmitLeave(model);
                // Return AppNo in success response
                return Ok(new { success = true, message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}
