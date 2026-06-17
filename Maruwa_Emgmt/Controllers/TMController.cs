using Maruwa_Emgmt.BAL;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.TM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Maruwa_Emgmt.Controllers
{
    public class TMController : Controller
    {
        private readonly bll_trainingMaster _bll;
        public TMController(bll_trainingMaster bll)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
        }

        public IActionResult Trainingmaster()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetNextTrainingCode()
        {
            try
            {
                string nextCode = await _bll.GetNextTrainingCodeAsync();
                return Json(new { nextCode });
            }
            catch
            {
                return StatusCode(500, new { message = "Failed to generate code" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainingList()
        {
            try
            {
                var data = await _bll.GetTrainingListAsync();

                if (data == null)
                    data = new List<trainingMaster>();

                return new JsonResult(data); // explicit JsonResult
            }
            catch (Exception ex)
            {
                // Return JSON in a proper JsonResult
                var errorObj = new { success = false, message = "Internal Server Error" };
                return new JsonResult(errorObj) { StatusCode = 500 };
            }
        }

        [HttpGet]//("GetTrainingById")
        public async Task<IActionResult> GetTrainingById(string code)
        {
            var data = await _bll.GetTrainingByIdAsync(code);
            if (data == null) return NotFound();
            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTraining([FromBody] trainingMaster model)
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
                        model.createdBy = employee.empcode;
                    }
                }
                #endregion

                model.createdDate = DateTime.Now;

                await _bll.SaveTrainingAsync(model);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                // Return meaningful message to frontend
                return BadRequest(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTraining([FromBody] trainingMaster model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.code))
                return BadRequest(new { message = "Invalid training data" });

            model.modifiedDate = DateTime.Now;

            // Login-ID
            string userJson = HttpContext.Session.GetString("EmployeeDetails");
            if (!string.IsNullOrEmpty(userJson))
            {
                var employee = JsonSerializer.Deserialize<tblempmaster>(userJson);
                model.modifiedBy = employee?.empcode;
            }

            try
            {
                var updated = await _bll.UpdateTrainingAsync(model);

                if (!updated)
                    return NotFound(new { message = "Training not found or already updated" });

                return Ok(new { message = "Training updated successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while updating training"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTraining([FromBody] trainingMaster model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.code))
                return BadRequest(new { message = "Invalid training code" });
            try
            {
                var deleted = await _bll.DeleteTrainingAsync(model.code);
                if (!deleted)
                    return NotFound(new { message = "Training not found or already deleted" });
                return Ok(new { message = "Training deleted successfully" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Ok(new { message = "Training already deleted" }); // Record already deleted by another user
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while deleting training"
                });
            }
        }

    }
}
