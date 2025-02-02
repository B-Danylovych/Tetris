using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tetris
{

    public class GameMain
    {
        public int Rows { get; }
        public int Cols { get; }
        private readonly int HiddenRowOnTop = 2;

        public List<List<GridValue>> Grid { get; protected set; }

        private int _lockDelayTick = 500;
        private int _iterationTick = 500;

        public int LockDelayTick
        {
            get => _lockDelayTick;
            set
            {
                if (value > 0)
                    _lockDelayTick = value;
                else
                    throw new ArgumentException("LockDelayTick must be a positive number");
            }
        }

        public int IterationTick
        {
            get => _iterationTick;
            set
            {
                if (value > 0)
                    _iterationTick = value;
                else
                    throw new ArgumentException("IterationTick must be a positive number");
            }
        }

        public int ScoreNum { get; private set; }
        public int LinesNum { get; private set; }

        public bool IsGameOver { get; private set; }

        private readonly Random random = new Random();

        public Shape BufferShape { get; protected set; }
        public Shape CurrentShape { get; protected set; }
        public Shape ProjectedShape { get; protected set; }

        public GameMain(int rows, int cols)
        {
            Rows = rows + HiddenRowOnTop;
            Cols = cols;
            Grid = Enumerable.Range(0, Rows).Select(i => Enumerable.Repeat(GridValue.Empty, Cols).ToList()).ToList();

            AddShapeInBuffer();
        }

        private void AddShapeInBuffer()
        {
            GridValue typeShape = GetRandomGridValue();
            BufferShape = new Shape(typeShape, Dir_Rotation.Up);
        }

        public GridValue GetRandomGridValue()
        {
            Array values = Enum.GetValues(typeof(GridValue));
            GridValue randomValue;

            do
            {
                randomValue = (GridValue)values.GetValue(this.random.Next(values.Length));
            } while (randomValue == GridValue.Empty);

            return randomValue;
        }

        public void AddShape()
        {
            CurrentShape = BufferShape.DeepCopy();

            int shapeWidth = CurrentShape.ColumnCount;
            int shapeHeight = CurrentShape.RowCount;

            int[] rowsPosition = Enumerable.Range(0, shapeHeight)
                .Select(i => this.Rows - 1 - i).ToArray();
            int[] columsPosition = Enumerable.Range(0, shapeWidth)
                .Select(i => (this.Cols / 2 - shapeWidth / 2) + i).ToArray();

            CurrentShape.SetNewPositionOnGrid(rowsPosition, columsPosition);

            ProjectedShape = CheckFallDown(CurrentShape);
            AddShapeInBuffer();
        }

        private Shape CheckFallDown(Shape curShape)
        {
            List<int> projFall = new();
            int highestProjectionOfTiles;
            Shape projectedShape = curShape.DeepCopy();

            int shapeWidth = curShape.ColumnCount;
            int shaprHeight = curShape.RowCount;

            for (int c = 0; c < shapeWidth; c++)
            {
                for (int r = shaprHeight - 1; r >= 0; r--)
                {
                    if (curShape.ShapeValue[r, c] != GridValue.Empty)
                    {
                        for (int i = curShape.RowsPosition[r]; i >= 0; i--)
                        {
                            if (i == 0 || Grid[i - 1][curShape.ColumnsPosition[c]] != GridValue.Empty)
                            {
                                projFall.Add(i + r);
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            highestProjectionOfTiles = projFall.Max();

            int[] projRowsPosition = Enumerable.Range(0, shaprHeight)
                .Select(i => highestProjectionOfTiles - i).ToArray();

            projectedShape.SetNewPositionOnGrid(projRowsPosition, projectedShape.ColumnsPosition);

            return projectedShape;
        }

        public bool MoveDown()
        {
            if (CurrentShape.RowsPosition[0] > ProjectedShape.RowsPosition[0])
            {
                CurrentShape.MovePositionDown();
                return true;
            }
            else
                return false;
        }

        public void AddShapeTilesOnGrid()
        {
            for (int r = 0; r < CurrentShape.RowCount; r++)
            {
                for (int c = 0; c < CurrentShape.ColumnCount; c++)
                {
                    if (CurrentShape.ShapeValue[r, c] != GridValue.Empty)
                    {
                        Grid[CurrentShape.RowsPosition[r]][CurrentShape.ColumnsPosition[c]] = CurrentShape.ShapeValue[r, c];
                    }
                }
            }
        }

        public void RemoveLines()
        {
            int linesRemoved = 0;
            for (int r = 0; r < Grid.Count - HiddenRowOnTop; r++)
            {
                bool isFull = true;
                for (int c = 0; c < Grid[r].Count; c++)
                {
                    if (Grid[r][c] == GridValue.Empty)
                    {
                        isFull = false;
                        break;
                    }
                }
                if (isFull)
                {
                    Grid.RemoveAt(r);
                    Grid.Add(Enumerable.Repeat(GridValue.Empty, Cols).ToList());
                    linesRemoved++;
                    r--;
                }
            }
            // 10*10 = 100, 20*20=400, 30*30 = 900, 40*40+400=1600+400=2000
            int addScore = (linesRemoved < 4) ? (int)Math.Pow(linesRemoved * 10, 2) : (int)Math.Pow(linesRemoved * 10, 2) + 400;
            this.ScoreNum += addScore;
            this.LinesNum += linesRemoved;
        }

        private bool CanMoveLeft(Shape curShape)
        {
            int shapeWidth = curShape.ColumnCount;
            int shapeHeight = curShape.RowCount;

            for (int r = 0; r < shapeHeight; r++)
            {
                for (int c = 0; c < shapeWidth; c++)
                {
                    if (curShape.ShapeValue[r, c] != GridValue.Empty)
                    {
                        if (curShape.ColumnsPosition[c] == 0 || 
                            Grid[curShape.RowsPosition[r]][curShape.ColumnsPosition[c] - 1] != GridValue.Empty)
                        {
                            return false;
                        }
                        break;
                    }
                }
            }
            return true;
        }

        public bool MoveLeft()
        {
            if (!CanMoveLeft(CurrentShape))
                return false;

            CurrentShape.MovePositionLeft();

            ProjectedShape = CheckFallDown(CurrentShape);
            return true;
        }

        private bool CanMoveRight(Shape curShape)
        {
            int shaprWidth = curShape.ColumnCount;
            int shapeHeight = curShape.RowCount;

            for (int r = 0; r < shapeHeight; r++)
            {
                for (int c = shaprWidth - 1; c >= 0; c--)
                {
                    if (curShape.ShapeValue[r, c] != GridValue.Empty)
                    {
                        if (curShape.ColumnsPosition[c] == Cols - 1 || 
                            Grid[curShape.RowsPosition[r]][curShape.ColumnsPosition[c] + 1] != GridValue.Empty)
                        {
                            return false;
                        }
                        break;
                    }
                }
            }
            return true;
        }

        public bool MoveRight()
        {
            if (!CanMoveRight(CurrentShape))
                return false;

            CurrentShape.MovePositionRight();

            ProjectedShape = CheckFallDown(CurrentShape);
            return true;
        }

        private bool CanRotate(bool isClockwise, Shape curShape)
        {
            Shape rotationShape = curShape.DeepCopy();

            if (isClockwise)
                rotationShape.RotateClockwise();
            else
                rotationShape.RotateCounterclockwise();

            int shapeWidth = rotationShape.ColumnCount;
            int shapeHeight = rotationShape.RowCount;

            for (int r = 0; r < shapeHeight; r++)
            {
                for (int c = 0; c < shapeWidth; c++)
                {
                    if (rotationShape.ShapeValue[r, c] != GridValue.Empty)
                    {
                        if (rotationShape.RowsPosition[r] < 0 ||
                            rotationShape.ColumnsPosition[c] < 0 || rotationShape.ColumnsPosition[c] >= Cols ||
                            Grid[rotationShape.RowsPosition[r]][rotationShape.ColumnsPosition[c]] != GridValue.Empty)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void Rotate(bool isClockwise)
        {
            if (!CanRotate(isClockwise, CurrentShape))
                return;

            if(isClockwise)
                CurrentShape.RotateClockwise();
            else
                CurrentShape.RotateCounterclockwise();

            ProjectedShape = CheckFallDown(CurrentShape);
        }

        public void checkGameOver()
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Grid[Rows - HiddenRowOnTop][c] != GridValue.Empty)
                    IsGameOver = true;
            }
        }
    }
}
