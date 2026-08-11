namespace SalePredict.Models
{
    public class ProductPredictionResultViewModel
    {
        public string ProductName { get; set; }

        public int ThreeMonthsAgoSales { get; set; }

        public int TwoMonthsAgoSales { get; set; }

        public int LastMonthSales { get; set; }

        public float Last3MonthAverage { get; set; }

        public bool Prediction { get; set; }

        public float Probability { get; set; }
    }

}
