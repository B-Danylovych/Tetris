using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class ShapeMoveOption
    {
        public int Score { get; private set; }
        public int PositionOfCurrentFigure { get; private set; } // ліва верхня клітинка поля фігури на полі гри
        public Shape ProjectedFigure { get; private set; }
        public ShapeMoveOption(Shape projectedFigure, int positionOfCurrentFigure, int score)
        {
            ProjectedFigure = projectedFigure;
            PositionOfCurrentFigure = positionOfCurrentFigure;
            Score = score;
        }
    }
}
