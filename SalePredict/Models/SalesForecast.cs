namespace SalePredict.Models
{
    public class SalesForecast
    {
        public float[] ForecastedSales { get; set; }

        public float[] LowerBoundSales { get; set; }

        public float[] UpperBoundSales { get; set; }
    }
}
