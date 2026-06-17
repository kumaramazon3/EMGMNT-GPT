using Maruwa_Emgmt.BAL;
using Maruwa_Emgmt.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;

namespace Maruwa_Emgmt.Controllers
{
    public class HomeController : Controller
    {

        #region Default functions

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Privacy_A()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion

        #region Commented Code

        #region Monthly product Sales Data

        //private readonly ProductSalesService _bal;
        //public HomeController(ProductSalesService bal)
        //{
        //    _bal = bal;
        //}
        //[HttpGet]
        //public JsonResult GetSalesData(int year = 2024)
        //{
        //    var data = _bal.GetMonthlySales(year);
        //    return Json(data);
        //}

        //[HttpGet]
        //public IActionResult GetLast30DaysProductSales()
        //{
        //    var result = _bal.GetLast30DaysSales();

        //    return Json(result);  // AJAX receives JSON
        //}




        #endregion

        #region Hard Code Data
        //[HttpGet]
        //[Route("Home/GetWeeklyProductSales")]
        //public IActionResult GetWeeklyProductSales()
        //{
        //    var data = _bal.GetWeeklyProductSalesData();

        //    return Json(new
        //    {
        //        labels = data.labels,
        //        apple = data.apple,
        //        orange = data.orange,
        //        watermelon = data.watermelon
        //    });
        //}

        //[HttpGet]
        //[Route("Home/GetMonthlyProductSales")]
        //public IActionResult GetMonthlyProductSales()
        //{
        //    try
        //    {
        //        var data = _bal.GetMonthlyProductSalesData();
        //        return Json(new
        //        {
        //            labels = data.labels,
        //            apple = data.apple,
        //            orange = data.orange,
        //            watermelon = data.watermelon
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optional: log error
        //        return Json(new { error = true, message = ex.Message });
        //    }
        //}

        #endregion

        #endregion
    }
}
