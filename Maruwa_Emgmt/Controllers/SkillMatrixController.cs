using Maruwa_Emgmt.BAL;
using Maruwa_Emgmt.BAL.SkillMatrix;
using Maruwa_Emgmt.BAL.Training;
using Maruwa_Emgmt.InterFace.SkillMatrix;
using Maruwa_Emgmt.Models.SkillMatrix;
using Maruwa_Emgmt.Models.TM;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Maruwa_Emgmt.Controllers
{
    public class SkillMatrixController : Controller
    {
        #region Page Load

        private readonly bll_skmdata _bllskm;
        private readonly bal_tbldropdownData _dropdownBal;
        public SkillMatrixController(bll_skmdata bllskm, bal_tbldropdownData dropdownBal)
        {
            _bllskm = bllskm;
            _dropdownBal = dropdownBal;  // Assign the new BAL to the field
        }
        public IActionResult createSkill()
        {
            return View();
        }
        public IActionResult skmList()
        {
            return View();
        }
        public IActionResult skmReport()
        {
            return View();
        }
        public IActionResult Reportskm()
        {
            return View();
        }

        public IActionResult skmOldReport()
        {
            return View();
        }

        #endregion

        [HttpGet]
        public IActionResult GetSubDeptSections(string SubDeptCode)
        {
            if (string.IsNullOrEmpty(SubDeptCode))
            {
                return BadRequest("Department code is required.");
            }

            var sections = _dropdownBal.GetSubDeptSectionsFromSkillMatrix(SubDeptCode);
            return Json(sections);  // Return sections as JSON
        }

        [HttpGet]
        public IActionResult GetLogYears()
        {
            var data = _bllskm.GetLogYears();
            return Json(data); // No JsonRequestBehavior needed in Core
        }
        [HttpGet]
        public IActionResult FindAllSectionList()
        {
            var data = _bllskm.GetAllSectionList();
            return Json(data);
        }

        #region Insert New Skill Matrix

        [HttpPost]
        public async Task<IActionResult> InsertSkillMatrixList([FromBody] List<Insert_skillMatrix> models)
        {
            try
            {
                if (models == null)
                {
                    return Json(new { success = false, message = "Model is NULL" });
                }
                else
                {
                    //bool rtnval = await _bllskm.UpdateLogTableStatusAsync();
                    string empCode = HttpContext.Session.GetString("EmpCode");
                    foreach (var model in models)
                    {
                        model.CreatedBy = empCode;
                    }
                    await _bllskm.SaveSkillMatrixListAsync(models); // bulk insert method
                    return Json(new { success = true, message = "All data inserted successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        #endregion

        #region Skill Matrix List Update
        [HttpGet]
        public async Task<IActionResult> GetSkillMatrix(string departmentCode, string subDepartmentCode, string sectionCode, DateTime? logDate)
        {
            try
            {
                var data = await _bllskm.GetSkillMatrixAsync(departmentCode, subDepartmentCode, sectionCode, logDate);
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

        [HttpGet]
        public async Task<IActionResult> new_GetSkillMatrix(string departmentCode, string subDepartmentCode, string sectionCode, string aliasName, string? logDate)
        {
            try
            {
                //var data = await _bllskm.new_GetSkillMatrixAsync(departmentCode, subDepartmentCode, sectionCode, aliasName, logDate);
                var data = await _bllskm.GetSkillMatrixAsync(departmentCode, subDepartmentCode, sectionCode, aliasName, logDate);
                return Json(new
                {
                    success = true,
                    data = data
                });
            }
            catch (ArgumentException argEx)
            {
                // Friendly message for validation errors
                return Json(new
                {
                    success = false,
                    message = argEx.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSkillMatrix([FromBody] List<new_skillMatrixDocumentData> skillMatrix)
        {
            if (skillMatrix == null || !skillMatrix.Any())
                return Json(new { success = false, message = "No data to save." });

            try
            {
                string empCode = HttpContext.Session.GetString("EmpCode") ?? "SYSTEM";
                await _bllskm.new_SaveSkillMatrixAsync(skillMatrix, empCode);
                return Json(new { success = true, message = "Skill matrix saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        #endregion

        #region Report List

        [HttpGet]
        public async Task<IActionResult> GetSkillMatrixReport(string departmentCode, string subDepartmentCode, string sectionCode,string Reporttype, string aliasName)
        {
            try
            {
                var data = await _bllskm.GetSkillMatrixReportAsync(departmentCode, subDepartmentCode, sectionCode, Reporttype, aliasName);
                return Json(new
                {
                    success = true,
                    data = data
                });
            }
            catch (ArgumentException argEx)
            {
                // Friendly message for validation errors
                return Json(new
                {
                    success = false,
                    message = argEx.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSkillMatrixReportList(string departmentCode, string sectionCode)
        {
            try
            {
                var data = await _bllskm.GetSkillMatrixReportListAsync(departmentCode, sectionCode);
                return Json(new
                {
                    success = true,
                    data = data
                });
            }
            catch (ArgumentException argEx)
            {
                // Friendly message for validation errors
                return Json(new
                {
                    success = false,
                    message = argEx.Message
                });
            }
            catch (Exception ex)
            {
                // Return user-friendly message
                return Json(new
                {
                    success = false,
                    message = ex.Message,//"An error occurred while fetching Skill Matrix data. Please try again.",
                    error = ex.Message  // optional, remove in production
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSkillMatrixReportByDocNo(string DocNo, string Dept, string secCode)
        {
            try
            {
                var data = await _bllskm.GetSkillMatrixReportByDocNo(DocNo, Dept, secCode);
                return Json(new
                {
                    success = true,
                    data = data
                });
            }
            catch (ArgumentException argEx)
            {
                // Friendly message for validation errors
                return Json(new
                {
                    success = false,
                    message = argEx.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    error = ex.Message
                });
            }
        }

        #endregion

        #region Commented Code

        //[HttpPost]
        //public async Task<IActionResult> InsertSkillMatrix_Old([FromBody] skillMatrixDocumentData model)
        //{
        //    try
        //    {
        //        string empCode = HttpContext.Session.GetString("EmpCode");
        //        model.CreatedBy = empCode;
        //        await _bllskm.SaveSkillMatrixAsync(model);

        //        return Json(new { success = true, message = "Data inserted successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = ex.Message
        //        });
        //    }
        //}

        //[HttpPost]//("DeleteAllSkillMatrix")
        //public async Task<IActionResult> DeleteAllSkillMatrix()
        //{
        //    try
        //    {
        //        await _bllskm.DeleteAllSkillMatrixAsync();
        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log exception if needed
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}
        #endregion
    }
}