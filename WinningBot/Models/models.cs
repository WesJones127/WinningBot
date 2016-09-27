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
        public int spawn { get; set; }
        public int enemyEnergy { get; set; }
        public int enemySpawn { get; set; }
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

        public Coord(int x, int y, int? id = null)
        {
            id = id;
            X = x;
            Y = y;
        }

        /// <summary>
        /// Use boolean to determine which Move variable we want to create the Coord from
        /// Using just an Int would conflict with the default constructor
        /// </summary>
        /// <param name="move"></param>
        /// <param name="moveToIndex"></param>
        /// <param name="cols"></param>
        public Coord(Move move, bool moveToIndex, int cols)
        {
            int playerIndex = (moveToIndex) ? move.to : move.from;

            X = playerIndex % cols;
            Y = playerIndex / cols;
        }

        public Coord Copy()
        {
            return new Coord(X, Y);
        }

        public bool EqualTo(Coord otherCoord)
        {
            return X == otherCoord.X && Y == otherCoord.Y;
        }

        public int ToIndex(int cols)
        {
            double index = cols * Y + X;
            return Convert.ToInt32(index);
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

        public Move(int f, int t)
        {
            from = f;
            to = t;
        }

        public string Print()
        {
            return "From " + from + " to " + to;
        }
    }
}