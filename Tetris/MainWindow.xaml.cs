using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Printing;
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
        private const int NUM_OF_TOP_RECORDS_TABLE = 5;

        private readonly int maxShapesHeightInUpDirection;
        private readonly int maxShapesWidthInUpDirection;

        private readonly string topScoresPath = "TxtFiles/TopScores.txt";
        private readonly string topLinesPath = "TxtFiles/TopLines.txt";

        private readonly int rows = 20, columns = 10;
        private readonly int hiddenRowsOnTop;

        private readonly Image[,] gridImages;
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

        public GameMain Game;
        private AI_Game AI_Game;

        private bool paused = true;
        private bool leaved = false;
        private bool windowActivated = true;

        private bool aiActivated = false;
        private bool aiWasActivatedDuringGame = false;
        private int aiSpeed = 0;

        private bool leftIsPressed = false;
        private bool rightIsPressed = false;

        private TopRecord[] TopScores { get; set; }
        private TopRecord[] TopLines { get; set; }
        private TopRecord[] TopCurrent { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetUpGrid();

            TopScores = GetOrCreateTopRecords(topScoresPath);
            TopLines = GetOrCreateTopRecords(topLinesPath);
            TopCurrent = TopScores;

            BindTopRecordsToUniformGridBorders(TopScores, MenuScoreTable);
            BindTopRecordsToUniformGridBorders(TopLines, MenuLinesTable);
            BindTopRecordsToUniformGridBorders(TopCurrent, StartScoresTable);
            BindTopRecordsToUniformGridBorders(TopCurrent, GameOverScoresTable);

            var maxShapesSizesInUpDirection = GetMaxShapesSizesInUpDirection();
            maxShapesHeightInUpDirection = maxShapesSizesInUpDirection.maxHeight;
            maxShapesWidthInUpDirection = maxShapesSizesInUpDirection.maxWidth;

            hiddenRowsOnTop = maxShapesHeightInUpDirection;
            Game = new GameMain(rows, hiddenRowsOnTop, columns);
        }

        private Image[,] SetUpGrid()
        {
            Image[,] images = new Image[rows, columns];
            TetrisGrid.Rows = rows;
            TetrisGrid.Columns = columns;
            for (int r = rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < columns; c++)
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

        private TopRecord[] GetOrCreateTopRecords(string path)
        {
            TopRecord[] topRecords = new TopRecord[NUM_OF_TOP_RECORDS_TABLE];

            bool isFileExist = VerifyTxtFileExistenceAndCorrectData(path);

            if (isFileExist)
                topRecords = ReadTxtFile(path);
            else
                for (int i = 0; i < topRecords.Length; i++)
                    topRecords[i] = new TopRecord();

            return topRecords;
        }

        private void BindTopRecordsToUniformGridBorders(TopRecord[] topRecords, UniformGrid uniformGrid)
        {
            for (int i = 0; i < topRecords.Length; i++)
            {
                TextBlock textBlock = CreateTopRecordTextBlock();

                Binding binding = CreateBindingToTopRecordValue(topRecords[i]);

                textBlock.SetBinding(TextBlock.TextProperty, binding);

                AddTextBlockToBorder(textBlock, (Border)uniformGrid.Children[i]);
            }
        }

        private TextBlock CreateTopRecordTextBlock() => new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            FontSize = 26,
            FontWeight = FontWeights.Bold,
        };

        private Binding CreateBindingToTopRecordValue(TopRecord topRecord)
        => new Binding("Value")
        {
            Source = topRecord,
            Mode = BindingMode.TwoWay
        };

        private void AddTextBlockToBorder(TextBlock textBlock, Border bord)
        {
            Border border = bord;
            border.Child = textBlock;
        }

        private bool VerifyTxtFileExistenceAndCorrectData(string path)
        {
            if (File.Exists(path))
            {
                string[] txtData = File.ReadLines(path).Take(NUM_OF_TOP_RECORDS_TABLE).ToArray();

                if (txtData.Length < NUM_OF_TOP_RECORDS_TABLE)
                    txtData = ToFillMissingLines(txtData);

                bool txtFileHasIntLines = IsTxtFileHasIntLines(txtData);

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

            int linesToFill = NUM_OF_TOP_RECORDS_TABLE - incompleteTxtData.Length;
            for (int i = 0; i < linesToFill; i++)
                completeTxtData.Add("0");

            return completeTxtData.ToArray();
        }

        private bool IsTxtFileHasIntLines(string[] txtData)
        {
            foreach (string line in txtData)
                if (!int.TryParse(line, out _))
                    return false;

            return true;
        }

        private string[] GetResetTxtData()
        {
            string[] txtData = new string[NUM_OF_TOP_RECORDS_TABLE];
            Array.Fill(txtData, "0");

            return txtData;
        }

        private string[] SortTxtData(string[] txtData)
        {
            int[] txtIntData = new int[NUM_OF_TOP_RECORDS_TABLE];

            for (int i = 0; i < txtData.Length; i++)
                txtIntData[i] = int.Parse(txtData[i]);

            txtIntData = txtIntData.OrderByDescending(x => x).ToArray();

            return txtIntData.Select(i => i.ToString()).ToArray();
        }

        private TopRecord[] ReadTxtFile(string path)
        {
            string[] txtData = File.ReadLines(path).Take(NUM_OF_TOP_RECORDS_TABLE).ToArray();

            TopRecord[] parsedTxtData = new TopRecord[NUM_OF_TOP_RECORDS_TABLE];

            for (int i = 0; i < parsedTxtData.Length; i++)
            {
                parsedTxtData[i] = new TopRecord();
                parsedTxtData[i].Value = int.Parse(txtData[i]);
            }

            return parsedTxtData;
        }

        private void RewriteTxtFileWithTopRecords(string path, TopRecord[] topRecords)
        {
            string[] topRecordsValues = new string[NUM_OF_TOP_RECORDS_TABLE];

            for (int i = 0; i < topRecordsValues.Length; i++)
                topRecordsValues[i] = topRecords[i].Value.ToString();

            File.WriteAllLines(path, topRecordsValues);
        }

        private (int maxHeight, int maxWidth) GetMaxShapesSizesInUpDirection()
        {
            int maxHeight = 0;
            int maxWidth = 0;

            foreach (GridValue gridValue in Enum.GetValues(typeof(GridValue)))
            {
                if (gridValue != GridValue.Empty)
                {
                    Shape shape = new Shape(gridValue);

                    int currentHeight = shape.Height;

                    int currentWidth = shape.Width;

                    maxHeight = Math.Max(maxHeight, currentHeight);
                    maxWidth = Math.Max(maxWidth, currentWidth);
                }
            }

            return (maxHeight, maxWidth);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (paused || aiActivated)
                return;

            switch (e.Key)
            {
                case Key.A:
                case Key.Left:
                    HandleLeftRightKeyPress(Game.MoveLeft, leftIsPressed);
                    leftIsPressed = true;
                    break;
                case Key.D:
                case Key.Right:
                    HandleLeftRightKeyPress(Game.MoveRight, rightIsPressed);
                    rightIsPressed = true;
                    break;
                case Key.S:
                case Key.Down:
                    Game.ActivateFastDrop();
                    break;
                case Key.W:
                case Key.Up:
                    HandleRotatingKeyPress(GameMain.DirectionOfRotation.isClockwise);
                    break;
                case Key.Z:
                    HandleRotatingKeyPress(GameMain.DirectionOfRotation.isCounterclockwise);
                    break;
            }
        }

        private void HandleLeftRightKeyPress(Func<bool> moveInDirection, bool isKeyPressed)
        {
            bool isMoved = moveInDirection();
            if (isMoved)
            {
                if (isKeyPressed)
                    moveInDirection();

                Game.SetProjectedShape();
                DrawGameGrid();
            }
        }

        private void HandleRotatingKeyPress(GameMain.DirectionOfRotation directionOfRotation)
        {
            if (Game.Rotate(directionOfRotation))
            {
                Game.SetProjectedShape();
                DrawGameGrid();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (aiActivated)
                return;

            switch (e.Key)
            {
                case Key.S:
                case Key.Down:
                    Game.DeactivateFastDrop();
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

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            StartBorder.Visibility = Visibility.Collapsed;
            await GameStartToEnd();
        }

        private async Task GameStartToEnd()
        {
            DrawShapeInBufferGrid(Game.BufferShape);
            await ShowCountDown();
            Game.SetShapes();
            DrawShapeInBufferGrid(Game.BufferShape);
            await GameLoop();
            aiActivated = false;
            if (Game.IsGameOver)
            {
                if(!aiWasActivatedDuringGame)
                    NewTopRecords();
                else
                {
                    GameOverScoreText.Text = $"AI Score: ";
                    GameOverLineText.Text = $"AI Lines: ";
                    GameOverScoreText.Foreground = new SolidColorBrush(Colors.Red);
                    GameOverLineText.Foreground = new SolidColorBrush(Colors.Red);
                    GameOverScoreNum.Foreground = new SolidColorBrush(Colors.Red);
                    GameOverLineNum.Foreground = new SolidColorBrush(Colors.Red);
                }
                GameOverScoreNum.Text = Game.ScoreNum.ToString();
                GameOverLineNum.Text = Game.LinesNum.ToString();
                Pause_AI_Grid.Visibility = Visibility.Collapsed;
                CollapseAI_Elements();
                MenuBorder.Visibility = Visibility.Visible;
                GameOverBorder.Visibility = Visibility.Visible;
            }
            else
            {
                ScoreTextBlock.Text = $"Score: 0";
                LineTextBlock.Text = $"Lines: 0";
                ScoreTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                LineTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            }
            aiWasActivatedDuringGame = false;
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
            AISpeedSlider.Visibility = Visibility.Visible;
            paused = false;
            if (!windowActivated)
                PauseGame();
        }

        private async Task GameLoop()
        {
            while (!Game.IsGameOver)
            {
                await WaitPause();
                if (leaved)
                {
                    leaved = false;
                    return;
                }
                await RunGame();
            }
        }

        private async Task RunGame()
        {
            if (aiActivated)
            {
                AI_Game.ResetValues();
                await MoveByAIBestMoveOption();
                if (leaved)
                    return;
                AI_Game.UptadeGrid();
            }
            else
                await Task.Delay(Game.IterationTick);

            bool canMoveDown = Game.MoveDown();
            if (!canMoveDown)
            {
                if (!aiActivated || aiSpeed < 3)
                    await Task.Delay(Game.LockDelayTick);
                else
                    await Task.Delay(1);

                canMoveDown = Game.MoveDown();
                if (!canMoveDown)
                {
                    Game.SetShapeOnGrid();
                    Game.RemoveLines();
                    Game.CheckGameOver();
                    if (Game.IsGameOver)
                        return;
                    Game.SetShapes();
                    DrawShapeInBufferGrid(Game.BufferShape);
                    Game.IncreaseGameDifficulty();

                    ScoreTextBlock.Text = $"Score: {Game.ScoreNum}";
                    LineTextBlock.Text = $"Lines: {Game.LinesNum}";
                }
            }

            DrawGameGrid();
        }

        private async Task MoveByAIBestMoveOption()
        {
            if (aiSpeed < 2)
            {
                foreach (Action action in AI_Game.BestMoveOption.Actions)
                {
                    await Task.Delay(aiSpeed == 0 ? 50 : 20);

                    if (aiSpeed >= 2)
                        break;

                    if (!aiActivated)
                        return;

                    await WaitPause();
                    if (leaved)
                        return;

                    action.Invoke();
                    Game.SetProjectedShape();
                    DrawGameGrid();
                }

                while (Game.MoveDown())
                {
                    await Task.Delay(aiSpeed == 0 ? 50 : 20);

                    if (aiSpeed >= 2)
                        break;

                    if (!aiActivated)
                        return;

                    await WaitPause();
                    if (leaved)
                        return;

                    DrawGameGrid();
                }
            }
            if (aiSpeed >= 2)
            {
                Game.CurrentShape = AI_Game.BestMoveOption.ProjectedShape.DeepCopy();
                Game.SetProjectedShape();
                DrawGameGrid();
            }
        }

        private async Task WaitPause()
        {
            while (paused)
            {
                if (leaved)
                    return;

                await Task.Delay(100);
            }
        }

        public void DrawShapeInBufferGrid(Shape shape)
        {
            int shapeWidth = shape.ColumnsCount;
            int shapeHeight = shape.Height;

            int startRowValueIndex = GetFirstNonEmptyRowIndexOfShapeValue(shape);
            int endRowValueIndex = startRowValueIndex + shapeHeight;

            BufferShapeGrid.Children.Clear();

            BufferShapeGrid.Columns = shapeWidth;
            BufferShapeGrid.Rows = shapeHeight;

            double tileSize = BufferShapeBorder.ActualWidth / maxShapesWidthInUpDirection;

            for (int r = startRowValueIndex; r < endRowValueIndex; r++)
            {
                for (int c = 0; c < shapeWidth; c++)
                {
                    Image image = new Image
                    {
                        Source = gridValToImage[shape.ShapeGrid[r, c]],
                        Width = tileSize,
                    };
                    BufferShapeGrid.Children.Add(image);
                }
            }
        }

        private int GetFirstNonEmptyRowIndexOfShapeValue(Shape shape)
        {
            for (int r = 0; r < shape.RowsCount; r++)
                for (int c = 0; c < shape.ColumnsCount; c++)
                    if (shape.ShapeGrid[r, c] != GridValue.Empty)
                        return r;

            throw new InvalidOperationException("The loop did not return a value.");
        }

        private void DrawGameGrid()
        {
            DrawGrid();
            DrawShapesOnGridWithOpacity(Game.ProjectedShape, 0.2);
            DrawShapesOnGridWithOpacity(Game.CurrentShape, 1);
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
        private void DrawShapesOnGridWithOpacity(Shape shape, double opacity)
        {
            for (int r = 0; r < shape.RowsCount; r++)
            {
                for (int c = 0; c < shape.ColumnsCount; c++)
                {
                    GridValue shapeTile = shape.ShapeGrid[r, c];
                    int rowPosition = shape.RowsPosition[r];
                    int columnPosition = shape.ColumnsPosition[c];

                    if (shapeTile != GridValue.Empty && rowPosition < gridImages.GetLength(0))
                    {
                        gridImages[rowPosition, columnPosition].Source = gridValToImage[shapeTile];
                        gridImages[rowPosition, columnPosition].Opacity = opacity;
                    }
                }
            }
        }

        private void NewTopRecords()
        {
            if (Game.ScoreNum > TopScores.Last().Value)
            {
                AddNewRecordAndSortTopRecords(TopScores, Game.ScoreNum);

                if (File.Exists(topScoresPath))
                    RewriteTxtFileWithTopRecords(topScoresPath, TopScores);
            }
            if (Game.LinesNum > TopLines.Last().Value)
            {
                AddNewRecordAndSortTopRecords(TopLines, Game.LinesNum);

                if (File.Exists(topLinesPath))
                    RewriteTxtFileWithTopRecords(topLinesPath, TopLines);
            }
        }

        private void AddNewRecordAndSortTopRecords(TopRecord[] topRecords, int newScore)
        {
            int[] sortedRecords = topRecords.Select(x => x.Value).ToArray();

            sortedRecords[sortedRecords.Length - 1] = newScore;
            sortedRecords = sortedRecords.OrderDescending().ToArray();

            for (int i = 0; i < topRecords.Length; i++)
                topRecords[i].Value = sortedRecords[i];
        }

        private void ArrowsButton_Click(object sender, RoutedEventArgs e)
        {
            if (TopCurrent == TopScores)
            {
                TopCurrent = TopLines;
                StartScoresTitle.Text = GameOverScoresTitle.Text = "HIGH LINES";
            }
            else
            {
                TopCurrent = TopScores;
                StartScoresTitle.Text = GameOverScoresTitle.Text = "HIGH SCORES";
            }

            BindTopRecordsToUniformGridBorders(TopCurrent, StartScoresTable);
            BindTopRecordsToUniformGridBorders(TopCurrent, GameOverScoresTable);
        }

        private async void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            GameOverScoreText.Text = $"Your Score: ";
            GameOverLineText.Text = $"Your Lines: ";
            ScoreTextBlock.Text = $"Score: 0";
            LineTextBlock.Text = $"Lines: 0";
            GameOverScoreText.Foreground = new SolidColorBrush(Colors.Black);
            GameOverLineText.Foreground = new SolidColorBrush(Colors.Black);
            GameOverScoreNum.Foreground = new SolidColorBrush(Colors.Black);
            GameOverLineNum.Foreground = new SolidColorBrush(Colors.Black);
            ScoreTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            LineTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            GameOverBorder.Visibility = Visibility.Collapsed;
            CountDownText.Visibility = Visibility.Visible;
            Game = new GameMain(rows, hiddenRowsOnTop, columns);
            DrawGrid();
            await GameStartToEnd();
        }

        private async void Resume_Click(object sender, RoutedEventArgs e)
        {
            MainMenuBorder.Visibility = Visibility.Collapsed;
            CountDownText.Visibility = Visibility.Visible;
            await ShowCountDown();
        }

        private void Window_Activated(object sender, EventArgs e)
            => windowActivated = true;

        private void Window_Deactivated(object sender, EventArgs e)
        {
            windowActivated = false;

            if (!paused)
                PauseGame();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
            => PauseGame();

        private void PauseGame()
        {
            paused = true;
            Pause_AI_Grid.Visibility = Visibility.Collapsed;
            AISpeedSlider.Visibility = Visibility.Collapsed;
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
            CollapseAI_Elements();
            CountDownText.Visibility = Visibility.Visible;
            StartBorder.Visibility = Visibility.Visible;

            if(!aiWasActivatedDuringGame)
                NewTopRecords();

            Game = new GameMain(rows, hiddenRowsOnTop, columns);
            aiActivated = false;
            DrawGrid();
        }

        private void CancelQuit_Click(object sender, RoutedEventArgs e)
        {
            QuitMenuBorder.Visibility = Visibility.Collapsed;
            MainMenuBorder.Visibility = Visibility.Visible;
        }

        private void AI_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!aiActivated)
            {
                aiWasActivatedDuringGame = true;
                ScoreTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                LineTextBlock.Foreground = new SolidColorBrush(Colors.Red);

                AI_Button.Content = "STOP";
                PauseButton.FontSize = 20;
                AI_Button.FontSize = 20;

                GameInfoGrid.RowDefinitions.Clear();

                GameInfoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.4, GridUnitType.Star) });
                GameInfoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.2, GridUnitType.Star) });
                GameInfoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.2, GridUnitType.Star) });
                GameInfoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.2, GridUnitType.Star) });

                Grid.SetRow(Pause_AI_Grid, 3);

                AISpeedSlider.Visibility = Visibility.Visible;

                Pause_AI_Grid.Margin = new Thickness(20, 0, 20, 20);

                AI_Game = new AI_Game(Game);
                aiActivated = true;
            }
            else
            {
                CollapseAI_Elements();
                aiActivated = false;
            }

            Game.DeactivateFastDrop();
            leftIsPressed = false;
            rightIsPressed = false;
        }

        private void CollapseAI_Elements()
        {
            AI_Button.Content = "AI";
            PauseButton.FontSize = 30;
            AI_Button.FontSize = 30;

            GameInfoGrid.RowDefinitions.Clear();

            GameInfoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.4, GridUnitType.Star) });
            GameInfoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.3, GridUnitType.Star) });
            GameInfoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.3, GridUnitType.Star) });

            AISpeedSlider.Visibility = Visibility.Collapsed;

            Grid.SetRow(Pause_AI_Grid, 2);

            Pause_AI_Grid.Margin = new Thickness(20, 20, 20, 20);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            aiSpeed = (int)AISpeedSlider.Value;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            GameOverScoreText.Text = $"Your Score: ";
            GameOverLineText.Text = $"Your Lines: ";
            ScoreTextBlock.Text = $"Score: 0";
            LineTextBlock.Text = $"Lines: 0";
            ScoreTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            LineTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            GameOverScoreText.Foreground = new SolidColorBrush(Colors.Black);
            GameOverLineText.Foreground = new SolidColorBrush(Colors.Black);
            GameOverScoreNum.Foreground = new SolidColorBrush(Colors.Black);
            GameOverLineNum.Foreground = new SolidColorBrush(Colors.Black);
            GameOverBorder.Visibility = Visibility.Collapsed;
            CountDownText.Visibility = Visibility.Visible;
            StartBorder.Visibility = Visibility.Visible;
            Game = new GameMain(rows, hiddenRowsOnTop, columns);
            DrawGrid();
        }
    }
}