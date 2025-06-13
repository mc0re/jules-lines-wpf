using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinesGame; // Your game's namespace
using System.Windows.Media; // For Color
using System.Linq; // For Linq methods on collections
using System.Collections.Generic; // For List

namespace LinesGame.Tests
{
    [TestClass]
    public class GameBoardTests
    {
        private void PlaceBall(GameBoard board, int row, int col, Color color)
        {
            Cell? cell = board.GetCell(row, col);
            if (cell != null)
            {
                // In a real test environment, directly setting cell.Color might bypass
                // some logic if Cell.Color setter does more than just set the field.
                // For LinesGame, Cell.Color setter also updates HasBall and calls OnPropertyChanged.
                // This is generally fine for testing the GameBoard's line detection logic,
                // as GameBoard operates on the state (Color/HasBall) of the cells.
                cell.Color = color;
            }
            else
            {
                Assert.Fail($"Test setup error: Cell ({row},{col}) not found.");
            }
        }

        [TestMethod]
        public void Test_HorizontalLine_5Balls_ClearsCorrectly()
        {
            // Arrange
            GameBoard board = new GameBoard();
            Color testColor = Colors.Red;
            // Place 5 balls horizontally in the middle of the board
            // Cells (4,2), (4,3), (4,4), (4,5), (4,6)
            PlaceBall(board, 4, 2, testColor);
            PlaceBall(board, 4, 3, testColor);
            PlaceBall(board, 4, 4, testColor); // This will be the "last moved ball"
            PlaceBall(board, 4, 5, testColor);
            PlaceBall(board, 4, 6, testColor);

            Cell lastMovedCell = board.GetCell(4, 4)!;
            Assert.IsNotNull(lastMovedCell, "Last moved cell should not be null.");

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            Assert.AreEqual(5, clearedCells.Count, "Should clear 5 cells for a 5-ball horizontal line.");
            Assert.AreEqual(10, board.Score, "Score should be 10 for 5 balls.");

            // Verify that the correct cells were marked for clearing
            // (CheckForLines returns them, actual clearing is done by ClearCells)
            // and that they were indeed the ones we placed.
            Assert.IsTrue(clearedCells.All(c => c.Color == testColor), "All cleared cells should be of the test color before clearing.");

            List<Cell> expectedCleared = new List<Cell>
            {
                board.GetCell(4,2)!, board.GetCell(4,3)!, board.GetCell(4,4)!, board.GetCell(4,5)!, board.GetCell(4,6)!
            };
            CollectionAssert.AreEquivalent(expectedCleared, clearedCells, "The cleared cells are not the ones expected.");

            // Optional: Verify cells are cleared if ClearCells was called internally (it's not by CheckForLines)
            // board.ClearCells(clearedCells); // If we want to test the state after clearing
            // Assert.IsTrue(clearedCells.All(c => !c.HasBall), "Cells should be empty after clearing.");
        }

        [TestMethod]
        public void Test_VerticalLine_5Balls_ClearsCorrectly()
        {
            // Arrange
            GameBoard board = new GameBoard();
            Color testColor = Colors.Green;
            // Place 5 balls vertically: (2,4), (3,4), (4,4), (5,4), (6,4)
            PlaceBall(board, 2, 4, testColor);
            PlaceBall(board, 3, 4, testColor);
            PlaceBall(board, 4, 4, testColor); // Last moved
            PlaceBall(board, 5, 4, testColor);
            PlaceBall(board, 6, 4, testColor);

            Cell lastMovedCell = board.GetCell(4, 4)!;
            Assert.IsNotNull(lastMovedCell);

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            Assert.AreEqual(5, clearedCells.Count, "Should clear 5 cells for a 5-ball vertical line.");
            Assert.AreEqual(10, board.Score, "Score should be 10 for 5 balls.");
            List<Cell> expectedCleared = new List<Cell>
            {
                board.GetCell(2,4)!, board.GetCell(3,4)!, board.GetCell(4,4)!, board.GetCell(5,4)!, board.GetCell(6,4)!
            };
            CollectionAssert.AreEquivalent(expectedCleared, clearedCells, "The cleared vertical cells are not the ones expected.");
        }

        [TestMethod]
        public void Test_DiagonalTopLeftBottomRight_5Balls_ClearsCorrectly()
        {
            // Arrange
            GameBoard board = new GameBoard();
            Color testColor = Colors.Blue;
            // Place 5 balls diagonally \: (2,2), (3,3), (4,4), (5,5), (6,6)
            PlaceBall(board, 2, 2, testColor);
            PlaceBall(board, 3, 3, testColor);
            PlaceBall(board, 4, 4, testColor); // Last moved
            PlaceBall(board, 5, 5, testColor);
            PlaceBall(board, 6, 6, testColor);

            Cell lastMovedCell = board.GetCell(4, 4)!;
            Assert.IsNotNull(lastMovedCell);

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            Assert.AreEqual(5, clearedCells.Count, "Should clear 5 cells for a 5-ball TL-BR diagonal line.");
            Assert.AreEqual(10, board.Score, "Score should be 10 for 5 balls.");
            List<Cell> expectedCleared = new List<Cell>
            {
                board.GetCell(2,2)!, board.GetCell(3,3)!, board.GetCell(4,4)!, board.GetCell(5,5)!, board.GetCell(6,6)!
            };
            CollectionAssert.AreEquivalent(expectedCleared, clearedCells, "The cleared TL-BR diagonal cells are not the ones expected.");
        }

        [TestMethod]
        public void Test_DiagonalTopRightBottomLeft_5Balls_ClearsCorrectly()
        {
            // Arrange
            GameBoard board = new GameBoard();
            Color testColor = Colors.Yellow;
            // Place 5 balls diagonally /: (2,6), (3,5), (4,4), (5,3), (6,2)
            PlaceBall(board, 2, 6, testColor);
            PlaceBall(board, 3, 5, testColor);
            PlaceBall(board, 4, 4, testColor); // Last moved
            PlaceBall(board, 5, 3, testColor);
            PlaceBall(board, 6, 2, testColor);

            Cell lastMovedCell = board.GetCell(4, 4)!;
            Assert.IsNotNull(lastMovedCell);

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            Assert.AreEqual(5, clearedCells.Count, "Should clear 5 cells for a 5-ball TR-BL diagonal line.");
            Assert.AreEqual(10, board.Score, "Score should be 10 for 5 balls.");
            List<Cell> expectedCleared = new List<Cell>
            {
                board.GetCell(2,6)!, board.GetCell(3,5)!, board.GetCell(4,4)!, board.GetCell(5,3)!, board.GetCell(6,2)!
            };
            CollectionAssert.AreEquivalent(expectedCleared, clearedCells, "The cleared TR-BL diagonal cells are not the ones expected.");
        }

        [TestMethod]
        public void Test_Line_MoreThan5Balls_ClearsCorrectly()
        {
            // Arrange
            GameBoard board = new GameBoard();
            Color testColor = Colors.Purple;
            // Place 6 balls horizontally: (4,1) to (4,6)
            PlaceBall(board, 4, 1, testColor);
            PlaceBall(board, 4, 2, testColor);
            PlaceBall(board, 4, 3, testColor);
            PlaceBall(board, 4, 4, testColor); // Last moved
            PlaceBall(board, 4, 5, testColor);
            PlaceBall(board, 4, 6, testColor);

            Cell lastMovedCell = board.GetCell(4, 4)!;
            Assert.IsNotNull(lastMovedCell);

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            // Score: 10 for 5, +2 for each additional. 6 balls = 10 + (1*2) = 12
            Assert.AreEqual(6, clearedCells.Count, "Should clear 6 cells for a 6-ball horizontal line.");
            Assert.AreEqual(12, board.Score, "Score should be 12 for 6 balls.");
            List<Cell> expectedCleared = new List<Cell>
            {
                board.GetCell(4,1)!, board.GetCell(4,2)!, board.GetCell(4,3)!, board.GetCell(4,4)!, board.GetCell(4,5)!, board.GetCell(4,6)!
            };
            CollectionAssert.AreEquivalent(expectedCleared, clearedCells, "The cleared 6-ball line cells are not the ones expected.");
        }

        [TestMethod]
        public void Test_NoLine_LessThan5Balls()
        {
            // Arrange
            GameBoard board = new GameBoard();
            Color testColor = Colors.Orange;
            PlaceBall(board, 4, 2, testColor);
            PlaceBall(board, 4, 3, testColor);
            PlaceBall(board, 4, 4, testColor); // Last moved
            PlaceBall(board, 4, 5, testColor); // 4 balls

            Cell lastMovedCell = board.GetCell(4, 4)!;
            Assert.IsNotNull(lastMovedCell);
            int initialScore = board.Score;

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            Assert.AreEqual(0, clearedCells.Count, "Should clear 0 cells if less than 5 balls.");
            Assert.AreEqual(initialScore, board.Score, "Score should not change if no line is formed.");
        }

        [TestMethod]
        public void Test_LineWithGap_NoClear()
        {
            // Arrange
            GameBoard board = new GameBoard();
            Color testColor = Colors.Cyan;
            // Line: (4,2), (4,3) --GAP-- (4,5), (4,6). Last moved is (4,3)
            PlaceBall(board, 4, 2, testColor);
            PlaceBall(board, 4, 3, testColor); // Last moved
            // board.GetCell(4,4) is empty (gap)
            PlaceBall(board, 4, 5, testColor);
            PlaceBall(board, 4, 6, testColor);

            Cell lastMovedCell = board.GetCell(4, 3)!;
            Assert.IsNotNull(lastMovedCell);
            int initialScore = board.Score;

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            Assert.AreEqual(0, clearedCells.Count, "Should clear 0 cells if line has a gap.");
            Assert.AreEqual(initialScore, board.Score, "Score should not change for a line with a gap.");
        }

        [TestMethod]
        public void Test_MixedColors_NoLine()
        {
            // Arrange
            GameBoard board = new GameBoard();
            PlaceBall(board, 4, 2, Colors.Red);
            PlaceBall(board, 4, 3, Colors.Green);
            PlaceBall(board, 4, 4, Colors.Blue); // Last moved
            PlaceBall(board, 4, 5, Colors.Yellow);
            PlaceBall(board, 4, 6, Colors.Purple);

            Cell lastMovedCell = board.GetCell(4, 4)!;
            Assert.IsNotNull(lastMovedCell);
            int initialScore = board.Score;

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            Assert.AreEqual(0, clearedCells.Count, "Should clear 0 cells if colors are mixed.");
            Assert.AreEqual(initialScore, board.Score, "Score should not change for mixed colors.");
        }

        [TestMethod]
        public void Test_MultipleLines_Lshape_ClearsAll()
        {
            // Arrange
            GameBoard board = new GameBoard();
            Color testColor = Colors.Red;
            // Horizontal line: (4,0) (4,1) (4,2) (4,3) (4,4) <- last moved
            // Vertical line:   (0,4) (1,4) (2,4) (3,4) (4,4) <- last moved
            // Total 9 unique cells to clear (4,4) is common.
            // Horizontal
            for (int i = 0; i < 5; i++) PlaceBall(board, 4, i, testColor);
            // Vertical (excluding common cell (4,4) which is already placed)
            for (int i = 0; i < 4; i++) PlaceBall(board, i, 4, testColor);

            Cell lastMovedCell = board.GetCell(4, 4)!;
            Assert.IsNotNull(lastMovedCell);

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            Assert.AreEqual(9, clearedCells.Count, "Should clear 9 cells for an L-shape of two 5-ball lines.");
            // Score: 5 balls = 10 points. Another 5 balls (sharing one) also 10 points.
            // The current scoring in CheckForLines is:
            // baseScore = 10; additionalBalls = cellsInLine.Count - minLineLength; Score += baseScore + (additionalBalls * 2);
            // If cellsInLine (HashSet) has 9 balls: 10 + (4*2) = 18 points. This seems correct for combined line.
            Assert.AreEqual(18, board.Score, "Score should be 18 for 9 balls cleared (5+5-1 common).");
        }

        [TestMethod]
        public void Test_EdgeCase_LineAtBorder_Horizontal()
        {
            // Arrange
            GameBoard board = new GameBoard();
            Color testColor = Colors.Blue;
            // Line at row 0: (0,0) (0,1) (0,2) (0,3) (0,4)
            for (int i = 0; i < 5; i++) PlaceBall(board, 0, i, testColor);

            Cell lastMovedCell = board.GetCell(0, 2)!; // Middle of the line
            Assert.IsNotNull(lastMovedCell);

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            Assert.AreEqual(5, clearedCells.Count, "Should clear 5 cells for a line at the border.");
            Assert.AreEqual(10, board.Score, "Score should be 10 for 5 balls at border.");
             List<Cell> expectedCleared = new List<Cell>();
            for (int i = 0; i < 5; i++) expectedCleared.Add(board.GetCell(0,i)!);
            CollectionAssert.AreEquivalent(expectedCleared, clearedCells, "The cleared border line cells are not the ones expected.");
        }

        [TestMethod]
        public void Test_EdgeCase_LineAtCorner_Diagonal()
        {
            // Arrange
            GameBoard board = new GameBoard();
            Color testColor = Colors.Green;
            // Diagonal line from corner (0,0): (0,0) (1,1) (2,2) (3,3) (4,4)
            for (int i = 0; i < 5; i++) PlaceBall(board, i, i, testColor);

            Cell lastMovedCell = board.GetCell(0,0)!; // Last moved is one end of the line
            Assert.IsNotNull(lastMovedCell);

            // Act
            List<Cell> clearedCells = board.CheckForLines(lastMovedCell);

            // Assert
            Assert.AreEqual(5, clearedCells.Count, "Should clear 5 cells for a diagonal line from the corner.");
            Assert.AreEqual(10, board.Score, "Score should be 10 for 5 balls in corner diagonal.");
            List<Cell> expectedCleared = new List<Cell>();
            for (int i = 0; i < 5; i++) expectedCleared.Add(board.GetCell(i,i)!);
            CollectionAssert.AreEquivalent(expectedCleared, clearedCells, "The cleared corner diagonal cells are not the ones expected.");
        }
    }
}
