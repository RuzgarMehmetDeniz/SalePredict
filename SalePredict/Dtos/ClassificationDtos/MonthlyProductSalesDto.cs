namespace SalePredict.Dtos.ClassificationDtos
{
    public class MonthlyProductSalesDto
    {
        public string ProductName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalQuantity { get; set; }
    }
}
