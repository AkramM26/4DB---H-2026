using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using HugoLand.Core.Services;
using HugoLand.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Media;

namespace HugoLand.WPF
{
    public partial class MainWindow : Window
    {
        private readonly HugoLandContext _context;
        private readonly GameService _gameService;

        private string ConnectionString;




        public MainWindow()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            ConnectionString = config.GetConnectionString("HugoLand")
                ?? throw new InvalidOperationException("Missing connection string 'HugoLand' in appsettings.json");

            _context = HugoLandContextFactory.Create(ConnectionString);
            _context.Database.Migrate();

            _gameService = new GameService(_context);

            Closed += (_, _) => _context.Dispose();
            AudioManager.MusiqueFond.Play();


        }

        private async void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            AudioManager.MenuSound.Play();



            var dlg = new NewGameDialog { Owner = this };
            if (dlg.ShowDialog() != true) return;

            try
            {
                await _gameService.CreateGameAsync(
                    GameConstants.gameSizeX, GameConstants.gameSizeY,
                    dlg.GameName,false, dlg.GameDescription);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Could not create game",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var gameId = _context.CurrentGameId;
            await LaunchGameAsync(gameId, startTurnOnOpen: true);
        }

        private async void btnLoadGame_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();


            var dlg = new LoadGame(_context) { Owner = this };
            if (dlg.ShowDialog() != true || dlg.SelectedGameId is null) return;

            _gameService.LoadGame(dlg.SelectedGameId.Value);
            await LaunchGameAsync(dlg.SelectedGameId.Value, startTurnOnOpen: false);
        }

        private async System.Threading.Tasks.Task LaunchGameAsync(Guid gameId, bool startTurnOnOpen)
        {
            var gameWindow = new GameDisplay(_context, gameId, startTurnOnOpen) { Owner = this };
            Hide();
            try
            {
                gameWindow.ShowDialog();
            }
            finally
            {
                Show();
            }
            await System.Threading.Tasks.Task.CompletedTask;
        }

        private void btnMapTemplate_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();    

            var window = new MapEditorOptions(ConnectionString);
            window.Show();
            this.Close();
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();

            Application.Current.Shutdown();
        }

        private void btnMapTemplateList_Click(object sender, RoutedEventArgs e)
        {
            var window = new MapTemplateList(ConnectionString);
            window.Show();
            this.Close();
        }

        private async void btnNewGameWithTemplate_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CreateGame(_context) { Owner = this };
            if (dlg.ShowDialog() != true || dlg.SelectedGameId is null) return;

            _gameService.LoadGame(dlg.SelectedGameId.Value);
            Guid newGameId = await _gameService.SaveGameAsync();
            await LaunchGameAsync(newGameId, startTurnOnOpen: false);
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();

            var dlg = new HistoryGame(_context) { Owner = this };
            if (dlg.ShowDialog() != true || dlg.SelectedGameId is null) return;
            Guid gameId = dlg.SelectedGameId.Value;

            // Appeler le rapport détaillé
        }

        private void btnPlayerHistory_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();

            var dlg = new PlayerHistory(_context) { Owner = this };
            if (dlg.ShowDialog() != true) return;
        }
    }
}
