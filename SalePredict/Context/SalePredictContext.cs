using Microsoft.EntityFrameworkCore;
using SalePredict.Entities;

namespace SalePredict.Context
{
    public class SalePredictContext:DbContext
    {
        public SalePredictContext(DbContextOptions<SalePredictContext> options) : base(options) { }

        public DbSet<Sale> Sales { get; set; }
    }
}
