using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class ParticalGap : CellPosition
    {
        public enum SideAccess
        {
            Left,
            Right
        }

        public SideAccess Side {  get; }

        public ParticalGap(int row, int column, SideAccess side) : base(row, column)
        {
            Side = side;
        }
    }
}
