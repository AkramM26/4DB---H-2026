using HugoLand.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HugoLand;

internal class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = config.GetConnectionString("HugoLand")
            ?? throw new InvalidOperationException("Missing connection string 'HugoLand' in appsettings.json");

        using var context = HugoLandContextFactory.Create(connectionString);
        await context.Database.MigrateAsync();

        MenuManager menuManager = new MenuManager(context);
        menuManager.GameLoop();
    }
}
