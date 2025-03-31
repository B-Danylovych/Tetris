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
        public Action[] Actions { get; }

        public ShapeMoveOption(Shape projectedFigure, int score, Action[] actions)
        {
            ProjectedFigure = projectedFigure;
            Score = score;
            Actions = actions;
        }
    }
}
