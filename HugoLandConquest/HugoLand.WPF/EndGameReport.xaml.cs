using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HugoLand.WPF
{
    /// <summary>
    /// Logique d'interaction pour EndGameReport.xaml
    /// </summary>
    public partial class EndGameReport : Window
    {

        private readonly HugoLandContext _context;
        private Guid _gameId;
        private List<int> _player1Forces;
        private List<int> _player2Forces;





        public EndGameReport(HugoLandContext context, Guid gameId )
        {
            InitializeComponent();

            _context = context;
            _gameId = gameId;
            ApplyStats();
            this.DataContext = new EndGameReportGraphics(_player1Forces, _player2Forces);
        }



        public class EndGameReportGraphics(List<int> Values1, List<int> Values2)
        {

            //private static IReadOnlyCollection<double> _player2Forces;
            //private static IReadOnlyCollection<double> _player1Forces;


            public ISeries[] Series { get; set; } = [new LineSeries<int>
            {
                Values = Values1 , // Vos points
                Fill = null, // Supprime le remplissage sous la ligne
                GeometrySize = 10 // Taille des points
             }];

            public ISeries[] Series2 { get; set; } = [new LineSeries<int>
            {
                Values = Values2, // Vos points
                Fill = null, // Supprime le remplissage sous la ligne
                GeometrySize = 10 // Taille des points
             }];
        }

        private async Task ApplyStats()
        {
            try
            {
                //var gameId = _gameId;
                var game = _context.Games.FirstOrDefault(g => g.Id == _gameId);
                var playerActions = game.PlayerActions;

                var actionsRanking = playerActions.GroupBy(pa => pa.ActionType).OrderByDescending(g => g.Count());


                var winnerplayernumber = game.WinnerPlayerNumber;
                var turn = game.TurnNumber;
                var player1 = game.Players.First(p => p.PlayerNumber == game.PlayerTurn);
                var player2 = game.Players.First(p => p.PlayerNumber != game.PlayerTurn);

                // Player 1 stats
                int p1FortificationNumber = player1.Installations.
                                            Where(i => i.InstallationType == InstallationType.Fortification).Count();
                int p1InstallationNumber = player1.Installations.Count();
                int p1FinalForce = player1.MilitaryDetachments.Sum(m => m.MilitaryForce);
                int p1Income = player1.Income;
                int p1Cost = player1.Cost;
                int p1Gold = player1.Gold;
                int p1TurnInDept = player1.TurnInDept;
                _player1Forces = player1.ForcesTable;


                // Player 2 stats
                int p2FortificationNumber = player2.Installations.
                                            Where(i => i.InstallationType == InstallationType.Fortification).Count();
                int p2InstallationNumber = player2.Installations.Count();
                int p2FinalForce = player2.MilitaryDetachments.Sum(m => m.MilitaryForce);
                int p2Income = player2.Income;
                int p2Cost = player2.Cost;
                int p2Gold = player2.Gold;
                int p2TurnInDept = player2.TurnInDept;
                _player2Forces = player2.ForcesTable;

                //Main Display
                txtStats.Text = $"TURNS : {game.TurnNumber} \n" +
                                $"\nWINNER : PLAYER {game.WinnerPlayerNumber}" +
                                $"\nLOSER  : PLAYER {(game.WinnerPlayerNumber == 1 ? 2 : 1)}";
                txtStats.Text += "\n\nACTIONS RANKING";

                int cpt = 1;
                foreach (var action in actionsRanking)
                {
                    txtStats.Text += $"\n{cpt} - {action.Key}";
                    cpt++;
                }


                //Display for player 1 report 
                tbP1Report.Text = $"Fortifications:{p1FortificationNumber} \nInstallations : {p1InstallationNumber} \nForce: {p1FinalForce} " +
                    $"\nIncome: {p1Income}  \nCost: {p1Cost}  \nGold:{p1Gold} \nTurnInDept: {p1TurnInDept}";


                //Display for player 2 report 
                tbP2Report.Text = $"Fortifications:{p2FortificationNumber} \nInstallations : {p2InstallationNumber} \nForce: {p2FinalForce} " +
                    $"\nIncome: {p2Income}  \nCost: {p2Cost} \nGold:{p2Gold} \nTurnInDept: {p2TurnInDept} ";

                //var tbs = grdPlayersStats.Children.OfType<TextBlock>().ToList();

                //foreach (var tb in tbs)
                //{                   
                //    if (tb.Name.Contains("1"))
                //    {
                //        tb.Text = $"Fortifications:{p1FortificationNumber} \nInstallations : {p1InstallationNumber} \nForce: {p1FinalForce} " +
                //    $"\nIncome: {p1Income}  \nCost: {p1Cost}  \nTurnInDept: {p1TurnInDept}";
                //    }

                //}


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //private async Task<Game?> FetchGameGraphAsync()
        //{
        //    return await _context.Games
        //        .Include(g => g.Players)
        //        .Include(g => g.Territories).ThenInclude(t => t.MilitaryDetachment).ThenInclude(m => m!.Player)
        //        .Include(g => g.Territories).ThenInclude(t => t.Installation)
        //        .Include(g => g.MilitaryDetachments).ThenInclude(m => m.Territory)
        //        .FirstOrDefaultAsync();
        //}

    }
}
