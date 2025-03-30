using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class CellPosition
    {
        public int X { get; set; }
        public int Y { get; set; }

        public CellPosition(int row, int column)
        {
            X = column;
            Y = row;
        }
    }
}
