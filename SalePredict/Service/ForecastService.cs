using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using SalePredict.Context;
using SalePredict.Dtos.ClassificationDtos;
using SalePredict.Dtos.ForecastDtos;

namespace SalePredict.Service
{
    public class ForecastService
    {
        private readonly SalePredictContext _context;
        private readonly MLContext _mlContext;

        public ForecastService(SalePredictContext context, MLContext mlContext)
        {
            _context = context;
            _mlContext = mlContext;
        }

        public async Task<List<DailySalesDto>> GetIstanbulDailySalesAsync()
        {
            var values = await _context.Sales.Where(x => x.City == "İstanbul")
                .GroupBy(x => x.SaleDate.Date)
                .Select(x => new DailySalesDto
                {
                    SaleDate = x.Key,
                    TotalSalesQuantity = x.Sum(y => y.SalesQuantity)
                })
                .OrderBy(x => x.SaleDate).ToListAsync();
            return values;
        }

        public async Task<IDataView> GetTrainingDataAsync()
        {
            var dailySales = await GetIstanbulDailySalesAsync();
            return _mlContext.Data.LoadFromEnumerable(dailySales);
        }

        public async Task<SalesForecastDto> ForecastAsync()
        {
            var dailySales = await GetIstanbulDailySalesAsync();
            var dataView = _mlContext.Data.LoadFromEnumerable(dailySales);
            int trainSize = dailySales.Count;

            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                    outputColumnName: "ForecastedSales",
                    inputColumnName: "TotalSalesQuantity",
                    windowSize: 7,
                    seriesLength: 30,
                    trainSize: trainSize,
                    horizon: 7,
                    confidenceLevel: 0.95f,
                    confidenceLowerBoundColumn: "LowerBoundSales",
                    confidenceUpperBoundColumn: "UpperBoundSales"
                );

            var model = pipeline.Fit(dataView);
            var forecastEngine = model.CreateTimeSeriesEngine<DailySalesDto, SalesForecastDto>(_mlContext);
            var forecast = forecastEngine.Predict();
            return forecast;
        }

        public async Task<List<MonthlyProductSalesDto>> GetMonthlyProductSalesAsync(string city)
        {
            var values = await _context.Sales
                .Where(x => x.City == city)
                .GroupBy(x => new { x.ProductName, x.SaleDate.Year, x.SaleDate.Month })
                .Select(g => new MonthlyProductSalesDto
                {
                    ProductName = g.Key.ProductName,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalQuantity = g.Sum(x => x.SalesQuantity)
                })
                .OrderBy(x => x.ProductName).ThenBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return values;
        }

        public List<ProductSalesFeatureDto> BuildTrainingWindows(List<MonthlyProductSalesDto> monthlySales, int threshold)
        {
            var result = new List<ProductSalesFeatureDto>();

            var grouped = monthlySales
                .GroupBy(x => x.ProductName);

            foreach (var group in grouped)
            {
                var months = group
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList();

                // i, i+1, i+2 -> 3 aylık pencere; i+3 -> label ayı
                for (int i = 0; i + 3 < months.Count; i++)
                {
                    result.Add(new ProductSalesFeatureDto
                    {
                        ProductName = months[i].ProductName,
                        MonthMinus3 = months[i].TotalQuantity,
                        MonthMinus2 = months[i + 1].TotalQuantity,
                        MonthMinus1 = months[i + 2].TotalQuantity,
                        AverageSales = (months[i].TotalQuantity + months[i + 1].TotalQuantity + months[i + 2].TotalQuantity) / 3f,
                        Label = months[i + 3].TotalQuantity >= threshold
                    });
                }
            }

            return result;
        }

        public async Task<BinaryClassificationResultDto> TrainAndPredictAsync(string city, int threshold = 7000)
        {
            var monthlySales = await GetMonthlyProductSalesAsync(city);
            var windows = BuildTrainingWindows(monthlySales, threshold);

            var dataView = _mlContext.Data.LoadFromEnumerable(windows);

            // Train/Test split (%80/%20)
            var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(ProductSalesFeatureDto.MonthMinus3),
                    nameof(ProductSalesFeatureDto.MonthMinus2),
                    nameof(ProductSalesFeatureDto.MonthMinus1),
                    nameof(ProductSalesFeatureDto.AverageSales))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: nameof(ProductSalesFeatureDto.Label),
                    featureColumnName: "Features"));

            var model = pipeline.Fit(split.TrainSet);

            // Test set üzerinde değerlendirme
            var testPredictions = model.Transform(split.TestSet);
            var metrics = _mlContext.BinaryClassification.Evaluate(
                testPredictions,
                labelColumnName: nameof(ProductSalesFeatureDto.Label));

            // Gerçek "gelecek ay" tahmini için label'ı olmayan en güncel pencereler
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductSalesFeatureDto, ProductSalesPredictionDto>(model);
            var latestWindows = BuildLatestWindowsForPrediction(monthlySales);

            var predictions = latestWindows.Select(w =>
            {
                var pred = predictionEngine.Predict(w);
                return new ProductPredictionResultDto
                {
                    ProductName = w.ProductName,
                    PredictedLabel = pred.PredictedLabel,
                    Probability = pred.Probability
                };
            }).ToList();

            return new BinaryClassificationResultDto
            {
                Accuracy = metrics.Accuracy,
                Precision = metrics.PositivePrecision,
                Recall = metrics.PositiveRecall,
                F1Score = metrics.F1Score,
                AUC = metrics.AreaUnderRocCurve,
                Predictions = predictions
            };
        }
        public List<ProductSalesFeatureDto> BuildLatestWindowsForPrediction(List<MonthlyProductSalesDto> monthlySales)
        {
            var result = new List<ProductSalesFeatureDto>();

            var grouped = monthlySales.GroupBy(x => x.ProductName);

            foreach (var group in grouped)
            {
                var months = group
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList();

                if (months.Count < 3) continue;

                var last3 = months.Skip(months.Count - 3).ToList();

                result.Add(new ProductSalesFeatureDto
                {
                    ProductName = last3[0].ProductName,
                    MonthMinus3 = last3[0].TotalQuantity,
                    MonthMinus2 = last3[1].TotalQuantity,
                    MonthMinus1 = last3[2].TotalQuantity,
                    AverageSales = (last3[0].TotalQuantity + last3[1].TotalQuantity + last3[2].TotalQuantity) / 3f
                    // Label set edilmiyor - bu gerçek bir "bilinmeyen gelecek" tahmini
                });
            }

            return result;
        }
    }
}