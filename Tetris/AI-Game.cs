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
        private List<ShapeMoveOption> ShapeMoveOptions { get; set; }
            = new List<ShapeMoveOption>();

        public ShapeMoveOption BestMoveOption { get; private set; }

        public int[] TopTilesOfColumns { get; }
        public int[][] Gaps { get; }
        public int NumOfGaps { get; }
        public AccessibleGap[][] AccessibleGaps { get; }

        GameMain Game { get; }

        public AI_Game(GameMain game) : base(game.Rows - game.HiddenRowsOnTop, game.HiddenRowsOnTop, game.Columns)
        {
            Game = game;
            BufferShape = SetShapeClone(game.BufferShape);
            CurrentShape = SetShapeClone(game.CurrentShape);
            ProjectedShape = SetShapeClone(game.ProjectedShape);
            Grid = SetGridClone(game.Grid);

            TopTilesOfColumns = GetTopTiles();
            Gaps = GetExistingGaps();
            AccessibleGaps = GetAccessibleGaps();

            NumOfGaps = CountGaps();

            CheckAllDirectionOfRotationProjectedShape();
            BestMoveOption = GetBestMoveOption();
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
                    actionList.Add(() => Game.Rotate(directionOfRotation));
                    CheckAllPositionsProjectedShapes(actionList.ToArray());
                }
            }
            return rotationAttempts;
        }

        public void CheckAllPositionsProjectedShapes(Action[] actions)
        {
            ShapeMoveOptions.Add(GetCurrentPositionMoveOption(actions));

            CheckAccessibleGaps(actions);

            CheckAllPositionsInDirection(MoveLeft, true, actions);

            CheckAllPositionsInDirection(MoveRight, true, actions);
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
                    actionList.Add(() => Game.MoveDown());
                    if (IsCurrentShapeNearAccessibleGap(columnOfAccessibleGaps))
                        CheckAllPositionsInDirection(MoveInDirection, false, actionList.ToArray());
                }
                CurrentShape = currentShape.DeepCopy();
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

            Shape currentShape = CurrentShape.DeepCopy();

            while (MoveInDirection())
            {
                SetProjectedShape();
                actionList.Add(() => MoveInDirection.Method.Invoke(Game, null));
                ShapeMoveOptions.Add(GetCurrentPositionMoveOption(actionList.ToArray()));

                if (isTopCheck)
                    CheckAccessibleGaps(actionList.ToArray());
            }

            CurrentShape = currentShape.DeepCopy();
            SetProjectedShape();
        }

        private ShapeMoveOption GetCurrentPositionMoveOption(Action[] actions)
            => new ShapeMoveOption(ProjectedShape, CalculateMoveOptionScore(ProjectedShape), actions);

        public int CalculateMoveOptionScore(Shape projShape)
        {
            int score = 0;


            score -= IsGameOverPosition(projShape) ? 100000 : 0;

            int[] fullLinesIndices = GetFullLinesIndices(projShape);
            int clearedLinesCount = fullLinesIndices.Length;

            score += (int)Math.Pow(clearedLinesCount * 10, 2);
            score += (clearedLinesCount < 4) ? 0 : 400;

            score += IsShapeEliminatedByFullLines(projShape, fullLinesIndices) ? 700 : 0;

            int createdOrFilledGapsScore = NumOfGaps < 10 ? 300 : 400;
            score -= CalculateCreatedGaps(projShape, fullLinesIndices) * createdOrFilledGapsScore;

            score += CountFilledAccessibleGaps(projShape) * createdOrFilledGapsScore;

            int highOfShape = CalculateHighOfShape(projShape) - clearedLinesCount;

            if (highOfShape >= 15)
                score -= highOfShape * 30;
            else
                score -= highOfShape * 20;

            score -= CountCreatedCliffs(projShape) * 150;

            int impactOnExistingGapsScore = NumOfGaps < 10 ? 100 : 150;
            score -= CalculateImpactOnExistingGaps(projShape, clearedLinesCount) * impactOnExistingGapsScore;

            score += CountLateralSharedEdges(projShape) * 60 - projShape.Height * 60;

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

                if (isLineFullCheck(currentLine))
                    fullLines.Add(rowPosition);
            }

            return fullLines.ToArray();
        }

        private bool IsShapeEliminatedByFullLines(Shape projShape, int[] fullLinesIndices)
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

        private int CalculateImpactOnExistingGaps(Shape projShape, int clearedLinesCount)
        {
            int blockingImpact = 0;

            for (int c = 0; c < projShape.ColumnsCount; c++)
            {
                int overlayingTilesCount = CountTilesInShapeGridColumn(projShape, c) - clearedLinesCount;

                if (overlayingTilesCount <= 0)
                    continue;

                int lowestRowPosTile = GetLowestRowPositionTileOfShapeInColumn(projShape, c);

                int affectedGapsCount = Gaps[c].TakeWhile(x => x < lowestRowPosTile).Count();

                blockingImpact += overlayingTilesCount * affectedGapsCount;
            }
            return blockingImpact;
        }

        private int CountTilesInShapeGridColumn(Shape projShape, int column)
        {
            int numOfTiles = 0;
            for (int row = 0; row < projShape.RowsCount; row++)
            {
                if (projShape.ShapeGrid[row, column] != GridValue.Empty)
                    numOfTiles++;
            }
            return numOfTiles;
        }

        private int GetLowestRowPositionTileOfShapeInColumn(Shape projShape, int column)
        {
            for (int row = projShape.RowsCount - 1; row >= 0; row--)
            {
                if (projShape.ShapeGrid[row, column] != GridValue.Empty)
                    return projShape.RowsPosition[row];
            }
            return -1;
        }

        private int CalculateCreatedGaps(Shape projShape, int[] fullLinesIndices)
        {
            int numOfGaps = 0;

            for (int c = 0; c < projShape.ColumnsCount; c++)
            {
                if (!HasNonFullLineTileInColumn(projShape, fullLinesIndices, c))
                    continue;

                int lowestRowPosTile = GetLowestRowPositionTileOfShapeInColumn(projShape, c);

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
                }
            }
            return false;
        }

        private bool isCellOutsideOrFilled(CellPosition offSetCell)
        {
            return offSetCell.Column >= Columns || offSetCell.Column < 0
                || Grid[offSetCell.Row][offSetCell.Column] != GridValue.Empty;
        }


        private int CountFilledAccessibleGaps(Shape projShape)
        {
            int numOfFilledAccessibleGaps = 0;

            for (int c = 0; c < projShape.ColumnsCount; c++)
            {
                int numOfTiles = CountTilesInShapeGridColumn(projShape, c);

                if (numOfTiles == 0)
                    continue;

                int highestRowPosTile = GetHighestRowPositionTileOfShapeInColumn(projShape, c);

                if (highestRowPosTile < TopTilesOfColumns[projShape.ColumnsPosition[c]])
                    numOfFilledAccessibleGaps += numOfTiles;
            }
            return numOfFilledAccessibleGaps;
        }

        private int GetHighestRowPositionTileOfShapeInColumn(Shape projShape, int column)
        {
            for (int row = 0; row < projShape.RowsCount; row++)
            {
                if (projShape.ShapeGrid[row, column] != GridValue.Empty)
                    return projShape.RowsPosition[row];
            }
            return -1;
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

            int shapeTileColumnIndex = checkDir == checkDir.Left
                ? projShape.StartColumnIndex : projShape.StartColumnIndex + projShape.Width - 1;

            CellPosition shapeTile = GetHighestTilePositionOfShapeInColumn(projShape, shapeTileColumnIndex);

            int neighborColumn = shapeTile.Column + dirOffset;
            int topTileRowOfNeighborColumn = (neighborColumn < 0 || neighborColumn >= Columns)
                ? Rows
                : TopTilesOfColumns[shapeTile.Column + dirOffset];

            int heightDifference = shapeTile.Row - topTileRowOfNeighborColumn;

            if (heightDifference >= 0)
            {
                int secondNeighborColumn = shapeTile.Column + (2 * dirOffset);
                int topTileRowOfSecondNeighborColumn =
                    (secondNeighborColumn < 0 || secondNeighborColumn >= Columns)
                    ? Rows
                    : TopTilesOfColumns[shapeTile.Column + (2 * dirOffset)];

                int secondHeightDifference = topTileRowOfSecondNeighborColumn - topTileRowOfNeighborColumn;

                return heightDifference <= secondHeightDifference ? heightDifference : secondHeightDifference;
            }
            else
            {
                int neighborShapeTileColumnIndex = shapeTileColumnIndex - dirOffset;

                if (neighborShapeTileColumnIndex >= projShape.ColumnsCount || neighborShapeTileColumnIndex < 0)
                    return 0;

                int RowPosOfNeighborShapeTile =
                    GetHighestRowPositionTileOfShapeInColumn(projShape, neighborShapeTileColumnIndex);

                int secondHeightDifference = shapeTile.Row - RowPosOfNeighborShapeTile;

                return heightDifference >= secondHeightDifference ? -heightDifference : -secondHeightDifference;
            }
        }

        private CellPosition GetHighestTilePositionOfShapeInColumn(Shape projShape, int columnIndex)
            => new CellPosition
                (GetHighestRowPositionTileOfShapeInColumn(projShape, columnIndex),
                projShape.ColumnsPosition[columnIndex]);

        private bool IsGameOverPosition(Shape projShape)
        {
            for (int c = 0; c < projShape.ColumnsCount; c++)
            {
                if (GetHighestRowPositionTileOfShapeInColumn(projShape, c) >= Rows - HiddenRowsOnTop - 1)
                    return true;
            }
            return false;
        }
    }
}
