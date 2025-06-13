using System.Collections.Generic;

namespace LinesGame
{
    public class Pathfinder
    {
        public static List<Cell>? GetPath(GameBoard board, Cell startCell, Cell endCell)
        {
            if (startCell == null || endCell == null || board == null)
                return null;

            // The end cell must be empty for a valid move.
            // This is usually checked by the caller before calling GetPath.
            // If endCell has a ball and it's not the startCell itself, no path.
            if (endCell.HasBall && endCell != startCell)
                 return null;

            Queue<Cell> queue = new Queue<Cell>();
            HashSet<Cell> visited = new HashSet<Cell>();
            Dictionary<Cell, Cell> predecessors = new Dictionary<Cell, Cell>();

            queue.Enqueue(startCell);
            visited.Add(startCell);

            int[] dRow = { -1, 1, 0, 0 }; // Changes for row (Up, Down)
            int[] dCol = { 0, 0, -1, 1 }; // Changes for column (Left, Right)

            Cell? foundDestinationCell = null;

            while (queue.Count > 0)
            {
                Cell currentCell = queue.Dequeue();

                if (currentCell == endCell)
                {
                    foundDestinationCell = currentCell;
                    break; // Path found, exit BFS
                }

                for (int i = 0; i < 4; i++) // Explore neighbors
                {
                    int nextRow = currentCell.Row + dRow[i];
                    int nextCol = currentCell.Column + dCol[i];
                    Cell? neighborCell = board.GetCell(nextRow, nextCol);

                    if (neighborCell != null && !visited.Contains(neighborCell))
                    {
                        // Path can only go through empty cells OR the very end cell (which is the target).
                        // If neighborCell is the endCell, it's allowed regardless of HasBall (already checked initially).
                        if (!neighborCell.HasBall || neighborCell == endCell)
                        {
                            visited.Add(neighborCell);
                            predecessors[neighborCell] = currentCell; // Record predecessor
                            queue.Enqueue(neighborCell);
                        }
                    }
                }
            }

            if (foundDestinationCell == null)
            {
                return null; // No path found to endCell
            }

            // Reconstruct path from endCell back to startCell
            List<Cell> path = new List<Cell>();
            Cell backtrackCell = foundDestinationCell; // Should be endCell

            while (backtrackCell != startCell)
            {
                path.Add(backtrackCell);
                if (!predecessors.TryGetValue(backtrackCell, out Cell? predecessor))
                {
                    // This indicates a broken path or startCell was unreachable,
                    // which shouldn't happen if foundDestinationCell is not null and is endCell.
                    // Or if startCell == endCell, this loop is skipped.
                    return null; // Error in path reconstruction or disconnected graph
                }
                backtrackCell = predecessor;
            }
            path.Add(startCell); // Add the start cell itself
            path.Reverse();      // Path is from startCell to endCell

            return path;
        }
    }
}
