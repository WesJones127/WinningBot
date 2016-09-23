using System;
using System.Collections.Generic;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public class BaseStrategy : IStrategy
    {
        public List<Move> getMoves(Game game)
        {
            int energy, spawn, enemyenergy, enemySpawn;
            List<Coord> playerCoords, enemyCoords;
            List<Move> moves = new List<Move>();

            if (game.player == "r")
            {
                energy = game.state.p1.energy;
                spawn = game.state.p1.spawn;
                enemyenergy = game.state.p2.energy;
                enemySpawn = game.state.p2.spawn;
                playerCoords = GetPoints(game, "r");
                enemyCoords = GetPoints(game, "b");
            }
            else
            {
                energy = game.state.p2.energy;
                spawn = game.state.p2.spawn;
                enemyenergy = game.state.p1.energy;
                enemySpawn = game.state.p1.spawn;
                playerCoords = GetPoints(game, "b");
                enemyCoords = GetPoints(game, "r");
            }

            foreach (Coord coord in playerCoords)
            {
                List<Coord> adjacent = GetAdjacentCoords(coord, game);
                if (adjacent.Count >= 1)
                {
                    Random r = new Random();
                    int rInt = r.Next(0, adjacent.Count);
                    int from = ConvertCoordToIndex(coord, game.state.cols);
                    int to = ConvertCoordToIndex(adjacent[rInt], game.state.cols);

                    Move move = new Move(from, to);
                    moves.Add(move);
                }
            }

            return moves;
        }

        internal Coord ConvertIndexToCoord(int playerIndex, int rows, int columns)
        {
            int x = playerIndex % columns;
            int y = playerIndex / rows;

            return new Coord(x, y);
        }

        internal int ConvertCoordToIndex(Coord coord, int columns)
        {
            double index = columns * coord.Y + coord.X;
            return Convert.ToInt32(index);
        }

        internal List<Coord> GetPoints(Game game, string character)
        {
            List<Coord> playerCoords = new List<Coord>();

            int index = 0;
            int offset = 0;
            string gridSearch = game.state.grid;
            while (index != -1)
            {
                index = gridSearch.IndexOf(character, StringComparison.Ordinal);

                if (index >= 0)
                {
                    Coord point = ConvertIndexToCoord(index + offset, game.state.rows, game.state.cols);
                    playerCoords.Add(point);
                    gridSearch = gridSearch.Substring(index + 1);
                    offset = index + offset + 1;
                }
            }
            return playerCoords;
        }

        internal List<Coord> GetAdjacentCoords(Coord coord, Game game)
        {
            List<Coord> Coords = new List<Coord>();
            int X = coord.X;
            int Y = coord.Y;

            if (X > 0)
                Coords.Add(new Coord(X - 1, Y));

            if (X < game.state.cols - 2)
                Coords.Add(new Coord(X + 1, Y));

            if (Y > 0)
                Coords.Add(new Coord(X, Y - 1));

            if (Y < game.state.rows - 2)
                Coords.Add(new Coord(X, Y + 1));

            return Coords;
        }

    }
}