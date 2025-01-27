using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class AI_Grid : GameMain
    {
        public AI_Grid(int rows, int cols, Figure bufferFigure, Figure currentFigure, List<List<GridValue>> grid) : base(rows, cols)
        {
            BufferFigure = setFigureClone(bufferFigure);
            CurrentFigure = setFigureClone(currentFigure);
            Grid = new List<List<GridValue>>(grid);
        }

        public Figure setFigureClone(Figure cloneFigure)
        {
            return cloneFigure.Clone();
        }
    }
}
