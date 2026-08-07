using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using SalePredict.Context;
using SalePredict.Models;

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
        public async Task<List<DailySaleData>> GetIstanbulDailySalesAsync()
        {
            var values = await _context.Sales.Where(x => x.City == "İstanbul")
                .GroupBy(x => x.SaleDate.Date)
                .Select(x => new DailySaleData
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
        public async Task<SalesForecast> ForecastAsync()
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
            var forecastEngine = model.CreateTimeSeriesEngine<DailySaleData, SalesForecast>(_mlContext);

            var forecast = forecastEngine.Predict();

            return forecast;
        }


    }
}
