using System.Reflection;

namespace SalePredict.Entities
{
    namespace SalePredict.Entities
    {
        public class Sale
        {
            public int SaleId { get; set; }
            public DateTime SaleDate { get; set; }
            public string ProductName { get; set; }
            public decimal ProductPrice { get; set; }
            public int SalesQuantity { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalPrice { get; set; }
            public string Gender { get; set; }
            public string PaymentMethod { get; set; }
            public string Country { get; set; }
            public string City { get; set; }
        }
    }
}
