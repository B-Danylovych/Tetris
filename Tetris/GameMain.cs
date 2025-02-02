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
        public int Columns { get; }
        private readonly int HiddenRowOnTop = 2;

        public List<List<GridValue>> Grid { get; protected set; }

        private int _lockDelayTick = 500;
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

        private int _iterationTick = 500;
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

        public GameMain(int rows, int columns)
        {
            Rows = rows + HiddenRowOnTop;
            Columns = columns;
            Grid = Enumerable.Range(0, Rows)
                .Select(i => Enumerable.Repeat(GridValue.Empty, Columns).ToList()).ToList();

            SetBufferShape();
        }

        public void SetBufferShape()
        {
            GridValue shapeType = GetRandomShapeValue();
            BufferShape = new Shape(shapeType, Dir_Rotation.Up);
        }

        private GridValue GetRandomShapeValue()
        {
            Array gridValues = Enum.GetValues(typeof(GridValue));
            GridValue randomValue;

            do
            {
                object? gridValue = gridValues.GetValue(random.Next(gridValues.Length));
                if (gridValue == null)
                    throw new InvalidOperationException("Unexpected null value in enum.");

                randomValue = (GridValue)gridValue;
            }
            while (randomValue == GridValue.Empty);

            return randomValue;
        }

        public void SetCurrentShape()
        {
            CurrentShape = BufferShape.DeepCopy();

            int shapeWidth = CurrentShape.ColumnCount;
            int shapeHeight = CurrentShape.RowCount;

            int[] rowsPosition = Enumerable.Range(0, shapeHeight)
                .Select(i => this.Rows - 1 - i).ToArray();
            int[] columsPosition = Enumerable.Range(0, shapeWidth)
                .Select(i => (this.Columns / 2 - shapeWidth / 2) + i).ToArray();

            CurrentShape.SetNewPositionOnGrid(rowsPosition, columsPosition);

            ProjectedShape = GetProjectedDownShape(CurrentShape);
        }

        private Shape GetProjectedDownShape(Shape curShape)
        {
            int highestProjTile = GetHighestProjectedTileRow(curShape);

            Shape projectedShape = curShape.DeepCopy();

            int[] projRowsPosition = Enumerable.Range(0, projectedShape.RowCount)
                .Select(i => highestProjTile - i).ToArray();

            projectedShape.SetNewPositionOnGrid(projRowsPosition, projectedShape.ColumnsPosition);

            return projectedShape;
        }

        private int GetHighestProjectedTileRow(Shape curShape)
        {
            List<int> projTilesFall = new();

            int shapeWidth = curShape.ColumnCount;
            int shapeHeight = curShape.RowCount;

            for (int c = 0; c < shapeWidth; c++)
            {
                for (int r = shapeHeight - 1; r >= 0; r--)
                {
                    if (curShape.ShapeValue[r, c] != GridValue.Empty)
                    {
                        projTilesFall.Add(ProjectTileDown
                            (curShape.RowsPosition[r], curShape.ColumnsPosition[c]) + r);
                        break;
                    }
                }
            }

            return projTilesFall.Max();
        }

        private int ProjectTileDown(int rowPosOnGrid, int columnPosOnGrid)
        {
            for (int i = rowPosOnGrid; i >= 0; i--)
            {
                if (i == 0 || Grid[i - 1][columnPosOnGrid] != GridValue.Empty)
                {
                    return i;
                }
            }
            throw new InvalidOperationException("The loop did not return a value.");
        }

        public void SetShapeOnGrid()
        {
            for (int r = 0; r < CurrentShape.RowCount; r++)
            {
                for (int c = 0; c < CurrentShape.ColumnCount; c++)
                {
                    if (CurrentShape.ShapeValue[r, c] != GridValue.Empty)
                    {
                        int rowOnGrid = CurrentShape.RowsPosition[r];
                        int columnOnGrid = CurrentShape.ColumnsPosition[c];

                        Grid[rowOnGrid][columnOnGrid] = CurrentShape.ShapeValue[r, c];
                    }
                }
            }
        }

        public void RemoveLines()
        {
            int linesRemoved = 0;
            for (int r = 0; r < Grid.Count - HiddenRowOnTop; r++)
            {
                if (isLineFullCheck(r))
                {
                    Grid.RemoveAt(r);
                    Grid.Add(Enumerable.Repeat(GridValue.Empty, Columns).ToList());
                    linesRemoved++;
                    r--;
                }
            }
            SetNewScore(linesRemoved);
        }

        private bool isLineFullCheck(int row)
        {
            for (int c = 0; c < Grid[row].Count; c++)
                if (Grid[row][c] == GridValue.Empty)
                    return false;

            return true;
        }

        private void SetNewScore(int linesRemoved)
        {
            // 10*10 = 100, 20*20=400, 30*30 = 900, 40*40+400=1600+400=2000
            int addScore = (int)Math.Pow(linesRemoved * 10, 2);
            addScore += (linesRemoved < 4) ? 0 : 400;
            this.ScoreNum += addScore;
            this.LinesNum += linesRemoved;
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
                        int rowOnGrid = curShape.RowsPosition[r];
                        int columnOnGrid = curShape.ColumnsPosition[c];

                        if (columnOnGrid == 0 || 
                            Grid[rowOnGrid][columnOnGrid - 1] != GridValue.Empty)
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

            ProjectedShape = GetProjectedDownShape(CurrentShape);

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
                        int rowOnGrid = curShape.RowsPosition[r];
                        int columnOnGrid = curShape.ColumnsPosition[c];

                        if (columnOnGrid == Columns - 1 || 
                            Grid[rowOnGrid][columnOnGrid + 1] != GridValue.Empty)
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

            ProjectedShape = GetProjectedDownShape(CurrentShape);
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
                            rotationShape.ColumnsPosition[c] < 0 || rotationShape.ColumnsPosition[c] >= Columns ||
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

            ProjectedShape = GetProjectedDownShape(CurrentShape);
        }

        public void checkGameOver()
        {
            for (int c = 0; c < Columns; c++)
            {
                if (Grid[Rows - HiddenRowOnTop][c] != GridValue.Empty)
                    IsGameOver = true;
            }
        }
    }
}
