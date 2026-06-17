using Maruwa_Emgmt.Models;

namespace Maruwa_Emgmt.InterFace
{
    public interface IProductSalesRepository
    {
        List<T_tblProductSales> GetMonthlySales(int year);

        List<DailyProductSales> GetLast30DaysSales();

        (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon)GetDailyProductSalesData();
        //(List<string> labels, List<int> apple, List<int> orange, List<int> watermelon) GetWeeklyProductSalesData();
        (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon) GetWeeklyProductSalesData();

        (List<string> labels, List<int> apple, List<int> orange, List<int> watermelon) GetMonthlyProductSalesData();

    }
}
