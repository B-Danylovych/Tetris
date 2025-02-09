using System.ComponentModel;
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
        private const int NUM_OF_TOP_TABLE = 5;

        private readonly int rows = 20, cols = 10;
        private readonly Image[,] gridImages;
        GameMain Game;

        private bool paused = true;
        private bool leaved = false;
        private bool windowActivated = true;

        public enum TopType
        {
            isScore,
            isLines
        }
        private TopType currentTopType = TopType.isScore;

        private readonly TextBlock[] startScoresTable;
        private readonly TextBlock[] gameOverScoresTable;
        private readonly TextBlock[] menuLinesTable;
        private readonly TextBlock[] menuScoreTable;

        public enum TopUniformGridType
        {
            isTopCurrent,
            isTopScores,
            isTopLines
        }
        private readonly TextBlock[] menuScoresUniformGrid;
        private readonly TextBlock[] menuLinesUniformGrid;
        private readonly TextBlock[] otherCurrentUniformGrid;

        private TopRecord[] TopScores { get; set; }
        private TopRecord[] TopLines { get; set; }
        private TopRecord[] TopCurrent { get; set; }

        private readonly string topScoresPath = "TxtFiles/TopScores.txt";
        private readonly string topLinesPath = "TxtFiles/TopLines.txt";

        // локально зберігатиме значення найвищих показників
        // буде корисним якщо не існує текстових файлів
        string[] topScores = new string[5] { "0", "0", "0", "0", "0" };
        string[] topLines = new string[5] { "0", "0", "0", "0", "0" };

        private bool leftIsPressed = false;
        private bool rightIsPressed = false;

        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new Dictionary<GridValue, ImageSource>()
        {
            { GridValue.Empty, Images.Empty},
            { GridValue.I_Shape, Images.I_Shape},
            { GridValue.O_Shape, Images.O_Shape},
            { GridValue.T_Shape, Images.T_Shape},
            { GridValue.Z_Shape, Images.Z_Shape},
            { GridValue.S_Shape, Images.S_Shape},
            { GridValue.L_Shape, Images.L_Shape},
            { GridValue.J_Shape, Images.J_Shape},
        };

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetUpGrid();

            TopScores = SetUpTopArrays(TopType.isScore);
            TopLines = SetUpTopArrays(TopType.isLines);
            TopCurrent = TopScores;


            // в аргументах встановлюємо uniformGrif x:Name і relative шлях до текстового файлу
            startScoresTable = SetUpScoresTextBlocks(StartScoresTable, topScoresPath, TopType.isScore);
            gameOverScoresTable = SetUpScoresTextBlocks(GameOverScoresTable, topScoresPath, TopType.isScore);
            menuLinesTable = SetUpScoresTextBlocks(MenuLinesTable, topLinesPath, TopType.isLines);
            menuScoreTable = SetUpScoresTextBlocks(MenuScoreTable, topScoresPath, TopType.isScore);

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
                        Source = Images.Empty
                    };
                    images[r, c] = image;
                    TetrisGrid.Children.Add(image);
                }
            }
            return images;
        }

        private TextBlock[] SetUpScoresTextBlocks(UniformGrid uniformGrid, string path,
            TopType table)
        {
            List<string> txtData;
            if (table == TopType.isScore)
                txtData = ReadTxtFile(path, TopType.isScore);
            else
                txtData = ReadTxtFile(path, TopType.isLines);


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

        private TopRecord[] SetUpTopArrays(TopType txtFileType)
        {
            TopRecord[] topRecords = new TopRecord[NUM_OF_TOP_TABLE];

            bool isFileExist = CheckAndCorrectTxtFile(txtFileType);

            if (isFileExist)
                topRecords = ReadFile(txtFileType);
            else
                for (int i = 0; i < topRecords.Length; i++)
                    topRecords[i] = new TopRecord();

            return topRecords;
        }

        private void SetUpTextBlocks(TopUniformGridType table)
        {
            TopRecord[] topRecords;

            if (table == TopUniformGridType.isTopCurrent)
                topRecords = TopCurrent;
            else if (table == TopUniformGridType.isTopScores)
                topRecords = TopScores;
            else
                topRecords = TopLines;

            TextBlock[] textBlocks = new TextBlock[NUM_OF_TOP_TABLE];
            for (int i = 0; i < textBlocks.Length; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FontSize = 26,
                    FontWeight = FontWeights.Bold,
                };
                Binding binding = new Binding("Value")
                {
                    Source = topRecords[i],
                    Mode = BindingMode.TwoWay
                };

                textBlock.SetBinding(TextBlock.TextProperty, binding);

                textBlocks[i] = textBlock;

                if (table == TopUniformGridType.isTopCurrent)
                {
                    Border border = (Border)StartScoresTable.Children[i];
                    border.Child = textBlock;

                    TextBlock textBlock2 = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        FontSize = 26,
                        FontWeight = FontWeights.Bold,
                    };

                    textBlock2.SetBinding(TextBlock.TextProperty, binding);

                    border = (Border)GameOverScoresTable.Children[i];
                    border.Child = textBlock2;
                }
                else if (table == TopUniformGridType.isTopScores)
                {
                    Border border = (Border)MenuScoreTable.Children[i];
                    border.Child = textBlock;
                }
                else
                {
                    Border border = (Border)MenuLinesTable.Children[i];
                    border.Child = textBlock;
                }
            }
        }

        private TopRecord[] ReadFile(TopType txtFileType)
        {
            string path = txtFileType == TopType.isScore ? topScoresPath : topLinesPath;

            string[] txtData = File.ReadLines(path).Take(NUM_OF_TOP_TABLE).ToArray();

            TopRecord[] parsedTxtData = new TopRecord[NUM_OF_TOP_TABLE];

            for (int i = 0; i < parsedTxtData.Length; i++)
                parsedTxtData[i].Value = int.Parse(txtData[i]);

            return parsedTxtData;
        }

        private List<string> ReadTxtFile(string path, TopType table)
        {
            List<string> txtData;

            if (File.Exists(path))
                txtData = File.ReadLines(path).ToList();
            else
            {
                //MessageBox.Show($"File {path} doesn't exists");
                if (table == TopType.isScore)
                    txtData = topScores.ToList();
                else
                    txtData = topLines.ToList();
            }
            return txtData;
        }

        private bool CheckAndCorrectTxtFile(TopType txtFileType)
        {
            string path = txtFileType == TopType.isScore ? topScoresPath : topLinesPath;

            if (File.Exists(path))
            {
                string[] txtData = File.ReadLines(path).Take(NUM_OF_TOP_TABLE).ToArray();

                if (txtData.Length < NUM_OF_TOP_TABLE)
                    txtData = ToFillMissingLines(txtData);

                bool txtFileHasIntLines = isTxtFileHasIntLines(txtData);

                if (!txtFileHasIntLines)
                    txtData = GetResetTxtData();
                else
                    txtData = SortTxtData(txtData);

                File.WriteAllLines(path, txtData);

                return true;
            }
            else
                return false;
        }

        private string[] ToFillMissingLines(string[] incompleteTxtData)
        {
            List<string> completeTxtData = incompleteTxtData.ToList();

            int linesToFill = NUM_OF_TOP_TABLE - incompleteTxtData.Length;
            for (int i = 0; i < linesToFill; i++)
                completeTxtData.Add("0");

            return completeTxtData.ToArray();
        }

        private bool isTxtFileHasIntLines(string[] txtData)
        {
            foreach (string line in txtData)
                if (!int.TryParse(line, out _))
                    return false;

            return true;
        }

        private string[] GetResetTxtData()
        {
            string[] txtData = new string[NUM_OF_TOP_TABLE];
            Array.Fill(txtData, "0");

            return txtData;
        }

        private string[] SortTxtData(string[] txtData)
        {
            int[] txtIntData = new int[NUM_OF_TOP_TABLE];

            for (int i = 0; i < txtData.Length; i++)
                txtIntData[i] = int.Parse(txtData[i]);

            Array.Sort(txtIntData);
            Array.Reverse(txtIntData);

            return txtIntData.Select(i => i.ToString()).ToArray();
        }

        //private int[] ReadTxtFile(string path, Scores curTable)
        //{
        //    int[] txtData;

        //    if (File.Exists(path))
        //        txtData = File.ReadLines(path).Take(NUM_OF_TOP_SCORE_TABLE).ToArray();
        //}

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

            if (currentTopType == TopType.isScore)
            {
                currentTopType = TopType.isLines;
                StartScoresTitle.Text = "HIGH LINES";
                GameOverScoresTitle.Text = "HIGH LINES";

                txtData = ReadTxtFile(topLinesPath, currentTopType);
            }
            else
            {
                currentTopType = TopType.isScore;
                StartScoresTitle.Text = "HIGH SCORES";
                GameOverScoresTitle.Text = "HIGH SCORES";

                txtData = ReadTxtFile(topScoresPath, currentTopType);
            }

            // поганий код, тому що усюди встановлюю 5 елементів вручну, в даному випадку startScoresTable.Length = 5,
            // попри те що проходжу також і по gameOverScoresTable
            for (int i = 0; i < startScoresTable.Length; i++)
            {
                startScoresTable[i].Text = txtData[i];
                gameOverScoresTable[i].Text = txtData[i];
            }
        }

        private void DrawShapesOnGrid(Shape shape, double opacity)
        {
            for (int r = 0; r < shape.RowsPosition.Length; r++)
            {
                for (int c = 0; c < shape.ColumnsPosition.Length; c++)
                {
                    if (shape.RowsPosition[r] > gridImages.GetLength(0) - 1 || shape.RowsPosition[r] < 0 ||
                       shape.ColumnsPosition[c] > gridImages.GetLength(1) - 1 || shape.ColumnsPosition[c] < 0)
                    {
                        continue;
                    }
                    if (shape.ShapeValue[r, c] != GridValue.Empty)
                    {
                        gridImages[shape.RowsPosition[r], shape.ColumnsPosition[c]].Source = gridValToImage[shape.ShapeValue[r, c]];
                        gridImages[shape.RowsPosition[r], shape.ColumnsPosition[c]].Opacity = opacity;
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
            DrawShapesOnGrid(Game.ProjectedShape, 0.2);
            DrawShapesOnGrid(Game.CurrentShape, 1);
        }

        public void DrawBufferShape()
        {
            Shape shape = Game.BufferShape;

            int countCols = shape.ShapeValue.GetLength(1);
            int countRows = 0;
            int startRowsIndex = 0;

            for (int r = 0; r < shape.ShapeValue.GetLength(0); r++)
            {
                for (int c = 0; c < countCols; c++)
                {
                    if (shape.ShapeValue[r, c] != GridValue.Empty)
                    {
                        if (countRows == 0)
                            startRowsIndex = r;

                        countRows++;
                        break;
                    }
                }
            }

            BufferShapeGrid.Children.Clear();

            BufferShapeGrid.Columns = countCols;
            BufferShapeGrid.Rows = countRows;

            // ділимо на 4, оскільки максимальна ширина тетраміно фігури - 4, а саме у I Фігурі, яка "лежить"(має Direction - Top/Bottom)
            double tileSize = BufferShapeBorder.ActualWidth / 4;

            for (int r = startRowsIndex; r < startRowsIndex + countRows; r++)
            {
                for (int c = 0; c < countCols; c++)
                {
                    Image image = new Image
                    {
                        Source = gridValToImage[shape.ShapeValue[r, c]],
                        Width = tileSize,
                    };
                    BufferShapeGrid.Children.Add(image);
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (paused)
                return;

            switch (e.Key)
            {
                case Key.A:
                case Key.Left:
                    bool isMovedLeft = Game.MoveLeft();
                    if (isMovedLeft)
                    {
                        if (leftIsPressed)
                            Game.MoveLeft();

                        Game.SetProjectedShape();
                        Draw();
                    }
                    leftIsPressed = true;
                    break;
                case Key.D:
                case Key.Right:
                    bool isMovedRight = Game.MoveRight();
                    if (isMovedRight)
                    {
                        if (rightIsPressed)
                            Game.MoveRight();

                        Game.SetProjectedShape();
                        Draw();
                    }
                    rightIsPressed = true;
                    break;
                case Key.S:
                case Key.Down:
                    Game.IterationTick = 50;
                    break;
                case Key.W:
                case Key.Up:
                    Game.Rotate(GameMain.DirectionOfRotation.isClockwise);
                    Game.SetProjectedShape();
                    Draw();
                    break;
                case Key.Z:
                    Game.Rotate(GameMain.DirectionOfRotation.isCounterclockwise);
                    Game.SetProjectedShape();
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
                    Game.IterationTick = 500;
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
            Pause_AI_Grid.Visibility = Visibility.Visible;
            paused = false;
            if (!windowActivated)
            {
                PauseGame();
            }
        }

        private async Task GameLoop()
        {
            while (!Game.IsGameOver)
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

        private async Task RunGame()
        {
            await Task.Delay(Game.IterationTick);

            if (paused)
                return;

            bool canMoveDown = Game.MoveDown();
            if (!canMoveDown)
            {
                await Task.Delay(Game.LockDelayTick);
                canMoveDown = Game.MoveDown();
                if (!canMoveDown)
                {
                    Game.SetShapeOnGrid();
                    Game.RemoveLines();
                    Game.CheckGameOver();
                    if (Game.IsGameOver)
                        return;
                    Game.SetCurrentShape();
                    Game.SetProjectedShape();
                    Game.SetBufferShape();
                    DrawBufferShape();
                    if (Game.IterationTick > 20)
                        Game.IterationTick--;

                    if (Game.LockDelayTick > 100)
                        Game.LockDelayTick--;

                    ScoreTextBlock.Text = $"Score: {Game.ScoreNum}";
                    LineTextBlock.Text = $"Lines: {Game.LinesNum}";
                }
            }

            Draw();
        }

        private void NewTopScores()
        {
            List<int> txtDataScore = ReadTxtFile(topScoresPath, TopType.isScore).ConvertAll(int.Parse);
            List<int> txtDataLines = ReadTxtFile(topLinesPath, TopType.isLines).ConvertAll(int.Parse);

            if (Game.ScoreNum > txtDataScore.Last())
            {
                txtDataScore[txtDataScore.Count - 1] = Game.ScoreNum;
                txtDataScore.Sort();
                txtDataScore.Reverse();
            }
            if (Game.LinesNum > txtDataLines.Last())
            {
                txtDataLines[txtDataLines.Count - 1] = Game.LinesNum;
                txtDataLines.Sort();
                txtDataLines.Reverse();
            }

            RewriteTxtFile(txtDataScore.ConvertAll(i => i.ToString()), topScoresPath, true);
            RewriteTxtFile(txtDataLines.ConvertAll(i => i.ToString()), topLinesPath, false);

            // поганий код з startScoresTable.Length(вкотре нагадаю) :)
            for (int i = 0; i < startScoresTable.Length; i++)
            {
                if (currentTopType == TopType.isScore)
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
            DrawBufferShape();
            await ShowCountDown();
            Game.SetCurrentShape();
            Game.SetProjectedShape();
            Game.SetBufferShape();
            DrawBufferShape();
            await GameLoop();
            if (Game.IsGameOver)
            {
                NewTopScores();
                GameOverScoreNum.Text = Game.ScoreNum.ToString();
                GameOverLineNum.Text = Game.LinesNum.ToString();
                Pause_AI_Grid.Visibility = Visibility.Collapsed;
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
            if (Pause_AI_Grid.Visibility == Visibility.Visible)
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
            Pause_AI_Grid.Visibility = Visibility.Collapsed;
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

        private void AI_Button_Click(object sender, RoutedEventArgs e)
        {

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