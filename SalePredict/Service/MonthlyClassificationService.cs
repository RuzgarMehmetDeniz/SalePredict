using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using SalePredict.Context;
using SalePredict.Models;

namespace SalePredict.Service
{
    public class MonthlyClassificationService
    {
        private readonly SalePredictContext _context;
        private readonly MLContext _mlContext;

        private const int SalesThreshold = 7000;

        public MonthlyClassificationService(
            SalePredictContext context,
            MLContext mlContext)
        {
            _context = context;
            _mlContext = mlContext;
        }

        // 1. Milano'daki satışları ürün + yıl + ay bazında toplar
        private async Task<List<MonthlyProductSale>> GetMilanoMonthlySalesAsync()
        {
            var values = await _context.Sales
                .Where(x => x.City == "Milano")
                .GroupBy(x => new
                {
                    x.ProductName,
                    Year = x.SaleDate.Year,
                    Month = x.SaleDate.Month
                })
                .Select(x => new MonthlyProductSale
                {
                    ProductName = x.Key.ProductName,
                    Year = x.Key.Year,
                    Month = x.Key.Month,
                    TotalSalesQuantity = x.Sum(y => y.SalesQuantity)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return values;
        }

        // 2. Geçmiş aylardan Binary Classification eğitim datası oluşturur
        public async Task<List<ProductMonthlyClassificationData>>
            GetTrainingDataAsync()
        {
            var monthlySales = await GetMilanoMonthlySalesAsync();

            var trainingData =
                new List<ProductMonthlyClassificationData>();

            var productGroups =
                monthlySales.GroupBy(x => x.ProductName);

            foreach (var productGroup in productGroups)
            {
                var sales = productGroup
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList();

                // En az 4 ay gerekiyor:
                // İlk 3 ay feature
                // 4. ay label
                if (sales.Count < 4)
                    continue;

                for (int i = 3; i < sales.Count; i++)
                {
                    var threeMonthsAgo = sales[i - 3];
                    var twoMonthsAgo = sales[i - 2];
                    var lastMonth = sales[i - 1];
                    var targetMonth = sales[i];

                    trainingData.Add(
                        new ProductMonthlyClassificationData
                        {
                            ProductName = productGroup.Key,

                            ThreeMonthsAgoSales =
                                threeMonthsAgo.TotalSalesQuantity,

                            TwoMonthsAgoSales =
                                twoMonthsAgo.TotalSalesQuantity,

                            LastMonthSales =
                                lastMonth.TotalSalesQuantity,

                            Last3MonthAverage =
                                (
                                    threeMonthsAgo.TotalSalesQuantity +
                                    twoMonthsAgo.TotalSalesQuantity +
                                    lastMonth.TotalSalesQuantity
                                ) / 3f,

                            TargetMonth =
                                targetMonth.Month,

                            Label =
                                targetMonth.TotalSalesQuantity
                                >= SalesThreshold
                        });
                }
            }

            return trainingData;
        }

        // 3. Milano'daki bütün ürünler için gelecek ay tahmin yapar
        public async Task<List<ProductPredictionResultViewModel>>
            PredictAllProductsAsync()
        {
            // Eğitim verisini oluştur
            var trainingData =
                await GetTrainingDataAsync();

            if (trainingData.Count == 0)
            {
                throw new InvalidOperationException(
                    "Modeli eğitmek için yeterli veri bulunamadı.");
            }

            // Binary classification için hem true hem false veri olmalı
            if (!trainingData.Any(x => x.Label) ||
                !trainingData.Any(x => !x.Label))
            {
                throw new InvalidOperationException(
                    "Modeli eğitmek için hem 7000 üzeri " +
                    "hem de 7000 altı satış kayıtları bulunmalıdır.");
            }

            IDataView dataView =
                _mlContext.Data.LoadFromEnumerable(trainingData);

            // 4. ML.NET Pipeline
            var pipeline =
                _mlContext.Transforms.Categorical
                    .OneHotEncoding(
                        outputColumnName: "ProductNameEncoded",
                        inputColumnName: "ProductName")

                .Append(
                    _mlContext.Transforms.Concatenate(
                        "Features",
                        "ProductNameEncoded",
                        "ThreeMonthsAgoSales",
                        "TwoMonthsAgoSales",
                        "LastMonthSales",
                        "Last3MonthAverage",
                        "TargetMonth"))

                .Append(
                    _mlContext.Transforms.NormalizeMeanVariance(
                        "Features"))

                .Append(
                    _mlContext.BinaryClassification.Trainers
                        .SdcaLogisticRegression(
                            labelColumnName: "Label",
                            featureColumnName: "Features"));

            // 5. Modeli eğit
            var model =
                pipeline.Fit(dataView);

            // 6. Prediction Engine
            var predictionEngine =
                _mlContext.Model
                    .CreatePredictionEngine<
                        ProductMonthlyClassificationData,
                        ProductMonthlyPrediction>(model);

            // Güncel aylık Milano satışlarını tekrar al
            var monthlySales =
                await GetMilanoMonthlySalesAsync();

            var results =
                new List<ProductPredictionResultViewModel>();

            var productGroups =
                monthlySales.GroupBy(x => x.ProductName);

            // 7. Her ürünü ayrı ayrı tahmin et
            foreach (var productGroup in productGroups)
            {
                var sales = productGroup
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList();

                // Tahmin için son 3 aya ihtiyacımız var
                if (sales.Count < 3)
                    continue;

                var threeMonthsAgo =
                    sales[^3];

                var twoMonthsAgo =
                    sales[^2];

                var lastMonth =
                    sales[^1];

                // Son ayın bir sonraki ayını bul
                int targetMonth =
                    lastMonth.Month == 12
                        ? 1
                        : lastMonth.Month + 1;

                float average =
                    (
                        threeMonthsAgo.TotalSalesQuantity +
                        twoMonthsAgo.TotalSalesQuantity +
                        lastMonth.TotalSalesQuantity
                    ) / 3f;

                var input =
                    new ProductMonthlyClassificationData
                    {
                        ProductName =
                            productGroup.Key,

                        ThreeMonthsAgoSales =
                            threeMonthsAgo.TotalSalesQuantity,

                        TwoMonthsAgoSales =
                            twoMonthsAgo.TotalSalesQuantity,

                        LastMonthSales =
                            lastMonth.TotalSalesQuantity,

                        Last3MonthAverage =
                            average,

                        TargetMonth =
                            targetMonth
                    };

                var prediction =
                    predictionEngine.Predict(input);

                results.Add(
                    new ProductPredictionResultViewModel
                    {
                        ProductName =
                            productGroup.Key,

                        ThreeMonthsAgoSales =
                            threeMonthsAgo.TotalSalesQuantity,

                        TwoMonthsAgoSales =
                            twoMonthsAgo.TotalSalesQuantity,

                        LastMonthSales =
                            lastMonth.TotalSalesQuantity,

                        Last3MonthAverage =
                            average,

                        Prediction =
                            prediction.PredictedLabel,

                        Probability =
                            prediction.Probability
                    });
            }

            // Önce yüksek ihtimalli ürünler
            return results
                .OrderByDescending(x => x.Probability)
                .ToList();
        }
    }
}
