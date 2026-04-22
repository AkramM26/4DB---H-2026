using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using Microsoft.EntityFrameworkCore;
using static HugoLand.Core.Services.ArmyService;

namespace HugoLand.Core.Services
{
    public class GameMaster(HugoLandContext context, IGameView view)
    {
        private readonly HugoLandContext _context = context;
        private readonly IGameView _view = view;
        private readonly GameService _gameService = new(context);
        private readonly ArmyService _armyService = new(context);
        private readonly InstallationService _installationService = new(context);

        public async Task RunAsync()
        {
            bool showMainMenu = true;
            while (showMainMenu)
            {
                bool skipGame = false;
                showMainMenu = false;

                int choice = await _view.ShowMainMenuAsync();
                switch (choice)
                {
                    case 1:
                        var (name, desc) = await _view.ShowNewGameInputAsync();
                        await _gameService.CreateGameAsync(GameConstants.gameSizeX, GameConstants.gameSizeY, name, desc);
                        break;
                    case 2:
                        List<Game> games = await _context.Games.IgnoreQueryFilters().ToListAsync();
                        int gameNumber = await _view.ShowLoadingMenuAsync(games);
                        if (gameNumber == -1)
                        {
                            showMainMenu = true;
                            skipGame = true;
                        }
                        else
                            _gameService.LoadGame(games[gameNumber].Id);
                        break;
                    case 3:
                        return;
                }

                while (!skipGame)
                {
                    await _gameService.StartTurnAsync();

                    var game = await _context.Games
                        .Include(g => g.MilitaryDetachments)
                            .ThenInclude(m => m.Territory)
                            .ThenInclude(t => t.Installation)
                        .Include(g => g.Players)
                        .FirstOrDefaultAsync();

                    MilitaryDetachment? militaryDetachment = null;
                    bool showActionChoice = true;
                    bool showArmyAction = false;

                    if (game!.IsFinished)
                    {
                        await _view.ShowVictoryScreenAsync(game);
                        showActionChoice = false;
                        skipGame = true;
                        showMainMenu = true;
                    }

                    while (showActionChoice)
                    {
                        _view.RefreshDisplay();
                        choice = await _view.ShowActionChoiceAsync();
                        switch (choice)
                        {
                            case 1:
                                militaryDetachment = await _view.ShowArmyChoiceAsync(game.MilitaryDetachments, game);
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
                            choice = await _view.ShowArmyActionAsync(militaryDetachment);
                            switch (choice)
                            {
                                case 1:
                                {
                                    char moveChoice = ' ';
                                    while (moveChoice != 'X')
                                    {
                                        List<Territory> lstTerritory = await _armyService.TryMove(
                                            militaryDetachment,
                                            militaryDetachment.Territory.PositionX,
                                            militaryDetachment.Territory.PositionY);

                                        moveChoice = await _view.ShowMoveMenuAsync(lstTerritory, militaryDetachment);

                                        MoveResult? moveResult = null;
                                        if (moveChoice == 'N')
                                            moveResult = await _armyService.Move(militaryDetachment.Id, Movements.North);
                                        else if (moveChoice == 'S')
                                            moveResult = await _armyService.Move(militaryDetachment.Id, Movements.South);
                                        else if (moveChoice == 'E')
                                            moveResult = await _armyService.Move(militaryDetachment.Id, Movements.East);
                                        else if (moveChoice == 'W')
                                            moveResult = await _armyService.Move(militaryDetachment.Id, Movements.West);

                                        if (moveResult != null)
                                            await _view.ShowMoveResultAsync(moveResult);
                                    }
                                    break;
                                }
                                case 2:
                                {
                                    await _installationService.BuildCampAsync(militaryDetachment.Id);
                                    break;
                                }
                                case 3:
                                {
                                    await _installationService.UpgradeCampToFortificationAsync(militaryDetachment.Id);
                                    break;
                                }
                                case 4:
                                {
                                    int reinforceNumber = await _view.ShowReinforceNumberAsync(militaryDetachment);
                                    Player player = game.Players.First(p => p.PlayerNumber == game.PlayerTurn);
                                    await _armyService.Reinforce(militaryDetachment, reinforceNumber, player);
                                    break;
                                }
                                case 5:
                                {
                                    List<Territory> lstTerritory = await _armyService.TryMove(
                                        militaryDetachment,
                                        militaryDetachment.Territory.PositionX,
                                        militaryDetachment.Territory.PositionY);

                                    int splitNumber = await _view.ShowSplitNumberAsync(militaryDetachment);
                                    char moveChoice = await _view.ShowMoveMenuAsync(lstTerritory, militaryDetachment);

                                    if (splitNumber == 0)
                                        showActionChoice = true;
                                    else if (moveChoice == 'N')
                                        await _armyService.Split(militaryDetachment.Id, Movements.North, splitNumber);
                                    else if (moveChoice == 'S')
                                        await _armyService.Split(militaryDetachment.Id, Movements.South, splitNumber);
                                    else if (moveChoice == 'E')
                                        await _armyService.Split(militaryDetachment.Id, Movements.East, splitNumber);
                                    else if (moveChoice == 'W')
                                        await _armyService.Split(militaryDetachment.Id, Movements.West, splitNumber);
                                    break;
                                }
                                case 6:
                                    break;
                            }
                            showArmyAction = false;
                        }
                    }
                }
            }
        }
    }
}
