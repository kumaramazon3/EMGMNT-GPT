using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;

namespace Maruwa_Emgmt.BAL
{
    public class ProductSalesService
    {
        private readonly IProductSalesRepository _repository;

        public ProductSalesService(IProductSalesRepository repository)
        {
            _repository = repository;
        }

        public List<T_tblProductSales> GetMonthlySales(int year)
        {
            return _repository.GetMonthlySales(year);
        }

        public object GetLast30DaysSales()
        {
            try
            {
                var data = _repository.GetLast30DaysSales();

                // Convert to chart-friendly JSON structure
                var labels = data
                    .Select(x => x.SalesDate.ToString("MMM-dd"))
                    .Distinct()
                    .ToList();

                var apple = data
                    .Where(x => x.ProductName == "Apple")
                    .OrderBy(x => x.SalesDate)
                    .Select(x => x.Quantity)
                    .ToList();

                var orange = data
                    .Where(x => x.ProductName == "Orange")
                    .OrderBy(x => x.SalesDate)
                    .Select(x => x.Quantity)
                    .ToList();

                var watermelon = data
                    .Where(x => x.ProductName == "Watermelon")
                    .OrderBy(x => x.SalesDate)
                    .Select(x => x.Quantity)
                    .ToList();

                return new
                {
                    labels = labels,
                    apple = apple,
                    orange = orange,
                    watermelon = watermelon
                };
            }
            catch (Exception ex)
            {
                // TODO: log the error (Console.WriteLine, ILogger, etc.)
                Console.WriteLine("Error in GetLast30DaysSales: " + ex.Message);

                // Return empty chart structure to avoid breaking the frontend
                return new
                {
                    labels = new List<string>(),
                    apple = new List<int>(),
                    orange = new List<int>(),
                    watermelon = new List<int>()
                };
            }
        }


        public (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon)GetDailyProductSales()
        {
            return _repository.GetDailyProductSalesData();
        }

        public (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon) GetWeeklyProductSalesData()
        {
            return _repository.GetWeeklyProductSalesData();
        }

        public (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon) GetMonthlyProductSalesData()
        {
            return _repository.GetMonthlyProductSalesData();
        }
    }
}
