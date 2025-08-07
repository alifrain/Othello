using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OthelloWPF
{
    public class ObservableBoard : IBoard, INotifyPropertyChanged
    {
        private IPiece[,] _grid;

        public IPiece[,] Grid
        {
            get => _grid;
            private set
            {
                _grid = value;
                OnPropertyChanged();
            }
        }

        public IPiece this[int row, int col]
        {
            get => _grid[row, col];
            set
            {
                if (_grid[row, col]?.Color != value?.Color)
                {
                    _grid[row, col] = value;
                    OnPropertyChanged($"Item[{row},{col}]");
                    OnPropertyChanged(nameof(Grid));
                    BoardChanged?.Invoke();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event System.Action? BoardChanged;

        public ObservableBoard()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            _grid = new IPiece[8, 8];

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    _grid[row, col] = new Piece(ColorType.None);
                }
            }

            _grid[3, 3] = new Piece(ColorType.White);
            _grid[3, 4] = new Piece(ColorType.Black);
            _grid[4, 3] = new Piece(ColorType.Black);
            _grid[4, 4] = new Piece(ColorType.White);
        }

        public void ResetBoard()
        {
            InitializeBoard();
            OnPropertyChanged(nameof(Grid));
            BoardChanged?.Invoke();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}