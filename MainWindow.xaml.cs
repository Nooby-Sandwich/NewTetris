using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NewTetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ImageSource[] tileImage = new ImageSource[]
        {
            new BitmapImage(new Uri("GameAssets/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/TileRed.png", UriKind.Relative))

        };

        private readonly ImageSource[] blockImages = new ImageSource[]
        {
            new BitmapImage(new Uri("GameAssets/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("GameAssets/Block-Z.png", UriKind.Relative))

        };

        private readonly Image[,] imageControls;
        private readonly int maxDelay = 1800;   // Increase this value for a slower initial speed
        private readonly int minDelay = 57;    // Decrease this value for a faster minimum speed
        private int maxSpeedLevel = 5; // Assigned but not used, consider removing if unnecessary
        private int SpeedLevel
        {
            get
            {
                const int speedIncreaseThreshold1 = 500;
                const int speedIncreaseThreshold2 = 1000;

                if (gameState.Score < speedIncreaseThreshold1)
                {
                    // Gradual increase for scores under 500
                    return gameState.Score / 50;
                }
                else if (gameState.Score < speedIncreaseThreshold2)
                {
                    // Gradual increase for scores between 500 and 1000
                    return (gameState.Score - speedIncreaseThreshold1) / 50 + 10;
                }
                else
                {
                    // Gradual increase for scores above 1000
                    return (gameState.Score - speedIncreaseThreshold2) / 50 + 15;
                }
            }
        }




        private GameState gameState = new GameState();

        private Image[,] SetupGameCanvas(GameGrid grid)
        {
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];
            int cellSize = 25;

            for(int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    Image imageControl = new Image
                    {
                        Width = cellSize,
                        Height = cellSize
                    };
                    Canvas.SetTop(imageControl, (r - 2) * cellSize + 10);
                    Canvas.SetLeft(imageControl, c * cellSize);
                    GameCanvas.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }

            return imageControls;
        }

        public MainWindow()
        {
            InitializeComponent();
            imageControls = SetupGameCanvas(gameState.GameGrid);
        }


        private void DrawGrid(GameGrid grid)
        {
            for(int r = 0; r < grid.Rows; r++)
            {
                for(int c = 0; c < grid.Columns; c++)
                {
                    int id = grid[r, c];
                    imageControls[r, c].Opacity = 1;
                    imageControls[r, c].Source = tileImage[id];
                }
            }
        }

        private void DrawBlock(Block block)
        {
            foreach(Position p in block.TilePosition())
            {
                imageControls[p.Row, p.Column].Opacity = 1;
                imageControls[p.Row, p.Column].Source = tileImage[block.Id];
            }
        }

        private void DrawNextBlock(BlockQueue blockQueue)
        {
            Block next = blockQueue.NextBlock;
            NextImage.Source = blockImages[next.Id];
        }
        
        private void DrawHeldBlock(Block heldBlock)
        {
            if(heldBlock == null)
            {
                HoldImage.Source = blockImages[0];
            }
            else
            {
                HoldImage.Source = blockImages[heldBlock.Id];
            }
        }

        private void DrawGhostBlock(Block block)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePosition())
            {
                imageControls[p.Row + dropDistance, p.Column].Opacity = 0.25;
                imageControls[p.Row + dropDistance, p.Column].Source = tileImage[block.Id];

            }

        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                Draw(gameState);

                int delay = Math.Max(minDelay, maxDelay - (SpeedLevel * 20)); // Adjust the multiplier as needed
                await Task.Delay(delay);

                gameState.MoveBlockDown();

                if (gameState.GameOver)
                {
                    Draw(gameState);
                    GameOverMenu.Visibility = Visibility.Visible;
                    FinalScoreText.Text = $"Score: {gameState.Score}";
                }
            }
        }


        private void Draw(GameState gameState)
        {
            DrawGrid(gameState.GameGrid);
            DrawGhostBlock(gameState.CurrentBlock);
            DrawBlock(gameState.CurrentBlock);
            DrawNextBlock(gameState.BlockQueue);
            DrawHeldBlock(gameState.HeldBlock);
            ScoreText.Text = $"Score: {gameState.Score}";
        }

        



        private void Window_keyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                case Key.A:
                    gameState.MoveBlockLeft();
                    break;

                case Key.Right:
                case Key.D:
                    gameState.MoveBlockRight();
                    break;

                case Key.Down:
                case Key.S:
                    gameState.MoveBlockDown();
                    break;

                case Key.Up:
                case Key.W:
                    gameState.RotateClockWise();
                    break;

                case Key.RightCtrl:
                case Key.E:
                    gameState.RotateCounterClockWise();
                    break;

                case Key.C:
                case Key.RightShift:
                    gameState.HoldBlock();
                    break;

                case Key.Space:
                case Key.Enter:
                    gameState.DropBlock();
                    break;

                default:
                    return;
            }

            Draw(gameState);
        }


        private async void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            await GameLoop();
        }

        private async void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            gameState = new GameState();
            GameOverMenu.Visibility = Visibility.Hidden;
            await GameLoop();
        }

    }
}