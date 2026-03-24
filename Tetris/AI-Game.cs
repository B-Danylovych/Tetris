using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
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
        private ShapeMoveOption[] ShapeMoveOptions { get; set; }

        public ShapeMoveOption BestMoveOption { get; private set; }

        private int[] TopTilesOfColumns { get; set; }
        private int[][] Gaps { get; set; }
        private int NumOfGaps { get; set; }
        private AccessibleGap[][] AccessibleGaps { get; set; }

        GameMain Game { get; }

        public AI_Game(GameMain game) : base(game.Rows - game.HiddenRowsOnTop, game.HiddenRowsOnTop, game.Columns)
        {
            Game = game;
            Grid = SetGridClone(Game.Grid);

            ResetValues();
        }

        [MemberNotNull(nameof(TopTilesOfColumns), nameof(Gaps), nameof(AccessibleGaps),
                   nameof(ShapeMoveOptions), nameof(BestMoveOption))]
        public void ResetValues()
        {
            BufferShape = SetShapeClone(Game.BufferShape);
            CurrentShape = SetShapeClone(Game.CurrentShape);
            ProjectedShape = SetShapeClone(Game.ProjectedShape);

            TopTilesOfColumns = GetTopTiles();
            Gaps = GetExistingGaps();
            AccessibleGaps = GetAccessibleGaps();

            NumOfGaps = CountGaps();

            ShapeMoveOptions = GetAllMoveOptions();
            BestMoveOption = GetBestMoveOption();
        }

        public void UptadeGrid()
        {
            CurrentShape = BestMoveOption.ProjectedShape.DeepCopy();
            SetShapeOnGrid();
            RemoveLines();
        }

        private Shape SetShapeClone(Shape cloneShape)
            => cloneShape.DeepCopy();

        private List<List<GridValue>> SetGridClone(List<List<GridValue>> grid)
            => grid.Select(row => new List<GridValue>(row)).ToList();

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

        private int[][] GetExistingGaps()
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
                    gapsInColumn.Add(row);
            }
            return gapsInColumn.ToArray();
        }

        private int CountGaps()
        {
            int length = 0;

            for (int c = 0; c < Gaps.Length; c++)
                length += Gaps[c].Length;

            return length;
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

        private ShapeMoveOption[] GetAllMoveOptions()
        {
            List<ShapeMoveOption> moveOptions = new List<ShapeMoveOption>();

            moveOptions.AddRange(GetMoveOptionsForAllPositionsCurrentRotation(new Action[0]));

            Shape currentShape = CurrentShape.DeepCopy();

            int rotationAttempts = 3;

            moveOptions.AddRange(GetMoveOptionsForAllPositionsOfRotations(DirectionOfRotation.isClockwise, ref rotationAttempts));

            CurrentShape = currentShape.DeepCopy();
            SetProjectedShape();

            moveOptions.AddRange(GetMoveOptionsForAllPositionsOfRotations(DirectionOfRotation.isCounterclockwise, ref rotationAttempts));

            return moveOptions.ToArray();
        }

        private ShapeMoveOption[] GetMoveOptionsForAllPositionsOfRotations
            (DirectionOfRotation directionOfRotation, ref int attempts)
        {
            List<ShapeMoveOption> moveOptions = new List<ShapeMoveOption>();

            List<Action> actionList = new List<Action>();

            while (attempts > 0)
            {
                attempts--;

                if (Rotate(directionOfRotation))
                {
                    SetProjectedShape();
                    actionList.Add(() => Game.Rotate(directionOfRotation));
                    moveOptions.AddRange(GetMoveOptionsForAllPositionsCurrentRotation(actionList.ToArray()));
                }
                else
                    break;
            }
            return moveOptions.ToArray();
        }

        public ShapeMoveOption[] GetMoveOptionsForAllPositionsCurrentRotation(Action[] actions)
        {
            List<ShapeMoveOption> moveOptions = new List<ShapeMoveOption>();

            moveOptions.Add(GetCurrentPositionMoveOption(actions));

            moveOptions.AddRange(GetMoveOptionsToAccessibleGaps(actions));

            moveOptions.AddRange(GetPositionsByDirection(MoveLeft, true, actions));

            moveOptions.AddRange(GetPositionsByDirection(MoveRight, true, actions));

            return moveOptions.ToArray();
        }

        private ShapeMoveOption[] GetMoveOptionsToAccessibleGaps(Action[] actions)
        {
            List<ShapeMoveOption> moveOptions = new List<ShapeMoveOption>();

            foreach (AccessibleGap[] columnOfAccessibleGaps in AccessibleGaps)
            {
                if (!IsCurrentShapeNearColumnOfAccessibleGaps(columnOfAccessibleGaps))
                    continue;

                List<Action> actionList = actions.ToList();

                Shape currentShape = CurrentShape.DeepCopy();

                Func<bool> MoveInDirection = columnOfAccessibleGaps[0].Side == AccessibleGap.SideAccess.Left
                    ? MoveRight : MoveLeft;

                if (IsCurrentShapeNearAccessibleGap(columnOfAccessibleGaps))
                    moveOptions.AddRange(GetPositionsByDirection(MoveInDirection, false, actionList.ToArray()));

                while (MoveDown())
                {
                    actionList.Add(() => Game.MoveDown());
                    if (IsCurrentShapeNearAccessibleGap(columnOfAccessibleGaps))
                        moveOptions.AddRange(GetPositionsByDirection(MoveInDirection, false, actionList.ToArray()));
                }
                CurrentShape = currentShape.DeepCopy();
            }

            return moveOptions.ToArray();
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

        private ShapeMoveOption[] GetPositionsByDirection(Func<bool> MoveInDirection, bool isTopCheck, Action[] actions)
        {
            List<ShapeMoveOption> moveOptions = new List<ShapeMoveOption>();

            List<Action> actionList = actions.ToList();

            Shape currentShape = CurrentShape.DeepCopy();

            while (MoveInDirection())
            {
                SetProjectedShape();
                actionList.Add(() => MoveInDirection.Method.Invoke(Game, null));
                moveOptions.Add(GetCurrentPositionMoveOption(actionList.ToArray()));

                if (isTopCheck)
                    moveOptions.AddRange(GetMoveOptionsToAccessibleGaps(actionList.ToArray()));
            }

            CurrentShape = currentShape.DeepCopy();
            SetProjectedShape();

            return moveOptions.ToArray();
        }

        public ShapeMoveOption GetCurrentPositionMoveOption(Action[] actions)
            => new ShapeMoveOption(ProjectedShape, CalculateMoveOptionScore(ProjectedShape), actions);

        public int CalculateMoveOptionScore(Shape projShape)
        {
            if (IsGameOverPosition(projShape))
                return int.MinValue;

            int score = 0;

            int[] fullLinesIndices = GetFullLinesIndices(projShape);
            int clearedLinesCount = fullLinesIndices.Length;

            score += (int)Math.Pow(clearedLinesCount * 10, 2);
            score += (clearedLinesCount < 4) ? 0 : 400;

            int createdOrFilledGapsScore = NumOfGaps < 10 ? 300 : 400;

            score -= CalculateCreatedGaps(projShape, fullLinesIndices) * createdOrFilledGapsScore;

            score += CountFilledAccessibleGaps(projShape) * createdOrFilledGapsScore;

            int highOfShape = CalculateHighOfShape(projShape) - clearedLinesCount;

            if (highOfShape >= 15)
                score -= highOfShape * 30;
            else
                score -= highOfShape * 20;

            score -= CountCreatedCliffs(projShape) * 150;

            int impactOnExistingGapsScore = NumOfGaps < 10 ? 20 : 40;
            score -= CalculateImpactOnExistingGaps(projShape, clearedLinesCount) * impactOnExistingGapsScore;

            score += CountLateralSharedEdges(projShape) * 60 - projShape.Height * 60;

            return score;
        }

        private bool IsGameOverPosition(Shape projShape)
        {
            if (projShape.RowsPosition[projShape.StartRowIndex] >= Rows - HiddenRowsOnTop - 1)
                return true;
            else
                return false;
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

                if (isLineFullCheck(currentLine))
                    fullLines.Add(rowPosition);
            }

            return fullLines.ToArray();
        }

        private int CalculateCreatedGaps(Shape projShape, int[] fullLinesIndices)
        {
            int numOfGaps = 0;

            for (int c = 0; c < projShape.ColumnsCount; c++)
            {
                if (!HasNonFullLineTileInColumn(projShape, fullLinesIndices, c))
                    continue;

                int lowestRowPosTile = projShape.GetLowestRowPositionTileInColumn(c);

                for (int r = lowestRowPosTile - 1; r >= 0; r--)
                {
                    if (Grid[r][projShape.ColumnsPosition[c]] == GridValue.Empty)
                        numOfGaps++;
                    else
                        break;
                }
            }

            return numOfGaps;
        }

        private bool HasNonFullLineTileInColumn(Shape projShape, int[] fullLinesIndices, int column)
        {
            for (int row = 0; row < projShape.RowsCount; row++)
            {
                if (projShape.ShapeGrid[row, column] != GridValue.Empty
                    && !fullLinesIndices.Contains(projShape.RowsPosition[row]))
                {
                    return true;
                }
            }
            return false;
        }

        private int CountFilledAccessibleGaps(Shape projShape)
        {
            int numOfFilledAccessibleGaps = 0;

            for (int c = 0; c < projShape.ColumnsCount; c++)
            {
                int numOfTiles = projShape.CountTilesInColumn(c);

                if (numOfTiles == 0)
                    continue;

                int highestRowPosTile = projShape.GetHighestRowPositionTileInColumn(c);

                if (highestRowPosTile < TopTilesOfColumns[projShape.ColumnsPosition[c]])
                    numOfFilledAccessibleGaps += numOfTiles;
            }
            return numOfFilledAccessibleGaps;
        }

        private int CalculateHighOfShape(Shape projShape)
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

        private int CountCreatedCliffs(Shape projShape)
        {
            int numOfCliffs = 0;

            if (CalculateCliffSizeByDirection(projShape, checkDir.Left) > 1)
                numOfCliffs++;

            if (CalculateCliffSizeByDirection(projShape, checkDir.Right) > 1)
                numOfCliffs++;

            return numOfCliffs;
        }

        private enum checkDir
        {
            Left,
            Right
        }

        private int CalculateCliffSizeByDirection(Shape projShape, checkDir checkDir)
        {
            int dirOffset = checkDir == checkDir.Left ? -1 : 1;

            int edgeColumnShapeIndex = checkDir == checkDir.Left
                ? projShape.StartColumnIndex
                : projShape.StartColumnIndex + projShape.Width - 1;

            int edgeColumnShapePosition = projShape.ColumnsPosition[edgeColumnShapeIndex];

            int edgeColumnShapeHeight = projShape.GetHighestRowPositionTileInColumn(edgeColumnShapeIndex);

            int neighboringColumnHeight 
                = GetTopTileOfNeighborColumn(edgeColumnShapePosition + dirOffset);

            int firstHeightDifference = edgeColumnShapeHeight - neighboringColumnHeight;

            if (firstHeightDifference >= 0)
            {
                int twoStepNeighborColumnHeight 
                    = GetTopTileOfNeighborColumn(edgeColumnShapePosition + (2 * dirOffset));

                int secondHeightDifference = twoStepNeighborColumnHeight - neighboringColumnHeight;

                return firstHeightDifference <= secondHeightDifference ? firstHeightDifference : secondHeightDifference;
            }
            else
            {
                if (edgeColumnShapeIndex - dirOffset >= projShape.ColumnsCount || edgeColumnShapeIndex - dirOffset < 0)
                    return 0;

                int innerNeighborColumnShapeHeight = projShape.GetHighestRowPositionTileInColumn(edgeColumnShapeIndex - dirOffset);

                int secondHeightDifference = edgeColumnShapeHeight - innerNeighborColumnShapeHeight;

                return -firstHeightDifference <= -secondHeightDifference ? -firstHeightDifference : -secondHeightDifference;
            }
        }

        private int GetTopTileOfNeighborColumn(int neighborColumn)
        {
            return (neighborColumn < 0 || neighborColumn >= Columns)
                ? Rows
                : TopTilesOfColumns[neighborColumn];
        }

        private int CalculateImpactOnExistingGaps(Shape projShape, int clearedLinesCount)
        {
            int blockingImpact = 0;

            for (int c = 0; c < projShape.ColumnsCount; c++)
            {
                int overlayingTilesCount = projShape.CountTilesInColumn(c) - clearedLinesCount;

                if (overlayingTilesCount > 0)
                {
                    int lowestRowPosTile = projShape.GetLowestRowPositionTileInColumn(c);

                    int affectedGapsCount = Gaps[projShape.ColumnsPosition[c]].TakeWhile(x => x < lowestRowPosTile).Count();

                    blockingImpact += overlayingTilesCount * affectedGapsCount;
                }
            }
            return blockingImpact;
        }

        private int CountLateralSharedEdges(Shape projShape)
        {
            int numOfEdges = 0;

            for (int r = 0; r < projShape.RowsCount; r++)
            {
                if (projShape.RowsPosition[r] < 0)
                    break;

                numOfEdges += HasSharedEdgeInRowByLeft(projShape, r) ? 1 : 0;
                numOfEdges += HasSharedEdgeInRowByRight(projShape, r) ? 1 : 0;
            }
            return numOfEdges;
        }
        private bool HasSharedEdgeInRowByLeft(Shape projShape, int row)
        {
            for (int column = 0; column < projShape.ColumnsCount; column++)
            {
                if (projShape.ShapeGrid[row, column] != GridValue.Empty)
                {
                    CellPosition offSetCell
                        = new CellPosition(projShape.RowsPosition[row], projShape.ColumnsPosition[column] - 1);

                    if (isCellOutsideOrFilled(offSetCell))
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        private bool HasSharedEdgeInRowByRight(Shape projShape, int row)
        {
            for (int column = projShape.ColumnsCount - 1; column >= 0; column--)
            {
                if (projShape.ShapeGrid[row, column] != GridValue.Empty)
                {
                    CellPosition offSetCell
                        = new CellPosition(projShape.RowsPosition[row], projShape.ColumnsPosition[column] + 1);

                    if (isCellOutsideOrFilled(offSetCell))
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        private bool isCellOutsideOrFilled(CellPosition offSetCell)
        {
            return offSetCell.Column >= Columns || offSetCell.Column < 0
                || Grid[offSetCell.Row][offSetCell.Column] != GridValue.Empty;
        }

        private ShapeMoveOption GetBestMoveOption()
        {
            ShapeMoveOption bestShapeMoveOption = ShapeMoveOptions[0];

            foreach (ShapeMoveOption shapeMoveOption in ShapeMoveOptions)
            {
                bestShapeMoveOption = shapeMoveOption.Score > bestShapeMoveOption.Score
                    ? shapeMoveOption
                    : bestShapeMoveOption;
            }

            return bestShapeMoveOption;
        }
    }
}
