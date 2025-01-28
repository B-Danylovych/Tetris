using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class AI_Grid : GameMain
    {
        public List<FigureMoveOption> FigureMoveOptions { get; private set; }
            = new List<FigureMoveOption>();
        public AI_Grid(int rows, int cols, Figure bufferFigure, Figure currentFigure, Figure projectedFigure, List<List<GridValue>> grid) : base(rows, cols)
        {
            BufferFigure = setFigureClone(bufferFigure);
            CurrentFigure = setFigureClone(currentFigure);
            ProjectedFigure = setFigureClone(projectedFigure);
            Grid = new List<List<GridValue>>(grid);
        }

        public Figure setFigureClone(Figure cloneFigure)
        {
            return cloneFigure.Clone();
        }

        public void CheckAllPositionsProjectedFigures()
        {
            FigureMoveOptions.Add(GetCurrentPositionMoveOption());

            int[] currentColumnIndexOnGrid = new int[CurrentFigure.ColumnCount];
            Array.Copy(CurrentFigure.ColumnIndexOnGrid, currentColumnIndexOnGrid, CurrentFigure.ColumnCount);

            CheckAllPositionsInDirection(MoveLeft);

            CurrentFigure.ColumnIndexOnGrid = currentColumnIndexOnGrid;

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

        private FigureMoveOption GetCurrentPositionMoveOption()
        {
            return new FigureMoveOption(ProjectedFigure,
                CurrentFigure.ColumnIndexOnGrid[0], CalculateMoveOptionScore(ProjectedFigure));
        }

        public int CalculateMoveOptionScore(Figure projFig)
        {
            int score = 0;

            score += CalculateHighFigureScore(projFig);

            int[] fullLinesIndices = CalculateFullLines(projFig);
            score -= fullLinesIndices.Length * 2;

            return score;
        }

        private int CalculateHighFigureScore(Figure projFig)
        {
            for (int r = 0; r < projFig.RowCount; r++)
            {
                for (int c = 0; c < projFig.ColumnCount; c++)
                {
                    if (projFig.FigureValue[r, c] != GridValue.Empty)
                        return (projFig.RowIndexOnGrid[0] + r);
                }
            }
            throw new InvalidOperationException("The projectedFigure is empty.");
        }

        //private int CalculateGapsFigureScore(Figure projFig, int[] fullLinesIndices)
        //{

        //}

        private int[] CalculateFullLines(Figure projFig)
        {
            LinkedList<int> fullLines = new LinkedList<int>();
            for (int r = 0; r < projFig.RowIndexOnGrid.Length; r++)
            {
                bool isFull = true;
                for (int c = 0; c < Grid[projFig.RowIndexOnGrid[r]].Count; c++)
                {
                    if (Grid[projFig.RowIndexOnGrid[r]][c] == GridValue.Empty)
                    {
                        if (c >= projFig.ColumnIndexOnGrid[0] &&
                        c <= projFig.ColumnIndexOnGrid[projFig.ColumnIndexOnGrid.Length - 1])
                        {
                            if (projFig.FigureValue[r,
                                c - projFig.ColumnIndexOnGrid[0]] == GridValue.Empty)
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
                    fullLines.AddLast(projFig.RowIndexOnGrid[r]);
            }

            return fullLines.ToArray();
        }
    }
}
