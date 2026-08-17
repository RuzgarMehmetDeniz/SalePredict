using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalePredict.Context;
using SalePredict.Service;

namespace SalePredict.Controllers
{
    public class BinaryClassificationController : Controller
    {
        private readonly ForecastService _service;
        private readonly SalePredictContext _context;

        public BinaryClassificationController(ForecastService service, SalePredictContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task<IActionResult> Index(string city, int threshold = 7000)
        {
            var cities = await _context.Sales
                .Select(x => x.City)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            ViewBag.Cities = cities;
            ViewBag.SelectedCity = city;
            ViewBag.Threshold = threshold;

            if (string.IsNullOrEmpty(city))
            {
                return View(); // henüz şehir seçilmedi, sadece dropdown göster
            }

            var result = await _service.TrainAndPredictAsync(city, threshold);
            return View(result);
        }
    }
}
