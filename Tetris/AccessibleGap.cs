using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class AccessibleGap : CellPosition
    {
        public enum SideAccess
        {
            Left,
            Right
        }

        public SideAccess Side {  get; }

        public AccessibleGap(int row, int column, SideAccess side) : base(row, column)
        {
            Side = side;
        }
    }
}
