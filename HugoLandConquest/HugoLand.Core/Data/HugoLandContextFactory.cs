using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Data
{
    public class HugoLandContextFactory : IDesignTimeDbContextFactory<HugoLandContext>
    {
        public HugoLandContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HugoLandContext>();

            optionsBuilder
                .UseSqlite("DataSource=:memory:")
                .UseLazyLoadingProxies()
                .EnableSensitiveDataLogging();

            return new HugoLandContext(optionsBuilder.Options);
        }
    }
}
