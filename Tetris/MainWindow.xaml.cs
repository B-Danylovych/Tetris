using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tetris
{
    public partial class MainWindow : Window
    {

        private readonly int rows = 20, cols = 10;
        private readonly Image[,] gridImages;
        GameMain Game;

        // ці змінні можна було б перемістити у клас GameMain
        private bool paused = true;
        private bool leaved = false;
        private bool windowActivated = true;

        // якщо true на екрані буде top Score таблиця, якщо false top Line таблиця
        private bool isScoreTable = true;

        private readonly TextBlock[] startScoresTable;
        private readonly TextBlock[] gameOverScoresTable;
        private readonly TextBlock[] menuLinesTable;
        private readonly TextBlock[] menuScoreTable;

        string topScoresPath = "TxtFiles/TopScores.txt";
        string topLinesPath = "TxtFiles/TopLines.txt";

        // локально зберігатиме значення найвищих показників
        // буде корисним якщо не існує текстових файлів
        string[] topScores = new string[5] { "0", "0", "0", "0", "0" };
        string[] topLines = new string[5] { "0", "0", "0", "0", "0" };

        private bool leftIsPressed = false;
        private bool rightIsPressed = false;

        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new Dictionary<GridValue, ImageSource>()
        {
            { GridValue.Empty, null},
            { GridValue.I_Figure, Images.I_Figure},
            { GridValue.O_Figure, Images.O_Figure},
            { GridValue.T_Figure, Images.T_Figure},
            { GridValue.Z_Figure, Images.Z_Figure},
            { GridValue.S_Figure, Images.S_Figure},
            { GridValue.L_Figure, Images.L_Figure},
            { GridValue.J_Figure, Images.J_Figure},
        };

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetUpGrid();

            // в аргументах встановлюємо uniformGrif x:Name і relative шлях до текстового файлу
            startScoresTable = SetUpScoresTextBlocks(StartScoresTable, topScoresPath, true);
            gameOverScoresTable = SetUpScoresTextBlocks(GameOverScoresTable, topScoresPath, true);
            menuLinesTable = SetUpScoresTextBlocks(MenuLinesTable, topLinesPath, false);
            menuScoreTable = SetUpScoresTextBlocks(MenuScoreTable, topScoresPath, true);

            Game = new GameMain(rows, cols);
        }

        private Image[,] SetUpGrid()
        {
            Image[,] images = new Image[rows, cols];
            TetrisGrid.Rows = rows;
            TetrisGrid.Columns = cols;
            for (int r = rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = null
                    };
                    images[r, c] = image;
                    TetrisGrid.Children.Add(image);
                }
            }
            return images;
        }

        // можна використовувати Binding для прив'язки тексту до всіх textBlock-ів одночасно,
        // проте я не можу його зрозуміти поки що
        private TextBlock[] SetUpScoresTextBlocks(UniformGrid uniformGrid, string path, bool isScoreTable)
        {
            List<string> txtData;
            if (isScoreTable)
                txtData = ReadTxtFile(path, true);
            else
                txtData = ReadTxtFile(path, false);


            // вказувати кількість елементів [5] таким чином тут - це погана ідея, проте поки я так роблю
            TextBlock[] textBlocks = new TextBlock[5];
            for (int i = 0; i < textBlocks.Length; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FontSize = 26,
                    FontWeight = FontWeights.Bold,
                    Text = txtData[i]
                };
                textBlocks[i] = textBlock;

                Border border = (Border)uniformGrid.Children[i];
                border.Child = textBlock;
            }

            return textBlocks;
        }

        private List<string> ReadTxtFile(string path, bool isScoreTable)
        {
            List<string> txtData;

            if (File.Exists(path))
                txtData = File.ReadLines(path).ToList();
            else
            {
                //MessageBox.Show($"File {path} doesn't exists");
                if(isScoreTable)
                    txtData = topScores.ToList();
                else
                    txtData = topLines.ToList();
            }
            return txtData;
        }

        private void RewriteTxtFile(List<string> newTxtData, string path, bool isScoreTable)
        {
            if (File.Exists(path))
                File.WriteAllLines(path, newTxtData);
            else
            {
                //MessageBox.Show($"File {path} doesn't exists");
                if (isScoreTable)
                    topScores = newTxtData.ToArray();
                else
                    topLines = newTxtData.ToArray();
            }
        }

        private void ArrowsButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> txtData;

            if (isScoreTable)
            {
                isScoreTable = false;
                StartScoresTitle.Text = "HIGH LINES";
                GameOverScoresTitle.Text = "HIGH LINES";

                txtData = ReadTxtFile(topLinesPath, isScoreTable);
            }
            else
            {
                isScoreTable = true;
                StartScoresTitle.Text = "HIGH SCORES";
                GameOverScoresTitle.Text = "HIGH SCORES";

                txtData = ReadTxtFile(topScoresPath, isScoreTable);
            }

            // поганий код, тому що усюди встановлюю 5 елементів вручну, в даному випадку startScoresTable.Length = 5,
            // попри те що проходжу також і по gameOverScoresTable
            for (int i = 0; i < startScoresTable.Length; i++)
            {
                startScoresTable[i].Text = txtData[i];
                gameOverScoresTable[i].Text = txtData[i];
            }
        }

        private void DrawFiguresOnGrid(Figure figure, double opacity)
        {
            for (int r = 0; r < figure.RowIndexOnGrid.Length; r++)
            {
                for (int c = 0; c < figure.ColumnIndexOnGrid.Length; c++)
                {
                    if (figure.RowIndexOnGrid[r] > gridImages.GetLength(0) - 1 || figure.RowIndexOnGrid[r] < 0 ||
                       figure.ColumnIndexOnGrid[c] > gridImages.GetLength(1) - 1 || figure.ColumnIndexOnGrid[c] < 0)
                    {
                        continue;
                    }
                    if (figure.FigureValue[r, c] != GridValue.Empty)
                    {
                        gridImages[figure.RowIndexOnGrid[r], figure.ColumnIndexOnGrid[c]].Source = gridValToImage[figure.FigureValue[r, c]];
                        gridImages[figure.RowIndexOnGrid[r], figure.ColumnIndexOnGrid[c]].Opacity = opacity;
                    }
                }
            }
        }

        private void DrawGrid()
        {
            for (int r = 0; r < gridImages.GetLength(0); r++)
            {
                for (int c = 0; c < gridImages.GetLength(1); c++)
                {
                    gridImages[r, c].Source = gridValToImage[this.Game.Grid[r][c]];
                    gridImages[r, c].Opacity = 1;
                }
            }
        }

        private void Draw()
        {
            DrawGrid();
            DrawFiguresOnGrid(Game.ProjectedFigure, 0.2);
            DrawFiguresOnGrid(Game.CurrentFigure, 1);
        }

        public void DrawBufferFigure()
        {
            Figure figure = Game.BufferFigure;

            int countCols = figure.FigureValue.GetLength(1);
            int countRows = 0;
            int startRowsIndex = 0;

            for (int r = 0; r < figure.FigureValue.GetLength(0); r++)
            {
                for (int c = 0; c < countCols; c++)
                {
                    if (figure.FigureValue[r, c] != GridValue.Empty)
                    {
                        if (countRows == 0)
                            startRowsIndex = r;

                        countRows++;
                        break;
                    }
                }
            }

            BufferFigureGrid.Children.Clear();

            BufferFigureGrid.Columns = countCols;
            BufferFigureGrid.Rows = countRows;

            // ділимо на 4, оскільки максимальна ширина тетраміно фігури - 4, а саме у I Фігурі, яка "лежить"(має Direction - Top/Bottom)
            double tileSize = BufferFigureBorder.ActualWidth / 4;

            for (int r = startRowsIndex; r < startRowsIndex + countRows; r++)
            {
                for (int c = 0; c < countCols; c++)
                {
                    Image image = new Image
                    {
                        Source = gridValToImage[figure.FigureValue[r, c]],
                        Width = tileSize,
                    };
                    BufferFigureGrid.Children.Add(image);
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (paused)
                return;

            bool canMove;
            switch (e.Key)
            {
                case Key.A:
                case Key.Left:
                    canMove = Game.MoveLeft();
                    Draw();
                    if (canMove && leftIsPressed)
                    {
                        Game.MoveLeft();
                        Draw();
                    }
                    leftIsPressed = true;
                    break;
                case Key.D:
                case Key.Right:
                    canMove = Game.MoveRight();
                    Draw();
                    if (canMove && rightIsPressed)
                    {
                        Game.MoveRight();
                        Draw();
                    }
                    rightIsPressed = true;
                    break;
                case Key.S:
                case Key.Down:
                    Game.iterationTick = 50;
                    break;
                case Key.W:
                case Key.Up:
                    Game.Rotate(true);
                    Draw();
                    break;
                case Key.Z:
                    Game.Rotate(false);
                    Draw();
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.S:
                case Key.Down:
                    Game.iterationTick = 500;
                    break;
                case Key.A:
                case Key.Left:
                    leftIsPressed = false;
                    break;
                case Key.D:
                case Key.Right:
                    rightIsPressed = false;
                    break;
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                CountDownText.Text = i.ToString();
                await Task.Delay(500);
            }
            CountDownText.Visibility = Visibility.Collapsed;
            MenuBorder.Visibility = Visibility.Collapsed;
            PauseButton.Visibility = Visibility.Visible;
            paused = false;
            if (!windowActivated)
            {
                PauseGame();
            }
        }

        private async Task RunGame()
        {
            await Task.Delay(Game.iterationTick);

            if (paused)
                return;

            bool canMoveDown = Game.MoveDown();
            if (!canMoveDown)
            {
                await Task.Delay(Game.lockDelayTick);
                canMoveDown = Game.MoveDown();
                if (!canMoveDown)
                {
                    Game.AddFigureTilesOnGrid();
                    Game.RemoveLines();
                    Game.checkGameOver();
                    if (Game.isGameOver)
                        return;
                    Game.AddFigure();
                    DrawBufferFigure();
                    if (Game.iterationTick > 20)
                        Game.iterationTick--;

                    if (Game.lockDelayTick > 100)
                        Game.lockDelayTick--;

                    ScoreTextBlock.Text = $"Score: {Game.Score}";
                    LineTextBlock.Text = $"Lines: {Game.Line}";
                }
            }

            Draw();
        }

        private async Task GameLoop()
        {
            while (!Game.isGameOver)
            {
                if (paused)
                {
                    if (leaved)
                    {
                        leaved = false;
                        return;
                    }
                    await Task.Delay(100);
                    continue;
                }

                await RunGame();
            }
        }

        private void NewTopScores()
        {
            List<int> txtDataScore = ReadTxtFile(topScoresPath, true).ConvertAll(int.Parse);
            List<int> txtDataLines = ReadTxtFile(topLinesPath, false).ConvertAll(int.Parse);

            if (Game.Score > txtDataScore.Last())
            {
                txtDataScore[txtDataScore.Count - 1] = Game.Score;
                txtDataScore.Sort();
                txtDataScore.Reverse();
            }
            if (Game.Line > txtDataLines.Last())
            {
                txtDataLines[txtDataLines.Count - 1] = Game.Line;
                txtDataLines.Sort();
                txtDataLines.Reverse();
            }

            RewriteTxtFile(txtDataScore.ConvertAll(i => i.ToString()), topScoresPath, true);
            RewriteTxtFile(txtDataLines.ConvertAll(i => i.ToString()), topLinesPath, false);

            // поганий код з startScoresTable.Length(вкотре нагадаю) :)
            for (int i = 0; i < startScoresTable.Length; i++)
            {
                if (this.isScoreTable)
                {
                    startScoresTable[i].Text = txtDataScore.ConvertAll(i => i.ToString())[i];
                    gameOverScoresTable[i].Text = txtDataScore.ConvertAll(i => i.ToString())[i];
                }
                else
                {
                    startScoresTable[i].Text = txtDataLines.ConvertAll(i => i.ToString())[i];
                    gameOverScoresTable[i].Text = txtDataLines.ConvertAll(i => i.ToString())[i];
                }
                menuScoreTable[i].Text = txtDataScore.ConvertAll(i => i.ToString())[i];
                menuLinesTable[i].Text = txtDataLines.ConvertAll(i => i.ToString())[i];
            }
        }

        private async Task StartToEnd()
        {
            DrawBufferFigure();
            await ShowCountDown();
            Game.AddFigure();
            DrawBufferFigure();
            await GameLoop();
            if (Game.isGameOver)
            {
                NewTopScores();
                PauseButton.Visibility = Visibility.Collapsed;
                MenuBorder.Visibility = Visibility.Visible;
                GameOverBorder.Visibility = Visibility.Visible;
            }
            else
            {
                ScoreTextBlock.Text = $"Score: 0";
                LineTextBlock.Text = $"Lines: 0";
            }
        }

        private async void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            ScoreTextBlock.Text = $"Score: 0";
            LineTextBlock.Text = $"Lines: 0";
            GameOverBorder.Visibility = Visibility.Collapsed;
            CountDownText.Visibility = Visibility.Visible;
            Game = new GameMain(rows, cols);
            DrawGrid();
            await StartToEnd();
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            StartBorder.Visibility = Visibility.Collapsed;
            await StartToEnd();
        }

        private async void Resume_Click(object sender, RoutedEventArgs e)
        {
            MainMenuBorder.Visibility = Visibility.Collapsed;
            CountDownText.Visibility = Visibility.Visible;
            await ShowCountDown();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            windowActivated = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            windowActivated = false;
            if (PauseButton.Visibility == Visibility.Visible)
            {
                PauseGame();
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        private void PauseGame()
        {
            paused = true;
            PauseButton.Visibility = Visibility.Collapsed;
            MenuBorder.Visibility = Visibility.Visible;
            MainMenuBorder.Visibility = Visibility.Visible;
        }

        private void HighScores_Click(object sender, RoutedEventArgs e)
        {
            MainMenuBorder.Visibility = Visibility.Collapsed;
            HighScoresMenuBorder.Visibility = Visibility.Visible;
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            MainMenuBorder.Visibility = Visibility.Collapsed;
            QuitMenuBorder.Visibility = Visibility.Visible;
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            HighScoresMenuBorder.Visibility = Visibility.Collapsed;
            MainMenuBorder.Visibility = Visibility.Visible;
        }

        private void OkQuit_Click(object sender, RoutedEventArgs e)
        {
            leaved = true;
            QuitMenuBorder.Visibility = Visibility.Collapsed;
            CountDownText.Visibility = Visibility.Visible;
            StartBorder.Visibility = Visibility.Visible;

            NewTopScores();

            Game = new GameMain(rows, cols);
            DrawGrid();
        }

        private void CancelQuit_Click(object sender, RoutedEventArgs e)
        {
            QuitMenuBorder.Visibility = Visibility.Collapsed;
            MainMenuBorder.Visibility = Visibility.Visible;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            ScoreTextBlock.Text = $"Score: 0";
            LineTextBlock.Text = $"Lines: 0";
            GameOverBorder.Visibility = Visibility.Collapsed;
            CountDownText.Visibility = Visibility.Visible;
            StartBorder.Visibility = Visibility.Visible;
            Game = new GameMain(rows, cols);
            DrawGrid();
        }
    }
}