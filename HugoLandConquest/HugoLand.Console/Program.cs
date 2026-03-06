using HugoLand.Core.Data;
using HugoLand.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace HugoLand;

internal class Program
{
    static async Task Main(string[] args)
    {
        using var context = new HugoLandContextFactory().CreateDbContext([]);
        // need to be changed when installing the a db not in memory!!!
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        GameService gameService = new GameService(context);
        await gameService.CreateGameAsync();


        GameDisplay.ShowGame(context);

    }
}
