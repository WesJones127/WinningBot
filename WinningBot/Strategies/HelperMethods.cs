using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public static class HelperMethods
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
    }
}