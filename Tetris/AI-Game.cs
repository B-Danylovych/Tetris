using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Tetris
{
    public class AI_Game : GameMain
    {
        public List<ShapeMoveOption> ShapeMoveOptions { get; private set; }
            = new List<ShapeMoveOption>();

        public AI_Game(int rows, int hiddenRowsOnTop, int cols,
            Shape bufferShape, Shape currentShape, Shape projectedShape, List<List<GridValue>> grid)
            : base(rows, hiddenRowsOnTop, cols)
        {
            BufferShape = SetShapeClone(bufferShape);
            CurrentShape = SetShapeClone(currentShape);
            ProjectedShape = SetShapeClone(projectedShape);
            Grid = SetGridClone(grid);
        }

        private Shape SetShapeClone(Shape cloneShape)
            => cloneShape.DeepCopy();

        private List<List<GridValue>> SetGridClone(List<List<GridValue>> grid)
            => new List<List<GridValue>>(grid);

        public void CheckAllPositionsProjectedShapes()
        {
            ShapeMoveOptions.Add(GetCurrentPositionMoveOption());

            int[] currentColumnsPosition = new int[CurrentShape.ColumnCount];
            Array.Copy(CurrentShape.ColumnsPosition, currentColumnsPosition, CurrentShape.ColumnCount);

            CheckAllPositionsInDirection(MoveLeft);

            CheckAllPositionsInDirection(MoveRight);
        }

        private void CheckAllPositionsInDirection(Func<bool> moveInDirection)
        {
            bool canMove = moveInDirection();
            while (canMove)
            {
                ShapeMoveOptions.Add(GetCurrentPositionMoveOption());
                canMove = moveInDirection();
            }
        }

        private ShapeMoveOption GetCurrentPositionMoveOption()
            => new ShapeMoveOption(ProjectedShape, CalculateMoveOptionScore(ProjectedShape));

        public int CalculateMoveOptionScore(Shape projShape)
        {
            int score = 0;

            score += CalculateHighShapeScore(projShape);

            int[] fullLinesIndices = CalculateFullLines(projShape);
            score -= fullLinesIndices.Length * 2;

            return score;
        }

        private int CalculateHighShapeScore(Shape projShape)
        {
            for (int r = 0; r < projShape.RowCount; r++)
                for (int c = 0; c < projShape.ColumnCount; c++)
                    if (projShape.ShapeGrid[r, c] != GridValue.Empty)
                        return (projShape.RowsPosition[r]);

            throw new InvalidOperationException("The projectedShape is empty.");
        }

        private int[] CalculateFullLines(Shape projShape)
        {
            List<int> fullLines = new List<int>();

            for (int r = 0; r < projShape.RowCount; r++)
            {
                int rowPosition = projShape.RowsPosition[r];
                if (rowPosition < 0)
                    break;

                GridValue[] currentLine = Grid[rowPosition].ToArray();

                for (int c = 0; c < projShape.ColumnCount; c++)
                {
                    if (projShape.ShapeGrid[r, c] != GridValue.Empty)
                    {
                        int columnPosition = projShape.ColumnsPosition[c];
                        currentLine[columnPosition] = projShape.ShapeGrid[r, c];
                    }
                }

                if (IsLineFull(currentLine))
                    fullLines.Add(rowPosition);
            }

            return fullLines.ToArray();
        }

        private bool IsLineFull(GridValue[] currentLine)
        {
            foreach (GridValue gridValue in currentLine)
                if (gridValue == GridValue.Empty)
                    return false;

            return true;
        }

        private void GetGapsOfMoveOption(Shape projShape, int[] fullLinesIndices)
        {
            int numOfGaps = 0;

            TilePosition[] lowestShapeTiles = GetLowestNotFromFullLinesTiles(projShape, fullLinesIndices);

            foreach (TilePosition tile in lowestShapeTiles)
            {
                for (int r = tile.Y - 1; r >= 0; r--)
                {
                    if (fullLinesIndices.Contains(r))
                        continue;

                    if (Grid[r][tile.X] == GridValue.Empty)
                        numOfGaps++;
                    else
                        break;
                }
            }
        }

        private TilePosition[] GetLowestNotFromFullLinesTiles(Shape projShape, int[] fullLinesIndices)
        {
            List<TilePosition> lowestShapeTiles = new List<TilePosition>();

            int[] reverseRowIndices = GetRowIndicesWithoutFullLines(projShape.RowsPosition, fullLinesIndices);
            Array.Reverse(reverseRowIndices);

            for (int c = 0; c < projShape.ColumnCount; c++)
            {
                foreach (int r in reverseRowIndices)
                {
                    if (projShape.ShapeGrid[r, c] != GridValue.Empty)
                    {
                        lowestShapeTiles.Add(
                            new TilePosition(projShape.RowsPosition[r], projShape.ColumnsPosition[c])
                        );
                        break;
                    }
                }
            }

            return lowestShapeTiles.ToArray();
        }

        private int[] GetRowIndicesWithoutFullLines(int[] currentRowIndices, int[] fullLinesIndices)
        {
            List<int> rowIndicesWithoutFullLines = Enumerable.Range(0, currentRowIndices.Length).ToList();

            rowIndicesWithoutFullLines.RemoveAll(i => fullLinesIndices.Contains(currentRowIndices[i]));

            return rowIndicesWithoutFullLines.ToArray();
        }
    }
}
