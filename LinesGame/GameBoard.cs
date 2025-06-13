using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LinesGame
{
    public class GameBoard : INotifyPropertyChanged
    {
        private static readonly Random random = new Random();
        private int _score;
        private bool _isGameOver;
        public ObservableCollection<Color> NextBallColors { get; }
        public static readonly List<Color> AvailableColors = new List<Color>
        {
            Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple, Colors.Orange, Colors.Cyan
        };

        public const int GridRows = 9;
        public const int GridColumns = 9;

        public ObservableCollection<Cell> Cells { get; }

        public int Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            private set // Typically only set by game logic within GameBoard
            {
                if (_isGameOver != value)
                {
                    _isGameOver = value;
                    OnPropertyChanged();
                }
            }
        }

        public GameBoard()
        {
            Cells = new ObservableCollection<Cell>();
            for (int r = 0; r < GridRows; r++)
            {
                for (int c = 0; c < GridColumns; c++)
                {
                    Cells.Add(new Cell { Row = r, Column = c, Color = Colors.Transparent });
                }
            }
            Score = 0;
            IsGameOver = false;
            NextBallColors = new ObservableCollection<Color>();
            GenerateNextBalls(3); // Initialize with 3 next balls
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void GenerateNextBalls(int count)
        {
            NextBallColors.Clear();
            for (int i = 0; i < count; i++)
            {
                NextBallColors.Add(AvailableColors[random.Next(AvailableColors.Count)]);
            }
        }

        public List<Cell> CheckForLines(Cell lastMovedCell)
        {
            if (!lastMovedCell.HasBall) return new List<Cell>();

            HashSet<Cell> cellsInLine = new HashSet<Cell>();
            Color ballColor = lastMovedCell.Color;
            int minLineLength = 5;

            // Directions: {rowChange, colChange}
            int[][] directions =
            {
                new[] { 0, 1 }, // Horizontal
                new[] { 1, 0 }, // Vertical
                new[] { 1, 1 }, // Diagonal \
                new[] { 1, -1 } // Diagonal /
            };

            foreach (var dir in directions)
            {
                List<Cell> currentLine = new List<Cell> { lastMovedCell };
                // Check in one direction
                for (int i = 1; i < minLineLength; i++)
                {
                    Cell? cell = GetCell(lastMovedCell.Row + i * dir[0], lastMovedCell.Column + i * dir[1]);
                    if (cell != null && cell.HasBall && cell.Color == ballColor)
                        currentLine.Add(cell);
                    else
                        break;
                }
                // Check in opposite direction
                for (int i = 1; i < minLineLength; i++)
                {
                    Cell? cell = GetCell(lastMovedCell.Row - i * dir[0], lastMovedCell.Column - i * dir[1]);
                    if (cell != null && cell.HasBall && cell.Color == ballColor)
                        currentLine.Add(cell);
                    else
                        break;
                }

                if (currentLine.Count >= minLineLength)
                {
                    foreach (var cell in currentLine)
                        cellsInLine.Add(cell);
                }
            }

            if (cellsInLine.Count >= minLineLength)
            {
                // Basic scoring: 10 points for 5 balls, +2 for each additional ball
                int baseScore = 10;
                int additionalBalls = cellsInLine.Count - minLineLength;
                Score += baseScore + (additionalBalls * 2);
                return cellsInLine.ToList();
            }
            return new List<Cell>();
        }

        public void ClearCells(List<Cell> cellsToClear)
        {
            foreach (var cell in cellsToClear)
            {
                cell.Color = Colors.Transparent; // This will trigger OnPropertyChanged in Cell
            }
        }

        private void CheckGameOver()
        {
            if (GetEmptyCells().Count == 0)
            {
                IsGameOver = true;
            }
        }

        private List<Cell> GetEmptyCells()
        {
            return Cells.Where(cell => !cell.HasBall).ToList();
        }

        public void PlaceNextBallsAndGenerateNew()
        {
            if (IsGameOver) return;

            List<Cell> emptyCells = GetEmptyCells();

            // Check if there's enough space for all balls in NextBallColors
            if (emptyCells.Count < NextBallColors.Count)
            {
                // Not enough space. Place what can be placed, then set game over.
                foreach (Color ballColor in NextBallColors)
                {
                    if (emptyCells.Count == 0) break; // No more empty cells
                    int randomIndex = random.Next(emptyCells.Count);
                    Cell cellToPopulate = emptyCells[randomIndex];
                    cellToPopulate.Color = ballColor;
                    emptyCells.RemoveAt(randomIndex);
                }
                IsGameOver = true;
                // No need to call CheckGameOver() here as we've manually set it
                // and no new balls will be generated if it's game over.
                return; // Exit early
            }

            // Place current NextBallColors
            foreach (Color ballColor in NextBallColors)
            {
                 // This check should be redundant if the above logic for emptyCells.Count < NextBallColors.Count is correct
                if (emptyCells.Count == 0)
                {
                    IsGameOver = true; // Should ideally not be reached if initial check is robust
                    break;
                }
                int randomIndex = random.Next(emptyCells.Count);
                Cell cellToPopulate = emptyCells[randomIndex];
                cellToPopulate.Color = ballColor;
                emptyCells.RemoveAt(randomIndex);
            }

            // Generate a new set of next balls (e.g., 3 for the next turn)
            // The number here (3) should be consistent with what player expects.
            GenerateNextBalls(3);
            CheckGameOver(); // Check if placing balls filled the board.
        }

        public void AddInitialRandomBalls(int count)
        {
            if (IsGameOver) return; // Should not be true if Reset was called, but good check

            List<Cell> emptyCells = GetEmptyCells();
            int ballsToAdd = Math.Min(count, emptyCells.Count);

            for (int i = 0; i < ballsToAdd; i++)
            {
                if (emptyCells.Count == 0) break;

                int randomIndex = random.Next(emptyCells.Count);
                Cell cellToPopulate = emptyCells[randomIndex];

                // For initial balls, generate colors directly
                int randomColorIndex = random.Next(AvailableColors.Count);
                cellToPopulate.Color = AvailableColors[randomColorIndex];

                emptyCells.RemoveAt(randomIndex);
            }
            // After initial balls, we don't immediately check for game over
            // as the game starts with these. The next turn will.
            // However, if initial placement itself fills the board, it's game over.
            CheckGameOver();
        }

        public void Reset()
        {
            foreach (var cell in Cells)
            {
                cell.Color = Colors.Transparent;
                cell.IsSelected = false; // Reset selection state if it's on the cell
            }
            Score = 0;
            IsGameOver = false;
            // NextBallColors are generated here to ensure they are ready for the UI
            // even before AddInitialRandomBalls might be called externally.
            GenerateNextBalls(3);
        }

        public Cell? GetCell(int row, int col)
        {
            if (row < 0 || row >= GridRows || col < 0 || col >= GridColumns)
            {
                return null;
            }
            return Cells[row * GridColumns + col]; // Assumes Cells is populated row by row
        }
    }
}
