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
    public class AI_Grid : GameMain
    {
        public List<ShapeMoveOption> FigureMoveOptions { get; private set; }
            = new List<ShapeMoveOption>();
        public AI_Grid(int rows, int hiddenRowsOnTop, int cols,
            Shape bufferFigure, Shape currentFigure, Shape projectedFigure, List<List<GridValue>> grid)
            : base(rows, hiddenRowsOnTop, cols)
        {
            BufferShape = setFigureClone(bufferFigure);
            CurrentShape = setFigureClone(currentFigure);
            ProjectedShape = setFigureClone(projectedFigure);
            Grid = new List<List<GridValue>>(grid);
        }

        public Shape setFigureClone(Shape cloneFigure)
        {
            return cloneFigure.DeepCopy();
        }

        public void CheckAllPositionsProjectedFigures()
        {
            FigureMoveOptions.Add(GetCurrentPositionMoveOption());

            int[] currentColumnsPosition = new int[CurrentShape.ColumnCount];
            Array.Copy(CurrentShape.ColumnsPosition, currentColumnsPosition, CurrentShape.ColumnCount);

            CheckAllPositionsInDirection(MoveLeft);

            //CurrentFigure.ColumnsPosition = currentColumnIndexOnGrid;

            CheckAllPositionsInDirection(MoveRight);
        }

        private void CheckAllPositionsInDirection(Func<bool> moveDirection)
        {
            bool canMove = true;
            while (true)
            {
                canMove = moveDirection();
                if (canMove)
                    FigureMoveOptions.Add(GetCurrentPositionMoveOption());
                else
                    break;
            }
        }

        private ShapeMoveOption GetCurrentPositionMoveOption()
        {
            return new ShapeMoveOption(ProjectedShape,
                CurrentShape.ColumnsPosition[0], CalculateMoveOptionScore(ProjectedShape));
        }

        public int CalculateMoveOptionScore(Shape projFig)
        {
            int score = 0;

            score += CalculateHighFigureScore(projFig);

            int[] fullLinesIndices = CalculateFullLines(projFig);
            score -= fullLinesIndices.Length * 2;

            return score;
        }

        private int CalculateHighFigureScore(Shape projFig)
        {
            for (int r = 0; r < projFig.RowCount; r++)
            {
                for (int c = 0; c < projFig.ColumnCount; c++)
                {
                    if (projFig.ShapeValue[r, c] != GridValue.Empty)
                        return (projFig.RowsPosition[0] + r);
                }
            }
            throw new InvalidOperationException("The projectedFigure is empty.");
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
                    if (projFig.ShapeValue[r, c] != GridValue.Empty)
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

        private int[] CalculateFullLines(Shape projFig)
        {
            LinkedList<int> fullLines = new LinkedList<int>();
            for (int r = 0; r < projFig.RowsPosition.Length; r++)
            {
                bool isFull = true;
                for (int c = 0; c < Grid[projFig.RowsPosition[r]].Count; c++)
                {
                    if (Grid[projFig.RowsPosition[r]][c] == GridValue.Empty)
                    {
                        if (c >= projFig.ColumnsPosition[0] &&
                        c <= projFig.ColumnsPosition[projFig.ColumnsPosition.Length - 1])
                        {
                            if (projFig.ShapeValue[r,
                                c - projFig.ColumnsPosition[0]] == GridValue.Empty)
                            {
                                isFull = false;
                                break;
                            }
                        }
                        else
                        {
                            isFull = false;
                            break;
                        }
                    }
                }
                if (isFull)
                    fullLines.AddLast(projFig.RowsPosition[r]);
            }

            return fullLines.ToArray();
        }
    }
}
