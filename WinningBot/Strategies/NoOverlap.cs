using System;
using System.Collections.Generic;
using System.Diagnostics;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public class NoOverlap : BaseStrategy, IStrategy
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
                List<Coord> adjacent = GetAdjacentCoords(coord, game, playerCoords);
                if (adjacent.Count >= 1)
                {
                    Random r = new Random();
                    int rInt = r.Next(0, adjacent.Count);
                    int from = ConvertCoordToIndex(coord, game.state.cols);
                    int to = ConvertCoordToIndex(adjacent[rInt], game.state.cols);

                    coord.X = adjacent[rInt].X;
                    coord.Y = adjacent[rInt].Y;
                    Move move = new Move(from, to);
                    moves.Add(move);
                }
            }

            return moves;
        }

        internal List<Coord> GetAdjacentCoords(Coord coord, Game game, List<Coord> takenCoords)
        {
            List<Coord> Coords = new List<Coord>();
            int X = coord.X;
            int Y = coord.Y;

            if (X > 0 && !CoordOccupied(X - 1, Y, game.state.cols, takenCoords))
                Coords.Add(new Coord(X - 1, Y));

            if (X < game.state.cols - 2 && !CoordOccupied(X + 1, Y, game.state.cols, takenCoords))
                Coords.Add(new Coord(X + 1, Y));

            if (Y > 0 && !CoordOccupied(X, Y - 1, game.state.cols, takenCoords))
                Coords.Add(new Coord(X, Y - 1));

            if (Y < game.state.rows - 2 && !CoordOccupied(X, Y + 1, game.state.cols, takenCoords))
                Coords.Add(new Coord(X, Y + 1));

            return Coords;
        }

        internal bool CoordOccupied(int X, int Y, int cols, List<Coord> takenCoords)
        {
            foreach (Coord takenCoord in takenCoords)
            {
                if (takenCoord.X == X && takenCoord.Y == Y)
                {
                    Debug.WriteLine("Point taken: " + ConvertCoordToIndex(takenCoord, cols));
                    return true;
                }
            }

            return false;
        }
    }
}