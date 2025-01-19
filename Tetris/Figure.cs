using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public class Figure
    {
        public GridValue[,] FigureValue { get; private set; }
        private readonly GridValue figureType;
        public Dir_Rotation Direction { get; private set; }

        public int[] RowIndexOnGrid, ColumnIndexOnGrid;
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }

        public Figure(GridValue figure, Dir_Rotation direction)
        {
            figureType = figure;
            SetNewDirection(direction);
            RowCount = this.FigureValue.GetLength(0);
            ColumnCount = this.FigureValue.GetLength(1);
        }

        public Figure Clone()
        {
            Figure newFigure = new Figure(this.figureType, this.Direction)
            {
                FigureValue = (GridValue[,])this.FigureValue.Clone(),
                RowCount = this.RowCount,
                ColumnCount = this.ColumnCount
            };
            return newFigure;
        }

        public void SetNewDirection(Dir_Rotation dir)
        {
            this.Direction = dir;
            this.FigureValue = figureType switch
            {
                GridValue.I_Figure => I_Figure_Direction(Direction),
                GridValue.O_Figure => O_Figure_Direction(),
                GridValue.T_Figure => T_Figure_Direction(Direction),
                GridValue.L_Figure => L_Figure_Direction(Direction),
                GridValue.J_Figure => J_Figure_Direction(Direction),
                GridValue.Z_Figure => Z_Figure_Direction(Direction),
                GridValue.S_Figure => S_Figure_Direction(Direction)
            };
        }

        private GridValue[,] O_Figure_Direction()
        {
            GridValue[,] figureO = new GridValue[2, 2];
            figureO[0, 0] = GridValue.O_Figure;
            figureO[0, 1] = GridValue.O_Figure;
            figureO[1, 0] = GridValue.O_Figure;
            figureO[1, 1] = GridValue.O_Figure;

            return figureO;
        }

        private GridValue[,] I_Figure_Direction(Dir_Rotation direction)
        {
            GridValue[,] figureI = new GridValue[4, 4];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    figureI[1, 0] = GridValue.I_Figure;
                    figureI[1, 1] = GridValue.I_Figure;
                    figureI[1, 2] = GridValue.I_Figure;
                    figureI[1, 3] = GridValue.I_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureI[0, 2] = GridValue.I_Figure;
                    figureI[1, 2] = GridValue.I_Figure;
                    figureI[2, 2] = GridValue.I_Figure;
                    figureI[3, 2] = GridValue.I_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureI[2, 0] = GridValue.I_Figure;
                    figureI[2, 1] = GridValue.I_Figure;
                    figureI[2, 2] = GridValue.I_Figure;
                    figureI[2, 3] = GridValue.I_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureI[0, 1] = GridValue.I_Figure;
                    figureI[1, 1] = GridValue.I_Figure;
                    figureI[2, 1] = GridValue.I_Figure;
                    figureI[3, 1] = GridValue.I_Figure;
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
                    figureT[0, 1] = GridValue.T_Figure;
                    figureT[1, 0] = GridValue.T_Figure;
                    figureT[1, 1] = GridValue.T_Figure;
                    figureT[1, 2] = GridValue.T_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureT[1, 2] = GridValue.T_Figure;
                    figureT[0, 1] = GridValue.T_Figure;
                    figureT[1, 1] = GridValue.T_Figure;
                    figureT[2, 1] = GridValue.T_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureT[2, 1] = GridValue.T_Figure;
                    figureT[1, 0] = GridValue.T_Figure;
                    figureT[1, 1] = GridValue.T_Figure;
                    figureT[1, 2] = GridValue.T_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureT[1, 0] = GridValue.T_Figure;
                    figureT[0, 1] = GridValue.T_Figure;
                    figureT[1, 1] = GridValue.T_Figure;
                    figureT[2, 1] = GridValue.T_Figure;
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
                    figureL[0, 2] = GridValue.L_Figure;
                    figureL[1, 0] = GridValue.L_Figure;
                    figureL[1, 1] = GridValue.L_Figure;
                    figureL[1, 2] = GridValue.L_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureL[2, 2] = GridValue.L_Figure;
                    figureL[0, 1] = GridValue.L_Figure;
                    figureL[1, 1] = GridValue.L_Figure;
                    figureL[2, 1] = GridValue.L_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureL[2, 0] = GridValue.L_Figure;
                    figureL[1, 0] = GridValue.L_Figure;
                    figureL[1, 1] = GridValue.L_Figure;
                    figureL[1, 2] = GridValue.L_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureL[0, 0] = GridValue.L_Figure;
                    figureL[0, 1] = GridValue.L_Figure;
                    figureL[1, 1] = GridValue.L_Figure;
                    figureL[2, 1] = GridValue.L_Figure;
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
                    figureJ[0, 0] = GridValue.J_Figure;
                    figureJ[1, 0] = GridValue.J_Figure;
                    figureJ[1, 1] = GridValue.J_Figure;
                    figureJ[1, 2] = GridValue.J_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureJ[0, 2] = GridValue.J_Figure;
                    figureJ[0, 1] = GridValue.J_Figure;
                    figureJ[1, 1] = GridValue.J_Figure;
                    figureJ[2, 1] = GridValue.J_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureJ[2, 2] = GridValue.J_Figure;
                    figureJ[1, 0] = GridValue.J_Figure;
                    figureJ[1, 1] = GridValue.J_Figure;
                    figureJ[1, 2] = GridValue.J_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureJ[2, 0] = GridValue.J_Figure;
                    figureJ[0, 1] = GridValue.J_Figure;
                    figureJ[1, 1] = GridValue.J_Figure;
                    figureJ[2, 1] = GridValue.J_Figure;
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
                    figureZ[0, 0] = GridValue.Z_Figure;
                    figureZ[0, 1] = GridValue.Z_Figure;
                    figureZ[1, 1] = GridValue.Z_Figure;
                    figureZ[1, 2] = GridValue.Z_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureZ[0, 2] = GridValue.Z_Figure;
                    figureZ[1, 2] = GridValue.Z_Figure;
                    figureZ[1, 1] = GridValue.Z_Figure;
                    figureZ[2, 1] = GridValue.Z_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureZ[1, 0] = GridValue.Z_Figure;
                    figureZ[1, 1] = GridValue.Z_Figure;
                    figureZ[2, 1] = GridValue.Z_Figure;
                    figureZ[2, 2] = GridValue.Z_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureZ[0, 1] = GridValue.Z_Figure;
                    figureZ[1, 1] = GridValue.Z_Figure;
                    figureZ[1, 0] = GridValue.Z_Figure;
                    figureZ[2, 0] = GridValue.Z_Figure;
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
                    figureS[0, 2] = GridValue.S_Figure;
                    figureS[0, 1] = GridValue.S_Figure;
                    figureS[1, 1] = GridValue.S_Figure;
                    figureS[1, 0] = GridValue.S_Figure;
                    break;
                case Dir_Rotation.Right:
                    figureS[0, 1] = GridValue.S_Figure;
                    figureS[1, 1] = GridValue.S_Figure;
                    figureS[1, 2] = GridValue.S_Figure;
                    figureS[2, 2] = GridValue.S_Figure;
                    break;
                case Dir_Rotation.Down:
                    figureS[1, 2] = GridValue.S_Figure;
                    figureS[1, 1] = GridValue.S_Figure;
                    figureS[2, 1] = GridValue.S_Figure;
                    figureS[2, 0] = GridValue.S_Figure;
                    break;
                case Dir_Rotation.Left:
                    figureS[0, 0] = GridValue.S_Figure;
                    figureS[1, 0] = GridValue.S_Figure;
                    figureS[1, 1] = GridValue.S_Figure;
                    figureS[2, 1] = GridValue.S_Figure;
                    break;
            }
            return figureS;
        }
    }
}
