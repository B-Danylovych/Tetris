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
                CurrentFigure.ColumnIndexOnGrid[0], CalculateMoveOption(ProjectedFigure));
        }

        public int CalculateMoveOption(Figure projectedFigure)
        {

        }
    }
}
