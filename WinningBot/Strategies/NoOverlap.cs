using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public class NoOverlap : BaseStrategy, IStrategy
    {
        public List<Move> getMoves(Game game)
        {
            int energy, spawn, enemyenergy, enemySpawn;
            List<Coord> playerCoords, enemyCoords, energyCoords;
            List<Move> moves = new List<Move>();

            energyCoords = GetPoints(game, "*");

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

            List<Coord> occupiedCoords = playerCoords.ToList();
            List<Coord> enemyCoordsIncludingPossibleMoves =
                enemyCoords.SelectMany(ec => GetAdjacentCoords(ec, game)).ToList().Concat(enemyCoords).ToList();

            foreach (Coord coord in playerCoords)
            {
                Coord nearestEnergy = findNearestEnergy(energyCoords, coord);
                Move move = moveTowardsCoord(coord, nearestEnergy, game.state.cols);
                Coord newPlayerCoord = ConvertIndexToCoord(move.to, game.state.rows, game.state.cols);
                
                if (!CoordOccupied(newPlayerCoord, game.state.cols, enemyCoordsIncludingPossibleMoves.Concat(occupiedCoords).ToList()))
                {
                    moves.Add(move);
                    occupiedCoords.RemoveAll(c=>c.X == coord.X && c.Y == coord.Y);
                    occupiedCoords.Add(new Coord(newPlayerCoord.X, newPlayerCoord.Y));
                }
            }

            return moves;
        }

        internal Coord findNearestEnergy(List<Coord> energyCoords, Coord bot)
        {
            int fewestMoves = 0;
            Coord nearestEnergy = null;

            foreach (Coord energyCoord in energyCoords)
            {
                int horizontalMoves = Math.Abs(energyCoord.X - bot.X);
                int verticalMoves = Math.Abs(energyCoord.Y - bot.Y);
                int totalMoves = horizontalMoves + verticalMoves;

                if (totalMoves < fewestMoves || fewestMoves == 0)
                {
                    fewestMoves = totalMoves;
                    nearestEnergy = energyCoord;
                }
            }

            return nearestEnergy;
        }

        internal Move moveTowardsCoord(Coord from, Coord to, int cols)
        {
            Coord newCoord = new Coord(from.X, from.Y);

            if (from.X > to.X)
                newCoord.X = from.X - 1;
            else if (from.X < to.X)
                newCoord.X = from.X + 1;
            else if (from.Y > to.Y)
                newCoord.Y = from.Y - 1;
            else if (from.Y < to.Y)
                newCoord.Y = from.Y + 1;
            
            return new Move(ConvertCoordToIndex(from, cols), ConvertCoordToIndex(newCoord, cols));
        }

        internal List<Coord> GetAdjacentCoords(Coord coord, Game game, List<Coord> takenCoords)
        {
            List<Coord> Coords = new List<Coord>();
            int X = coord.X;
            int Y = coord.Y;

            if (X > 0 && !CoordOccupied(new Coord(X-1, Y), game.state.cols, takenCoords))
                Coords.Add(new Coord(X - 1, Y));

            if (X < game.state.cols - 2 && !CoordOccupied(new Coord(X + 1, Y), game.state.cols, takenCoords))
                Coords.Add(new Coord(X + 1, Y));

            if (Y > 0 && !CoordOccupied(new Coord(X, Y - 1), game.state.cols, takenCoords))
                Coords.Add(new Coord(X, Y - 1));

            if (Y < game.state.rows - 2 && !CoordOccupied(new Coord(X, Y + 1), game.state.cols, takenCoords))
                Coords.Add(new Coord(X, Y + 1));

            return Coords;
        }

        internal bool CoordOccupied(Coord coord, int cols, List<Coord> occupiedCoords)
        {
            foreach (Coord takenCoord in occupiedCoords)
            {
                if (takenCoord.X == coord.X && takenCoord.Y == coord.Y)
                {
                    //Debug.WriteLine("Point taken: " + ConvertCoordToIndex(takenCoord, cols));
                    Debug.WriteLine("Coord taken: " + takenCoord.X + "," + takenCoord.Y);
                    return true;
                }
            }

            return false;
        }

    }
}