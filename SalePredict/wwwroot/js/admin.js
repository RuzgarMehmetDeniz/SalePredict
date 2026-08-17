/* ==========================================================================
   SalePredict Admin UI — shared behaviors
   ÖNEMLİ: Bu dosya hiçbir satış/tahmin verisi İÇERMEZ.
   Tüm veriler her sayfanın kendi .cshtml dosyasında
   <script type="application/json" id="...-data"> etiketleri içine
   Razor tarafından @Html.Raw(Json.Serialize(Model.X)) ile basılır.
   Bu dosya sadece o veriyi okuyup Chart.js'e besler.
   ========================================================================== */

(function () {
  "use strict";

  /* ---------- Sidebar collapse/responsive ---------- */
  function initSidebar() {
    var shell = document.querySelector(".app-shell");
    var toggle = document.querySelector("[data-sidebar-toggle]");
    if (!shell || !toggle) return;

    var saved = localStorage.getItem("sp_sidebar_collapsed");
    if (saved === "1") shell.classList.add("is-collapsed");

    toggle.addEventListener("click", function () {
      shell.classList.toggle("is-collapsed");
      localStorage.setItem("sp_sidebar_collapsed", shell.classList.contains("is-collapsed") ? "1" : "0");
    });
  }

  /* ---------- JSON veri okuma yardımcıları ----------
     Kullanım (Razor tarafında):
     <script type="application/json" id="sales-overview-data">
       @Html.Raw(Json.Serialize(Model.SalesOverviewChart))
     </script>
     JS tarafında:
     const data = SP.readJson("sales-overview-data");
  ------------------------------------------------------ */
  function readJson(elementId) {
    var el = document.getElementById(elementId);
    if (!el) return null;
    try {
      return JSON.parse(el.textContent);
    } catch (e) {
      console.warn("SP.readJson: '" + elementId + "' parse edilemedi.", e);
      return null;
    }
  }

  /* ---------- Ortak Chart.js tema ayarları ---------- */
  var palette = {
    primary: "#4F6BFF",
    primarySoft: "rgba(79,107,255,0.12)",
    success: "#16A34A",
    warning: "#D97706",
    danger: "#DC2626",
    info: "#0EA5E9",
    grid: "#E4E7F0",
    text: "#6B7280"
  };

  function applyChartDefaults() {
    if (typeof Chart === "undefined") return;
    Chart.defaults.font.family = "'Inter', sans-serif";
    Chart.defaults.font.size = 12;
    Chart.defaults.color = palette.text;
    Chart.defaults.plugins.legend.display = false;
    Chart.defaults.plugins.tooltip.backgroundColor = "#1A1F36";
    Chart.defaults.plugins.tooltip.padding = 10;
    Chart.defaults.plugins.tooltip.cornerRadius = 8;
    Chart.defaults.elements.line.tension = 0.35;
  }

  /* ---------- Chart oluşturucular ----------
     Her fonksiyon, Model'den gelen ve id="...-data" script etiketine
     basılmış veriyi bekler. Beklenen JSON şekli fonksiyon üstünde belirtilir.
  --------------------------------------------------------------------- */

  // Beklenen JSON: { labels: string[], actual: number[], forecast?: number[] }
  function renderLineArea(canvasId, dataElId, opts) {
    var canvas = document.getElementById(canvasId);
    var data = readJson(dataElId);
    if (!canvas || !data) return null;
    opts = opts || {};
    var datasets = [{
      label: opts.actualLabel || "Actual",
      data: data.actual,
      borderColor: palette.primary,
      backgroundColor: palette.primarySoft,
      fill: true,
      pointRadius: 0,
      borderWidth: 2
    }];
    if (data.forecast) {
      datasets.push({
        label: opts.forecastLabel || "Forecast",
        data: data.forecast,
        borderColor: palette.warning,
        borderDash: [5, 4],
        backgroundColor: "transparent",
        fill: false,
        pointRadius: 0,
        borderWidth: 2
      });
    }
    return new Chart(canvas.getContext("2d"), {
      type: "line",
      data: { labels: data.labels, datasets: datasets },
      options: {
        responsive: true, maintainAspectRatio: false,
        interaction: { mode: "index", intersect: false },
        scales: {
          x: { grid: { display: false } },
          y: { grid: { color: palette.grid }, beginAtZero: false }
        }
      }
    });
  }

  // Beklenen JSON: { labels: string[], values: number[] }
  function renderBar(canvasId, dataElId, color) {
    var canvas = document.getElementById(canvasId);
    var data = readJson(dataElId);
    if (!canvas || !data) return null;
    return new Chart(canvas.getContext("2d"), {
      type: "bar",
      data: {
        labels: data.labels,
        datasets: [{ data: data.values, backgroundColor: color || palette.primary, borderRadius: 6, maxBarThickness: 34 }]
      },
      options: {
        responsive: true, maintainAspectRatio: false,
        scales: { x: { grid: { display: false } }, y: { grid: { color: palette.grid } } }
      }
    });
  }

  // Beklenen JSON: { labels: string[], values: number[] }
  function renderHorizontalBar(canvasId, dataElId, color) {
    var canvas = document.getElementById(canvasId);
    var data = readJson(dataElId);
    if (!canvas || !data) return null;
    return new Chart(canvas.getContext("2d"), {
      type: "bar",
      data: { labels: data.labels, datasets: [{ data: data.values, backgroundColor: color || palette.primary, borderRadius: 6 }] },
      options: {
        indexAxis: "y", responsive: true, maintainAspectRatio: false,
        scales: { x: { grid: { color: palette.grid } }, y: { grid: { display: false } } }
      }
    });
  }

  // Beklenen JSON: { labels: string[], values: number[] }
  function renderDonut(canvasId, dataElId, colors) {
    var canvas = document.getElementById(canvasId);
    var data = readJson(dataElId);
    if (!canvas || !data) return null;
    return new Chart(canvas.getContext("2d"), {
      type: "doughnut",
      data: {
        labels: data.labels,
        datasets: [{ data: data.values, backgroundColor: colors || [palette.primary, palette.info, palette.success, palette.warning, palette.danger], borderWidth: 0 }]
      },
      options: { responsive: true, maintainAspectRatio: false, cutout: "68%" }
    });
  }

  // Beklenen JSON: { labels: string[], series: [{ name, values, color }] }
  function renderGroupedBar(canvasId, dataElId) {
    var canvas = document.getElementById(canvasId);
    var data = readJson(dataElId);
    if (!canvas || !data) return null;
    var colorList = [palette.primary, palette.info, palette.success, palette.warning];
    return new Chart(canvas.getContext("2d"), {
      type: "bar",
      data: {
        labels: data.labels,
        datasets: data.series.map(function (s, i) {
          return { label: s.name, data: s.values, backgroundColor: s.color || colorList[i % colorList.length], borderRadius: 6, maxBarThickness: 26 };
        })
      },
      options: {
        responsive: true, maintainAspectRatio: false,
        plugins: { legend: { display: true, position: "bottom", labels: { boxWidth: 8, usePointStyle: true } } },
        scales: { x: { grid: { display: false } }, y: { grid: { color: palette.grid } } }
      }
    });
  }

  // Beklenen JSON: { labels: string[], actual: number[], expected: number[], anomalyIndexes: number[] }
  function renderAnomalyChart(canvasId, dataElId) {
    var canvas = document.getElementById(canvasId);
    var data = readJson(dataElId);
    if (!canvas || !data) return null;
    var anomalySet = new Set(data.anomalyIndexes || []);
    return new Chart(canvas.getContext("2d"), {
      type: "line",
      data: {
        labels: data.labels,
        datasets: [
          { label: "Actual", data: data.actual, borderColor: palette.primary, backgroundColor: "transparent", pointRadius: function (ctx) { return anomalySet.has(ctx.dataIndex) ? 5 : 0; }, pointBackgroundColor: palette.danger, borderWidth: 2 },
          { label: "Expected", data: data.expected, borderColor: palette.grid, borderDash: [4, 4], backgroundColor: "transparent", pointRadius: 0, borderWidth: 1.5 }
        ]
      },
      options: {
        responsive: true, maintainAspectRatio: false,
        interaction: { mode: "index", intersect: false },
        scales: { x: { grid: { display: false } }, y: { grid: { color: palette.grid } } }
      }
    });
  }

  // Sparkline (KPI kartları içindeki mini grafik). Beklenen JSON: number[]
  function renderSparkline(canvasId, dataElId, color) {
    var canvas = document.getElementById(canvasId);
    var values = readJson(dataElId);
    if (!canvas || !values) return null;
    return new Chart(canvas.getContext("2d"), {
      type: "line",
      data: { labels: values.map(function (_, i) { return i; }), datasets: [{ data: values, borderColor: color || palette.primary, borderWidth: 1.75, pointRadius: 0, fill: false }] },
      options: {
        responsive: true, maintainAspectRatio: false,
        plugins: { tooltip: { enabled: false } },
        scales: { x: { display: false }, y: { display: false } }
      }
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    initSidebar();
    applyChartDefaults();
    document.dispatchEvent(new CustomEvent("sp:ready"));
  });

  window.SP = {
    readJson: readJson,
    palette: palette,
    renderLineArea: renderLineArea,
    renderBar: renderBar,
    renderHorizontalBar: renderHorizontalBar,
    renderDonut: renderDonut,
    renderGroupedBar: renderGroupedBar,
    renderAnomalyChart: renderAnomalyChart,
    renderSparkline: renderSparkline
  };
})();
