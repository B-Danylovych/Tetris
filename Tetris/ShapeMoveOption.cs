using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class ShapeMoveOption
    {
        public Shape ProjectedShape { get; }
        public int Score { get; }
        public Action[] Actions { get; }

        public ShapeMoveOption(Shape projectedShape, int score, Action[] actions)
        {
            ProjectedShape = projectedShape;
            Score = score;
            Actions = actions;
        }
    }
}
