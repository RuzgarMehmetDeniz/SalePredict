namespace SalePredict.Dtos.ForecastDtos
{
    public class ForecastPageViewModel
    {
        public string City { get; set; } = string.Empty;
        public List<string> Cities { get; set; } = new();
        public List<DailySalesDto> Last30Days { get; set; } = new();
        public List<ForecastResultDto> Forecast { get; set; } = new();
    }
}
