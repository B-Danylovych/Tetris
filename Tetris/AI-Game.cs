using System;
using System.Collections.Generic;
using System.Data.Common;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Shapes;
using static Tetris.AccessibleGap;

namespace Tetris
{
    public class AI_Game : GameMain
    {
        public List<ShapeMoveOption> ShapeMoveOptions { get; private set; }
            = new List<ShapeMoveOption>();

        public int[] TopTilesOfColumns { get; private set; }
        public int[][] Gaps { get; private set; }
        public AccessibleGap[][] AccessibleGaps { get; private set; }

        public AI_Game(int rows, int hiddenRowsOnTop, int cols,
            Shape bufferShape, Shape currentShape, Shape projectedShape, List<List<GridValue>> grid)
            : base(rows, hiddenRowsOnTop, cols)
        {
            BufferShape = SetShapeClone(bufferShape);
            CurrentShape = SetShapeClone(currentShape);
            ProjectedShape = SetShapeClone(projectedShape);
            Grid = SetGridClone(grid);

            TopTilesOfColumns = GetTopTiles();
            Gaps = GetGapsToTopTiles();
            AccessibleGaps = GetAccessibleGaps();
        }

        private Shape SetShapeClone(Shape cloneShape)
            => cloneShape.DeepCopy();

        private List<List<GridValue>> SetGridClone(List<List<GridValue>> grid)
            => new List<List<GridValue>>(grid);

        private int[] GetTopTiles()
        {
            int[] topTiles = new int[Columns];
            for (int c = 0; c < Columns; c++)
            {
                topTiles[c] = GetTopTileOfColumn(c);
            }
            return topTiles;
        }

        private int GetTopTileOfColumn(int column)
        {
            for (int row = Rows - HiddenRowsOnTop - 1; row >= 0; row--)
            {
                if (Grid[row][column] != GridValue.Empty)
                    return row;
            }
            return -1;
        }

        private int[][] GetGapsToTopTiles()
        {
            int[][] gaps = new int[Columns][];
            for (int c = 0; c < Columns; c++)
            {
                gaps[c] = GetGapsToTopTileOfColumn(c, TopTilesOfColumns[c]);
            }
            return gaps;
        }

        private int[] GetGapsToTopTileOfColumn(int column, int topTile)
        {
            List<int> gapsInColumn = new List<int>();
            for (int row = 0; row < topTile; row++)
            {
                if (Grid[row][column] == GridValue.Empty)
                {
                    gapsInColumn.Add(row);
                }
            }
            return gapsInColumn.ToArray();
        }

        private AccessibleGap[][] GetAccessibleGaps()
        {
            List<AccessibleGap[]> accessibleGaps = new List<AccessibleGap[]>();
            for (int column = 0; column < Gaps.Length; column++)
            {
                if (Gaps[column].Length == 0)
                    continue;

                CellPosition topGap = GetTopGapOfGapsInColumn(column, Gaps[column]);

                AccessibleGap.SideAccess sideAccessOfTopGap = GetAccessibilityOfGap(topGap);

                if (sideAccessOfTopGap != AccessibleGap.SideAccess.None)
                {
                    AccessibleGap TopAccessibleGap =
                        new AccessibleGap(topGap.Row, topGap.Column, sideAccessOfTopGap);

                    accessibleGaps.Add(
                        GetAccessibleGapsOfTopAccessibleGapColumn(TopAccessibleGap)
                    );
                }
            }
            return accessibleGaps.ToArray();
        }

        private CellPosition GetTopGapOfGapsInColumn(int column, int[] gapsInColumn)
        {
            int row = gapsInColumn[gapsInColumn.Length - 1];
            return new CellPosition(row, column);
        }

        private AccessibleGap.SideAccess GetAccessibilityOfGap(CellPosition gap)
        {
            if (IsGapAccessibleInLeft(gap))
                return AccessibleGap.SideAccess.Left;
            else if (IsGapAccessibleInRight(gap))
                return AccessibleGap.SideAccess.Right;
            else
                return AccessibleGap.SideAccess.None;
        }

        private AccessibleGap[] GetAccessibleGapsOfTopAccessibleGapColumn(AccessibleGap topAccessibleGap)
        {
            List<AccessibleGap> AccessibleGapsInColumn = new List<AccessibleGap>();

            int column = topAccessibleGap.Column;
            int[] gapsInColumn = Gaps[column];
            AccessibleGap.SideAccess sideAccess = topAccessibleGap.Side;

            Func<CellPosition, bool> CheckInDirection = GetCheckInDirectionDelegateBySideAccess(sideAccess);
            bool nextGapsIsAccessible = false;

            for (int r = 0; r < gapsInColumn.Length; r++)
            {
                CellPosition gap = new CellPosition(gapsInColumn[r], column);

                if (nextGapsIsAccessible || CheckInDirection(gap))
                {
                    AccessibleGapsInColumn.Add(new AccessibleGap(gap.Row, gap.Column, sideAccess));
                    nextGapsIsAccessible = true;
                }
            }
            return AccessibleGapsInColumn.ToArray();
        }

        private Func<CellPosition, bool> GetCheckInDirectionDelegateBySideAccess
            (AccessibleGap.SideAccess sideAccess)
        {
            return (sideAccess == AccessibleGap.SideAccess.Left)
                    ? IsGapAccessibleInLeft : IsGapAccessibleInRight;
        }

        private bool IsGapAccessibleInLeft(CellPosition gap)
        {
            if (gap.Column < 2 ||
                gap.Row <= TopTilesOfColumns[gap.Column - 1] ||
                gap.Row <= TopTilesOfColumns[gap.Column - 2])
            {
                return false;
            }
            else
                return true;
        }

        private bool IsGapAccessibleInRight(CellPosition gap)
        {
            if (gap.Column >= Columns - 2 ||
                gap.Row <= TopTilesOfColumns[gap.Column + 1] ||
                gap.Row <= TopTilesOfColumns[gap.Column + 2])
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
                == columnOfAccessibleGaps[0].Column;
            }
            else
            {
                int startColumnIndex = CurrentShape.StartColumnIndex;

                return CurrentShape.ColumnsPosition[startColumnIndex] - 1
                == columnOfAccessibleGaps[0].Column;
            }
        }

        private bool IsCurrentShapeNearAccessibleGap(AccessibleGap[] columnOfAccessibleGaps)
        {
            int startRowPosition = CurrentShape.RowsPosition[CurrentShape.StartRowIndex];
            int lastRowPosition = CurrentShape.RowsPosition[CurrentShape.StartRowIndex + CurrentShape.Height - 1];

            foreach (AccessibleGap accessibleGap in columnOfAccessibleGaps)
            {
                return accessibleGap.Row <= startRowPosition && accessibleGap.Row >= lastRowPosition;
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

            int[] fullLinesIndices = GetFullLinesIndices(projShape);
            score -= fullLinesIndices.Length * 2;

            score -= IsShapeOfMoveOptionEliminatedByFullLines(projShape, fullLinesIndices) ? 10000 : 0;

            score += CalculateHighShapeScore(projShape);

            score += CalculateNumOfCreatedGapsByMoveOption(projShape) * 3;

            score += CalculateImpactOnExistingGapsWithMoveOption(projShape, fullLinesIndices.Length);

            return score;
        }

        private int[] GetFullLinesIndices(Shape projShape)
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

        private bool IsShapeOfMoveOptionEliminatedByFullLines(Shape projShape, int[] fullLinesIndices)
        {
            for (int r = 0; r < projShape.RowsCount; r++)
            {
                if (fullLinesIndices.Contains(projShape.RowsPosition[r]))
                    continue;

                for (int c = 0; c < projShape.ColumnsCount; c++)
                {
                    if (projShape.ShapeGrid[r, c] != GridValue.Empty)
                        return false;
                }
            }
            return true;
        }

        private int CalculateHighShapeScore(Shape projShape)
        {
            for (int r = 0; r < projShape.RowsCount; r++)
            {
                for (int c = 0; c < projShape.ColumnsCount; c++)
                {
                    if (projShape.ShapeGrid[r, c] != GridValue.Empty)
                        return (projShape.RowsPosition[r]);
                }
            }

            throw new InvalidOperationException("The projectedShape is empty.");
        }

        private int CalculateImpactOnExistingGapsWithMoveOption(Shape projShape, int clearedLinesCount)
        {
            int blockingImpact = 0;

            for (int c = 0; c < projShape.ColumnsCount; c++)
            {
                int overlayingTilesCount = GetNumOfTilesInColumnOfShapeGrid(projShape, c) - clearedLinesCount;

                if (overlayingTilesCount <= 0)
                    continue;

                int lowestRowPositionTile = GetLowestRowPositionTileOfShapeGrid(projShape, c);

                int affectedGapsCount = Gaps[c].TakeWhile(x => x < lowestRowPositionTile).Count();

                blockingImpact += overlayingTilesCount * affectedGapsCount;
            }
            return blockingImpact;
        }

        private int GetNumOfTilesInColumnOfShapeGrid(Shape projShape, int column)
        {
            int numOfTiles = 0;
            for (int row = 0; row < projShape.RowsCount; row++)
            {
                if (projShape.ShapeGrid[row, column] != GridValue.Empty)
                    numOfTiles++;
            }
            return numOfTiles;
        }

        private int GetLowestRowPositionTileOfShapeGrid(Shape projShape, int column)
        {
            for (int row = projShape.RowsCount - 1; row >= 0; row--)
            {
                if (projShape.ShapeGrid[row, column] != GridValue.Empty)
                    return row;
            }
            return -1;
        }

        private int CalculateNumOfCreatedGapsByMoveOption(Shape projShape)
        {
            int numOfGaps = 0;

            for (int c = 0; c < projShape.ColumnsCount; c++)
            {
                int lowestRowPositionTile = GetLowestRowPositionTileOfShapeGrid(projShape, c);

                for (int r = lowestRowPositionTile - 1; r >= 0; r--)
                {
                    if (Grid[r][c] == GridValue.Empty)
                        numOfGaps++;
                    else
                        break;
                }
            }

            return numOfGaps;
        }

        private int GetNumOfLateralSharedEdges(Shape projShape)
        {
            int numOfEdges = 0;

            for (int r = 0; r < projShape.RowsCount; r++)
            {
                numOfEdges += CountSharedEdgesInRowByDirection(projShape, r, SharedEdgeDir.Left);
                numOfEdges += CountSharedEdgesInRowByDirection(projShape, r, SharedEdgeDir.Right);
            }

            return numOfEdges;
        }

        private enum SharedEdgeDir
        {
            Left,
            Right
        }

        private int CountSharedEdgesInRowByDirection(Shape projShape, int row, SharedEdgeDir edgeDirection)
        {
            int numOfEdgesInRow = 0;

            int offSetCheck = edgeDirection == SharedEdgeDir.Left ? -1 : 1;
            int startOfIteration = edgeDirection == SharedEdgeDir.Left ? 0 : projShape.ColumnsCount - 1;
            int endOfIteration = edgeDirection == SharedEdgeDir.Left ? projShape.ColumnsCount : -1;
            int stepOfIteration = edgeDirection == SharedEdgeDir.Left ? 1 : -1;

            bool isFirstTileInRow = true;
            for (int column = startOfIteration; column != endOfIteration; column += stepOfIteration)
            {
                if (projShape.ShapeGrid[row, column] != GridValue.Empty)
                {
                    if (!isFirstTileInRow ||
                        projShape.RowsPosition[row] == 0 ||
                        Grid[projShape.RowsPosition[row] + offSetCheck][column] != GridValue.Empty)
                    {
                        numOfEdgesInRow++;
                    }

                    isFirstTileInRow = false;
                }
            }

            return numOfEdgesInRow;
        }
    }
}
