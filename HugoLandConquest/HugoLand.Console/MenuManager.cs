using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using HugoLand.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HugoLand.Core.Services.ArmyService;

namespace HugoLand
{
    public class MenuManager(HugoLandContext context)
    {
        private HugoLandContext Context = context;
        private GameService _gameService = new GameService(context);
        private ArmyService _armyService = new ArmyService(context);
        private InstallationService _installationService = new InstallationService(context);

        public async void GameLoop()
        {
            bool showMainMenu = true;
            while (showMainMenu)
            {
                bool skipGame = false;
                showMainMenu = false;
                int choice = MenuDisplay.ShowMainMenu();
                switch (choice)
                {
                    case 1:
                        await _gameService.CreateGameAsync();
                        break;
                    case 2:
                        List<Game> games = await Context.Games.IgnoreQueryFilters().ToListAsync();
                        int gameNumber = MenuDisplay.ShowLoadingMenu(games);
                        if (gameNumber == -1)
                        {
                            showMainMenu = true;
                            skipGame = true;
                        }
                        else
                            _gameService.LoadGame(games[gameNumber].Id);
                        break;
                    case 3: return;
                }

                while (!skipGame)
                {
                    await _gameService.StartTurnAsync();

                    var game = await Context.Games
                        .Include(g => g.MilitaryDetachments)
                        .ThenInclude(m => m.Territory)
                        .ThenInclude(t => t.Installation)
                        .Include(g => g.Players)
                        .FirstOrDefaultAsync();
                    MilitaryDetachment? militaryDetachment = null;

                    bool showActionChoice = true;
                    bool showArmyAction = false;

                    if (game.IsFinished)
                    {
                        MenuDisplay.ShowVictoryScreen(game, Context);
                        showActionChoice = false;
                        skipGame = true;
                        showMainMenu = true;
                    }

                    while (showActionChoice)

                    {
                        choice = MenuDisplay.ShowActionChoice(Context);
                        switch (choice)
                        {
                            case 1:
                                militaryDetachment = MenuDisplay.ShowArmyChoice(game.MilitaryDetachments, game, Context);
                                showActionChoice = true;
                                showArmyAction = true;
                                break;
                            case 2:
                                await _gameService.SaveGameAsync();
                                showActionChoice = true;
                                showArmyAction = false;
                                break;
                            case 3:
                                showActionChoice = false;
                                skipGame = true;
                                showMainMenu = true;
                                showArmyAction = false;
                                break;
                            case 4:
                                showActionChoice = false;
                                showArmyAction = false;
                                await _gameService.EndTurnAsync();
                                break;
                        }
                        if (showArmyAction && militaryDetachment != null)
                        {
                            choice = MenuDisplay.ShowArmyAction(militaryDetachment, Context);
                            switch (choice)
                            {
                                case 1:
                                    {
                                        char moveChoice = ' ';
                                        while (moveChoice != 'X')
                                        {
                                            List<Territory> lstTerritory = await _armyService.TryMove(militaryDetachment, militaryDetachment.Territory.PositionX, militaryDetachment.Territory.PositionY);
                                            moveChoice = MenuDisplay.ShowMoveMenu(lstTerritory, militaryDetachment, Context);
                                            if (moveChoice == 'N')
                                            {
                                                MoveResult moveResult = await _armyService.Move(militaryDetachment.Id, Movements.North);
                                                // Show move result.
                                            }
                                            else if (moveChoice == 'S')
                                            {
                                                MoveResult moveResult = await _armyService.Move(militaryDetachment.Id, Movements.South);
                                                // Show move result.
                                            }
                                            else if (moveChoice == 'E')
                                            {
                                                MoveResult moveResult = await _armyService.Move(militaryDetachment.Id, Movements.East);
                                                // Show move result.
                                            }
                                            else if (moveChoice == 'W')
                                            {
                                                MoveResult moveResult = await _armyService.Move(militaryDetachment.Id, Movements.West);
                                                // Show move result.
                                            }
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        ResultService result = await _installationService.BuildCampAsync(militaryDetachment.Id);
                                        break;
                                    }
                                case 3:
                                    {
                                        ResultService result = await _installationService.UpgradeCampToFortificationAsync(militaryDetachment.Id);
                                        break;
                                    }
                                case 4:
                                    {
                                        int reinforceNumber = MenuDisplay.ShowReinforceNumber(militaryDetachment, Context);
                                        Player player = game!.Players.FirstOrDefault(p => p.PlayerNumber == game.PlayerTurn)!;
                                        ResultService result = await _armyService.Reinforce(militaryDetachment, reinforceNumber, player);
                                        break;
                                    }
                                case 5:
                                    {
                                        List<Territory> lstTerritory = await _armyService.TryMove(militaryDetachment, militaryDetachment.Territory.PositionX, militaryDetachment.Territory.PositionY);
                                        int splitNumber = MenuDisplay.ShowSplitNumber(militaryDetachment, Context);
                                        char moveChoice = MenuDisplay.ShowMoveMenu(lstTerritory, militaryDetachment, Context);

                                        if (splitNumber == 0)
                                            showActionChoice = true;
                                        else if (moveChoice == 'N')
                                        {
                                            MoveResult moveResult = await _armyService.Split(militaryDetachment.Id, Movements.North, splitNumber);
                                            // Show move result.
                                        }
                                        else if (moveChoice == 'S')
                                        {
                                            MoveResult moveResult = await _armyService.Split(militaryDetachment.Id, Movements.South, splitNumber);
                                            // Show move result.
                                        }
                                        else if (moveChoice == 'E')
                                        {
                                            MoveResult moveResult = await _armyService.Split(militaryDetachment.Id, Movements.East, splitNumber);
                                            // Show move result.
                                        }
                                        else if (moveChoice == 'W')
                                        {
                                            MoveResult moveResult = await _armyService.Split(militaryDetachment.Id, Movements.West, splitNumber);
                                            // Show move result.
                                        }
                                        break;
                                    }
                                case 6:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

            }
        }
    }
}
