using Maruwa_Emgmt.BAL;
using Maruwa_Emgmt.BAL.Training;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.TM;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Maruwa_Emgmt.Controllers
{
    public class TNAController : Controller
    {
        private readonly bll_tna _blltna;
        public TNAController(bll_tna blltna)
        {
            _blltna = blltna;
        }
        public IActionResult tnaList()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSkillMatrix(string departmentCode,string subDepartmentCode,string sectionCode,DateTime? logDate)
        {
            try
            {
                var data = await _blltna.GetSkillMatrixAsync(departmentCode, subDepartmentCode, sectionCode, logDate);
                return Json(new
                {
                    success = true,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching Skill Matrix.",
                    error = ex.Message   // remove this in production if sensitive
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSkillMatrix([FromBody] List<master_Skill_Matrix> skillMatrix)
        {
            if (skillMatrix == null || !skillMatrix.Any())
                return Json(new { success = false, message = "No data to save." });
            try
            {
                string empCode = HttpContext.Session.GetString("EmpCode");
                //skillMatrix.modifyedBy = empCode;

                await _blltna.SaveSkillMatrixAsync(skillMatrix, empCode);
                return Json(new { success = true, message = "Skill matrix saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
