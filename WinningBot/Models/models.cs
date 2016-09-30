using System;
using System.Collections.Generic;
using WinningBot.Strategies;

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
        public GridData gridData { get; set; }

        public Game()
        {
            gridData = new GridData();
        }
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

    public class GridData
    {
        public int energy { get; set; }
        public Coord spawnPoint { get; set; }
        public int enemyEnergy { get; set; }
        public Coord enemySpawn { get; set; }
        public List<Coord> playerCoords { get; set; }
        public List<Coord> enemyCoords { get; set; }
        public List<Coord> energyCoords { get; set; }
        public List<Coord> occupiedCoords { get; set; }
        public List<Coord> enemyCoordsIncludingPossibleMoves { get; set; }

        public GridData()
        {
            playerCoords = new List<Coord>();
            enemyCoords = new List<Coord>();
            energyCoords = new List<Coord>();
            occupiedCoords = new List<Coord>();
            enemyCoordsIncludingPossibleMoves = new List<Coord>();
        }
    }

    public class Coord
    {
        public int? id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Coord(int x, int y, int? coordId = null)
        {
            id = coordId;
            X = x;
            Y = y;
        }

        /// <summary>
        /// Use boolean to determine which Move variable we want to create the Coord from
        /// Using just an Int would conflict with the default constructor
        /// </summary>
        /// <param name="move"></param>
        /// <param name="useToIndex"></param>
        /// <param name="cols"></param>
        public Coord(Move move, bool useToIndex, int cols)
        {
            int playerIndex = (useToIndex) ? move.to : move.from;

            X = playerIndex % cols;
            Y = playerIndex / cols;
        }

        public Coord Copy()
        {
            return new Coord(X, Y);
        }

        public bool SameAs(Coord otherCoord)
        {
            return X == otherCoord.X && Y == otherCoord.Y;
        }

        public int ToIndex(int cols)
        {
            double index = cols * Y + X;
            return Convert.ToInt32(index);
        }

        public Coord AdjacentCoord(Direction direction)
        {
            switch (direction)
            {
                case Direction.UP:
                    return new Coord(X, Y - 1);
                case Direction.DOWN:
                    return new Coord(X, Y + 1);
                case Direction.LEFT:
                    return new Coord(X - 1, Y);
                case Direction.RIGHT:
                    return new Coord(X + 1, Y);
                case Direction.UP_LEFT:
                    return new Coord(X - 1, Y - 1);
                case Direction.UP_RIGHT:
                    return new Coord(X + 1, Y - 1);
                case Direction.DOWN_LEFT:
                    return new Coord(X - 1, Y + 1);
                case Direction.DOWN_RIGHT:
                    return new Coord(X + 1, Y + 1);
            }

            throw new Exception("Direction is missing");
        }

        public string Print()
        {
            return "(" + X + "," + Y + ")";
        }
    }
    public class Move
    {
        public int from { get; set; }
        public int to { get; set; }

        //public Move(Coord f, Coord t, int cols)
        //{
        //    from = f.ToIndex(cols);
        //    to = t.ToIndex(cols);
        //}

        //public Move(int f, int t)
        //{
        //    from = f;
        //    to = t;
        //}

        public Coord ToCoord(bool useFromIndex, int cols)
        {
            int index = (useFromIndex) ? from : to;
            return Util.ConvertIndexToCoord(index, cols, cols);
        }

        public string Print()
        {
            return "From " + from + " to " + to;
        }
    }

    public enum Direction
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        UP_LEFT,
        UP_RIGHT,
        DOWN_LEFT,
        DOWN_RIGHT
    }
}