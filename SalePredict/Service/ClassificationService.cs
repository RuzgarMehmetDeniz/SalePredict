using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using SalePredict.Context;
using SalePredict.Models;

namespace SalePredict.Service
{
    public class ClassificationService
    {
        private readonly SalePredictContext _context;
        private readonly MLContext _mlContext;

        public ClassificationService(SalePredictContext context, MLContext mLContext)
        {
            _context = context;
            _mlContext = mLContext;
        }
        public async Task<List<ProductSalesClassificationData>> GetKolnClassificationDataAsync()
        {
            var values = await _context.Sales
                .Where(x => x.City == "Köln")
                .GroupBy(x => new
                {
                    x.ProductName,
                    Year = x.SaleDate.Year,
                    Month = x.SaleDate.Month
                })
                .Select(x => new
                {
                    ProductName = x.Key.ProductName,
                    ProductPrice = x.Max(y => y.ProductPrice),
                    Month = x.Key.Month,
                    TotalSalesQuantity = x.Sum(y => y.SalesQuantity)
                })
                .ToListAsync();

            var trainingData = values
                .Select(x => new ProductSalesClassificationData
                {
                    ProductName = x.ProductName,
                    ProductPrice = (float)x.ProductPrice,
                    Month = x.Month,
                    Label = x.TotalSalesQuantity >= 7000
                })
                .ToList();

            return trainingData;
        }
        public async Task<ProductSalesPrediction> PredictAsync(string productName, float productPrice, int month)
        {
            var trainingData = await GetKolnClassificationDataAsync();

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding(
                        outputColumnName: "ProductNameEncoded",
                        inputColumnName: "ProductName")

                .Append(_mlContext.Transforms.Concatenate(
                        "Features",
                        "ProductNameEncoded",
                        "ProductPrice",
                        "Month"))

                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(dataView);

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductSalesClassificationData, ProductSalesPrediction>(model);

            var input = new ProductSalesClassificationData
            {
                ProductName = productName,
                ProductPrice = productPrice,
                Month = month
            };

            var prediction = predictionEngine.Predict(input);

            return prediction;
        }




    }
}
