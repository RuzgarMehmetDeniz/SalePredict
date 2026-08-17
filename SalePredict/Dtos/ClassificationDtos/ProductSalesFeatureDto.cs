namespace SalePredict.Dtos.ClassificationDtos
{
    public class ProductSalesFeatureDto
    {
        public string ProductName { get; set; }
        public float MonthMinus3 { get; set; }   // 3 ay önceki satış
        public float MonthMinus2 { get; set; }   // 2 ay önceki satış
        public float MonthMinus1 { get; set; }   // 1 ay önceki (en yakın geçmiş)
        public float AverageSales { get; set; }  // 3 aylık ortalama
        public bool Label { get; set; }          // Sonraki ay eşiği geçti mi (sadece training'de dolu)
    }
}