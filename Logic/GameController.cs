using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace OthelloWPF
{
    public class GameController : INotifyPropertyChanged
    {
        private Board _board;
        private Dictionary<IPlayer, IPiece> _players;
        private int[,] _directions;
        private IPlayer _currentPlayer;
        private bool _gameStarted;
        private bool _gameEnded;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<string>? OnGameEnded;
        public event Action<string>? OnTurnChanged;

        // Public properties for UI binding
        public IPlayer CurrentPlayer => _currentPlayer;
        public bool GameStarted => _gameStarted;
        public bool GameEnded => _gameEnded;
        public IPlayer Player1 => _players.Keys.First();
        public IPlayer Player2 => _players.Keys.Skip(1).First();

        private string _currentMessage = "";
        public string CurrentMessage
        {
            get => _currentMessage;
            private set
            {
                _currentMessage = value;
                OnPropertyChanged(nameof(CurrentMessage));
            }
        }

        private List<Position> _validMoves = new List<Position>();
        public List<Position> ValidMoves
        {
            get => _validMoves;
            private set
            {
                _validMoves = value;
                OnPropertyChanged(nameof(ValidMoves));
            }
        }

        public GameController(IPlayer player1, IPlayer player2, IPiece piece1, IPiece piece2, Board board)
        {
            _board = board;
            _players = new Dictionary<IPlayer, IPiece>
            {
                { player1, piece1 },
                { player2, piece2 }
            };

            _directions = new int[,]
            {
                { -1, -1 }, { -1, 0 }, { -1, 1 },
                { 0, -1 },           { 0, 1 },
                { 1, -1 }, { 1, 0 }, { 1, 1 }
            };

            _currentPlayer = player1;
            _gameStarted = false;
            _gameEnded = false;
        }

        // Main method that UI calls - returns the current board state after processing move
        public GameMoveResult ProcessMove(int row, int col)
        {
            var result = new GameMoveResult();

            if (_gameEnded || !_gameStarted)
            {
                result.IsValidMove = false;
                result.Message = "Game not started or already ended";
                result.Board = GetCurrentBoardState();
                result.ValidMoves = new List<Position>();
                result.CurrentPlayer = _currentPlayer;
                result.GameEnded = _gameEnded;
                return result;
            }

            var position = new Position(row, col);

            if (!_validMoves.Contains(position))
            {
                result.IsValidMove = false;
                result.Message = $"Invalid move! Position ({row},{col}) is not valid.";
                result.Board = GetCurrentBoardState();
                result.ValidMoves = _validMoves;
                result.CurrentPlayer = _currentPlayer;
                result.GameEnded = _gameEnded;
                return result;
            }

            // Apply the move
            ApplyMove(position, new Dictionary<IPlayer, IPiece> { { _currentPlayer, _players[_currentPlayer] } });
            UpdateScore();

            // Check if game is over
            if (IsGameOver())
            {
                EndGame();
                result.IsValidMove = true;
                result.Message = CurrentMessage;
                result.Board = GetCurrentBoardState();
                result.ValidMoves = new List<Position>();
                result.CurrentPlayer = _currentPlayer;
                result.GameEnded = true;
                return result;
            }

            // Switch turns and update valid moves
            SwitchTurn();
            UpdateValidMoves();

            // Handle case where current player has no valid moves
            if (_validMoves.Count == 0)
            {
                CurrentMessage = $"No valid moves for {_currentPlayer.UserName}. Skipping turn.";
                SwitchTurn();
                UpdateValidMoves();

                if (_validMoves.Count == 0)
                {
                    EndGame();
                    result.IsValidMove = true;
                    result.Message = CurrentMessage;
                    result.Board = GetCurrentBoardState();
                    result.ValidMoves = new List<Position>();
                    result.CurrentPlayer = _currentPlayer;
                    result.GameEnded = true;
                    return result;
                }
            }

            // Move was successful
            CurrentMessage = $"{_currentPlayer.UserName}'s turn";
            OnTurnChanged?.Invoke($"{_currentPlayer.UserName}'s turn ({_players[_currentPlayer].Color})");

            result.IsValidMove = true;
            result.Message = CurrentMessage;
            result.Board = GetCurrentBoardState();
            result.ValidMoves = _validMoves;
            result.CurrentPlayer = _currentPlayer;
            result.GameEnded = false;

            return result;
        }

        // Method to get current board state without making a move
        public GameMoveResult GetCurrentGameState()
        {
            var result = new GameMoveResult
            {
                IsValidMove = true, 
                Message = CurrentMessage,
                Board = GetCurrentBoardState(),
                ValidMoves = _validMoves,
                CurrentPlayer = _currentPlayer,
                GameEnded = _gameEnded
            };

            return result;
        }

        // Start a new game and return initial state
        public GameMoveResult StartGame()
        {
            _gameStarted = true;
            _gameEnded = false;

            UpdateScore();
            UpdateValidMoves();

            CurrentMessage = $"Game started! {_currentPlayer.UserName}'s turn";
            OnTurnChanged?.Invoke($"{_currentPlayer.UserName}'s turn ({_players[_currentPlayer].Color})");

            OnPropertyChanged(nameof(GameStarted));
            OnPropertyChanged(nameof(GameEnded));

            var result = new GameMoveResult
            {
                IsValidMove = true,
                Message = CurrentMessage,
                Board = GetCurrentBoardState(),
                ValidMoves = _validMoves,
                CurrentPlayer = _currentPlayer,
                GameEnded = false
            };

            return result;
        }

        // Reset game and return initial state
        public GameMoveResult ResetGame()
        {
            _board.ResetBoard();
            foreach (var player in _players.Keys)
            {
                player.Score = 2;
            }
            _currentPlayer = _players.Keys.First();
            _gameStarted = false;
            _gameEnded = false;
            _validMoves.Clear();
            CurrentMessage = "Game reset. Click 'New Game' to start.";

            OnPropertyChanged(nameof(GameStarted));
            OnPropertyChanged(nameof(GameEnded));
            OnPropertyChanged(nameof(CurrentPlayer));
            OnPropertyChanged(nameof(Player1));
            OnPropertyChanged(nameof(Player2));

            var result = new GameMoveResult
            {
                IsValidMove = true,
                Message = CurrentMessage,
                Board = GetCurrentBoardState(),
                ValidMoves = new List<Position>(),
                CurrentPlayer = _currentPlayer,
                GameEnded = false
            };

            return result;
        }

        // Helper method to get current board state as a clean 2D array
        private ColorType[,] GetCurrentBoardState()
        {
            var boardState = new ColorType[8, 8];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    boardState[row, col] = _board.Grid[row, col].Color;
                }
            }
            return boardState;
        }

        // Original private methods remain mostly unchanged
        private void UpdateScore()
        {
            foreach (var player in _players.Keys)
            {
                int count = 0;
                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        if (_board.Grid[row, col].Color == _players[player].Color)
                        {
                            count++;
                        }
                    }
                }
                player.Score = count;
            }

            OnPropertyChanged(nameof(Player1));
            OnPropertyChanged(nameof(Player2));
        }

        private void UpdateValidMoves()
        {
            ValidMoves = GetValidMoves(_board, new Dictionary<IPlayer, IPiece> { { _currentPlayer, _players[_currentPlayer] } });
        }

        private List<Position> GetValidMoves(IBoard board, Dictionary<IPlayer, IPiece> player)
        {
            var validMoves = new List<Position>();
            var playerPiece = player.Values.First();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board.Grid[row, col].Color == ColorType.None)
                    {
                        var flippedPositions = GetFlippedPositions(board, row, col, player);
                        if (flippedPositions.Count > 0)
                        {
                            validMoves.Add(new Position(row, col));
                        }
                    }
                }
            }

            return validMoves;
        }

        private List<Position> GetFlippedPositions(IBoard board, int row, int col, Dictionary<IPlayer, IPiece> player)
        {
            var flippedPositions = new List<Position>();
            var playerColor = player.Values.First().Color;
            var opponentColor = GetOpponentColor(playerColor);

            for (int dir = 0; dir < 8; dir++)
            {
                var tempFlipped = new List<Position>();
                int currentRow = row + _directions[dir, 0];
                int currentCol = col + _directions[dir, 1];

                while (IsValidPosition(currentRow, currentCol) &&
                       board.Grid[currentRow, currentCol].Color == opponentColor)
                {
                    tempFlipped.Add(new Position(currentRow, currentCol));
                    currentRow += _directions[dir, 0];
                    currentCol += _directions[dir, 1];
                }

                if (tempFlipped.Count > 0 &&
                    IsValidPosition(currentRow, currentCol) &&
                    board.Grid[currentRow, currentCol].Color == playerColor)
                {
                    flippedPositions.AddRange(tempFlipped);
                }
            }

            return flippedPositions;
        }

        private void ApplyMove(Position pos, Dictionary<IPlayer, IPiece> player)
        {
            var playerPiece = player.Values.First();
            var flippedPositions = GetFlippedPositions(_board, pos.Row, pos.Col, player);

            _board[pos.Row, pos.Col] = new Piece(playerPiece.Color);

            foreach (var flipPos in flippedPositions)
            {
                _board[flipPos.Row, flipPos.Col] = new Piece(playerPiece.Color);
            }
        }

        private void SwitchTurn()
        {
            _currentPlayer = _players.Keys.First(p => p != _currentPlayer);
            OnPropertyChanged(nameof(CurrentPlayer));
        }

        private void EndGame()
        {
            _gameEnded = true;
            UpdateScore();

            var winner = _players.Keys.OrderByDescending(p => p.Score).First();
            var loser = _players.Keys.OrderBy(p => p.Score).First();

            string message;
            if (winner.Score == loser.Score)
            {
                message = $"Game ended in a tie! Both players have {winner.Score} pieces.";
                CurrentMessage = "It's a TIE!";
            }
            else
            {
                message = $"Game Over! Winner: {winner.UserName} with {winner.Score} pieces! Final Score: {winner.UserName} {winner.Score} - {loser.Score} {loser.UserName}";
                CurrentMessage = $" WINNER: {winner.UserName}!";
            }

            OnPropertyChanged(nameof(GameEnded));
            OnGameEnded?.Invoke(message);
        }

        private bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        private ColorType GetOpponentColor(ColorType color)
        {
            return color == ColorType.Black ? ColorType.White : ColorType.Black;
        }

        private bool IsGameOver()
        {
            foreach (var playerPair in _players)
            {
                var validMoves = GetValidMoves(_board, new Dictionary<IPlayer, IPiece> { { playerPair.Key, playerPair.Value } });
                if (validMoves.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ColorType GetPieceColorAt(int row, int col)
        {
            if (IsValidPosition(row, col))
            {
                return _board.Grid[row, col].Color;
            }
            return ColorType.None;
        }

        public ColorType GetCurrentPlayerColor()
        {
            return _players[_currentPlayer].Color;
        }

        public bool IsHighlightedPosition(int row, int col)
        {
            return _validMoves.Contains(new Position(row, col));
        }
    }

    // New result class to encapsulate all game state information
    public class GameMoveResult
    {
        public bool IsValidMove { get; set; }
        public string Message { get; set; } = string.Empty;
        public ColorType[,] Board { get; set; } = new ColorType[8, 8];
        public List<Position> ValidMoves { get; set; } = new List<Position>();
        public IPlayer CurrentPlayer { get; set; }
        public bool GameEnded { get; set; }
    }
}