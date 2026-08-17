
using SalePredict.Web.Models.Shared;

namespace SalePredict.Web.Models.Admin
{
    // ===================== 1. DASHBOARD =====================
    public class DashboardViewModel
    {
        public KpiCardViewModel TotalSales { get; set; } = default!;
        public KpiCardViewModel TotalOrders { get; set; } = default!;
        public KpiCardViewModel AverageOrderValue { get; set; } = default!;
        public KpiCardViewModel TotalProducts { get; set; } = default!;
        public KpiCardViewModel ActiveCities { get; set; } = default!;

        public LineAreaChartData SalesOverview { get; set; } = default!;      // son N gün/ay satış trendi
        public CategoryValueChartData CategoryPerformance { get; set; } = default!;
        public List<TopProductRow> TopProducts { get; set; } = new();
        public CategoryValueChartData CityPerformance { get; set; } = default!;
        public CategoryValueChartData PaymentMethods { get; set; } = default!; // donut
        public GroupedSeriesChartData CampaignPerformance { get; set; } = default!; // kampanyalı vs kampanyasız
    }

    public class TopProductRow
    {
        public string ProductName { get; set; } = default!;
        public decimal TotalSales { get; set; }
        public decimal SharePercent { get; set; } // yatay bar genişliği için
    }

    // ===================== 2. FORECASTING =====================
    public class ForecastingViewModel
    {
        public List<FilterOption> Cities { get; set; } = new();
        public string? SelectedCity { get; set; }
        public DateTime? RangeStart { get; set; }
        public DateTime? RangeEnd { get; set; }
        public int ForecastHorizon { get; set; } = 7;

        public decimal NextSevenDaysTotal { get; set; }
        public decimal ExpectedTotalSales { get; set; }
        public decimal AverageDailyForecast { get; set; }
        public decimal ConfidenceLevelPercent { get; set; } = 95m;

        public LineAreaChartData ActualVsForecastChart { get; set; } = default!; // son 30 gün + 7 gün tahmin

        public List<ForecastTableRow> ForecastTable { get; set; } = new();

        // SSA parametreleri (ML.NET tarafında kullanılan gerçek değerler)
        public SsaParameters SsaParameters { get; set; } = default!;
    }

    public class ForecastTableRow
    {
        public DateTime Date { get; set; }
        public decimal Forecast { get; set; }
        public decimal LowerBound { get; set; }
        public decimal UpperBound { get; set; }
    }

    public class SsaParameters
    {
        public int WindowSize { get; set; }
        public int SeriesLength { get; set; }
        public int TrainSize { get; set; }
        public int Horizon { get; set; }
        public decimal ConfidenceLevel { get; set; }
    }

    // ===================== 3. BINARY CLASSIFICATION =====================
    public class BinaryClassificationViewModel
    {
        public List<FilterOption> Cities { get; set; } = new();
        public List<FilterOption> Products { get; set; } = new();
        public string? SelectedCity { get; set; }
        public string? SelectedProduct { get; set; }
        public string? TargetMonth { get; set; }
        public decimal SalesThreshold { get; set; } = 7000;

        public bool? PredictionResult { get; set; }       // YES/NO — null: henüz çalıştırılmadı
        public decimal? ProbabilityPercent { get; set; }

        public List<BinaryPredictionRow> Rows { get; set; } = new();
        public PaginationViewModel? Pagination { get; set; }
    }

    public class BinaryPredictionRow
    {
        public string ProductName { get; set; } = default!;
        public decimal Last3MonthsSales { get; set; }
        public decimal LastMonthSales { get; set; }
        public decimal ThreeMonthAverage { get; set; }
        public string TargetMonth { get; set; } = default!;
        public bool Prediction { get; set; }
        public decimal ProbabilityPercent { get; set; }
    }

    // ===================== 4. MULTICLASS CLASSIFICATION =====================
    public class MulticlassClassificationViewModel
    {
        public int TotalProducts { get; set; }
        public int LowCount { get; set; }
        public int MediumCount { get; set; }
        public int HighCount { get; set; }

        public CategoryValueChartData DistributionChart { get; set; } = default!; // Low/Medium/High dağılımı

        public List<MulticlassRow> Rows { get; set; } = new();
        public PaginationViewModel? Pagination { get; set; }
    }

    public class MulticlassRow
    {
        public string ProductName { get; set; } = default!;
        public string Category { get; set; } = default!;
        public decimal RecentSales { get; set; }
        public decimal AverageSales { get; set; }
        public string PredictedClass { get; set; } = default!; // Low / Medium / High
        public decimal ProbabilityPercent { get; set; }
    }

    // ===================== 5. ANOMALY DETECTION =====================
    public class AnomalyDetectionViewModel
    {
        public int TotalDays { get; set; }
        public int NormalDays { get; set; }
        public int AnomalyDays { get; set; }
        public decimal AnomalyRatePercent { get; set; }

        public AnomalyChartData DailySalesChart { get; set; } = default!;

        public List<AnomalyRow> Rows { get; set; } = new();
        public PaginationViewModel? Pagination { get; set; }
    }

    public class AnomalyRow
    {
        public DateTime Date { get; set; }
        public string City { get; set; } = default!;
        public decimal ActualSales { get; set; }
        public decimal ExpectedSales { get; set; }
        public decimal AnomalyScore { get; set; }
        public bool IsAnomaly { get; set; }
    }

    // ===================== 6. CLUSTERING =====================
    public class ClusteringViewModel
    {
        public List<ClusterSummary> Clusters { get; set; } = new();
        public List<CityClusterRow> Cities { get; set; } = new();
    }

    public class ClusterSummary
    {
        public int ClusterId { get; set; }
        public string Label { get; set; } = default!; // "High Value Cities" vb.
        public int CityCount { get; set; }
        public decimal AverageSales { get; set; }
        public string ColorHex { get; set; } = "#4F6BFF";
    }

    public class CityClusterRow
    {
        public string City { get; set; } = default!;
        public decimal AverageSales { get; set; }
        public decimal AverageBasketValue { get; set; }
        public decimal CampaignRatePercent { get; set; }
        public int ClusterId { get; set; }
        public string ClusterLabel { get; set; } = default!;
    }

    // ===================== 7. SALES ANALYSIS =====================
    public class SalesAnalysisViewModel
    {
        public FilterPanelViewModel Filters { get; set; } = default!;

        public decimal TotalSales { get; set; }
        public decimal TotalQuantity { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal DiscountRatePercent { get; set; }

        public LineAreaChartData SalesTrend { get; set; } = default!;
        public GroupedSeriesChartData CategoryBreakdown { get; set; } = default!;
    }

    // ===================== 8. PRODUCTS =====================
    public class ProductsViewModel
    {
        public FilterPanelViewModel Filters { get; set; } = default!;
        public List<ProductRow> Rows { get; set; } = new();
        public PaginationViewModel? Pagination { get; set; }
    }

    public class ProductRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string Category { get; set; } = default!;
        public decimal TotalQuantity { get; set; }
        public decimal TotalSales { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal CampaignSales { get; set; }
        public string Performance { get; set; } = default!; // Low/Medium/High badge
    }

    public class ProductDetailViewModel
    {
        public ProductRow Product { get; set; } = default!;
        public LineAreaChartData SalesTrend { get; set; } = default!;
        public string? ForecastNote { get; set; } // ML.NET tahmin özet metni
    }

    // ===================== 9. CITIES =====================
    public class CitiesViewModel
    {
        public List<CityRow> Rows { get; set; } = new();
        public PaginationViewModel? Pagination { get; set; }
    }

    public class CityRow
    {
        public string Country { get; set; } = default!;
        public string City { get; set; } = default!;
        public decimal TotalSales { get; set; }
        public int Orders { get; set; }
        public decimal AverageBasket { get; set; }
        public decimal CampaignRatePercent { get; set; }
        public string ClusterLabel { get; set; } = default!;
    }

    // ===================== 10. REPORTS =====================
    public class ReportsViewModel
    {
        public FilterPanelViewModel Filters { get; set; } = default!;
        public List<ReportCard> Cards { get; set; } = new();
        public LineAreaChartData TrendChart { get; set; } = default!;
    }

    public class ReportCard
    {
        public string Title { get; set; } = default!;
        public string Value { get; set; } = default!;
        public string? Description { get; set; }
    }

    // ===================== Categories / Orders (liste ekranları) =====================
    public class CategoriesViewModel
    {
        public List<CategoryRow> Rows { get; set; } = new();
    }

    public class CategoryRow
    {
        public string CategoryName { get; set; } = default!;
        public int ProductCount { get; set; }
        public decimal TotalSales { get; set; }
        public decimal SharePercent { get; set; }
    }

    public class OrdersViewModel
    {
        public FilterPanelViewModel Filters { get; set; } = default!;
        public List<OrderRow> Rows { get; set; } = new();
        public PaginationViewModel? Pagination { get; set; }
    }

    public class OrderRow
    {
        public string OrderNumber { get; set; } = default!;
        public DateTime OrderDate { get; set; }
        public string City { get; set; } = default!;
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = default!;
        public string Status { get; set; } = default!;
    }
}
