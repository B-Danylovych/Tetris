using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class Shape
    {
        public GridValue[,] ShapeValue { get; private set; }
        private readonly GridValue Type;
        public Dir_Rotation Direction { get; private set; }

        public int[] RowIndexOnGrid { get; set; }
        public int[] ColumnIndexOnGrid { get; set; }
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }

        public Shape(GridValue figure, Dir_Rotation direction)
        {
            Type = figure;
            SetNewDirection(direction);
            RowCount = this.ShapeValue.GetLength(0);
            ColumnCount = this.ShapeValue.GetLength(1);
        }

        public Shape Clone()
        {
            Shape newFigure = new Shape(this.Type, this.Direction)
            {
                ShapeValue = (GridValue[,])this.ShapeValue.Clone(),
                RowCount = this.RowCount,
                ColumnCount = this.ColumnCount
            };
            return newFigure;
        }

        public void SetNewDirection(Dir_Rotation dir)
        {
            this.Direction = dir;
            this.ShapeValue = Type switch
            {
                Tetris.GridValue.I_Figure => I_Figure_Direction(Direction),
                Tetris.GridValue.O_Figure => O_Figure_Direction(),
                Tetris.GridValue.T_Figure => T_Figure_Direction(Direction),
                Tetris.GridValue.L_Figure => L_Figure_Direction(Direction),
                Tetris.GridValue.J_Figure => J_Figure_Direction(Direction),
                Tetris.GridValue.Z_Figure => Z_Figure_Direction(Direction),
                Tetris.GridValue.S_Figure => S_Figure_Direction(Direction)
            };
        }

        private GridValue[,] O_Figure_Direction()
        {
            GridValue[,] figureO = new GridValue[2, 2];
            figureO[0, 0] = Tetris.GridValue.O_Figure;
            figureO[0, 1] = Tetris.GridValue.O_Figure;
            figureO[1, 0] = Tetris.GridValue.O_Figure;
            figureO[1, 1] = Tetris.GridValue.O_Figure;

            return figureO;
        }

        private GridValue[,] I_Figure_Direction(Dir_Rotation direction)
        {
            GridValue[,] figureI = new GridValue[4, 4];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    figureI[1, 0] = Tetris.GridValue.I_Figure;
                    figureI[1, 1] = Tetris.GridValue.I_Figure;
                    figureI[1, 2] = Tetris.GridValue.I_Figure;
                    figureI[1, 3] = Tetris.GridValue.I_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureI[0, 2] = Tetris.GridValue.I_Figure;
                    figureI[1, 2] = Tetris.GridValue.I_Figure;
                    figureI[2, 2] = Tetris.GridValue.I_Figure;
                    figureI[3, 2] = Tetris.GridValue.I_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureI[2, 0] = Tetris.GridValue.I_Figure;
                    figureI[2, 1] = Tetris.GridValue.I_Figure;
                    figureI[2, 2] = Tetris.GridValue.I_Figure;
                    figureI[2, 3] = Tetris.GridValue.I_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureI[0, 1] = Tetris.GridValue.I_Figure;
                    figureI[1, 1] = Tetris.GridValue.I_Figure;
                    figureI[2, 1] = Tetris.GridValue.I_Figure;
                    figureI[3, 1] = Tetris.GridValue.I_Figure;
                    break;
            }
            return figureI;
        }

        private GridValue[,] T_Figure_Direction(Dir_Rotation direction)
        {
            GridValue[,] figureT = new GridValue[3, 3];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    figureT[0, 1] = Tetris.GridValue.T_Figure;
                    figureT[1, 0] = Tetris.GridValue.T_Figure;
                    figureT[1, 1] = Tetris.GridValue.T_Figure;
                    figureT[1, 2] = Tetris.GridValue.T_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureT[1, 2] = Tetris.GridValue.T_Figure;
                    figureT[0, 1] = Tetris.GridValue.T_Figure;
                    figureT[1, 1] = Tetris.GridValue.T_Figure;
                    figureT[2, 1] = Tetris.GridValue.T_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureT[2, 1] = Tetris.GridValue.T_Figure;
                    figureT[1, 0] = Tetris.GridValue.T_Figure;
                    figureT[1, 1] = Tetris.GridValue.T_Figure;
                    figureT[1, 2] = Tetris.GridValue.T_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureT[1, 0] = Tetris.GridValue.T_Figure;
                    figureT[0, 1] = Tetris.GridValue.T_Figure;
                    figureT[1, 1] = Tetris.GridValue.T_Figure;
                    figureT[2, 1] = Tetris.GridValue.T_Figure;
                    break;
            }
            return figureT;
        }

        private GridValue[,] L_Figure_Direction(Dir_Rotation direction)
        {
            GridValue[,] figureL = new GridValue[3, 3];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    figureL[0, 2] = Tetris.GridValue.L_Figure;
                    figureL[1, 0] = Tetris.GridValue.L_Figure;
                    figureL[1, 1] = Tetris.GridValue.L_Figure;
                    figureL[1, 2] = Tetris.GridValue.L_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureL[2, 2] = Tetris.GridValue.L_Figure;
                    figureL[0, 1] = Tetris.GridValue.L_Figure;
                    figureL[1, 1] = Tetris.GridValue.L_Figure;
                    figureL[2, 1] = Tetris.GridValue.L_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureL[2, 0] = Tetris.GridValue.L_Figure;
                    figureL[1, 0] = Tetris.GridValue.L_Figure;
                    figureL[1, 1] = Tetris.GridValue.L_Figure;
                    figureL[1, 2] = Tetris.GridValue.L_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureL[0, 0] = Tetris.GridValue.L_Figure;
                    figureL[0, 1] = Tetris.GridValue.L_Figure;
                    figureL[1, 1] = Tetris.GridValue.L_Figure;
                    figureL[2, 1] = Tetris.GridValue.L_Figure;
                    break;
            }
            return figureL;
        }

        private GridValue[,] J_Figure_Direction(Dir_Rotation direction)
        {
            GridValue[,] figureJ = new GridValue[3, 3];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    figureJ[0, 0] = Tetris.GridValue.J_Figure;
                    figureJ[1, 0] = Tetris.GridValue.J_Figure;
                    figureJ[1, 1] = Tetris.GridValue.J_Figure;
                    figureJ[1, 2] = Tetris.GridValue.J_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureJ[0, 2] = Tetris.GridValue.J_Figure;
                    figureJ[0, 1] = Tetris.GridValue.J_Figure;
                    figureJ[1, 1] = Tetris.GridValue.J_Figure;
                    figureJ[2, 1] = Tetris.GridValue.J_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureJ[2, 2] = Tetris.GridValue.J_Figure;
                    figureJ[1, 0] = Tetris.GridValue.J_Figure;
                    figureJ[1, 1] = Tetris.GridValue.J_Figure;
                    figureJ[1, 2] = Tetris.GridValue.J_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureJ[2, 0] = Tetris.GridValue.J_Figure;
                    figureJ[0, 1] = Tetris.GridValue.J_Figure;
                    figureJ[1, 1] = Tetris.GridValue.J_Figure;
                    figureJ[2, 1] = Tetris.GridValue.J_Figure;
                    break;
            }
            return figureJ;
        }

        private GridValue[,] Z_Figure_Direction(Dir_Rotation direction)
        {
            GridValue[,] figureZ = new GridValue[3, 3];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    figureZ[0, 0] = Tetris.GridValue.Z_Figure;
                    figureZ[0, 1] = Tetris.GridValue.Z_Figure;
                    figureZ[1, 1] = Tetris.GridValue.Z_Figure;
                    figureZ[1, 2] = Tetris.GridValue.Z_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureZ[0, 2] = Tetris.GridValue.Z_Figure;
                    figureZ[1, 2] = Tetris.GridValue.Z_Figure;
                    figureZ[1, 1] = Tetris.GridValue.Z_Figure;
                    figureZ[2, 1] = Tetris.GridValue.Z_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureZ[1, 0] = Tetris.GridValue.Z_Figure;
                    figureZ[1, 1] = Tetris.GridValue.Z_Figure;
                    figureZ[2, 1] = Tetris.GridValue.Z_Figure;
                    figureZ[2, 2] = Tetris.GridValue.Z_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureZ[0, 1] = Tetris.GridValue.Z_Figure;
                    figureZ[1, 1] = Tetris.GridValue.Z_Figure;
                    figureZ[1, 0] = Tetris.GridValue.Z_Figure;
                    figureZ[2, 0] = Tetris.GridValue.Z_Figure;
                    break;
            }
            return figureZ;
        }

        private GridValue[,] S_Figure_Direction(Dir_Rotation direction)
        {
            GridValue[,] figureS = new GridValue[3, 3];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    figureS[0, 2] = Tetris.GridValue.S_Figure;
                    figureS[0, 1] = Tetris.GridValue.S_Figure;
                    figureS[1, 1] = Tetris.GridValue.S_Figure;
                    figureS[1, 0] = Tetris.GridValue.S_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureS[0, 1] = Tetris.GridValue.S_Figure;
                    figureS[1, 1] = Tetris.GridValue.S_Figure;
                    figureS[1, 2] = Tetris.GridValue.S_Figure;
                    figureS[2, 2] = Tetris.GridValue.S_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureS[1, 2] = Tetris.GridValue.S_Figure;
                    figureS[1, 1] = Tetris.GridValue.S_Figure;
                    figureS[2, 1] = Tetris.GridValue.S_Figure;
                    figureS[2, 0] = Tetris.GridValue.S_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureS[0, 0] = Tetris.GridValue.S_Figure;
                    figureS[1, 0] = Tetris.GridValue.S_Figure;
                    figureS[1, 1] = Tetris.GridValue.S_Figure;
                    figureS[2, 1] = Tetris.GridValue.S_Figure;
                    break;
            }
            return figureS;
        }
    }
}
