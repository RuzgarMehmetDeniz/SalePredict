namespace SalePredict.Dtos.ForecastDtos
{
    public class SalesForecastDto
    {
        public float[] ForecastedSales { get; set; } = Array.Empty<float>();
        public float[] LowerBoundSales { get; set; } = Array.Empty<float>();
        public float[] UpperBoundSales { get; set; } = Array.Empty<float>();
    }
}