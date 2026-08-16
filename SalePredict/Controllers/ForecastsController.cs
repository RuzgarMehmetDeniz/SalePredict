using Microsoft.AspNetCore.Mvc;
using SalePredict.Models;
using SalePredict.Service;

namespace SalePredict.Controllers
{
    public class ForecastsController : Controller
    {
        private readonly ForecastService _forecastService;


        public ForecastsController(ForecastService forecastService)
        {
            _forecastService = forecastService;
        }
        public async Task<IActionResult> Index()
        {
            var dailySales = await _forecastService.GetIstanbulDailySalesAsync();

            var forecast = await _forecastService.ForecastAsync();

            var lastDate = dailySales.Max(x => x.SaleDate);

            var results = new List<ForecastResultDto>();

            for (int i = 0; i < forecast.ForecastedSales.Length; i++)
            {
                results.Add(new ForecastResultDto
                {
                    Date = lastDate.AddDays(i + 1),

                    ForecastedSales =
                        forecast.ForecastedSales[i],

                    LowerBound =
                        forecast.LowerBoundSales[i],

                    UpperBound =
                        forecast.UpperBoundSales[i]
                });
            }

            return View(results);
        }

    }
}
