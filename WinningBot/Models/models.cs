namespace WinningBot.Models
{
    public class JsonData
    {
        public string Data { get; set; }
    }
    public class Game
    {
        public State state { get; set; }
        public string player { get; set; }
    }

    public class State
    {
        public int rows { get; set; }
        public int cols { get; set; }
        public Player p1 { get; set; }
        public Player p2 { get; set; }
        public string grid { get; set; }
        public int maxTurns { get; set; }
        public int turnsElapsed { get; set; }
        
    }

    public class Player
    {
        public int energy { get; set; }
        public int spawn { get; set; }
    }

    public class Coord
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Move
    {
        public int from { get; set; }
        public int to { get; set; }

        public Move(int f, int t)
        {
            from = f;
            to = t;
        }
    }
}