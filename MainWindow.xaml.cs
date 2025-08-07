using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace OthelloWPF
{
    public partial class MainWindow : Window
    {
        private GameController _gameController;
        private Button[,] _boardButtons;
        private const int BOARD_SIZE = 8;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            CreateGameBoard();
        }

        private void InitializeGame()
        {
            var dialog = new PlayerNameDialog();
            if (dialog.ShowDialog() == true)
            {
                var player1 = new Player(dialog.Player1Name);
                var player2 = new Player(dialog.Player2Name);
                var blackPiece = new Piece(ColorType.Black);
                var whitePiece = new Piece(ColorType.White);
                var board = new Board();

                _gameController = new GameController(player1, player2, blackPiece, whitePiece, board);
                _gameController.OnGameEnded += OnGameEnded;

                this.DataContext = _gameController;

                Player1Name.Text = player1.UserName;
                Player2Name.Text = player2.UserName;

                // Get initial game state and render UI
                var initialState = _gameController.GetCurrentGameState();
                RenderGameState(initialState);
            }
            else
            {
                var player1 = new Player("Player 1");
                var player2 = new Player("Player 2");
                var blackPiece = new Piece(ColorType.Black);
                var whitePiece = new Piece(ColorType.White);
                var board = new Board();

                _gameController = new GameController(player1, player2, blackPiece, whitePiece, board);
                _gameController.OnGameEnded += OnGameEnded;

                this.DataContext = _gameController;

                Player1Name.Text = "Player 1";
                Player2Name.Text = "Player 2";

                // Get initial game state and render UI
                var initialState = _gameController.GetCurrentGameState();
                RenderGameState(initialState);
            }
        }

        private void CreateGameBoard()
        {
            _boardButtons = new Button[BOARD_SIZE, BOARD_SIZE];
            GameBoardGrid.Children.Clear();

            GameBoardGrid.RowDefinitions.Clear();
            GameBoardGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                GameBoardGrid.RowDefinitions.Add(new RowDefinition());
                GameBoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    var button = new Button
                    {
                        Style = (Style)FindResource("BoardCellStyle"),
                        Content = new Grid(),
                        Tag = new Position(row, col)
                    };

                    button.Click += BoardCell_Click;

                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, col);

                    GameBoardGrid.Children.Add(button);
                    _boardButtons[row, col] = button;
                }
            }
        }

        private void BoardCell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Position pos)
            {
                var moveResult = _gameController.ProcessMove(pos.Row, pos.Col);
                RenderGameState(moveResult);
            }
        }

        // method untuk merender game state untuk mengupdate UI
        private void RenderGameState(GameMoveResult gameState)
        {
            if (gameState == null || _boardButtons == null) return;

            RenderBoard(gameState.Board); // uses array board from gc

            RenderValidMoves(gameState.ValidMoves); // uses list of valid moves from gc

            UpdateUIText(gameState); // updates current player and message

            UpdateScoreDisplay(); // updates player scores
        }

        private void RenderBoard(ColorType[,] boardState)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    var button = _boardButtons[row, col];
                    var grid = (Grid)button.Content;
                    grid.Children.Clear();

                    // Reset background to default
                    button.Background = new SolidColorBrush(Color.FromRgb(34, 139, 34));

                    // Render piece based on board state
                    var pieceColor = boardState[row, col];
                    if (pieceColor != ColorType.None)
                    {
                        var pieceEllipse = new Ellipse
                        {
                            Style = (Style)FindResource("GamePieceStyle"),
                            Fill = pieceColor == ColorType.Black ? Brushes.Black : Brushes.White
                        };

                        if (pieceColor == ColorType.White)
                        {
                            pieceEllipse.Stroke = Brushes.Black;
                            pieceEllipse.StrokeThickness = 2;
                        }

                        grid.Children.Add(pieceEllipse);
                    }
                }
            }
        }

        private void RenderValidMoves(System.Collections.Generic.List<Position> validMoves)
        {
            foreach (var move in validMoves)
            {
                var button = _boardButtons[move.Row, move.Col];
                var grid = (Grid)button.Content;

                // Highlight valid move positions
                button.Background = new SolidColorBrush(Color.FromRgb(144, 238, 144));

                // Add indicator dot
                var indicator = new Ellipse
                {
                    Width = 15,
                    Height = 15,
                    Fill = new SolidColorBrush(Color.FromRgb(0, 100, 0)),
                    Opacity = 0.7
                };
                grid.Children.Add(indicator);
            }
        }

        private void UpdateUIText(GameMoveResult gameState)
        {
            CurrentMessage.Text = gameState.Message;

            if (!gameState.GameEnded && gameState.CurrentPlayer != null)
            {
                var playerColor = _gameController.GetCurrentPlayerColor();
                CurrentPlayerText.Text = $"{gameState.CurrentPlayer.UserName}'s turn ({playerColor})";
            }
        }

        private void UpdateScoreDisplay()
        {
            if (_gameController == null) return;

            Player1Score.Text = $"Score: {_gameController.Player1.Score}";
            Player2Score.Text = $"Score: {_gameController.Player2.Score}";
        }

        private void OnGameEnded(string message)
        {
            CurrentPlayerText.Foreground = Brushes.White;
            CurrentMessage.Foreground = Brushes.White;

            MessageBox.Show(message, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Get game state after starting new game and render it
            var newGameState = _gameController.StartGame();
            RenderGameState(newGameState);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to restart the game?",
                                       "Restart Game",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Get game state after reset and render it
                var resetState = _gameController.ResetGame();
                RenderGameState(resetState);

                // Recreate board to ensure clean state
                CreateGameBoard();
                RenderGameState(resetState);
            }
        }

        private void ChangePlayersButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PlayerNameDialog();
            if (dialog.ShowDialog() == true)
            {
                if (_gameController != null)
                {
                    _gameController.OnGameEnded -= OnGameEnded;
                }

                var player1 = new Player(dialog.Player1Name);
                var player2 = new Player(dialog.Player2Name);
                var blackPiece = new Piece(ColorType.Black);
                var whitePiece = new Piece(ColorType.White);
                var board = new Board();

                _gameController = new GameController(player1, player2, blackPiece, whitePiece, board);
                _gameController.OnGameEnded += OnGameEnded;

                this.DataContext = _gameController;

                Player1Name.Text = player1.UserName;
                Player2Name.Text = player2.UserName;

                CreateGameBoard();

                // Get initial game state and render it
                var initialState = _gameController.GetCurrentGameState();
                RenderGameState(initialState);

                CurrentMessage.Text = "New players set! Click 'New Game' to start playing!";
            }
        }

        private void ShowRulesButton_Click(object sender, RoutedEventArgs e)
        {
            string rules = "OTHELLO RULES \n\n" +
                          "OBJECTIVE:\n" +
                          "Have the most pieces of your color on the board when the game ends.\n\n" +
                          "HOW TO PLAY:\n" +
                          "• Players take turns placing pieces on the board\n" +
                          "• You must place your piece to 'sandwich' opponent pieces\n" +
                          "• All opponent pieces between your new piece and existing pieces get flipped\n" +
                          "• Valid moves are highlighted in light green\n" +
                          "• If you have no valid moves, your turn is skipped\n" +
                          "• Game ends when neither player can move\n\n" +
                          "VISUAL INDICATORS:\n" +
                          "• The current player's info box is highlighted with a colored border\n" +
                          "• The turn indicator box changes color based on the current player\n" +
                          "• Dark colors indicate Black player's turn\n" +
                          "• Light colors indicate White player's turn\n\n" +
                          "CONTROLS:\n" +
                          "• Click on highlighted squares to make a move\n" +
                          "• Black pieces always go first\n" +
                          "• White pieces go second\n\n" +
                          "Good luck and have fun!";

            MessageBox.Show(rules, "Game Rules", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}