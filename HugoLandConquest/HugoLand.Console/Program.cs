using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using HugoLand.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace HugoLand;

internal class Program
{
    static async Task Main(string[] args)
    {
        using var context = new HugoLandContextFactory().CreateDbContext([]);
        // need to be changed when installing to a db not in memory!!!
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        MenuManager menuManager = new MenuManager(context);
        menuManager.GameLoop();
    }
}
