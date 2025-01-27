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

        public Figure BufferFigure { get; protected set; }
        public Figure CurrentFigure { get; protected set; }
        public Figure ProjectedFigure { get; protected set; }

        public GameMain(int rows, int cols)
        {
            Rows = rows + HiddenRowOnTop;
            Cols = cols;
            Grid = Enumerable.Range(0, Rows).Select(i => Enumerable.Repeat(GridValue.Empty, Cols).ToList()).ToList();

            AddFigureInBuffer();
        }

        private void AddFigureInBuffer()
        {
            GridValue typeFigure = GetRandomGridValue();
            BufferFigure = new Figure(typeFigure, Dir_Rotation.Up);
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

        public void AddFigure()
        {
            CurrentFigure = BufferFigure.Clone();

            int figureWidth = CurrentFigure.ColumnCount;
            int figureHeight = CurrentFigure.RowCount;

            CurrentFigure.RowIndexOnGrid = Enumerable.Range(0, figureHeight)
                .Select(i => this.Rows - 1 - i).ToArray();
            CurrentFigure.ColumnIndexOnGrid = Enumerable.Range(0, figureWidth)
                .Select(i => (this.Cols / 2 - figureWidth / 2) + i).ToArray();

            ProjectedFigure = CheckFallDown(CurrentFigure);
            AddFigureInBuffer();
        }

        private Figure CheckFallDown(Figure curFigure)
        {
            List<int> projFall = new();
            int highestProjectionOfTiles;
            Figure projectedFigure = curFigure.Clone();

            int figureWidth = curFigure.ColumnCount;
            int figureHeight = curFigure.RowCount;

            for (int c = 0; c < figureWidth; c++)
            {
                for (int r = figureHeight - 1; r >= 0; r--)
                {
                    if (curFigure.FigureValue[r, c] != GridValue.Empty)
                    {
                        for (int i = curFigure.RowIndexOnGrid[r]; i >= 0; i--)
                        {
                            if (i == 0 || Grid[i - 1][curFigure.ColumnIndexOnGrid[c]] != GridValue.Empty)
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

            projectedFigure.RowIndexOnGrid = Enumerable.Range(0, figureHeight).Select(i => highestProjectionOfTiles - i).ToArray();
            projectedFigure.ColumnIndexOnGrid = (int[])curFigure.ColumnIndexOnGrid.Clone();

            return projectedFigure;
        }

        public bool MoveDown()
        {
            if (CurrentFigure.RowIndexOnGrid[0] > ProjectedFigure.RowIndexOnGrid[0])
            {
                for (int r = 0; r < CurrentFigure.RowCount; r++)
                {
                    CurrentFigure.RowIndexOnGrid[r]--;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddFigureTilesOnGrid()
        {
            for (int r = 0; r < CurrentFigure.RowCount; r++)
            {
                for (int c = 0; c < CurrentFigure.ColumnCount; c++)
                {
                    if (CurrentFigure.FigureValue[r, c] != GridValue.Empty)
                    {
                        Grid[CurrentFigure.RowIndexOnGrid[r]][CurrentFigure.ColumnIndexOnGrid[c]] = CurrentFigure.FigureValue[r, c];
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

        private bool CanMoveLeft(Figure curFigure)
        {
            int figureWidth = curFigure.ColumnCount;
            int figureHeight = curFigure.RowCount;

            for (int r = 0; r < figureHeight; r++)
            {
                for (int c = 0; c < figureWidth; c++)
                {
                    if (curFigure.FigureValue[r, c] != GridValue.Empty)
                    {
                        if (curFigure.ColumnIndexOnGrid[c] == 0 || 
                            Grid[curFigure.RowIndexOnGrid[r]][curFigure.ColumnIndexOnGrid[c] - 1] != GridValue.Empty)
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
            if (!CanMoveLeft(CurrentFigure))
                return false;

            for (int c = 0; c < CurrentFigure.ColumnCount; c++)
            {
                CurrentFigure.ColumnIndexOnGrid[c]--;
            }

            ProjectedFigure = CheckFallDown(CurrentFigure);
            return true;
        }

        private bool CanMoveRight(Figure curFigure)
        {
            int figureWidth = curFigure.ColumnCount;
            int figureHeight = curFigure.RowCount;

            for (int r = 0; r < figureHeight; r++)
            {
                for (int c = figureWidth - 1; c >= 0; c--)
                {
                    if (curFigure.FigureValue[r, c] != GridValue.Empty)
                    {
                        if (curFigure.ColumnIndexOnGrid[c] == Cols - 1 || 
                            Grid[curFigure.RowIndexOnGrid[r]][curFigure.ColumnIndexOnGrid[c] + 1] != GridValue.Empty)
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
            if (!CanMoveRight(CurrentFigure))
                return false;

            for (int c = 0; c < CurrentFigure.ColumnCount; c++)
            {
                CurrentFigure.ColumnIndexOnGrid[c]++;
            }

            ProjectedFigure = CheckFallDown(CurrentFigure);
            return true;
        }


        private Dir_Rotation GetNewDirection(Dir_Rotation currentDirection, bool isClockwise)
        {
            int enumSize = Enum.GetValues(typeof(Dir_Rotation)).Length;

            if (isClockwise)
                return (Dir_Rotation)(((int)currentDirection == (enumSize - 1)) ? 0 : (int)currentDirection + 1);
            else
                return (Dir_Rotation)(((int)currentDirection == 0) ? (enumSize - 1) : (int)currentDirection - 1);
        }

        private bool CanRotate(bool isClockwise, Figure curFigure)
        {
            Figure rotationFigure = curFigure.Clone();
            rotationFigure.RowIndexOnGrid = (int[])curFigure.RowIndexOnGrid.Clone();
            rotationFigure.ColumnIndexOnGrid = (int[])curFigure.ColumnIndexOnGrid.Clone();

            Dir_Rotation newDir = GetNewDirection(rotationFigure.Direction, isClockwise);

            rotationFigure.SetNewDirection(newDir);

            int figureWidth = rotationFigure.ColumnCount;
            int figureHeight = rotationFigure.RowCount;

            for (int r = 0; r < figureHeight; r++)
            {
                for (int c = 0; c < figureWidth; c++)
                {
                    if (rotationFigure.FigureValue[r, c] != GridValue.Empty)
                    {
                        if (rotationFigure.RowIndexOnGrid[r] < 0 ||
                            rotationFigure.ColumnIndexOnGrid[c] < 0 || rotationFigure.ColumnIndexOnGrid[c] >= Cols ||
                            Grid[rotationFigure.RowIndexOnGrid[r]][rotationFigure.ColumnIndexOnGrid[c]] != GridValue.Empty)
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
            if (!CanRotate(isClockwise, CurrentFigure))
                return;

            Dir_Rotation newDir = GetNewDirection(CurrentFigure.Direction, isClockwise);

            CurrentFigure.SetNewDirection(newDir);

            ProjectedFigure = CheckFallDown(CurrentFigure);
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
