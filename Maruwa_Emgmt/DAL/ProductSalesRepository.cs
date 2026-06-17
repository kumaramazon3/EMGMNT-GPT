using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;
using Microsoft.Extensions.Options;

namespace Maruwa_Emgmt.DAL
{
    public class ProductSalesRepository : IProductSalesRepository
    {
        #region Product Names
        string _alumina = "Alumina";
        string _fax = "Fax";
        string _zurcania = "Zirconia";
        #endregion

        private readonly ApplicationDbContext _context;
        private readonly ILogger<da_tblempmaster> _logger;

        public ProductSalesRepository(ApplicationDbContext context, ILogger<da_tblempmaster> logger)
        {
            _context = context;
            _logger = logger;
        }

        public (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon) GetDailyProductSalesData()
        {
            // Get last 30 days date range
            DateTime startDate = DateTime.Now.Date.AddDays(-29);

            // Query database
            var data = _context.dailyproductsales
                               .Where(x => x.SalesDate >= startDate)
                               .OrderBy(x => x.SalesDate)
                               .ToList();

            // Build labels (unique dates)
            var labels = data.Select(x => x.SalesDate.ToString("MMM-dd"))
                             .Distinct()
                             .ToList();

            // Extract product data
            var apple = data.Where(x => x.ProductName == _fax)//"Fax"
                            .OrderBy(x => x.SalesDate)
                            .Select(x => x.Quantity)
                            .ToList();

            var orange = data.Where(x => x.ProductName == _zurcania)//"Zirconia"
                             .OrderBy(x => x.SalesDate)
                             .Select(x => x.Quantity)
                             .ToList();

            var watermelon = data.Where(x => x.ProductName == _alumina)//"Alumina"
                                 .OrderBy(x => x.SalesDate)
                                 .Select(x => x.Quantity)
                                 .ToList();

            return (labels, apple, orange, watermelon);
        }
        public (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon)GetWeeklyProductSalesData()
        {
            // Last 30 days
            DateTime startDate = DateTime.Now.Date.AddDays(-29);

            var data = _context.dailyproductsales
                               .Where(x => x.SalesDate >= startDate)
                               .OrderBy(x => x.SalesDate)
                               .ToList();

            // Group by week (week starts Monday)
            var groupedWeeks = data
                .GroupBy(x => System.Globalization.CultureInfo.CurrentCulture
                               .Calendar.GetWeekOfYear(
                                   x.SalesDate,
                                   System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                                   DayOfWeek.Monday))
                .OrderBy(g => g.Key)
                .ToList();

            List<string> labels = new List<string>();
            List<int> apple = new List<int>();
            List<int> orange = new List<int>();
            List<int> watermelon = new List<int>();

            int weekNumber = 1;

            foreach (var weekGroup in groupedWeeks)
            {
                labels.Add($"Week {weekNumber}");

                apple.Add(weekGroup.Where(x => x.ProductName == _fax).Sum(x => x.Quantity));//"Fax"
                orange.Add(weekGroup.Where(x => x.ProductName == _zurcania).Sum(x => x.Quantity));//"Zirconia"
                watermelon.Add(weekGroup.Where(x => x.ProductName == _alumina).Sum(x => x.Quantity)); //"Alumina"

                weekNumber++;
            }

            return (labels, apple, orange, watermelon);
        }

        /// <summary>
        /// Get monthly product sales grouped by month
        /// </summary>
        /// 
        public (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon) GetMonthlyProductSalesData()
        {
            CallAccessDB();

            try
            {
                // Get all sales for the current year
                var startDate = new DateTime(DateTime.Now.Year, 1, 1);
                var endDate = new DateTime(DateTime.Now.Year, 12, 31);

                var sales = _context.dailyproductsales
                    .Where(x => x.SalesDate >= startDate && x.SalesDate <= endDate)
                    .ToList();

                // Initialize result lists
                var labels = new List<string>();
                var apple = new List<int>();
                var orange = new List<int>();
                var watermelon = new List<int>();

                // Loop through each month
                for (int month = 1; month <= 12; month++)
                {
                    labels.Add(new DateTime(DateTime.Now.Year, month, 1).ToString("MMM"));

                    apple.Add(sales
                        .Where(x => x.ProductName == _fax && x.SalesDate.Month == month)//"Fax"
                        .Sum(x => x.Quantity));

                    orange.Add(sales
                        .Where(x => x.ProductName == _zurcania && x.SalesDate.Month == month)//"Zurcania"
                        .Sum(x => x.Quantity));

                    watermelon.Add(sales
                        .Where(x => x.ProductName ==_alumina && x.SalesDate.Month == month)// "Alumina"
                        .Sum(x => x.Quantity));
                }

                return (labels, apple, orange, watermelon);
            }
            catch (Exception ex)
            {
                // Log the exception if you have a logging mechanism
                Console.WriteLine($"Error in GetMonthlyProductSalesData: {ex.Message}");

                // Return empty data in case of failure
                return (new List<string>(), new List<int>(), new List<int>(), new List<int>());
            }
        }



        #region Access Database Code

        private void CallAccessDB()
        {
            string msg = string.Empty;
            try
            {

            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
        }
        #endregion

        #region Hard Code Data
        //public (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon) GetMonthlyProductSalesData_HardCode()
        //{
        //    // X-axis labels: months
        //    var labels = new List<string>
        //    {
        //        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        //        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        //    };

        //    // Hardcoded monthly sales for each product
        //    var apple = new List<int> { 120, 135, 150, 145, 160, 155, 170, 165, 180, 175, 190, 200 };
        //    var orange = new List<int> { 90, 100, 95, 110, 105, 120, 115, 130, 125, 140, 135, 150 };
        //    var watermelon = new List<int> { 60, 70, 65, 80, 75, 90, 85, 100, 95, 110, 105, 120 };

        //    return (labels, apple, orange, watermelon);
        //}


        //public (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon) GetDailyProductSalesData_HardCodeData()
        //{
        //    // Generate last 30 days
        //    var labels = new List<string>();
        //    for (int i = 29; i >= 0; i--)
        //    {
        //        var date = DateTime.Now.AddDays(-i);
        //        labels.Add(date.ToString("MMM-dd"));
        //    }

        //    // Hard-coded sample data
        //    var apple = new List<int> { 5, 8, 6, 4, 7, 8, 9, 6, 5, 6, 8, 9, 7, 5, 4, 6, 7, 8, 9, 10, 9, 8, 7, 5, 6, 7, 8, 9, 8, 7 };
        //    var orange = new List<int> { 3, 4, 3, 4, 5, 4, 5, 4, 4, 5, 6, 5, 4, 3, 4, 5, 6, 7, 7, 6, 5, 4, 4, 4, 5, 5, 6, 5, 4, 3 };
        //    var watermelon = new List<int> { 2, 3, 2, 3, 4, 3, 3, 3, 4, 4, 5, 4, 3, 3, 3, 4, 5, 6, 6, 5, 4, 3, 3, 3, 3, 4, 4, 3, 2, 2 };

        //    return (labels, apple, orange, watermelon);
        //}

        //public (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon) GetWeeklyProductSalesData_HardCodeData()
        //{
        //    var labels = new List<string> { "Week 1", "Week 2", "Week 3", "Week 4" };

        //    var apple = new List<int> { 45, 38, 42, 50 };
        //    var orange = new List<int> { 28, 32, 30, 26 };
        //    var watermelon = new List<int> { 18, 22, 20, 19 };

        //    return (labels, apple, orange, watermelon);
        //}

        #endregion

        public List<DailyProductSales> GetLast30DaysSales()
        {
            try
            {
                var fromDate = DateTime.Now.Date.AddDays(-29);

                return _context.dailyproductsales
                    .Where(x => x.SalesDate >= fromDate)
                    .OrderBy(x => x.SalesDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                // TODO: log the error (Serilog, NLog, Built-in ILogger, etc.)
                Console.WriteLine("Error in GetLast30DaysSales: " + ex.Message);
                return new List<DailyProductSales>(); // return empty list to avoid crashing
            }
        }

        public List<T_tblProductSales> GetMonthlySales(int year)
        {
            try
            {
                return _context.T_tblProductSales
                               .Where(x => x.SaleYear == year)
                               .OrderBy(x => x.ProductName)
                               .ThenBy(x => x.SaleMonth)
                               .ToList();
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // Example: using NLog, log4net, or simply Debug/Console
                Console.WriteLine("Error fetching monthly sales: " + ex.Message);

                // Return empty list to avoid breaking calling code
                return new List<T_tblProductSales>();
            }
        }
    
    }
}
