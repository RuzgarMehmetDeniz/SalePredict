using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalePredict.Context;
using SalePredict.Web.Models.Admin;
using SalePredict.Web.Models.Shared;
namespace SalePredict.Controllers
{
    public class AdminController : Controller
    {
        private readonly SalePredictContext _context;

        public AdminController(SalePredictContext context)
        {
            _context = context;
        }

        // ============================================================
        // DASHBOARD
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var maxDate = await _context.Sales
                .MaxAsync(x => (DateTime?)x.SaleDate);

            if (maxDate == null)
                return View(CreateEmptyDashboard());

            var lastDate = maxDate.Value.Date;

            var currentStart = lastDate.AddDays(-29);
            var previousStart = currentStart.AddDays(-59);
            var previousEnd = currentStart.AddDays(-30);

            var totalSales = await _context.Sales
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0;

            var totalOrders = await _context.Sales.CountAsync();

            var averageOrderValue = totalOrders > 0
                ? totalSales / totalOrders
                : 0;

            var totalProducts = await _context.Sales
                .Select(x => x.ProductName)
                .Distinct()
                .CountAsync();

            var activeCities = await _context.Sales
                .Select(x => x.City)
                .Distinct()
                .CountAsync();

            var currentSales = await _context.Sales
                .Where(x => x.SaleDate.Date >= currentStart &&
                            x.SaleDate.Date <= lastDate)
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0;

            var previousSales = await _context.Sales
                .Where(x => x.SaleDate.Date >= previousStart &&
                            x.SaleDate.Date <= previousEnd)
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0;

            var currentOrders = await _context.Sales
                .CountAsync(x => x.SaleDate.Date >= currentStart &&
                                 x.SaleDate.Date <= lastDate);

            var previousOrders = await _context.Sales
                .CountAsync(x => x.SaleDate.Date >= previousStart &&
                                 x.SaleDate.Date <= previousEnd);

            var currentAverage = currentOrders > 0
                ? currentSales / currentOrders
                : 0;

            var previousAverage = previousOrders > 0
                ? previousSales / previousOrders
                : 0;

            var sparkline = await _context.Sales
                .Where(x => x.SaleDate.Date >= lastDate.AddDays(-13))
                .GroupBy(x => x.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Sales = g.Sum(x => x.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .Select(x => x.Sales)
                .ToListAsync();

            var salesOverviewData = await _context.Sales
                .Where(x => x.SaleDate.Date >= lastDate.AddDays(-29))
                .GroupBy(x => x.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Sales = g.Sum(x => x.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var categoryData = await _context.Sales
                .GroupBy(x => x.CategoryName)
                .Select(g => new
                {
                    Name = g.Key,
                    Value = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.Value)
                .Take(8)
                .ToListAsync();

            var topProductData = await _context.Sales
                .GroupBy(x => x.ProductName)
                .Select(g => new
                {
                    Name = g.Key,
                    Value = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToListAsync();

            var maxProductSale = topProductData
                .Select(x => x.Value)
                .DefaultIfEmpty(1)
                .Max();

            var cityData = await _context.Sales
                .GroupBy(x => x.City)
                .Select(g => new
                {
                    Name = g.Key,
                    Value = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToListAsync();

            var paymentData = await _context.Sales
                .GroupBy(x => x.PaymentMethod)
                .Select(g => new
                {
                    Name = g.Key,
                    Value = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            var campaignData = await _context.Sales
                .GroupBy(x => x.IsCampaign)
                .Select(g => new
                {
                    IsCampaign = g.Key,
                    Value = g.Sum(x => x.TotalPrice)
                })
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalSales = new Web.Models.Shared.KpiCardViewModel
                {
                    Id = "kpi-total-sales",
                    Label = "Total Sales",
                    Value = $"₺{totalSales:N0}",
                    DeltaPercent = CalculateDelta(currentSales, previousSales),
                    Sparkline = sparkline
                },

                TotalOrders = new Web.Models.Shared.KpiCardViewModel
                {
                    Id = "kpi-total-orders",
                    Label = "Total Orders",
                    Value = totalOrders.ToString("N0"),
                    DeltaPercent = CalculateDelta(currentOrders, previousOrders),
                    Sparkline = Array.Empty<decimal>()
                },

                AverageOrderValue = new Web.Models.Shared.KpiCardViewModel
                {
                    Id = "kpi-average-order",
                    Label = "Average Order Value",
                    Value = $"₺{averageOrderValue:N0}",
                    DeltaPercent = CalculateDelta(currentAverage, previousAverage),
                    Sparkline = Array.Empty<decimal>()
                },

                TotalProducts = new Web.Models.Shared.KpiCardViewModel
                {
                    Id = "kpi-total-products",
                    Label = "Total Products",
                    Value = totalProducts.ToString("N0"),
                    DeltaPercent = 0,
                    Sparkline = Array.Empty<decimal>()
                },

                ActiveCities = new Web.Models.Shared.KpiCardViewModel
                {
                    Id = "kpi-active-cities",
                    Label = "Active Cities",
                    Value = activeCities.ToString("N0"),
                    DeltaPercent = 0,
                    Sparkline = Array.Empty<decimal>()
                },

                SalesOverview = new Web.Models.Shared.LineAreaChartData
                {
                    Labels = salesOverviewData
                        .Select(x => x.Date.ToString("dd.MM"))
                        .ToList(),

                    Actual = salesOverviewData
                        .Select(x => x.Sales)
                        .ToList()
                },

                CategoryPerformance = new Web.Models.Shared.CategoryValueChartData
                {
                    Labels = categoryData.Select(x => x.Name).ToList(),
                    Values = categoryData.Select(x => x.Value).ToList()
                },

                TopProducts = topProductData
                    .Select(x => new TopProductRow
                    {
                        ProductName = x.Name,
                        TotalSales = x.Value,
                        SharePercent = maxProductSale > 0
                            ? x.Value / maxProductSale * 100
                            : 0
                    })
                    .ToList(),
                
                CityPerformance = new Web.Models.Shared.CategoryValueChartData
                {
                    Labels = cityData.Select(x => x.Name).ToList(),
                    Values = cityData.Select(x => x.Value).ToList()
                },

                PaymentMethods = new Web.Models.Shared.CategoryValueChartData
                {
                    Labels = paymentData.Select(x => x.Name).ToList(),
                    Values = paymentData.Select(x => x.Value).ToList()
                },

                CampaignPerformance = new Web.Models.Shared.GroupedSeriesChartData
                {
                    Labels = new List<string>
                    {
                        "Kampanyalı",
                        "Kampanyasız"
                    },

                    Series = new List<ChartSeries>
                    {
                        new ChartSeries
                        {
                            Name = "Sales",
                            Values = new List<decimal>
                            {
                                campaignData
                                    .Where(x => x.IsCampaign)
                                    .Select(x => x.Value)
                                    .FirstOrDefault(),

                                campaignData
                                    .Where(x => !x.IsCampaign)
                                    .Select(x => x.Value)
                                    .FirstOrDefault()
                            }
                        }
                    }
                }
            };

            return View(model);
        }


        // ============================================================
        // SALES ANALYSIS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> SalesAnalysis(
            string? city,
            string? category,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _context.Sales.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(city))
                query = query.Where(x => x.City == city);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(x => x.CategoryName == category);

            if (startDate.HasValue)
                query = query.Where(x => x.SaleDate >= startDate.Value);

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                query = query.Where(x => x.SaleDate < end);
            }

            var totalSales = await query
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0;

            var totalQuantity = await query
                .SumAsync(x => (decimal?)x.SalesQuantity) ?? 0;

            var totalOrders = await query.CountAsync();

            var averageOrderValue = totalOrders > 0
                ? totalSales / totalOrders
                : 0;

            var discountRate = await query
                .AverageAsync(x => (decimal?)x.DiscountRate) ?? 0;

            var trend = await query
                .GroupBy(x => x.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Sales = g.Sum(x => x.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var categoryBreakdown = await query
                .GroupBy(x => x.CategoryName)
                .Select(g => new
                {
                    Category = g.Key,
                    Sales = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.Sales)
                .ToListAsync();

            var model = new SalesAnalysisViewModel
            {
                Filters = await CreateFilterPanel(),

                TotalSales = totalSales,
                TotalQuantity = totalQuantity,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue,
                DiscountRatePercent = discountRate,

                SalesTrend = new LineAreaChartData
                {
                    Labels = trend
                        .Select(x => x.Date.ToString("dd.MM.yyyy"))
                        .ToList(),

                    Actual = trend
                        .Select(x => x.Sales)
                        .ToList()
                },

                CategoryBreakdown = new GroupedSeriesChartData
                {
                    Labels = categoryBreakdown
                        .Select(x => x.Category)
                        .ToList(),

                    Series = new List<ChartSeries>
                    {
                        new ChartSeries
                        {
                            Name = "Sales",
                            Values = categoryBreakdown
                                .Select(x => x.Sales)
                                .ToList()
                        }
                    }
                }
            };

            return View(model);
        }


        // ============================================================
        // PRODUCTS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Products(int page = 1)
        {
            const int pageSize = 20;

            var totalItems = await _context.Sales
                .Select(x => x.ProductName)
                .Distinct()
                .CountAsync();

            var products = await _context.Sales
                .GroupBy(x => new
                {
                    x.ProductName,
                    x.CategoryName
                })
                .Select(g => new ProductRow
                {
                    ProductId = g.Min(x => x.SaleId),
                    ProductName = g.Key.ProductName,
                    Category = g.Key.CategoryName,
                    TotalQuantity = g.Sum(x => x.SalesQuantity),
                    TotalSales = g.Sum(x => x.TotalPrice),
                    AveragePrice = g.Average(x => x.ProductPrice),
                    CampaignSales = g.Where(x => x.IsCampaign)
                                     .Sum(x => x.TotalPrice),
                    Performance = ""
                })
                .OrderByDescending(x => x.TotalSales)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (products.Count > 0)
            {
                var salesValues = products
                    .Select(x => x.TotalSales)
                    .OrderBy(x => x)
                    .ToList();

                var lowLimit = salesValues.First();
                var highLimit = salesValues.Last();

                foreach (var product in products)
                {
                    if (highLimit == lowLimit)
                        product.Performance = "Medium";
                    else if (product.TotalSales <=
                             lowLimit + (highLimit - lowLimit) * 0.33m)
                        product.Performance = "Low";
                    else if (product.TotalSales >=
                             lowLimit + (highLimit - lowLimit) * 0.66m)
                        product.Performance = "High";
                    else
                        product.Performance = "Medium";
                }
            }

            var model = new ProductsViewModel
            {
                Filters = await CreateFilterPanel(),

                Rows = products,

                Pagination = new PaginationViewModel
                {
                    Controller = "Admin",
                    Action = "Products",
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };

            return View(model);
        }


        // ============================================================
        // CATEGORIES
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Sales
                .GroupBy(x => x.CategoryName)
                .Select(g => new
                {
                    Category = g.Key,
                    ProductCount = g
                        .Select(x => x.ProductName)
                        .Distinct()
                        .Count(),

                    TotalSales = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSales)
                .ToListAsync();

            var grandTotal = categories
                .Sum(x => x.TotalSales);

            var model = new CategoriesViewModel
            {
                Rows = categories
                    .Select(x => new CategoryRow
                    {
                        CategoryName = x.Category,
                        ProductCount = x.ProductCount,
                        TotalSales = x.TotalSales,
                        SharePercent = grandTotal > 0
                            ? x.TotalSales / grandTotal * 100
                            : 0
                    })
                    .ToList()
            };

            return View(model);
        }


        // ============================================================
        // CITIES
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Cities(int page = 1)
        {
            const int pageSize = 20;

            var totalItems = await _context.Sales
                .Select(x => x.City)
                .Distinct()
                .CountAsync();

            var cities = await _context.Sales
                .GroupBy(x => new
                {
                    x.Country,
                    x.City
                })
                .Select(g => new CityRow
                {
                    Country = g.Key.Country,
                    City = g.Key.City,

                    TotalSales = g.Sum(x => x.TotalPrice),

                    Orders = g.Count(),

                    AverageBasket = g.Average(x => x.TotalPrice),

                    CampaignRatePercent =
                        g.Count() > 0
                            ? g.Count(x => x.IsCampaign) * 100m / g.Count()
                            : 0,

                    ClusterLabel = "Henüz sınıflandırılmadı"
                })
                .OrderByDescending(x => x.TotalSales)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new CitiesViewModel
            {
                Rows = cities,

                Pagination = new PaginationViewModel
                {
                    Controller = "Admin",
                    Action = "Cities",
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };

            return View(model);
        }


        // ============================================================
        // ORDERS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Orders(int page = 1)
        {
            const int pageSize = 20;

            var totalItems = await _context.Sales.CountAsync();

            var orders = await _context.Sales
                .AsNoTracking()
                .OrderByDescending(x => x.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrderRow
                {
                    OrderNumber = $"ORD-{x.SaleId:D8}",
                    OrderDate = x.SaleDate,
                    City = x.City,
                    Total = x.TotalPrice,
                    PaymentMethod = x.PaymentMethod,
                    Status = "Completed"
                })
                .ToListAsync();

            var model = new OrdersViewModel
            {
                Filters = await CreateFilterPanel(),

                Rows = orders,

                Pagination = new PaginationViewModel
                {
                    Controller = "Admin",
                    Action = "Orders",
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };

            return View(model);
        }


        // ============================================================
        // REPORTS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var totalSales = await _context.Sales
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0;

            var totalOrders = await _context.Sales
                .CountAsync();

            var totalQuantity = await _context.Sales
                .SumAsync(x => (decimal?)x.SalesQuantity) ?? 0;

            var averageOrder = totalOrders > 0
                ? totalSales / totalOrders
                : 0;

            var trend = await _context.Sales
                .GroupBy(x => x.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Sales = g.Sum(x => x.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var model = new ReportsViewModel
            {
                Filters = await CreateFilterPanel(),

                Cards = new List<ReportCard>
                {
                    new ReportCard
                    {
                        Title = "Total Sales",
                        Value = $"₺{totalSales:N0}",
                        Description = "Toplam satış tutarı"
                    },

                    new ReportCard
                    {
                        Title = "Total Orders",
                        Value = totalOrders.ToString("N0"),
                        Description = "Toplam satış kaydı"
                    },

                    new ReportCard
                    {
                        Title = "Total Quantity",
                        Value = totalQuantity.ToString("N0"),
                        Description = "Toplam satılan ürün adedi"
                    },

                    new ReportCard
                    {
                        Title = "Average Order Value",
                        Value = $"₺{averageOrder:N0}",
                        Description = "Ortalama sipariş tutarı"
                    }
                },

                TrendChart = new LineAreaChartData
                {
                    Labels = trend
                        .Select(x => x.Date.ToString("dd.MM.yyyy"))
                        .ToList(),

                    Actual = trend
                        .Select(x => x.Sales)
                        .ToList()
                }
            };

            return View(model);
        }


        // ============================================================
        // ML.NET SAYFALARI
        // ============================================================

        [HttpGet]
        public IActionResult Forecasting(
            string? city,
            DateTime? rangeStart,
            DateTime? rangeEnd,
            int horizon = 7)
        {
            var model = new ForecastingViewModel
            {
                SelectedCity = city,
                RangeStart = rangeStart,
                RangeEnd = rangeEnd,
                ForecastHorizon = horizon,
                Cities = new List<FilterOption>(),
                ForecastTable = new List<ForecastTableRow>(),
                SsaParameters = new SsaParameters()
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult BinaryClassification(
            string? city,
            string? product,
            string? targetMonth,
            decimal threshold = 7000)
        {
            var model = new BinaryClassificationViewModel
            {
                SelectedCity = city,
                SelectedProduct = product,
                TargetMonth = targetMonth,
                SalesThreshold = threshold
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult MulticlassClassification()
        {
            return View(new MulticlassClassificationViewModel());
        }


        [HttpGet]
        public IActionResult AnomalyDetection()
        {
            return View(new AnomalyDetectionViewModel());
        }


        [HttpGet]
        public IActionResult Clustering()
        {
            return View(new ClusteringViewModel());
        }


        // ============================================================
        // PRODUCT DETAIL
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await _context.Sales
                .Where(x => x.SaleId == id)
                .Select(x => new ProductRow
                {
                    ProductId = x.SaleId,
                    ProductName = x.ProductName,
                    Category = x.CategoryName,
                    TotalQuantity = x.SalesQuantity,
                    TotalSales = x.TotalPrice,
                    AveragePrice = x.ProductPrice,
                    CampaignSales = x.IsCampaign
                        ? x.TotalPrice
                        : 0,
                    Performance = "Analiz ediliyor"
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            var trend = await _context.Sales
                .Where(x => x.ProductName == product.ProductName)
                .GroupBy(x => x.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Sales = g.Sum(x => x.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var model = new ProductDetailViewModel
            {
                Product = product,

                SalesTrend = new LineAreaChartData
                {
                    Labels = trend
                        .Select(x => x.Date.ToString("dd.MM.yyyy"))
                        .ToList(),

                    Actual = trend
                        .Select(x => x.Sales)
                        .ToList()
                },

                ForecastNote = "ML.NET tahmini henüz çalıştırılmadı."
            };

            return View(model);
        }


        // ============================================================
        // SETTINGS
        // ============================================================

        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }


        // ============================================================
        // FILTER PANEL
        // ============================================================

        private async Task<FilterPanelViewModel> CreateFilterPanel()
        {
            var cities = await _context.Sales
                .Select(x => x.City)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var categories = await _context.Sales
                .Select(x => x.CategoryName)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return new FilterPanelViewModel
            {
                Controller = "Admin",

                Fields = new List<FilterFieldViewModel>
                {
                    new FilterFieldViewModel
                    {
                        Name = "city",
                        Label = "City",
                        Type = FilterFieldType.Select,

                        Options = cities
                            .Select(x => new FilterOption
                            {
                                Value = x,
                                Text = x
                            })
                            .ToList()
                    },

                    new FilterFieldViewModel
                    {
                        Name = "category",
                        Label = "Category",
                        Type = FilterFieldType.Select,

                        Options = categories
                            .Select(x => new FilterOption
                            {
                                Value = x,
                                Text = x
                            })
                            .ToList()
                    }
                }
            };
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private static decimal CalculateDelta(decimal current, decimal previous)
        {
            if (previous == 0)
                return current > 0 ? 100 : 0;

            return ((current - previous) / previous) * 100;
        }


        private static decimal CalculateDelta(int current, int previous)
        {
            if (previous == 0)
                return current > 0 ? 100 : 0;

            return ((decimal)(current - previous) / previous) * 100;
        }


        private static DashboardViewModel CreateEmptyDashboard()
        {
            return new DashboardViewModel
            {
                TotalSales = EmptyKpi(
                    "kpi-total-sales",
                    "Total Sales",
                    "₺0"),

                TotalOrders = EmptyKpi(
                    "kpi-total-orders",
                    "Total Orders",
                    "0"),

                AverageOrderValue = EmptyKpi(
                    "kpi-average-order",
                    "Average Order Value",
                    "₺0"),

                TotalProducts = EmptyKpi(
                    "kpi-total-products",
                    "Total Products",
                    "0"),

                ActiveCities = EmptyKpi(
                    "kpi-active-cities",
                    "Active Cities",
                    "0"),

                SalesOverview = new LineAreaChartData(),

                CategoryPerformance = new CategoryValueChartData(),

                TopProducts = new List<TopProductRow>(),

                CityPerformance = new CategoryValueChartData(),

                PaymentMethods = new CategoryValueChartData(),

                CampaignPerformance = new GroupedSeriesChartData()
            };
        }


        private static KpiCardViewModel EmptyKpi(
            string id,
            string label,
            string value)
        {
            return new KpiCardViewModel
            {
                Id = id,
                Label = label,
                Value = value,
                DeltaPercent = 0,
                Sparkline = Array.Empty<decimal>()
            };
        }
    }
}