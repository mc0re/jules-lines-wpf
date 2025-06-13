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
// using System.Windows.Input; // Duplicate, remove
// using System.Windows.Media; // Duplicate, remove
using System.Windows.Shapes;
using System.Windows.Media.Animation; // Added for DoubleAnimation
using System.Threading.Tasks;     // Added for Task

namespace LinesGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GameBoard Game { get; set; }
        public Cell? SelectedBallCell { get; set; }
        private List<Cell>? _currentPath = null;

        public MainWindow()
        {
            InitializeComponent();
            Game = new GameBoard();
            DataContext = Game;
            Game.AddInitialRandomBalls(5); // Use new method for initial balls
        }

        private void ClearCurrentPathVisualization()
        {
            if (_currentPath != null)
            {
                foreach (Cell cellInPath in _currentPath)
                {
                    // Avoid unhighlighting the selected ball itself if it's part of the path display style
                    // or if the style for IsPathHighlight doesn't interfere with IsSelected style.
                    // For this implementation, IsPathHighlight is a separate background, so it's fine.
                    cellInPath.IsPathHighlight = false;
                }
                _currentPath = null;
            }
        }

        private async Task AnimateBallMove(Cell sourceCell, Cell targetCell, List<Cell> path)
        {
            if (path == null || path.Count < 2)
            {
                // Invalid path or no steps to take, reset selection and path highlight
                ClearCurrentPathVisualization();
                if (SelectedBallCell != null) SelectedBallCell.IsSelected = false;
                SelectedBallCell = null;
                return;
            }

            GameBoardItemsControl.IsEnabled = false;

            Ellipse ghostBall = new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = new SolidColorBrush(sourceCell.Color),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 0.75
            };

            double cellWidth = GameBoardItemsControl.ActualWidth / GameBoard.GridColumns;
            double cellHeight = GameBoardItemsControl.ActualHeight / GameBoard.GridRows;

            double startX = sourceCell.Column * cellWidth + (cellWidth - ghostBall.Width) / 2;
            double startY = sourceCell.Row * cellHeight + (cellHeight - ghostBall.Height) / 2;
            Canvas.SetLeft(ghostBall, startX);
            Canvas.SetTop(ghostBall, startY);
            animationCanvas.Children.Add(ghostBall);

            sourceCell.IsBallVisible = false;

            // Loop through path segments for animation
            for (int i = 0; i < path.Count - 1; i++)
            {
                Cell currentSegmentEndCell = path[i+1];
                double targetX = currentSegmentEndCell.Column * cellWidth + (cellWidth - ghostBall.Width) / 2;
                double targetY = currentSegmentEndCell.Row * cellHeight + (cellHeight - ghostBall.Height) / 2;

                DoubleAnimation animX = new DoubleAnimation(targetX, new Duration(TimeSpan.FromSeconds(0.15)));
                DoubleAnimation animY = new DoubleAnimation(targetY, new Duration(TimeSpan.FromSeconds(0.15)));

                TaskCompletionSource<bool> segmentTcs = new TaskCompletionSource<bool>();
                int animationsCompleted = 0;
                EventHandler onAnimationCompleted = (s, e) =>
                {
                    animationsCompleted++;
                    if (animationsCompleted == 2)
                    {
                        segmentTcs.TrySetResult(true);
                    }
                };
                animX.Completed += onAnimationCompleted;
                animY.Completed += onAnimationCompleted;

                ghostBall.BeginAnimation(Canvas.LeftProperty, animX);
                ghostBall.BeginAnimation(Canvas.TopProperty, animY);

                await segmentTcs.Task; // Wait for current segment animation
            }

            animationCanvas.Children.Remove(ghostBall);

            // Finalize move logic (targetCell is the end of the path)
            targetCell.Color = sourceCell.Color;
            sourceCell.Color = Colors.Transparent;

            sourceCell.IsBallVisible = true;
            targetCell.IsBallVisible = true;  // targetCell is path.Last()

            // Path visualization is cleared at the very end, after game logic
            // ClearCurrentPathVisualization(); // Moved to the end

            if (SelectedBallCell != null)  // SelectedBallCell is sourceCell
            {
                SelectedBallCell.IsSelected = false;
            }
            SelectedBallCell = null;

            List<Cell> linesFound = Game.CheckForLines(finalDestinationCell);
            if (linesFound.Count > 0)
            {
                Game.ClearCells(linesFound);
            }
            else
            {
                Game.PlaceNextBallsAndGenerateNew();
            }

            if (!Game.IsGameOver)
            {
                GameBoardItemsControl.IsEnabled = true;
            }
        }

        private void Cell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Game.IsGameOver || !GameBoardItemsControl.IsEnabled) return; // Prevent action if game over or mid-animation

            if (sender is FrameworkElement element && element.DataContext is Cell clickedCell)
            {
                if (SelectedBallCell == null)
                {
                    ClearCurrentPathVisualization();
                    if (clickedCell.HasBall)
                    {
                        SelectedBallCell = clickedCell;
                        SelectedBallCell.IsSelected = true;
                    }
                }
                else
                {
                    if (clickedCell == SelectedBallCell)
                    {
                        ClearCurrentPathVisualization();
                        SelectedBallCell.IsSelected = false;
                        SelectedBallCell = null;
                        return;
                    }

                    if (clickedCell.HasBall)
                    {
                        ClearCurrentPathVisualization();
                        SelectedBallCell.IsSelected = false;
                        SelectedBallCell = clickedCell;
                        SelectedBallCell.IsSelected = true;
                    }
                    else // Clicked on an empty cell
                    {
                        // Path visualization is cleared by AnimateBallMove or if no path
                        List<Cell>? path = Pathfinder.GetPath(Game, SelectedBallCell, clickedCell);

                        if (path != null && path.Count > 1) // Path exists and has at least one step
                        {
                            _currentPath = path; // Store for visualization
                            foreach (Cell cellInPathSegment in _currentPath)
                            {
                                cellInPathSegment.IsPathHighlight = true; // Highlight the path
                            }
                            _ = AnimateBallMove(SelectedBallCell, clickedCell, _currentPath); // Fire and forget
                        }
                        else // No path or path is just the start cell
                        {
                            // ClearCurrentPathVisualization(); // Not needed if nothing was highlighted
                            if (SelectedBallCell != null) SelectedBallCell.IsSelected = false;
                            SelectedBallCell = null;
                        }
                    }
                }
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCurrentPathVisualization();
            if (SelectedBallCell != null)
            {
                SelectedBallCell.IsSelected = false;
                SelectedBallCell = null;
            }
            Game.Reset();
            Game.AddInitialRandomBalls(5);
            // Path visualization should be clear already, but as a safeguard for NewGame
            ClearCurrentPathVisualization();
        }
    }
}
