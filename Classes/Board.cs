using OthelloWPF;

public class Board : IBoard
{
    public IPiece[,] Grid { get; set; }

    public Board()
    {
        Grid = new IPiece[8, 8];
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                Grid[row, col] = new Piece(ColorType.None);
            }
        }

        Grid[3, 3] = new Piece(ColorType.White);
        Grid[3, 4] = new Piece(ColorType.Black);
        Grid[4, 3] = new Piece(ColorType.Black);
        Grid[4, 4] = new Piece(ColorType.White);

    }
}