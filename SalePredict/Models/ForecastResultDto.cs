namespace SalePredict.Models
{
    public class ForecastResultDto
    {
        public DateTime Date { get; set; }
        public float ForecastedSales { get; set; }
        public float LowerBound { get; set; }
        public float UpperBound { get; set; }
    }
}
