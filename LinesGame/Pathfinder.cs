using System.Collections.Generic;

namespace LinesGame
{
    public class Pathfinder
    {
        public static bool HasPath(GameBoard board, Cell startCell, Cell endCell)
        {
            if (startCell == null || endCell == null || board == null)
                return false;

            // The end cell must be empty for a valid move.
            // This check is technically done by the caller's logic before calling HasPath,
            // but good to ensure. The path itself must consist of empty cells.
            if (endCell.HasBall && endCell != startCell) // endCell could be startCell if misclicked
                 return false;


            Queue<Cell> queue = new Queue<Cell>();
            HashSet<Cell> visited = new HashSet<Cell>();

            queue.Enqueue(startCell);
            visited.Add(startCell);

            int[] dRow = { -1, 1, 0, 0 }; // Changes for row (Up, Down)
            int[] dCol = { 0, 0, -1, 1 }; // Changes for column (Left, Right)

            while (queue.Count > 0)
            {
                Cell currentCell = queue.Dequeue();

                if (currentCell == endCell)
                {
                    return true; // Path found
                }

                for (int i = 0; i < 4; i++) // Explore neighbors
                {
                    int nextRow = currentCell.Row + dRow[i];
                    int nextCol = currentCell.Column + dCol[i];

                    Cell? neighborCell = board.GetCell(nextRow, nextCol);

                    if (neighborCell != null && !visited.Contains(neighborCell))
                    {
                        // Path can only go through empty cells OR the very end cell (which we know is empty or is the target)
                        if (!neighborCell.HasBall || neighborCell == endCell)
                        {
                            visited.Add(neighborCell);
                            queue.Enqueue(neighborCell);
                        }
                    }
                }
            }
            return false; // No path found
        }
    }
}
