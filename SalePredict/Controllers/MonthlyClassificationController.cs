using Microsoft.AspNetCore.Mvc;
using SalePredict.Service;

namespace SalePredict.Controllers
{
    public class MonthlyClassificationController : Controller
    {
        private readonly MonthlyClassificationService _classificationService;
        public MonthlyClassificationController(MonthlyClassificationService classificationService)
        {
            _classificationService = classificationService;
        }
        public async Task<IActionResult> Index()
        {
            var results = await _classificationService.PredictAllProductsAsync();
            return View(results);
        }
    }

}
