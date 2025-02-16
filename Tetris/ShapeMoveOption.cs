using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class ShapeMoveOption
    {
        public Shape ProjectedFigure { get; }
        public int Score { get; }

        public ShapeMoveOption(Shape projectedFigure, int score)
        {
            ProjectedFigure = projectedFigure;
            Score = score;
        }
    }
}
