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

        GameService gameService = new GameService(context);
        await gameService.CreateGameAsync();
        await gameService.SaveGameAsync();
        await gameService.StartTurnAsync();

        //Game game = context.Games
        //    .Include(g => g.Players)
        //    .Include(g => g.Territories)
        //    .ThenInclude(t => t.MilitaryDetachment)
        //    .Include(t => t.Territories)
        //    .ThenInclude(t => t.Installation)
        //    .First();
        //game.SaveName = $"Partie {DateTime.Now.ToString("yyyy-MM-dd HH'h'mm")}"; Pour le nom de la partie ?

        GameDisplay.ShowGame(context);
    }
}
