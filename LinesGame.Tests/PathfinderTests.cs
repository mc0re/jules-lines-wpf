using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinesGame;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace LinesGame.Tests
{
    [TestClass]
    public class PathfinderTests
    {
        // Helper method to create a board and place balls (obstacles)
        private GameBoard CreateBoardWithObstacles(List<(int r, int c, Color color)>? obstacles = null)
        {
            GameBoard board = new GameBoard(); // Uses default 9x9 constructor
            if (obstacles != null)
            {
                foreach (var (r, c, color) in obstacles)
                {
                    Cell? cell = board.GetCell(r, c);
                    if (cell != null)
                    {
                        cell.Color = color; // Places a ball, making it an obstacle
                    }
                    else
                    {
                        // This case should ideally not happen if coordinates are within 0-8
                        System.Diagnostics.Debug.WriteLine($"Warning: Obstacle cell ({r},{c}) out of bounds.");
                    }
                }
            }
            return board;
        }

        // Helper to get cell for readability in tests
        private Cell GetCell(GameBoard board, int r, int c)
        {
            Cell? cell = board.GetCell(r, c);
            Assert.IsNotNull(cell, $"Cell ({r},{c}) was expected but not found. Check board dimensions or coordinates.");
            return cell!;
        }

        // Helper for asserting path sequence
        private void AssertPathSequence(List<Cell>? actualPath, List<(int r, int c)> expectedCoordinates, string messagePrefix = "")
        {
            Assert.IsNotNull(actualPath, $"{messagePrefix}Path should not be null.");
            Assert.AreEqual(expectedCoordinates.Count, actualPath.Count, $"{messagePrefix}Path length mismatch. Expected {expectedCoordinates.Count}, got {actualPath.Count}.");
            for (int i = 0; i < expectedCoordinates.Count; i++)
            {
                Assert.AreEqual(expectedCoordinates[i].r, actualPath[i].Row, $"{messagePrefix}Mismatch at path index {i} (Row). Expected {expectedCoordinates[i].r}, got {actualPath[i].Row}.");
                Assert.AreEqual(expectedCoordinates[i].c, actualPath[i].Column, $"{messagePrefix}Mismatch at path index {i} (Column). Expected {expectedCoordinates[i].c}, got {actualPath[i].Column}.");
            }
        }

        [TestMethod]
        public void Test_GetPath_SimpleClearPath()
        {
            // Arrange
            GameBoard board = CreateBoardWithObstacles();
            Cell startCell = GetCell(board, 0, 0);
            Cell endCell = GetCell(board, 0, 2);

            // Act
            List<Cell>? path = Pathfinder.GetPath(board, startCell, endCell);

            // Assert
            var expected = new List<(int r, int c)> { (0, 0), (0, 1), (0, 2) };
            AssertPathSequence(path, expected, "SimpleClearPath: ");
        }

        [TestMethod]
        public void Test_GetPath_BlockedPath_ReturnsNull()
        {
            // Arrange
            var obstacles = new List<(int r, int c, Color color)> { (0, 1, Colors.Red) };
            GameBoard board = CreateBoardWithObstacles(obstacles);
            Cell startCell = GetCell(board, 0, 0);
            Cell endCell = GetCell(board, 0, 2);

            // Act
            List<Cell>? path = Pathfinder.GetPath(board, startCell, endCell);

            // Assert
            Assert.IsNull(path, "BlockedPath: Path should be null when directly blocked.");
        }

        [TestMethod]
        public void Test_GetPath_NoPathExists_ReturnsNull()
        {
            // Arrange
            var obstacles = new List<(int r, int c, Color color)>
            {
                (0, 1, Colors.Red), // Wall right of start
                (1, 0, Colors.Red)  // Wall below start
            };
            GameBoard board = CreateBoardWithObstacles(obstacles);
            Cell startCell = GetCell(board, 0, 0);
            Cell endCell = GetCell(board, 2, 2); // Some distant cell

            // Act
            List<Cell>? path = Pathfinder.GetPath(board, startCell, endCell);

            // Assert
            Assert.IsNull(path, "NoPathExists: Path should be null when start is walled off.");
        }

        [TestMethod]
        public void Test_GetPath_StartAndEndSame_ReturnsPathWithOneCell()
        {
            // Arrange
            GameBoard board = CreateBoardWithObstacles();
            Cell startCell = GetCell(board, 0, 0);
            Cell endCell = GetCell(board, 0, 0); // Start and End are the same

            // Act
            List<Cell>? path = Pathfinder.GetPath(board, startCell, endCell);

            // Assert
            var expected = new List<(int r, int c)> { (0, 0) };
            AssertPathSequence(path, expected, "StartAndEndSame: ");
        }

        [TestMethod]
        public void Test_GetPath_PathAlongEdge()
        {
            // Arrange
            GameBoard board = CreateBoardWithObstacles();
            Cell startCell = GetCell(board, 0, 0);
            Cell endCell = GetCell(board, 8, 0); // Path along the left edge

            // Act
            List<Cell>? path = Pathfinder.GetPath(board, startCell, endCell);

            // Assert
            var expected = new List<(int r, int c)>();
            for(int i = 0; i <= 8; i++) expected.Add((i,0));
            AssertPathSequence(path, expected, "PathAlongEdge: ");
        }

        [TestMethod]
        public void Test_GetPath_LongerWindingPath()
        {
            // Arrange
            var obstacles = new List<(int r, int c, Color color)>
            {
                (1,0, Colors.Red), (1,1, Colors.Red), (0,2, Colors.Red),
                (2,1, Colors.Red) // Added to make a more specific winding path
            };
            GameBoard board = CreateBoardWithObstacles(obstacles);
            Cell startCell = GetCell(board, 0, 0);
            Cell endCell = GetCell(board, 2, 2);
            // Expected path: (0,0) -> (0,1) -> (1,2) -> (2,2) - length 4
            // Or with (2,1) blocked: (0,0) -> (0,1) -> (0,2) NO -> (0,0) -> (0,1) -> (cell above 1,2) -> (1,2) -> (2,2)
            // Path: (0,0) -> (0,1) (cannot go (0,2)) -> (must go down if possible (1,1) blocked)
            // (0,0) -> (0,1)
            // (1,0)X (1,1)X
            // (0,2)X
            // (2,1)X
            // (0,0) -> (0,1)
            // -> (nothing to right or down)
            // This scenario is more complex than initially thought, let's simplify obstacles for a clear winding path
            // Obstacles: (0,1), (1,1)
            // Path: (0,0) -> (1,0) -> (2,0) -> (2,1) -> (2,2) -> (1,2) -> (0,2)
            // Let's make a simpler one: (0,0) to (2,2) with wall at (1,*)
            obstacles = new List<(int r, int c, Color color)>
            {
                (1,0, Colors.Red), (1,1, Colors.Red), (1,2, Colors.Red), (1,3, Colors.Red), (1,4, Colors.Red), (1,5,Colors.Red), (1,6,Colors.Red), (1,7,Colors.Red), (1,8,Colors.Red)
            };
            board = CreateBoardWithObstacles(obstacles); // Re-create board
            startCell = GetCell(board,0,0);
            endCell = GetCell(board,2,0);
            // Expected path: (0,0) -> (0,1) -> (0,2) -> ... -> (0,8) -> (1,8) no -> (2,8) -> (2,7) ... -> (2,0)
            // Path (0,0) -> (2,0) must go around row 1
            // (0,0) -> (0,1) -> ... -> (0,8) -> (???) -> (2,8) -> ... -> (2,0)
            // This path is long. Let's use a smaller defined one: (0,0) to (0,4) around (0,2)
            obstacles = new List<(int r, int c, Color color)> { (0,2,Colors.Red), (1,2,Colors.Red) }; // Block (0,2) and below it
            board = CreateBoardWithObstacles(obstacles);
            startCell = GetCell(board,0,1);
            endCell = GetCell(board,0,3);
            // Expected: (0,1) -> (1,1) -> (2,1) -> (2,2) -> (2,3) -> (1,3) -> (0,3) (Length 7)

            // Act
            List<Cell>? path = Pathfinder.GetPath(board, startCell, endCell);

            // Assert
            var expected = new List<(int r, int c)> { (0,1), (1,1), (2,1), (2,2), (2,3), (1,3), (0,3) };
            AssertPathSequence(path, expected, "LongerWindingPath: ");
        }

        [TestMethod]
        public void Test_GetPath_TargetBlockedButPathAroundPossible()
        {
            // Arrange
            // Start (0,0), End (0,2). Obstacle at (0,1), but (1,0), (1,1), (1,2) are clear.
            var obstacles = new List<(int r, int c, Color color)> { (0,1, Colors.Red) };
            GameBoard board = CreateBoardWithObstacles(obstacles);
            Cell startCell = GetCell(board, 0, 0);
            Cell endCell = GetCell(board, 0, 2);

            // Act
            List<Cell>? path = Pathfinder.GetPath(board, startCell, endCell);

            // Assert
            // Expected path: (0,0) -> (1,0) -> (1,1) -> (1,2) -> (0,2) -- Length 5
            var expected = new List<(int r, int c)> { (0,0), (1,0), (1,1), (1,2), (0,2) };
            AssertPathSequence(path, expected, "TargetBlockedButPathAroundPossible: ");
        }
    }
}
