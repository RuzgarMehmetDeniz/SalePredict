using Microsoft.AspNetCore.Mvc;
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
            var forecast =await _forecastService.ForecastAsync();
            return View(forecast);
        }

    }
}
