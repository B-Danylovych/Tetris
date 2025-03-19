using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    struct TilePosition
    {
        public int X { get; set; }
        public int Y { get; set; }

        public TilePosition(int row, int column)
        {
            X = column;
            Y = row;
        }
    }
}
