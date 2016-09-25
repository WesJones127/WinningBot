using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public class NoOverlap : IStrategy
    {
        List<Move> IStrategy.getMoves(Game game)
        {

            List<Move> moves = new List<Move>();

            try
            {
                foreach (Coord coord in game.gridData.playerCoords)
                {
                    Coord nearestEnergy = findNearestEnergy(game.gridData.energyCoords, coord);

                    if (nearestEnergy != null)
                    {
                        Move move = moveTowardsCoord(coord,
                            nearestEnergy,
                            game.state.cols,
                            game.gridData.enemyCoordsIncludingPossibleMoves.Concat(game.gridData.occupiedCoords)
                                .ToList());
                        Coord newPlayerCoord = HelperMethods.ConvertIndexToCoord(move.to, game.state.rows,
                            game.state.cols);

                        moves.Add(move);
                        game.gridData.occupiedCoords.RemoveAll(c => c.X == coord.X && c.Y == coord.Y);
                        game.gridData.occupiedCoords.Add(new Coord(newPlayerCoord.X, newPlayerCoord.Y));
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

        internal Move moveTowardsCoord(Coord from, Coord to, int cols, List<Coord> occupiedCoords)
        {
            Coord newCoord = new Coord(from.X, from.Y);

            if (from.X > to.X && !occupiedCoords.Exists(c => c.X == from.X - 1 && c.Y == newCoord.Y))
                newCoord.X = from.X - 1;
            else if (from.X < to.X && !occupiedCoords.Exists(c => c.X == from.X + 1 && c.Y == newCoord.Y))
                newCoord.X = from.X + 1;
            else if (from.Y > to.Y && !occupiedCoords.Exists(c => c.X == newCoord.X && c.Y == from.Y - 1))
                newCoord.Y = from.Y - 1;
            else if (from.Y < to.Y && !occupiedCoords.Exists(c => c.X == newCoord.X && c.Y == from.Y + 1))
                newCoord.Y = from.Y + 1;

            return new Move(HelperMethods.ConvertCoordToIndex(from, cols), HelperMethods.ConvertCoordToIndex(newCoord, cols));
        }

        internal List<Coord> GetAdjacentCoords(Coord coord, Game game, List<Coord> takenCoords)
        {
            List<Coord> Coords = new List<Coord>();
            int X = coord.X;
            int Y = coord.Y;

            if (X > 0 && !CoordOccupied(new Coord(X - 1, Y), game.state.cols, takenCoords))
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