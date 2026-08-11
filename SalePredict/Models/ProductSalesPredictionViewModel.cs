namespace SalePredict.Models
{
    public class ProductSalesPredictionViewModel
    {
        public string ProductName { get; set; }
        public float ProductPrice { get; set; }
        public int Month { get; set; }
        public bool? Prediction { get; set; }
        public float? Probability { get; set; }
    }

}
