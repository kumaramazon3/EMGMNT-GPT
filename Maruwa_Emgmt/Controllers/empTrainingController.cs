using Maruwa_Emgmt.BAL;
using Maruwa_Emgmt.BAL.Training;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.TM;
using Microsoft.AspNetCore.Mvc;
using Sql;
using System.Text;
using System.Text.Json;

namespace Maruwa_Emgmt.Controllers
{
    public class empTrainingController : Controller
    {
        #region Local Variables
        private readonly bll_empTraining _empBal;
        private readonly bll_trainingList _emptrainlst;
        private readonly bal_tbldropdownData _dropdownBal;
        private readonly ILogger<EmpMasterController> _logger;
        private readonly bll_trainingProgram _trainingProgram;


        public empTrainingController(bll_empTraining empBal, bll_trainingList emptrainlst, bal_tbldropdownData dropdownBal, bll_trainingProgram trainingProgram, ILogger<EmpMasterController> logger)
        {
            _empBal = empBal;
            _dropdownBal = dropdownBal;  // Assign the new BAL to the field
            _logger = logger;
            _emptrainlst = emptrainlst;
            _trainingProgram = trainingProgram;
        }
        #endregion

        public IActionResult emTraining()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EmpRegisteration(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { EmployeeData = "{}" });
                }

                // Decode Base64 empcode
                var decodedBytes = Convert.FromBase64String(Uri.UnescapeDataString(id));
                var empcode = Encoding.UTF8.GetString(decodedBytes);

                // Fetch employee
                var employee = await _empBal.GetByEmpCodeAsync(empcode);

                // Serialize employee JSON safely
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(employee,
                    new Newtonsoft.Json.JsonSerializerSettings
                    {
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    });

                return Json(new { EmployeeData = json });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EmpRegisteration for id: {Id}", id);
                return BadRequest("Unable to fetch employee data.");
            }
        }


        [HttpGet]//("getTrainingPrograms")
        public async Task<IActionResult> getTrainingPrograms()
        {
            try
            {
                var data = await _trainingProgram.GetAllTrainingProgramsAsync();
                return Ok(data);// Return 200 OK with data
            }
            catch (Exception ex)
            {
                // Optional: log the exception
                Console.WriteLine($"Error in GetTrainingPrograms: {ex.Message}");
                // Return JSON with error message
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainingById(int uid)
        {
            var training = await _trainingProgram.GetTrainingByIdAsync(uid);
            if (training == null)
                return NotFound();

            return Json(training); // Return JSON for JS to populate modal
        }

        #region DropDown List Load

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

        [HttpGet]
        public async Task<IActionResult> Getmaster_TrainingTitle()
        {
            var loginUser = HttpContext.Session.GetString("EmployeeDetails");
            if (string.IsNullOrEmpty(loginUser))
            {
                return Unauthorized("SessionExpired");
            }
            IEnumerable<trainingMaster> empList = await _dropdownBal.GetTrainingMasterlistAsync();
            return Json(empList);
        }



        #endregion




        #region Not in Use
        // GET: /empTraining/GetTrainingList
        [HttpGet]
        //[Route("empTraining/GetTrainingList")]
        public async Task<IActionResult> GetTrainingList()
        {
            try
            {
                var trainings = await _dropdownBal.GetTrainingMasterlistAsync();

                if (trainings == null || !trainings.Any())
                {
                    _logger.LogInformation("No training records found.");
                    return Json(new List<trainingMaster>()); // return empty array
                }

                // Return only required fields to reduce payload
                var trainingData = trainings.Select(t => new
                {
                    code = t.code,
                    titleName = t.titleName
                }).OrderBy(t => t.titleName).ToList();

                return Json(trainingData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching training master list.");
                return StatusCode(500, "Unable to fetch training list."); // server error
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainingDetails(string trainingCode,string type)
        {
            try
            {
                // Call BLL to get training details by title
                var training = await _emptrainlst.GettraininglistByTitleAsync(trainingCode, type);

                if (training == null)
                    return Json(null);
              
                return Json(training);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching training details for TitleName: {TrainingCode}", trainingCode);
                return StatusCode(500, "Unable to fetch training details");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveEmpTraining([FromBody] empTraining model)
        {
            try
            {
                if (model == null)
                    return Json(new { success = false, message = "Invalid input data." });

                #region Login-ID
                string userJson = HttpContext.Session.GetString("EmployeeDetails");
                if (!string.IsNullOrEmpty(userJson))
                {
                    var employee = JsonSerializer.Deserialize<tblempmaster>(userJson);
                    if (!string.IsNullOrWhiteSpace(employee.empcode))
                    {
                        model.createdBy = employee?.empcode ?? "";
                    }
                }
                #endregion

                bool isSaved = await _emptrainlst.InsertOrUpdateEmpTrainingAsync(model);

                if (isSaved)
                    return Json(new { success = true, message = "Training saved successfully!" });
                else
                    return Json(new { success = false, message = "No record was saved." });
            }
            catch (Exception ex)
            {
                // Optional: log exception in BLL/DAL
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}
