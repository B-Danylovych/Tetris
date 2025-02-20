using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private void CheckAllPositionsInDirection(Func<bool> moveDirection)
        {
            bool canMove = moveDirection();
            while (canMove)
            {
                ShapeMoveOptions.Add(GetCurrentPositionMoveOption());
                canMove = moveDirection();
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
            LinkedList<int> fullLines = new LinkedList<int>();

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

                if(IsLineFull(currentLine))
                    fullLines.AddLast(rowPosition);
            }

            return fullLines.ToArray();
        }

        private bool IsLineFull(GridValue[] currentLine)
        {
            foreach (GridValue gridVal in currentLine)
                if (gridVal == GridValue.Empty)
                    return false;

            return true;
        }

        private Tuple<int[], int>[] GetGaps(Shape projFig, int[] fullLinesIndices)
        {
            List<Tuple<int[],int>> gaps = new List<Tuple<int[], int>>();

            int[][] tiles = GetLowestNotFromFullLinesTiles(projFig, fullLinesIndices);

            foreach (int[] tile in tiles)
            {
                List<int> gapsInThisRow = new List<int>();
                for (int r = tile[0] - 1; r >= 0; r--)
                {
                    bool isFromFullLine = false;
                    foreach (int item in fullLinesIndices)
                    {
                        if (r == item)
                        {
                            isFromFullLine = true;
                            break;
                        }
                    }

                    if (!isFromFullLine && Grid[r][tile[1]] == GridValue.Empty)
                    {
                        gapsInThisRow.Add(r);
                    }
                }
                gaps.Add(Tuple.Create(gapsInThisRow.ToArray(), tile[1]));
            }

            return gaps.ToArray();
        }

        //private void DivideGapsIntoFullAndPartial(Figure projFig, int[] fullLinesIndices, 
        //    Tuple<int[], int>[] gaps)
        //{
        //    for (int i = 0; i < gaps.Length; i++)
        //    {
        //        GridValue[] valuesOfColumn = new GridValue[Grid.Count];
        //        for (int r = 0; r < Grid.Count; r++)
        //        {
        //            valuesOfColumn[r] = Grid[r][gaps[i].Item2];
        //        }

        //    }
        //}

        private int[][] GetLowestNotFromFullLinesTiles(Shape projFig, int[] fullLinesIndices)
        {
            List<int[]> lowestNotFromFullLinesTiles = new List<int[]>();

            for (int c = 0; c < projFig.ColumnCount; c++)
            {
                for (int r = projFig.RowCount - 1; r >= 0; r--)
                {
                    if (projFig.ShapeGrid[r, c] != GridValue.Empty)
                    {
                        bool isFromFullLine = false;
                        foreach (int item in fullLinesIndices)
                        {
                            if (projFig.RowsPosition[r] == item)
                            {
                                isFromFullLine = true;
                                break;
                            }
                        }
                        if (isFromFullLine)
                            continue;
                        else
                        {
                            lowestNotFromFullLinesTiles.Add
                                (new int[2] { projFig.RowsPosition[r], projFig.ColumnsPosition[c] });
                            break;
                        }
                    }
                }
            }

            return lowestNotFromFullLinesTiles.ToArray();
        }
    }
}
