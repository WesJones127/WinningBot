using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public static class Util
    {
        public static void parseGrid(ref Game game)
        {
            game.gridData.energyCoords = GetPoints(game, "*");

            if (game.player == "r")
            {
                game.gridData.energy = game.state.p1.energy;
                game.gridData.spawn = game.state.p1.spawn;
                game.gridData.enemyEnergy = game.state.p2.energy;
                game.gridData.enemySpawn = game.state.p2.spawn;
                game.gridData.playerCoords = GetPoints(game, "r");
                game.gridData.enemyCoords = GetPoints(game, "b");
            }
            else
            {
                game.gridData.energy = game.state.p2.energy;
                game.gridData.spawn = game.state.p2.spawn;
                game.gridData.enemyEnergy = game.state.p1.energy;
                game.gridData.enemySpawn = game.state.p1.spawn;
                game.gridData.playerCoords = GetPoints(game, "b");
                game.gridData.enemyCoords = GetPoints(game, "r");
            }

            game.gridData.occupiedCoords = new List<Coord>(game.gridData.playerCoords);
            Game gameCopy = game;
            game.gridData.enemyCoordsIncludingPossibleMoves =
                gameCopy.gridData.enemyCoords.SelectMany(ec => GetAdjacentCoords(ec, gameCopy)).ToList().Concat(gameCopy.gridData.enemyCoords).ToList();

        }

        public static List<Coord> GetPoints(Game game, string character)
        {
            List<Coord> playerCoords = new List<Coord>();
            int playerId = 0;

            int index = 0;
            int offset = 0;
            string gridSearch = game.state.grid;
            while (index != -1)
            {
                index = gridSearch.IndexOf(character, StringComparison.Ordinal);

                if (index >= 0)
                {
                    playerId++;
                    Coord point = ConvertIndexToCoord(index + offset, game.state.rows, game.state.cols);
                    point.id = playerId;
                    playerCoords.Add(point);
                    gridSearch = gridSearch.Substring(index + 1);
                    offset = index + offset + 1;
                }
            }
            return playerCoords;
        }

        public static List<Coord> GetAdjacentCoords(Coord coord, Game game)
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

        public static Coord ConvertIndexToCoord(int playerIndex, int rows, int columns)
        {
            int x = playerIndex % columns;
            int y = playerIndex / rows;

            return new Coord(x, y);
        }

        public static int ConvertCoordToIndex(Coord coord, int columns)
        {
            double index = columns * coord.Y + coord.X;
            return Convert.ToInt32(index);
        }


        public static bool CoordOccupied(Coord coord, int cols, List<Coord> occupiedCoords)
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

        public static Coord findNearestEnergy(List<Coord> energyCoords, Coord bot)
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

        public static Move moveTowardsCoord(Coord from, Coord to, int cols, List<Coord> occupiedCoords)
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

            return new Move(Util.ConvertCoordToIndex(from, cols), Util.ConvertCoordToIndex(newCoord, cols));
        }

        public static List<Coord> GetAdjacentCoords(Coord coord, Game game, List<Coord> takenCoords)
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
    }
}