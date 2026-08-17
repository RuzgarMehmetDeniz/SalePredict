using System;
using System.Collections.Generic;

namespace SalePredict.Web.Models.Shared
{
    /// <summary>Views/Shared/_KpiCard.cshtml için</summary>
    public class KpiCardViewModel
    {
        public string Id { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string Value { get; set; } = default!;
        public decimal DeltaPercent { get; set; }
        public IEnumerable<decimal> Sparkline { get; set; } = Array.Empty<decimal>();
        public string? IconSvgKey { get; set; }
    }

    /// <summary>Views/Shared/_FilterPanel.cshtml için</summary>
    public class FilterPanelViewModel
    {
        public string Controller { get; set; } = default!;
        public string Action { get; set; } = default!;
        public List<FilterFieldViewModel> Fields { get; set; } = new();
    }

    public enum FilterFieldType { Text, Select, DateRange }

    public class FilterFieldViewModel
    {
        public string Name { get; set; } = default!;
        public string Label { get; set; } = default!;
        public FilterFieldType Type { get; set; } = FilterFieldType.Select;
        public string? SelectedValue { get; set; }
        public string? Placeholder { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        // Select seçenekleri Controller'da DB'den (örn. Cities, Categories) doldurulur.
        public List<FilterOption> Options { get; set; } = new();
    }

    public class FilterOption
    {
        public string Value { get; set; } = default!;
        public string Text { get; set; } = default!;
    }

    /// <summary>Views/Shared/_DataTable.cshtml için</summary>
    public class DataTableViewModel
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public bool ShowExport { get; set; } = true;
        public List<DataTableColumn> Columns { get; set; } = new();
        // Her satır: Field adı -> hazır formatlanmış (string/decimal/HTML-badge markup) değer.
        // Controller/Service katmanında EF Core sorgusundan projekte edilir.
        public List<Dictionary<string, object>> Rows { get; set; } = new();
        public PaginationViewModel? Pagination { get; set; }
    }

    public class DataTableColumn
    {
        public string Header { get; set; } = default!;
        public string Field { get; set; } = default!;
        public bool Numeric { get; set; }
    }

    /// <summary>Views/Shared/_Pagination.cshtml için</summary>
    public class PaginationViewModel
    {
        public string Controller { get; set; } = default!;
        public string Action { get; set; } = default!;
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)Math.Max(PageSize, 1));
    }

    /// <summary>Chart.js'e SP.readJson üzerinden beslenen tipler (admin.js ile birebir eşleşir)</summary>
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

    public class GroupedSeriesChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<ChartSeries> Series { get; set; } = new();
    }

    public class ChartSeries
    {
        public string Name { get; set; } = default!;
        public List<decimal> Values { get; set; } = new();
        public string? Color { get; set; }
    }

    public class AnomalyChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Actual { get; set; } = new();
        public List<decimal> Expected { get; set; } = new();
        public List<int> AnomalyIndexes { get; set; } = new();
    }
}
