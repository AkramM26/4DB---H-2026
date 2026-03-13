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



        //GameService gameService = new GameService(context);
        //await gameService.CreateGameAsync();
        //await gameService.SaveGameAsync();
        //await gameService.StartTurnAsync();
        //await gameService.SaveGameAsync();

        //GameDisplay.ShowGame(context);
        //var player1 = context.Players.FirstOrDefault(p => p.PlayerNumber == 1);
        //var player2 = context.Players.FirstOrDefault(p => p.PlayerNumber == 2);

        //GameDisplay.AskAction(1,context);
        //GameDisplay.AskAction(2,context);

        //Game game = context.Games
        //    .Include(g => g.Players)
        //    .Include(g => g.Territories)
        //    .ThenInclude(t => t.MilitaryDetachment)
        //    .Include(t => t.Territories)
        //    .ThenInclude(t => t.Installation)
        //    .First();
        //game.SaveName = $"Partie {DateTime.Now.ToString("yyyy-MM-dd HH'h'mm")}"; Pour le nom de la partie ?

        //GameDisplay.ShowGame(context);
    }
}
