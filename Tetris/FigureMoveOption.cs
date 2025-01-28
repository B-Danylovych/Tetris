using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class FigureMoveOption
    {
        public int Score { get; private set; }
        public int PositionOfCurrentFigure { get; private set; } // ліва верхня клітинка поля фігури на полі гри
        public Figure ProjectedFigure { get; private set; }
        public FigureMoveOption(Figure projectedFigure, int positionOfCurrentFigure, int score)
        {
            ProjectedFigure = projectedFigure;
            PositionOfCurrentFigure = positionOfCurrentFigure;
            Score = score;
        }
    }
}
