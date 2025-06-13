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
using System.Windows.Navigation;
using System.Windows.Input;
using System.Windows.Media; // Required for Colors.Transparent
using System.Windows.Shapes;

namespace LinesGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GameBoard Game { get; set; }
        public Cell? SelectedBallCell { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Game = new GameBoard();
            DataContext = Game;
            Game.AddInitialRandomBalls(5); // Use new method for initial balls
        }

        private void Cell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Cell clickedCell)
            {
                if (SelectedBallCell == null) // No ball selected yet
                {
                    if (clickedCell.HasBall)
                    {
                        SelectedBallCell = clickedCell;
                        SelectedBallCell.IsSelected = true;
                    }
                }
                else // A ball is already selected
                {
                    if (clickedCell.HasBall) // Clicked on another ball
                    {
                        SelectedBallCell.IsSelected = false; // Deselect old
                        SelectedBallCell = clickedCell;
                        SelectedBallCell.IsSelected = true;  // Select new
                    }
                    else // Clicked on an empty cell (attempt to move)
                    {
                        if (Pathfinder.HasPath(Game, SelectedBallCell, clickedCell))
                        {
                            // Path exists, proceed with move
                            clickedCell.Color = SelectedBallCell.Color;
                            SelectedBallCell.Color = Colors.Transparent;
                            // Original SelectedBallCell.IsSelected will be set to false below

                            List<Cell> linesFound = Game.CheckForLines(clickedCell);
                            if (linesFound.Count > 0)
                            {
                                Game.ClearCells(linesFound);
                                // Score is updated within GameBoard.CheckForLines
                            }
                            else
                            {
                                Game.PlaceNextBallsAndGenerateNew(); // Use new method for subsequent turns
                            }
                        }
                        // else: No path, or clicked on a ball -> selection handled below or above.

                        // If a ball was selected, deselect it now, as the turn is over or selection changed.
                        if (SelectedBallCell != null)
                        {
                            SelectedBallCell.IsSelected = false;
                            SelectedBallCell = null;
                        }
                    }
                }
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBallCell != null)
            {
                SelectedBallCell.IsSelected = false;
                SelectedBallCell = null;
            }
            Game.Reset();
            Game.AddInitialRandomBalls(5); // Or your standard number of initial balls
        }
    }
}
