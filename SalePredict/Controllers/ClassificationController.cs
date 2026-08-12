using Microsoft.AspNetCore.Mvc;
using SalePredict.Models;
using SalePredict.Service;

namespace SalePredict.Controllers
{
    public class ClassificationController : Controller

    {
        private readonly ClassificationService _classificationService;

        public ClassificationController(ClassificationService classificationService)
        {
            _classificationService = classificationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ProductSalesPredictionViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProductSalesPredictionViewModel model)
        {
            var prediction = await _classificationService.PredictAsync(model.ProductName, model.ProductPrice, model.Month);

            model.Prediction = prediction.PredictedLabel;
            model.Probability = prediction.Probability;

            return View(model);
        }
    }
}