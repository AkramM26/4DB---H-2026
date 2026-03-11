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
        // need to be changed when installing to a db not in memory!!!
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        GameService gameService = new GameService(context);
        await gameService.CreateGameAsync();

        GameDisplay.ShowGame(context);
        var player1 = context.Players.FirstOrDefault(p => p.PlayerNumber == 1);
        var player2 = context.Players.FirstOrDefault(p => p.PlayerNumber == 2);
        GameDisplay.AskAction(1,context);
        GameDisplay.AskAction(2,context);

        ArmyService armyService = new ArmyService(context);
 

    }
}
