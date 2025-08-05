using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            // Show player name dialog
            var dialog = new PlayerNameDialog();
            if (dialog.ShowDialog() == true)
            {
                var player1 = new Player(dialog.Player1Name);
                var player2 = new Player(dialog.Player2Name);
                var blackPiece = new Piece(ColorType.Black);
                var whitePiece = new Piece(ColorType.White);

                _gameController = new GameController(player1, player2, blackPiece, whitePiece);

                _gameController.OnBoardUpdated += UpdateBoardDisplay;
                _gameController.OnGameEnded += OnGameEnded;
                _gameController.OnTurnChanged += OnTurnChanged;
                _gameController.OnValidMovesChanged += OnValidMovesChanged;
                _gameController.OnMessageChanged += OnMessageChanged;

                Player1Name.Text = player1.UserName;
                Player2Name.Text = player2.UserName;

                CurrentPlayerText.Text = $"{player1.UserName}'s Turn";
                CurrentMessage.Text = "Click 'New Game' to start playing!";
            }
            else
            {
                // Default players if dialog is cancelled
                var player1 = new Player("Player 1");
                var player2 = new Player("Player 2");
                var blackPiece = new Piece(ColorType.Black);
                var whitePiece = new Piece(ColorType.White);

                _gameController = new GameController(player1, player2, blackPiece, whitePiece);

                _gameController.OnBoardUpdated += UpdateBoardDisplay;
                _gameController.OnGameEnded += OnGameEnded;
                _gameController.OnTurnChanged += OnTurnChanged;
                _gameController.OnValidMovesChanged += OnValidMovesChanged;
                _gameController.OnMessageChanged += OnMessageChanged;

                Player1Name.Text = "Player 1";
                Player2Name.Text = "Player 2";
                CurrentPlayerText.Text = "Player 1's Turn";
                CurrentMessage.Text = "Click 'New Game' to start playing!";
            }
        }

        private void CreateGameBoard()
        {
            _boardButtons = new Button[BOARD_SIZE, BOARD_SIZE];
            GameBoardGrid.Children.Clear();

            // Create grid rows and columns
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
                        Content = new Grid(), // Container for the piece
                        Tag = new Position(row, col)
                    };

                    button.Click += BoardCell_Click;

                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, col);

                    GameBoardGrid.Children.Add(button);
                    _boardButtons[row, col] = button;
                }
            }

            UpdateBoardDisplay();
        }

        private void BoardCell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Position pos)
            {
                _gameController.TryMakeMove(pos.Row, pos.Col);
            }
        }

        private void UpdateBoardDisplay()
        {
            if (_gameController == null || _boardButtons == null) return;

            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    var button = _boardButtons[row, col];
                    var grid = (Grid)button.Content;
                    grid.Children.Clear();

                    var pieceColor = _gameController.GetPieceColorAt(row, col);

                    if (pieceColor != ColorType.None)
                    {
                        var piece = new Ellipse
                        {
                            Style = (Style)FindResource("GamePieceStyle"),
                            Fill = pieceColor == ColorType.Black ? Brushes.Black : Brushes.White
                        };

                        if (pieceColor == ColorType.White)
                        {
                            piece.Stroke = Brushes.Black;
                            piece.StrokeThickness = 2;
                        }

                        grid.Children.Add(piece);
                    }

                    if (_gameController.IsHighlightedPosition(row, col))
                    {
                        button.Background = new SolidColorBrush(Color.FromRgb(144, 238, 144)); 

                        var indicator = new Ellipse
                        {
                            Width = 15,
                            Height = 15,
                            Fill = new SolidColorBrush(Color.FromRgb(0, 100, 0)),
                            Opacity = 0.7
                        };
                        grid.Children.Add(indicator);
                    }
                    else
                    {
                        button.Background = new SolidColorBrush(Color.FromRgb(34, 139, 34)); // Forest green
                    }
                }
            }

            UpdateScoreDisplay();
        }

        private void UpdateScoreDisplay()
        {
            if (_gameController == null) return;

            Player1Score.Text = $"Score: {_gameController.Player1.Score}";
            Player2Score.Text = $"Score: {_gameController.Player2.Score}";
        }

        private void OnGameEnded(string message)
        {
            MessageBox.Show(message, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnTurnChanged(string turnInfo)
        {
            CurrentPlayerText.Text = turnInfo;
        }

        private void OnValidMovesChanged(System.Collections.Generic.List<Position> validMoves)
        {
            // Update board highlighting - this is handled in UpdateBoardDisplay
            UpdateBoardDisplay();
        }

        private void OnMessageChanged(string message)
        {
            CurrentMessage.Text = message;
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            _gameController.ResetBoard();
            _gameController.StartGame();
            UpdateBoardDisplay();
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
                _gameController.ResetBoard();
                CreateGameBoard();
            }
        }

        private void ChangePlayersButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PlayerNameDialog();
            if (dialog.ShowDialog() == true)
            {
                // Create new game controller with new players
                var player1 = new Player(dialog.Player1Name);
                var player2 = new Player(dialog.Player2Name);
                var blackPiece = new Piece(ColorType.Black);
                var whitePiece = new Piece(ColorType.White);

                _gameController = new GameController(player1, player2, blackPiece, whitePiece);

                // Resubscribe to events
                _gameController.OnBoardUpdated += UpdateBoardDisplay;
                _gameController.OnGameEnded += OnGameEnded;
                _gameController.OnTurnChanged += OnTurnChanged;
                _gameController.OnValidMovesChanged += OnValidMovesChanged;
                _gameController.OnMessageChanged += OnMessageChanged;

                // Update UI
                Player1Name.Text = player1.UserName;
                Player2Name.Text = player2.UserName;

                CreateGameBoard();
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
                          "CONTROLS:\n" +
                          "• Click on highlighted squares to make a move\n" +
                          "• Black pieces always go first\n" +
                          "• White pieces go second\n\n" +
                          "Good luck and have fun!";

            MessageBox.Show(rules, "Game Rules", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}