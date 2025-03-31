using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Shapes;

namespace Tetris
{
    public class AI_Game : GameMain
    {
        public List<ShapeMoveOption> ShapeMoveOptions { get; private set; }
            = new List<ShapeMoveOption>();

        public int[] TopTilesOfColumns {  get; private set; }
        public int[][] Gaps { get; private set; }
        public AccessibleGap[][] AccessibleGaps {  get; private set; }

        public AI_Game(int rows, int hiddenRowsOnTop, int cols,
            Shape bufferShape, Shape currentShape, Shape projectedShape, List<List<GridValue>> grid)
            : base(rows, hiddenRowsOnTop, cols)
        {
            BufferShape = SetShapeClone(bufferShape);
            CurrentShape = SetShapeClone(currentShape);
            ProjectedShape = SetShapeClone(projectedShape);
            Grid = SetGridClone(grid);

            TopTilesOfColumns = GetTopTilesOfColumns();
            Gaps = GetGapsToTopTiles(TopTilesOfColumns);
            AccessibleGaps = GetAccessibleGaps(Gaps);
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

        private int[][] GetGapsToTopTiles(int[] topTilesOfColumns)
        {
            int[][] gaps = new int[Columns][];
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
                gaps[c] = gapsInColumn.ToArray();
            }
            return gaps;
        }

        private AccessibleGap[][] GetAccessibleGaps(int[][] gaps)
        {
            List<AccessibleGap[]> accessibleGaps = new List<AccessibleGap[]>();
            for (int c = 0; c < gaps.Length; c++)
            {
                if (gaps[c].Length == 0)
                    continue;

                CellPosition topGap = new CellPosition(gaps[c][gaps[c].Length - 1], c);

                if (IsGapAccessibleInLeft(topGap))
                {
                    accessibleGaps.Add(
                        GetAccessibleGapsOfColumnInDirection(c, gaps[c], AccessibleGap.SideAccess.Left)
                    );
                }
                else if (IsGapAccessibleInRight(topGap))
                {
                    accessibleGaps.Add(
                        GetAccessibleGapsOfColumnInDirection(c, gaps[c], AccessibleGap.SideAccess.Right)
                    );
                }
            }
            return accessibleGaps.ToArray();
        }

        private AccessibleGap[] GetAccessibleGapsOfColumnInDirection
            (int column, int[] gapsInColumn, AccessibleGap.SideAccess sideAccess)
        {
            List<AccessibleGap> AccessibleGapsInColumn = new List<AccessibleGap>();

            bool nextGapsIsAccessible = false;

            Func<CellPosition, bool> CheckInDirection = (sideAccess == AccessibleGap.SideAccess.Left) 
                ? IsGapAccessibleInLeft : IsGapAccessibleInRight;

            for (int r = 0; r < gapsInColumn.Length; r++)
            {
                if (nextGapsIsAccessible)
                {
                    AccessibleGapsInColumn.Add(new AccessibleGap(gapsInColumn[r], column, sideAccess));
                    continue;
                }

                CellPosition gap = new CellPosition(gapsInColumn[r], column);
                if (CheckInDirection(gap))
                {
                    nextGapsIsAccessible = true;
                    AccessibleGapsInColumn.Add(new AccessibleGap(gapsInColumn[r], column, sideAccess));
                }
            }
            return AccessibleGapsInColumn.ToArray();
        }

        private bool IsGapAccessibleInLeft(CellPosition gap)
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

        private bool IsGapAccessibleInRight(CellPosition gap)
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
            CheckAllPositionsProjectedShapes(new Action[0]);

            Shape currentShape = CurrentShape.DeepCopy();

            int rotationAttempts = 3;

            rotationAttempts = CheckRotateShapeWithAttempts(DirectionOfRotation.isClockwise, rotationAttempts);

            CurrentShape = currentShape.DeepCopy();
            SetProjectedShape();

            CheckRotateShapeWithAttempts(DirectionOfRotation.isCounterclockwise, rotationAttempts);
        }

        private int CheckRotateShapeWithAttempts(DirectionOfRotation directionOfRotation, int attempts)
        {
            List<Action> actionList = new List<Action>();
            int rotationAttempts = attempts;
            bool canRotate = true;
            while (rotationAttempts > 0 && canRotate)
            {
                canRotate = Rotate(directionOfRotation);
                rotationAttempts--;

                if (canRotate)
                {
                    SetProjectedShape();
                    actionList.Add(() => Rotate(directionOfRotation));
                    CheckAllPositionsProjectedShapes(actionList.ToArray());
                }
            }
            return rotationAttempts;
        }

        public void CheckAllPositionsProjectedShapes(Action[] actions)
        {
            ShapeMoveOptions.Add(GetCurrentPositionMoveOption(actions));

            CheckAccessibleGaps(actions);

            Shape currentShape = CurrentShape.DeepCopy();

            CheckAllPositionsInDirection(MoveLeft, true, actions);

            CurrentShape = currentShape.DeepCopy();
            SetProjectedShape();

            CheckAllPositionsInDirection(MoveRight, true, actions);

            CurrentShape = currentShape.DeepCopy();
            SetProjectedShape();
        }

        private void CheckAccessibleGaps(Action[] actions)
        {
            foreach (AccessibleGap[] columnOfAccessibleGaps in AccessibleGaps)
            {
                if (!IsCurrentShapeNearColumnOfAccessibleGaps(columnOfAccessibleGaps))
                    continue;

                List<Action> actionList = actions.ToList();

                Shape currentShape = CurrentShape.DeepCopy();

                Func<bool> MoveInDirection = columnOfAccessibleGaps[0].Side == AccessibleGap.SideAccess.Left
                    ? MoveRight : MoveLeft;

                if (IsCurrentShapeNearAccessibleGap(columnOfAccessibleGaps))
                    CheckAllPositionsInDirection(MoveInDirection, false, actionList.ToArray());

                while (MoveDown())
                {
                    actionList.Add(() => MoveDown());
                    if (IsCurrentShapeNearAccessibleGap(columnOfAccessibleGaps))
                        CheckAllPositionsInDirection(MoveInDirection, false, actionList.ToArray());
                }
            }
        }

        private bool IsCurrentShapeNearColumnOfAccessibleGaps(AccessibleGap[] columnOfAccessibleGaps)
        {
            if (columnOfAccessibleGaps[0].Side == AccessibleGap.SideAccess.Left)
            {
                int lastColumnIndex = CurrentShape.StartColumnIndex + CurrentShape.Width - 1;

                return CurrentShape.ColumnsPosition[lastColumnIndex] + 1
                == columnOfAccessibleGaps[0].X;
            }
            else
            {
                int startColumnIndex = CurrentShape.StartColumnIndex;

                return CurrentShape.ColumnsPosition[startColumnIndex] - 1
                == columnOfAccessibleGaps[0].X;
            }
        }

        private bool IsCurrentShapeNearAccessibleGap(AccessibleGap[] columnOfAccessibleGaps)
        {
            int startRowPosition = CurrentShape.RowsPosition[CurrentShape.StartRowIndex];
            int lastRowPosition = CurrentShape.RowsPosition[CurrentShape.StartRowIndex + CurrentShape.Height - 1];

            foreach (AccessibleGap accessibleGap in columnOfAccessibleGaps)
            {
                return accessibleGap.Y <= startRowPosition && accessibleGap.Y >= lastRowPosition;
            }

            return false;
        }

        private void CheckAllPositionsInDirection(Func<bool> MoveInDirection, bool isTopCheck, Action[] actions)
        {
            List<Action> actionList = actions.ToList();

            while (MoveInDirection())
            {
                SetProjectedShape();
                actionList.Add(() => MoveInDirection());
                ShapeMoveOptions.Add(GetCurrentPositionMoveOption(actionList.ToArray()));

                if (isTopCheck)
                    CheckAccessibleGaps(actionList.ToArray());
            }
        }

        private ShapeMoveOption GetCurrentPositionMoveOption(Action[] actions)
            => new ShapeMoveOption(ProjectedShape, CalculateMoveOptionScore(ProjectedShape), actions);

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
