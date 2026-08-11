namespace SalePredict.Models
{
    public class ProductMonthlyClassificationData
    {
        public string ProductName { get; set; }

        public float LastMonthSales { get; set; }

        public float TwoMonthsAgoSales { get; set; }

        public float ThreeMonthsAgoSales { get; set; }

        public float Last3MonthAverage { get; set; }

        public float TargetMonth { get; set; }

        public bool Label { get; set; }
    }

}
