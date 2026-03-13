using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using HugoLand.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand
{
    public class MenuManager(HugoLandContext context)
    {
        private HugoLandContext Context = context;
        private GameService _gameService = new GameService(context);

        public async void GameLoop()
        {
            bool showMainMenu = true;
            while (showMainMenu)
            {
                showMainMenu = false;
                int choice = MenuDisplay.ShowMainMenu();
                switch (choice)
                {
                    case 1:
                        await _gameService.CreateGameAsync();
                        break;
                    case 2:
                        List<Game> games = await Context.Games.ToListAsync();
                        int gameNumber = MenuDisplay.ShowLoadingMenu(games);
                        if (gameNumber == -1)
                            showMainMenu = true;
                        else
                            _gameService.LoadGame(games[gameNumber].Id);
                        break;
                    case 3: return;
                }
                await _gameService.StartTurnAsync();
                
                var game = await Context.Games
                    .Include(g=>g.MilitaryDetachments)
                    .ThenInclude(m=> m.Territory)
                    .FirstOrDefaultAsync();
                MilitaryDetachment militaryDetachment;

                GameDisplay.ShowGame(context);
                choice = MenuDisplay.ShowActionChoice(context);
                switch (choice)
                {
                    case 1:
                        militaryDetachment = MenuDisplay.ShowArmyChoice(game.MilitaryDetachments, game);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
