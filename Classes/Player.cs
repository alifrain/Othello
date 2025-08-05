using OthelloWPF;

public class Player : IPlayer
{
    public string UserName { get; }
    public int Score { get; set; } = 2;

    public Player(string userName)
    {
        UserName = userName;
        Score = 0;
    }
}