using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shapes;

namespace Tetris
{
    public class AI_Game : GameMain
    {
        public List<ShapeMoveOption> ShapeMoveOptions { get; private set; }
            = new List<ShapeMoveOption>();

        public int[] TopTilesOfColumns {  get; private set; }
        public int[][] GapsInColumns { get; private set; }
        public ParticalGap[][] ParticalGaps {  get; private set; }

        public AI_Game(int rows, int hiddenRowsOnTop, int cols,
            Shape bufferShape, Shape currentShape, Shape projectedShape, List<List<GridValue>> grid)
            : base(rows, hiddenRowsOnTop, cols)
        {
            BufferShape = SetShapeClone(bufferShape);
            CurrentShape = SetShapeClone(currentShape);
            ProjectedShape = SetShapeClone(projectedShape);
            Grid = SetGridClone(grid);

            TopTilesOfColumns = GetTopTilesOfColumns();
            GapsInColumns = GetGapsInColumnsToTopTiles(TopTilesOfColumns);
            ParticalGaps = GetParticalGapsInColumns(GapsInColumns);
        }

        private Shape SetShapeClone(Shape cloneShape)
            => cloneShape.DeepCopy();

        private List<List<GridValue>> SetGridClone(List<List<GridValue>> grid)
            => new List<List<GridValue>>(grid);

        private int[] GetTopTilesOfColumns()
        {
            int[] topTiles = new int[Columns];
            for (int c = 0; c < Columns; c++)
            {
                int topTile = -1;
                for (int r = 0; r < (Rows - HiddenRowsOnTop - 1); r++)
                {
                    if (Grid[r][c] != GridValue.Empty)
                    {
                        topTile = r;
                    }
                }
                topTiles[c] = topTile;
            }
            return topTiles;
        }

        private int[][] GetGapsInColumnsToTopTiles(int[] topTilesOfColumns)
        {
            int[][] gapsInColumns = new int[Columns][];
            for (int c = 0; c < Columns; c++)
            {
                List<int> gapsInColumn = new List<int>();
                for (int r = 0; r < topTilesOfColumns[c]; r++)
                {
                    if (Grid[r][c] == GridValue.Empty)
                    {
                        gapsInColumn.Add(r);
                    }
                }
                gapsInColumns[c] = gapsInColumn.ToArray();
            }
            return gapsInColumns;
        }

        private ParticalGap[][] GetParticalGapsInColumns(int[][] gapsInColumns)
        {
            List<ParticalGap[]> particalGaps = new List<ParticalGap[]>();
            for (int c = 0; c < gapsInColumns.Length; c++)
            {
                if (gapsInColumns[c].Length == 0)
                    continue;

                CellPosition topGap = new CellPosition(gapsInColumns[c][gapsInColumns[c].Length - 1], c);

                if (IsGapPracticalInLeft(topGap))
                {
                    particalGaps.Add(
                        GetParticalGapsOfColumnInDirection(c, gapsInColumns[c], ParticalGap.SideAccess.Left)
                    );
                }
                else if (IsGapPracticalInRight(topGap))
                {
                    particalGaps.Add(
                        GetParticalGapsOfColumnInDirection(c, gapsInColumns[c], ParticalGap.SideAccess.Right)
                    );
                }
            }
            return particalGaps.ToArray();
        }

        private ParticalGap[] GetParticalGapsOfColumnInDirection
            (int column, int[] gapsInColumn, ParticalGap.SideAccess sideAccess)
        {
            List<ParticalGap> particalGapsInColumn = new List<ParticalGap>();

            bool nextGapsIsPartical = false;

            Func<CellPosition, bool> CheckInDirection = (sideAccess == ParticalGap.SideAccess.Left) 
                ? IsGapPracticalInLeft : IsGapPracticalInRight;

            for (int r = 0; r < gapsInColumn.Length; r++)
            {
                if (nextGapsIsPartical)
                {
                    particalGapsInColumn.Add(new ParticalGap(gapsInColumn[r], column, sideAccess));
                    continue;
                }

                CellPosition gap = new CellPosition(gapsInColumn[r], column);
                if (CheckInDirection(gap))
                {
                    nextGapsIsPartical = true;
                    particalGapsInColumn.Add(new ParticalGap(gapsInColumn[r], column, sideAccess));
                }
            }
            return particalGapsInColumn.ToArray();
        }

        private bool IsGapPracticalInLeft(CellPosition gap)
        {
            if (gap.X <= 1)
                return false;

            if (gap.Y <= TopTilesOfColumns[gap.X - 1] ||
                gap.Y <= TopTilesOfColumns[gap.X - 2])
            {
                return false;
            }
            else
                return true;
        }

        private bool IsGapPracticalInRight(CellPosition gap)
        {
            if (gap.X >= Columns - 2)
                return false;

            if (gap.Y <= TopTilesOfColumns[gap.X + 1] ||
                gap.Y <= TopTilesOfColumns[gap.X + 2])
            {
                return false;
            }
            else
                return true;
        }

        private void CheckAllDirectionOfRotationProjectedShape()
        {
            CheckAllPositionsProjectedShapes();

            Dir_Rotation direction = CurrentShape.Direction;

            int rotationAttempts = 3;

            rotationAttempts = CheckRotateShapeWithAttempts(DirectionOfRotation.isClockwise, rotationAttempts);

            CurrentShape.SetShapeValue(direction);

            CheckRotateShapeWithAttempts(DirectionOfRotation.isCounterclockwise, rotationAttempts);
        }

        private int CheckRotateShapeWithAttempts(DirectionOfRotation directionOfRotation, int attempts)
        {
            int rotationAttempts = attempts;
            bool canRotate = true;
            while (rotationAttempts > 0 && canRotate)
            {
                canRotate = Rotate(directionOfRotation);
                rotationAttempts--;

                if (canRotate)
                    CheckAllPositionsProjectedShapes();
            }
            return rotationAttempts;
        }

        public void CheckAllPositionsProjectedShapes()
        {
            ShapeMoveOptions.Add(GetCurrentPositionMoveOption());

            int[] currentColumnsPosition = CurrentShape.ColumnsPosition.ToArray();

            CheckAllPositionsInDirection(MoveLeft);

            CurrentShape.SetNewPositionOnGrid(CurrentShape.RowsPosition, currentColumnsPosition.ToArray());

            CheckAllPositionsInDirection(MoveRight);

            CurrentShape.SetNewPositionOnGrid(CurrentShape.RowsPosition, currentColumnsPosition.ToArray());
        }

        private void CheckAllPositionsInDirection(Func<bool> MoveInDirection)
        {
            bool canMove = MoveInDirection();
            while (canMove)
            {
                ShapeMoveOptions.Add(GetCurrentPositionMoveOption());
                canMove = MoveInDirection();
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

            score += CalculateGapsOfMoveOption(projShape, fullLinesIndices) * 3;

            return score;
        }

        private int CalculateHighShapeScore(Shape projShape)
        {
            for (int r = 0; r < projShape.RowsCount; r++)
                for (int c = 0; c < projShape.ColumnsCount; c++)
                    if (projShape.ShapeGrid[r, c] != GridValue.Empty)
                        return (projShape.RowsPosition[r]);

            throw new InvalidOperationException("The projectedShape is empty.");
        }

        private int[] CalculateFullLines(Shape projShape)
        {
            List<int> fullLines = new List<int>();

            for (int r = 0; r < projShape.RowsCount; r++)
            {
                int rowPosition = projShape.RowsPosition[r];
                if (rowPosition < 0)
                    break;

                GridValue[] currentLine = Grid[rowPosition].ToArray();

                for (int c = 0; c < projShape.ColumnsCount; c++)
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

        private int CalculateGapsOfMoveOption(Shape projShape, int[] fullLinesIndices)
        {
            int numOfGaps = 0;

            CellPosition[] lowestShapeTiles = GetLowestNotFromFullLinesTiles(projShape, fullLinesIndices);

            foreach (CellPosition tile in lowestShapeTiles)
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

            return numOfGaps;
        }

        private CellPosition[] GetLowestNotFromFullLinesTiles(Shape projShape, int[] fullLinesIndices)
        {
            List<CellPosition> lowestShapeTiles = new List<CellPosition>();

            int[] reverseRowIndices = GetRowIndicesWithoutFullLines(projShape.RowsPosition, fullLinesIndices);
            Array.Reverse(reverseRowIndices);

            for (int c = 0; c < projShape.ColumnsCount; c++)
            {
                foreach (int r in reverseRowIndices)
                {
                    if (projShape.ShapeGrid[r, c] != GridValue.Empty)
                    {
                        lowestShapeTiles.Add(
                            new CellPosition(projShape.RowsPosition[r], projShape.ColumnsPosition[c])
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
