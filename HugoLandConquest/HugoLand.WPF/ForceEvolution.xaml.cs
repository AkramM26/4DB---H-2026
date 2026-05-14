using HugoLand.Core.Constants;
using HugoLand.Core.Data;
using HugoLand.Core.Domain;
using LiveChartsCore;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HugoLand.WPF
{
    public partial class ForceEvolution : Window
    {
        private readonly HugoLandContext _context;
        private Graphic _graphic;
        private Guid _gameId;

        public ForceEvolution(HugoLandContext context, Guid gameId)
        {
            InitializeComponent();
            _context = context;
            _graphic = new Graphic();
            _gameId = gameId;
            Loaded += async (_, _) => await PopulateAsync();
        }

        private async System.Threading.Tasks.Task PopulateAsync()
        {
            var turnSnapShots = await _context.TurnSnapShots
                .IgnoreQueryFilters()
                .Where(t => t.GameId == _gameId)
                .ToListAsync();

            List<List<int>> valeurs = new List<List<int>>()
            {
                new List<int>(){ GameConstants.baseMilitaryForce }, // On rajoute les forces au tour 0
                new List<int>(){ GameConstants.baseMilitaryForce }
            };

            foreach (TurnSnapShot snapShot in turnSnapShots)
            {
                if (snapShot.PlayerNumber == 1)
                    valeurs[0].Add(snapShot.TotalMilitaryForce);
                else
                    valeurs[1].Add(snapShot.TotalMilitaryForce);
            }

            for (int i = 0; i < 2; i++)
            {
                _graphic.Series.Add(new LineSeries<int>
                {
                    Name = $"Player {i + 1}",
                    Values = valeurs[i],
                    Fill = null
                });
            }

            this.DataContext = _graphic;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Button sound 
            AudioManager.MenuSound.Play();
            DialogResult = false;
        }
    }

    public class Graphic
    {
        public List<ISeries> Series { get; set; } = new();

        public Axis[] XAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "Tours",
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Blue),
                    TextSize = 10,
                    MinStep = 1,       
                    ForceStepToMin = true,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }
                }
            };

        public Axis[] YAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "Force",
                    NamePaint = new SolidColorPaint(SKColors.Red),
                    LabelsPaint = new SolidColorPaint(SKColors.Green),
                    TextSize = 20,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 2,
                        PathEffect = new DashEffect(new float[] { 3, 3 })
                    }
                }
            };
    }
}