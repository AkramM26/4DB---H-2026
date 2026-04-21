using HugoLand.Core.Constants;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
    /// Logique d'interaction pour Game.xaml
    /// </summary>
    public partial class Game : Window
    {
        #region Champs
        //public int _X = GameConstants.gameSizeY;
        //public int _Y = GameConstants.gameSizeX;
        public int _X = 5;
        public int _Y = 5;


        #endregion
        public Game()
        {
            InitializeComponent();
            InitialiserLaGrille();

        }

        private void grdMain_Initialized(object sender, EventArgs e)
        {

        }



        public void InitialiserLaGrille()
        {
            // CHARGER TOUTES LES CASES 
            int iLignes = 8;
            int iColonnes = 16;
            int iElementsInField = grdField.Children.Count;
            int iElementsInCoordX = grdCoordX.Children.Count;
            int iElementsInCoordY = grdCoordY.Children.Count;

            Border[] CoordX = new Border[iColonnes];
            Border[] CoordY = new Border[iLignes];
            Border[,] AllCases = new Border[iElementsInField / iColonnes, iElementsInField / iLignes];

            for (int i = 0; i < iElementsInField; i++)
            {
                int ligne = i / iColonnes;
                int col = i % iColonnes;
                AllCases[ligne, col] = (Border)grdMain.FindName("bdrCase" + i);
                AllCases[ligne, col].Visibility = Visibility.Collapsed;
            }

            for (int i = 0; i < iElementsInCoordX; i++)
            {
                CoordX[i] = (Border)grdMain.FindName("bdrCoordX" + i);
                CoordX[i].Visibility = Visibility = Visibility.Collapsed;

            }

            for (int i = 0; i < iElementsInCoordY; i++)
            {
                CoordY[i] = (Border)grdMain.FindName("bdrCoordY" + i);
                CoordY[i].Visibility = Visibility = Visibility.Collapsed;

            }

            //AFFICHER LES CASES QUI SERONT JOUÉES 

            Border[,] CasesInPlay = new Border[_X, _Y];


            for (int i = 0; i < _X; i++)
            {
                CoordX[i].Visibility = Visibility = Visibility.Visible;
                CoordY[i].Visibility = Visibility = Visibility.Visible;
                for (int j = 0; j < _Y; j++)
                {


                    CasesInPlay[i, j] = AllCases[i, j];
                    CasesInPlay[i, j].Background = Brushes.LightBlue;
                    CasesInPlay[i, j].Visibility = Visibility.Visible;

                }
            }




        }

        private void btnGoToMenu_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            //Faire une sauvegarde automatique de la partie 
        }

    }
}
