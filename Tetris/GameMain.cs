using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tetris
{

    public class GameMain
    {
        private int HiddenRowsOnTop { get; }
        public int Rows { get; }
        public int Columns { get; }

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

        public GameMain(int rows, int hiddenRowsOnTop, int columns)
        {
            HiddenRowsOnTop = hiddenRowsOnTop;
            Rows = rows + HiddenRowsOnTop;
            Columns = columns;
            Grid = Enumerable.Range(0, Rows)
                .Select(i => Enumerable.Repeat(GridValue.Empty, Columns).ToList()).ToList();

            SetBufferShape();

            // для того щоб позбавитись від "non-null value" попереджень написав цей код:
            CurrentShape = BufferShape.DeepCopy();
            ProjectedShape = CurrentShape.DeepCopy();
        }

        public void SetShapes()
        {
            SetCurrentShape();
            SetProjectedShape();
            SetBufferShape();
        }

        [MemberNotNull(nameof(BufferShape))]
        public void SetBufferShape() => 
            BufferShape = new Shape(GetRandomShapeType());

        private GridValue GetRandomShapeType()
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
        }

        public void SetProjectedShape() => 
            ProjectedShape = GetProjectedDownShape(CurrentShape);

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
                    if (curShape.ShapeGrid[r, c] != GridValue.Empty)
                    {
                        projTilesFall.Add(ProjectTileDown
                            (curShape.RowsPosition[r], curShape.ColumnsPosition[c]) 
                            + r);
                        // добавлячи r отримуємо RowPosition[0] індекс
                        // відносно проектованої плитки
                        break;
                    }
                }
            }

            return projTilesFall.Max();
        }

        private int ProjectTileDown(int rowPosOnGrid, int columnPosOnGrid)
        {
            for (int i = rowPosOnGrid; i >= 0; i--)
                if (i == 0 || Grid[i - 1][columnPosOnGrid] != GridValue.Empty)
                    return i;

            throw new InvalidOperationException("The loop did not return a value.");
        }

        public void SetShapeOnGrid()
        {
            for (int r = 0; r < CurrentShape.RowCount; r++)
            {
                for (int c = 0; c < CurrentShape.ColumnCount; c++)
                {
                    if (CurrentShape.ShapeGrid[r, c] != GridValue.Empty)
                    {
                        int rowOnGrid = CurrentShape.RowsPosition[r];
                        int columnOnGrid = CurrentShape.ColumnsPosition[c];

                        Grid[rowOnGrid][columnOnGrid] = CurrentShape.ShapeGrid[r, c];
                    }
                }
            }
        }

        public void RemoveLines()
        {
            int linesRemoved = 0;
            for (int r = 0; r < Grid.Count - HiddenRowsOnTop; r++)
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
            // 10*10 = 100, 20*20=400, 30*30 = 900, 40*40+400=2000
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

        public bool MoveLeft()
        {
            if (!CanMoveLeft(CurrentShape))
                return false;

            CurrentShape.MovePositionLeft();

            return true;
        }

        private bool CanMoveLeft(Shape curShape)
        {
            int shapeWidth = curShape.ColumnCount;
            int shapeHeight = curShape.RowCount;

            for (int r = 0; r < shapeHeight; r++)
            {
                for (int c = 0; c < shapeWidth; c++)
                {
                    if (curShape.ShapeGrid[r, c] != GridValue.Empty)
                    {
                        if (!CanTileMoveLeft(curShape.RowsPosition[r], curShape.ColumnsPosition[c]))
                            return false;

                        break;
                    }
                }
            }
            return true;
        }

        private bool CanTileMoveLeft(int rowPosOnGrid, int columnPosOnGrid)
        {
            if (columnPosOnGrid == 0 ||
                    Grid[rowPosOnGrid][columnPosOnGrid - 1] != GridValue.Empty)
            {
                return false;
            }

            return true;
        }

        public bool MoveRight()
        {
            if (!CanShapeMoveRight(CurrentShape))
                return false;

            CurrentShape.MovePositionRight();

            return true;
        }

        private bool CanShapeMoveRight(Shape curShape)
        {
            int shaprWidth = curShape.ColumnCount;
            int shapeHeight = curShape.RowCount;

            for (int r = 0; r < shapeHeight; r++)
            {
                for (int c = shaprWidth - 1; c >= 0; c--)
                {
                    if (curShape.ShapeGrid[r, c] != GridValue.Empty)
                    {
                        if (!CanTileMoveRight(curShape.RowsPosition[r], curShape.ColumnsPosition[c]))
                            return false;
                        
                        break;
                    }
                }
            }
            return true;
        }

        private bool CanTileMoveRight(int rowPosOnGrid, int columnPosOnGrid)
        {
            if (columnPosOnGrid == Columns - 1 ||
                    Grid[rowPosOnGrid][columnPosOnGrid + 1] != GridValue.Empty)
            {
                return false;
            }

            return true;
        }

        public enum DirectionOfRotation
        {
            isClockwise,
            isCounterclockwise
        }

        public void Rotate(DirectionOfRotation direction)
        {
            if (!CanRotate(CurrentShape, direction))
                return;

            if (direction == DirectionOfRotation.isClockwise)
                CurrentShape.RotateClockwise();
            else
                CurrentShape.RotateCounterclockwise();
        }

        private bool CanRotate(Shape curShape, DirectionOfRotation direction)
        {
            Shape rotationShape = curShape.DeepCopy();

            if (direction == DirectionOfRotation.isClockwise)
                rotationShape.RotateClockwise();
            else
                rotationShape.RotateCounterclockwise();

            int shapeWidth = rotationShape.ColumnCount;
            int shapeHeight = rotationShape.RowCount;

            for (int r = 0; r < shapeHeight; r++)
            {
                for (int c = 0; c < shapeWidth; c++)
                {
                    if (rotationShape.ShapeGrid[r, c] != GridValue.Empty)
                    {
                        if (!IsTileOnEmptyGridValue(rotationShape.RowsPosition[r],
                            rotationShape.ColumnsPosition[c]))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool IsTileOnEmptyGridValue(int rowPosOnGrid, int columnPosOnGrid)
        {
            if (rowPosOnGrid < 0 || columnPosOnGrid < 0 || columnPosOnGrid >= Columns
                || Grid[rowPosOnGrid][columnPosOnGrid] != GridValue.Empty)
            {
                return false;
            }
            else
                return true;
        }

        public void CheckGameOver()
        {
            for (int c = 0; c < Columns; c++)
            {
                if (Grid[Rows - HiddenRowsOnTop - 1][c] != GridValue.Empty)
                    IsGameOver = true;
            }
        }
    }
}
