using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Tetris
{
    public class Shape
    {
        private readonly GridValue ShapeType;

        public GridValue[,] ShapeGrid { get; private set; }
        public Dir_Rotation Direction { get; private set; }

        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }

        public int[] RowsPosition { get; private set; }
        public int[] ColumnsPosition { get; private set; }

        public Shape DeepCopy()
        {
            Shape newShape = new Shape(this.ShapeType, this.Direction)
            {
                ShapeGrid = (GridValue[,])this.ShapeGrid.Clone(),
                RowCount = this.RowCount,
                ColumnCount = this.ColumnCount,
                RowsPosition = (int[])this.RowsPosition.Clone(),
                ColumnsPosition = (int[])this.ColumnsPosition.Clone()
            };
            return newShape;
        }

        public Shape(GridValue shapeType, Dir_Rotation direction)
        {
            if (shapeType == GridValue.Empty)
                throw new InvalidOperationException
                    ("Shape type cannot have the value GridValue.Empty");
            ShapeType = shapeType;
            SetShapeValue(direction);
            RowCount = this.ShapeGrid.GetLength(0);
            ColumnCount = this.ShapeGrid.GetLength(1);
            RowsPosition = new int[] { int.MinValue };
            ColumnsPosition = new int[] { int.MinValue };
        }

        public Shape(GridValue shapeType) : this(shapeType, Dir_Rotation.Up) { }

        [MemberNotNull(nameof(ShapeGrid))]
        public void SetShapeValue(Dir_Rotation dir)
        {
            this.Direction = dir;
            this.ShapeGrid = ShapeType switch
            {
                GridValue.O_Shape => set_O_ShapeValue(),
                GridValue.I_Shape => Set_I_ShapeValue(Direction),
                GridValue.T_Shape => Set_T_ShapeValue(Direction),
                GridValue.L_Shape => Set_L_ShapeValue(Direction),
                GridValue.J_Shape => Set_J_ShapeValue(Direction),
                GridValue.Z_Shape => Set_Z_ShapeValue(Direction),
                GridValue.S_Shape => Set_S_ShapeValue(Direction),
                _ => throw new InvalidOperationException($"Unsupported GridValue: {ShapeType}")
            };
        }

        public void RotateClockwise()
        {
            int enumSize = Enum.GetValues(typeof(Dir_Rotation)).Length;

            Dir_Rotation newDir = (Dir_Rotation)(((int)this.Direction == (enumSize - 1)) ? 0
                : (int)this.Direction + 1);

            SetShapeValue(newDir);
        }

        public void RotateCounterclockwise()
        {
            int enumSize = Enum.GetValues(typeof(Dir_Rotation)).Length;

            Dir_Rotation newDir = (Dir_Rotation)(((int)this.Direction == 0)
                ? (enumSize - 1) : (int)this.Direction - 1);

            SetShapeValue(newDir);
        }

        public void SetNewPositionOnGrid(int[] rowsPosition, int[] columnsPosition)
        {
            RowsPosition = rowsPosition;
            ColumnsPosition = columnsPosition;
        }

        public void MovePositionDown()
        {
            for (int i = 0; i < RowsPosition.Length; i++)
                RowsPosition[i]--;
        }

        public void MovePositionLeft()
        {
            for (int i = 0; i < ColumnsPosition.Length; i++)
                ColumnsPosition[i]--;
        }

        public void MovePositionRight()
        {
            for (int i = 0; i < ColumnsPosition.Length; i++)
                ColumnsPosition[i]++;
        }

        public int CalculateShapeHeight()
        {
            int height = 0;

            for (int r = 0; r < RowCount; r++)
            {
                if (Enumerable.Range(0, ColumnCount).Any(c =>
                    ShapeGrid[r, c] != GridValue.Empty))
                {
                    height++;
                }
            }

            return height;
        }

        public int CalculateShapeWidth()
        {
            int width = 0;

            for (int c = 0; c < ColumnCount; c++)
            {
                if (Enumerable.Range(0, RowCount).Any(r =>
                    ShapeGrid[r, c] != GridValue.Empty))
                {
                    width++;
                }
            }

            return width;
        }

        private GridValue[,] set_O_ShapeValue()
        {
            GridValue[,] shapeO = new GridValue[2, 2];
            shapeO[0, 0] = GridValue.O_Shape;
            shapeO[0, 1] = GridValue.O_Shape;
            shapeO[1, 0] = GridValue.O_Shape;
            shapeO[1, 1] = GridValue.O_Shape;

            return shapeO;
        }

        private GridValue[,] Set_I_ShapeValue(Dir_Rotation direction)
        {
            GridValue[,] shapeI = new GridValue[4, 4];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    shapeI[1, 0] = GridValue.I_Shape;
                    shapeI[1, 1] = GridValue.I_Shape;
                    shapeI[1, 2] = GridValue.I_Shape;
                    shapeI[1, 3] = GridValue.I_Shape;
                    break;
                case Dir_Rotation.Right:
                    shapeI[0, 2] = GridValue.I_Shape;
                    shapeI[1, 2] = GridValue.I_Shape;
                    shapeI[2, 2] = GridValue.I_Shape;
                    shapeI[3, 2] = GridValue.I_Shape;
                    break;
                case Dir_Rotation.Down:
                    shapeI[2, 0] = GridValue.I_Shape;
                    shapeI[2, 1] = GridValue.I_Shape;
                    shapeI[2, 2] = GridValue.I_Shape;
                    shapeI[2, 3] = GridValue.I_Shape;
                    break;
                case Dir_Rotation.Left:
                    shapeI[0, 1] = GridValue.I_Shape;
                    shapeI[1, 1] = GridValue.I_Shape;
                    shapeI[2, 1] = GridValue.I_Shape;
                    shapeI[3, 1] = GridValue.I_Shape;
                    break;
            }
            return shapeI;
        }

        private GridValue[,] Set_T_ShapeValue(Dir_Rotation direction)
        {
            GridValue[,] shapeT = new GridValue[3, 3];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    shapeT[0, 1] = GridValue.T_Shape;
                    shapeT[1, 0] = GridValue.T_Shape;
                    shapeT[1, 1] = GridValue.T_Shape;
                    shapeT[1, 2] = GridValue.T_Shape;
                    break;
                case Dir_Rotation.Right:
                    shapeT[1, 2] = GridValue.T_Shape;
                    shapeT[0, 1] = GridValue.T_Shape;
                    shapeT[1, 1] = GridValue.T_Shape;
                    shapeT[2, 1] = GridValue.T_Shape;
                    break;
                case Dir_Rotation.Down:
                    shapeT[2, 1] = GridValue.T_Shape;
                    shapeT[1, 0] = GridValue.T_Shape;
                    shapeT[1, 1] = GridValue.T_Shape;
                    shapeT[1, 2] = GridValue.T_Shape;
                    break;
                case Dir_Rotation.Left:
                    shapeT[1, 0] = GridValue.T_Shape;
                    shapeT[0, 1] = GridValue.T_Shape;
                    shapeT[1, 1] = GridValue.T_Shape;
                    shapeT[2, 1] = GridValue.T_Shape;
                    break;
            }
            return shapeT;
        }

        private GridValue[,] Set_L_ShapeValue(Dir_Rotation direction)
        {
            GridValue[,] shapeL = new GridValue[3, 3];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    shapeL[0, 2] = GridValue.L_Shape;
                    shapeL[1, 0] = GridValue.L_Shape;
                    shapeL[1, 1] = GridValue.L_Shape;
                    shapeL[1, 2] = GridValue.L_Shape;
                    break;
                case Dir_Rotation.Right:
                    shapeL[2, 2] = GridValue.L_Shape;
                    shapeL[0, 1] = GridValue.L_Shape;
                    shapeL[1, 1] = GridValue.L_Shape;
                    shapeL[2, 1] = GridValue.L_Shape;
                    break;
                case Dir_Rotation.Down:
                    shapeL[2, 0] = GridValue.L_Shape;
                    shapeL[1, 0] = GridValue.L_Shape;
                    shapeL[1, 1] = GridValue.L_Shape;
                    shapeL[1, 2] = GridValue.L_Shape;
                    break;
                case Dir_Rotation.Left:
                    shapeL[0, 0] = GridValue.L_Shape;
                    shapeL[0, 1] = GridValue.L_Shape;
                    shapeL[1, 1] = GridValue.L_Shape;
                    shapeL[2, 1] = GridValue.L_Shape;
                    break;
            }
            return shapeL;
        }

        private GridValue[,] Set_J_ShapeValue(Dir_Rotation direction)
        {
            GridValue[,] shapeJ = new GridValue[3, 3];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    shapeJ[0, 0] = GridValue.J_Shape;
                    shapeJ[1, 0] = GridValue.J_Shape;
                    shapeJ[1, 1] = GridValue.J_Shape;
                    shapeJ[1, 2] = GridValue.J_Shape;
                    break;
                case Dir_Rotation.Right:
                    shapeJ[0, 2] = GridValue.J_Shape;
                    shapeJ[0, 1] = GridValue.J_Shape;
                    shapeJ[1, 1] = GridValue.J_Shape;
                    shapeJ[2, 1] = GridValue.J_Shape;
                    break;
                case Dir_Rotation.Down:
                    shapeJ[2, 2] = GridValue.J_Shape;
                    shapeJ[1, 0] = GridValue.J_Shape;
                    shapeJ[1, 1] = GridValue.J_Shape;
                    shapeJ[1, 2] = GridValue.J_Shape;
                    break;
                case Dir_Rotation.Left:
                    shapeJ[2, 0] = GridValue.J_Shape;
                    shapeJ[0, 1] = GridValue.J_Shape;
                    shapeJ[1, 1] = GridValue.J_Shape;
                    shapeJ[2, 1] = GridValue.J_Shape;
                    break;
            }
            return shapeJ;
        }

        private GridValue[,] Set_Z_ShapeValue(Dir_Rotation direction)
        {
            GridValue[,] shapeZ = new GridValue[3, 3];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    shapeZ[0, 0] = GridValue.Z_Shape;
                    shapeZ[0, 1] = GridValue.Z_Shape;
                    shapeZ[1, 1] = GridValue.Z_Shape;
                    shapeZ[1, 2] = GridValue.Z_Shape;
                    break;
                case Dir_Rotation.Right:
                    shapeZ[0, 2] = GridValue.Z_Shape;
                    shapeZ[1, 2] = GridValue.Z_Shape;
                    shapeZ[1, 1] = GridValue.Z_Shape;
                    shapeZ[2, 1] = GridValue.Z_Shape;
                    break;
                case Dir_Rotation.Down:
                    shapeZ[1, 0] = GridValue.Z_Shape;
                    shapeZ[1, 1] = GridValue.Z_Shape;
                    shapeZ[2, 1] = GridValue.Z_Shape;
                    shapeZ[2, 2] = GridValue.Z_Shape;
                    break;
                case Dir_Rotation.Left:
                    shapeZ[0, 1] = GridValue.Z_Shape;
                    shapeZ[1, 1] = GridValue.Z_Shape;
                    shapeZ[1, 0] = GridValue.Z_Shape;
                    shapeZ[2, 0] = GridValue.Z_Shape;
                    break;
            }
            return shapeZ;
        }

        private GridValue[,] Set_S_ShapeValue(Dir_Rotation direction)
        {
            GridValue[,] shapeS = new GridValue[3, 3];
            switch (direction)
            {
                case Dir_Rotation.Up:
                    shapeS[0, 2] = GridValue.S_Shape;
                    shapeS[0, 1] = GridValue.S_Shape;
                    shapeS[1, 1] = GridValue.S_Shape;
                    shapeS[1, 0] = GridValue.S_Shape;
                    break;
                case Dir_Rotation.Right:
                    shapeS[0, 1] = GridValue.S_Shape;
                    shapeS[1, 1] = GridValue.S_Shape;
                    shapeS[1, 2] = GridValue.S_Shape;
                    shapeS[2, 2] = GridValue.S_Shape;
                    break;
                case Dir_Rotation.Down:
                    shapeS[1, 2] = GridValue.S_Shape;
                    shapeS[1, 1] = GridValue.S_Shape;
                    shapeS[2, 1] = GridValue.S_Shape;
                    shapeS[2, 0] = GridValue.S_Shape;
                    break;
                case Dir_Rotation.Left:
                    shapeS[0, 0] = GridValue.S_Shape;
                    shapeS[1, 0] = GridValue.S_Shape;
                    shapeS[1, 1] = GridValue.S_Shape;
                    shapeS[2, 1] = GridValue.S_Shape;
                    break;
            }
            return shapeS;
        }
    }
}
