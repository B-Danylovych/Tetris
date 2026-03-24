using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class CellPosition
    {
        public int Column { get; set; }
        public int Row { get; set; }

        public CellPosition(int row, int column)
        {
            Column = column;
            Row = row;
        }
    }
}
