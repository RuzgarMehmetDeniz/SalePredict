namespace SalePredict.Models
{
    public class ProductMonthlyPrediction
    {
        public bool PredictedLabel { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }

}
