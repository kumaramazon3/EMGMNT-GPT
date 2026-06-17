using Maruwa_Emgmt.BAL;
using Maruwa_Emgmt.InterFace;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Text.Json;

namespace Maruwa_Emgmt.Controllers
{
    public class LoginUserController : Controller
    {
        private readonly bal_tblempmaster _balEmpMaster;
        public LoginUserController(i_tblempmaster empMaster)
        {
            _balEmpMaster = new bal_tblempmaster(empMaster);
        }

        #region Connection with DB CODE
        [HttpPost]
        public IActionResult ValidateLogin([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.UserID) || string.IsNullOrEmpty(req.Password))
            {
                return Json(new
                {
                    success = false,
                    message = "User ID and Password are required!"
                });
            }

            var user = _balEmpMaster.ValidateLoginInfo(req.UserID, req.Password);
            if (user != null)
            {
                HttpContext.Session.SetString("EmpCode", user.empcode);// Store into session
                HttpContext.Session.SetString("Role", user.designation);// Store into session
                var userJson = System.Text.Json.JsonSerializer.Serialize(user);  // or JsonConvert.SerializeObject(user)
                HttpContext.Session.SetString("EmployeeDetails", userJson);

                return Json(new
                {
                    success = true,
                    //redirectUrl = Url.Action("empList", "EmpMaster")
                    redirectUrl = Url.Action("Index", "Home")// this for only Skill Matrix Home Page
                });
            }
            return Json(new
            {
                success = false,
                message = "Invalid User ID or Password!"
            });
        }

        #endregion

        public IActionResult Login()
        {
            return View();
        }
        public IActionResult UserLogin()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();// Clear all session data
            return RedirectToAction("Login", "LoginUser");// Redirect to Login page
        }
    }
    public class LoginRequest
    {
        public string UserID { get; set; }
        public string Password { get; set; }
    }
}
