using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace HugoLand.Core.Data
{
    public class HugoLandContextFactory : IDesignTimeDbContextFactory<HugoLandContext>
    {
        private const string DesignTimeFallbackConnectionString = "Data Source=hugoland.db";

        public static HugoLandContext Create(string connectionString)
        {
            var resolved = ResolveRelativeDataSource(connectionString);

            var options = new DbContextOptionsBuilder<HugoLandContext>()
                .UseSqlite(resolved)
                .UseLazyLoadingProxies()
                .EnableSensitiveDataLogging()
                .Options;

            return new HugoLandContext(options);
        }

        public HugoLandContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("HUGOLAND_CONNECTION_STRING")
                ?? DesignTimeFallbackConnectionString;

            return Create(connectionString);
        }

        private static string ResolveRelativeDataSource(string connectionString)
        {
            var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(builder.DataSource) && !Path.IsPathRooted(builder.DataSource))
            {
                builder.DataSource = Path.Combine(AppContext.BaseDirectory, builder.DataSource);
            }
            return builder.ToString();
        }
    }
}
