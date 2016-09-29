using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public static class Util
    {
        /// <summary>
        /// Copy Text: Util.Log(game.player, );
        /// </summary>
        /// <param name="player"></param>
        /// <param name="text"></param>
        public static void Log(string player, string text)
        {
            Debug.WriteLine(player.ToUpper() + " : " + text);
        }

        public static void ParseGrid(ref Game game)
        {
            game.gridData.energyCoords = GetPoints(game, "*");

            if (game.player == "r")
            {
                game.gridData.energy = game.state.p1.energy;
                game.gridData.spawnPoint = game.state.p1.spawn.ToCoord(game.state.rows, game.state.cols);
                game.gridData.enemyEnergy = game.state.p2.energy;
                game.gridData.enemySpawn = game.state.p2.spawn.ToCoord(game.state.rows, game.state.cols);
                game.gridData.playerCoords = GetPoints(game, "r");
                game.gridData.enemyCoords = GetPoints(game, "b");
            }
            else
            {
                game.gridData.energy = game.state.p2.energy;
                game.gridData.spawnPoint = game.state.p2.spawn.ToCoord(game.state.rows, game.state.cols);
                game.gridData.enemyEnergy = game.state.p1.energy;
                game.gridData.enemySpawn = game.state.p1.spawn.ToCoord(game.state.rows, game.state.cols);
                game.gridData.playerCoords = GetPoints(game, "b");
                game.gridData.enemyCoords = GetPoints(game, "r");
            }

            game.gridData.occupiedCoords = new List<Coord>(game.gridData.playerCoords);
            Game gameCopy = game;
            game.gridData.enemyCoordsIncludingPossibleMoves =
                gameCopy.gridData.enemyCoords.SelectMany(ec =>
                    GetAdjacentCoords(ec, gameCopy)).ToList().
                    Concat(gameCopy.gridData.enemyCoords)
                    .ToList();

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

        public static Coord ToCoord(this int index, int rows, int cols)
        {
            return ConvertIndexToCoord(index, rows, cols);
        }

        //public static Coord FindSpawnPoint(Game game)
        //{
        //    return (game.player == "r") ?
        //         game.state.p1.spawn.ToCoord(game.state.rows, game.state.cols) :
        //         game.state.p2.spawn.ToCoord(game.state.rows, game.state.cols);
        //}

        public static bool CoordOccupied(Coord coord, int cols, List<Coord> occupiedCoords)
        {
            foreach (Coord takenCoord in occupiedCoords)
            {
                if (takenCoord.EqualTo(coord))
                {
                    return true;
                }
            }

            return false;
        }

        public static Coord FindNearestEnergy(List<Coord> energyCoords, Coord bot)
        {
            int fewestMoves = 0;
            Coord nearestEnergy = null;

            foreach (Coord energyCoord in energyCoords)
            {
                int totalMoves = GetDistanceBetweenCoords(energyCoord, bot);

                if (totalMoves < fewestMoves || fewestMoves == 0)
                {
                    fewestMoves = totalMoves;
                    nearestEnergy = energyCoord;
                }
            }

            return nearestEnergy;
        }

        public static Move MoveTowardsCoord(Coord from, Coord to, Game game, ref List<Coord> botsToMove, ref List<Coord> occupiedCoords, bool avoidSpawnPoint = true)
        {
            if (from.EqualTo(to))
                return null;

            int cols = game.state.cols;
            List<Coord> adjacentCoords = GetAdjacentCoords(from, cols, occupiedCoords);
            
            // avoid moving back to spawn point
            if (avoidSpawnPoint)
                adjacentCoords.RemoveAll(c => c.EqualTo(game.gridData.spawnPoint));

            if (!adjacentCoords.Any())
                return null;

            Coord destination = null;

            if (from.X > to.X)
                destination = new Coord(from.X - 1, from.Y);
               // adjacentCoords.Sort((c1, c2) => c1.X.CompareTo(c2.X));
            else if (from.X < to.X)
                destination = new Coord(from.X + 1, from.Y);
               // adjacentCoords.Sort((c1, c2) => c2.X.CompareTo(c1.X));
            else if (from.Y > to.Y)
                destination = new Coord(from.X, from.Y - 1);
               // adjacentCoords.Sort((c1, c2) => c1.Y.CompareTo(c2.Y));
            else if (from.Y < to.Y)
                destination = new Coord(from.X, from.Y + 1);
               // adjacentCoords.Sort((c1, c2) => c2.Y.CompareTo(c1.Y));

            //Coord destination = adjacentCoords.First();
            if (destination == null)
                return null;

            Move move = new Move(){ from = from.ToIndex(cols), to = destination.ToIndex(cols)};
            botsToMove.RemoveAll(c => c.EqualTo(from));
            occupiedCoords.RemoveAll(c => c.EqualTo(from));
            occupiedCoords.Add(destination);

            return move;
        }

        public static List<Coord> GetAdjacentCoords(Coord coord, int cols, List<Coord> takenCoords)
        {
            List<Coord> Coords = new List<Coord>();
            int X = coord.X;
            int Y = coord.Y;

            if (X > 0 && !CoordOccupied(new Coord(X - 1, Y), cols, takenCoords))
                Coords.Add(new Coord(X - 1, Y));

            if (X < cols - 1 && !CoordOccupied(new Coord(X + 1, Y), cols, takenCoords))
                Coords.Add(new Coord(X + 1, Y));

            if (Y > 0 && !CoordOccupied(new Coord(X, Y - 1), cols, takenCoords))
                Coords.Add(new Coord(X, Y - 1));

            if (Y < cols - 1 && !CoordOccupied(new Coord(X, Y + 1), cols, takenCoords))
                Coords.Add(new Coord(X, Y + 1));

            return Coords;
        }

        public static Coord FindNearestBot(List<Coord> bots, Coord destination)
        {
            int? fewestMoves = null;
            Coord nearestBot = null;

            foreach (Coord bot in bots)
            {
                int totalMoves = GetDistanceBetweenCoords(destination, bot);

                if (fewestMoves == null || totalMoves < fewestMoves)
                {
                    fewestMoves = totalMoves;
                    nearestBot = bot;
                }
            }

            return nearestBot;
        }

        public static Direction Opposite(this Direction direction)
        {
            switch (direction)
            {
                case Direction.UP:
                    return Direction.DOWN;
                case Direction.DOWN:
                    return Direction.UP;
                case Direction.LEFT:
                    return Direction.RIGHT;
                case Direction.RIGHT:
                    return Direction.LEFT;
                default:
                    return Direction.UP;
            }
        }

        public static int GetDistanceBetweenCoords(Coord start, Coord finish)
        {
            int horizontalMoves = Math.Abs(start.X - finish.X);
            int verticalMoves = Math.Abs(start.Y - finish.Y);
            return horizontalMoves + verticalMoves;
        }

        public static bool GuardNeeded(Game game, List<Coord> playerCoords, List<Coord> enemyCoords)
        {
            // find player bot closest to spawn point
            Coord playerBot = FindNearestBot(playerCoords, game.gridData.spawnPoint);
            int playerDistanceFromSpawn = GetDistanceBetweenCoords(playerBot, game.gridData.spawnPoint);

            // find enemy bot closest to spawn point
            Coord enemyBot = FindNearestBot(enemyCoords, game.gridData.spawnPoint);
            int enemyDistanceFromSpawn = GetDistanceBetweenCoords(enemyBot, game.gridData.spawnPoint);

            
            Log(game.player, "guard needed = " + (enemyDistanceFromSpawn <= playerDistanceFromSpawn) + ".  bot count = " + playerCoords.Count);
            // return true if we need to move to defense position
            return (enemyDistanceFromSpawn <= playerDistanceFromSpawn);
        }

        public static List<Move> CreateSmallGuard(Game game, ref List<Coord> botsToMove, ref List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();
            //Coord guardPoint = (game.player == "r") ? new Coord(spawnPoint.X + 1, spawnPoint.Y + 1) : new Coord(spawnPoint.X - 1, spawnPoint.Y -1);

            if (botsToMove.Count < 2)
                return moves;

            Coord closestBot = FindNearestBot(botsToMove, game.gridData.spawnPoint);
            //moves.Add(MoveTowardsCoord(closestBot, guardPoint, game, coordsToAvoid));
            Move move = MoveTowardsCoord(closestBot, game.gridData.spawnPoint, game, ref  botsToMove, ref coordsToAvoid, false);
            
            if (move != null)
            {
                Log(game.player, "small guard move: " + move.Print());
                moves.Add(move);
            }

            return moves;
        }

        public static List<Move> CreateMediumGuard(Game game, ref List<Coord> botsToMove, ref List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();

            if (botsToMove.Count < 6)
                return moves;

            int cols = game.state.cols;

            Coord guardSpot1 = (game.player == "r")
                ? new Coord(game.gridData.spawnPoint.X - 1, game.gridData.spawnPoint.Y + 1)
                : new Coord(game.gridData.spawnPoint.X + 1, game.gridData.spawnPoint.Y - 1);

            Coord guardSpot2 = (game.player == "r")
                ? new Coord(game.gridData.spawnPoint.X + 1, game.gridData.spawnPoint.Y + 1)
                : new Coord(game.gridData.spawnPoint.X - 1, game.gridData.spawnPoint.Y - 1);

            Coord guardSpot3 = (game.player == "r")
                ? new Coord(game.gridData.spawnPoint.X + 1, game.gridData.spawnPoint.Y - 1)
                : new Coord(game.gridData.spawnPoint.X - 1, game.gridData.spawnPoint.Y + 1);

            Coord guardBot1 = FindNearestBot(botsToMove, guardSpot1);
            if (guardBot1 != null)
            {
                Move move = MoveTowardsCoord(guardBot1, guardSpot1, game, ref botsToMove, ref coordsToAvoid);
                if (move != null)
                    moves.Add(move);
                else
                // remove the chosen guard bot from the list of bots to move, even if we didn't move it
                // if it was already on the guard spot, then we don't want to move it again
                botsToMove.RemoveAll(c => c.EqualTo(guardBot1));
            }

            Coord guardBot2 = FindNearestBot(botsToMove, guardSpot2);
            if (guardBot2 != null)
            {
                Move move = MoveTowardsCoord(guardBot2, guardSpot2, game, ref botsToMove, ref  coordsToAvoid);
                if (move != null)
                    moves.Add(move);
                else
                // remove the chosen guard bot from the list of bots to move, even if we didn't move it
                // if it was already on the guard spot, then we don't want to move it again
                botsToMove.RemoveAll(c => c.Equals(guardBot2));
            }

            Coord guardBot3 = FindNearestBot(botsToMove, guardSpot3);
            if (guardBot3 != null)
            {
                Move move = MoveTowardsCoord(guardBot3, guardSpot3, game, ref botsToMove, ref coordsToAvoid);
                if (move != null)
                    moves.Add(move);
                else
                // remove the chosen guard bot from the list of bots to move, even if we didn't move it
                // if it was already on the guard spot, then we don't want to move it again
                botsToMove.RemoveAll(c => c.Equals(guardBot3));
            }

            return moves;

        }
    }
}