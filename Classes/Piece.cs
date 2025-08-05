using OthelloWPF;

public class Piece : IPiece
{
    public ColorType Color { get; }

    public Piece(ColorType color)
    {
        Color = color;
    }
}