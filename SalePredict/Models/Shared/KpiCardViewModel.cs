namespace SalePredict.Web.Models.Shared
{
    public class KpiCardViewModel
    {
        public string Id { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string Value { get; set; } = default!;
        public decimal DeltaPercent { get; set; }
        public IEnumerable<decimal> Sparkline { get; set; } = Array.Empty<decimal>();
    }

    public class LineAreaChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Actual { get; set; } = new();
        public List<decimal>? Forecast { get; set; }
    }

    public class CategoryValueChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Values { get; set; } = new();
    }

    public class ChartSeries
    {
        public string Name { get; set; } = default!;
        public List<decimal> Values { get; set; } = new();
    }

    public class GroupedSeriesChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<ChartSeries> Series { get; set; } = new();
    }

    public class AnomalyChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Actual { get; set; } = new();
        public List<decimal> Expected { get; set; } = new();
        public List<int> AnomalyIndexes { get; set; } = new();
    }

    public class FilterOption
    {
        public string Value { get; set; } = default!;
        public string Text { get; set; } = default!;
    }


    public enum FilterFieldType
    {
        Select,
        Text,
        DateRange
    }

    public class FilterFieldViewModel
    {
        public string Name { get; set; } = default!;
        public string Label { get; set; } = default!;
        public FilterFieldType Type { get; set; }
        public List<FilterOption> Options { get; set; } = new();
    }

    public class FilterPanelViewModel
    {
        public string Controller { get; set; } = default!;
        public List<FilterFieldViewModel> Fields { get; set; } = new();
    }

    public class PaginationViewModel
    {
        public string Controller { get; set; } = default!;
        public string Action { get; set; } = default!;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
    }
}