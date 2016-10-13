using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public static class Util
    {

        #region game setup


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
                game.gridData.spawnDisabled = game.state.p1.spawnDisabled;
                game.gridData.enemyEnergy = game.state.p2.energy;
                game.gridData.enemySpawn = game.state.p2.spawn.ToCoord(game.state.rows, game.state.cols);
                game.gridData.enemySpawnDisabled = game.state.p2.spawnDisabled;
                game.gridData.playerCoords = GetPoints(game, "r");
                game.gridData.enemyCoords = GetPoints(game, "b");
            }
            else
            {
                game.gridData.energy = game.state.p2.energy;
                game.gridData.spawnPoint = game.state.p2.spawn.ToCoord(game.state.rows, game.state.cols);
                game.gridData.spawnDisabled = game.state.p2.spawnDisabled;
                game.gridData.enemyEnergy = game.state.p1.energy;
                game.gridData.enemySpawn = game.state.p1.spawn.ToCoord(game.state.rows, game.state.cols);
                game.gridData.enemySpawnDisabled = game.state.p1.spawnDisabled;
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

        #endregion

        #region moves

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

        public static bool CoordOccupied(Coord coord, int cols, List<Coord> occupiedCoords)
        {
            foreach (Coord takenCoord in occupiedCoords)
            {
                if (takenCoord.SameAs(coord))
                {
                    return true;
                }
            }

            return false;
        }

        public static Move MoveTowardsCoord(Coord from, Coord to, Game game, List<Coord> botsToMove, List<Coord> occupiedCoords, bool avoidSpawnPoint = true)
        {
            if (from.SameAs(to))
            {
                Log(game.player, "from = to: " + from.Print());
                return null;
            }

            int cols = game.state.cols;
            List<Coord> adjacentCoords = GetAdjacentCoords(from, cols, occupiedCoords);

            // avoid moving back to spawn point
            if (avoidSpawnPoint)
                adjacentCoords.RemoveAll(c => c.SameAs(game.gridData.spawnPoint));

            if (!adjacentCoords.Any())
            {
                Log(game.player, "no adjacent cells");
                return null;
            }

            Coord destination = null;

            if (from.X > to.X)
                destination = adjacentCoords.FirstOrDefault(c => c.X < from.X);
            else if (from.X < to.X)
                destination = adjacentCoords.FirstOrDefault(c => c.X > from.X);

            if (destination == null)
            {
                if (from.Y > to.Y)
                    destination = adjacentCoords.FirstOrDefault(c => c.Y < from.Y);
                else if (from.Y < to.Y)
                    destination = adjacentCoords.FirstOrDefault(c => c.Y > from.Y);
            }

            if (destination == null)
                destination = adjacentCoords.FirstOrDefault();

            if (destination == null)
            {
                Log(game.player, "destination = null.  From: " + from.Print() + ", To: " + to.Print());
                return null;
            }

            Move move = new Move() { from = from.ToIndex(cols), to = destination.ToIndex(cols) };
            botsToMove.RemoveAll(c => c.SameAs(from));
            occupiedCoords.RemoveAll(c => c.SameAs(from));
            occupiedCoords.Add(destination.Copy());

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

        public static bool AddMoves(List<Move> existingMoveList, List<Move> newMoves)
        {
            bool movesMade = false;

            foreach (Move newMove in newMoves)
            {
                if (AddMove(existingMoveList, newMove))
                    movesMade = true;
            }

            return movesMade;
        }

        public static bool AddMove(List<Move> existingMoveList, Move newMove)
        {
            if (newMove == null)
                return false;

            existingMoveList.Add(new Move { from = newMove.from, to = newMove.to });

            return true;
        }

        public static void RecursiveMoveTowards(Coord from, Coord to, Game game, Coord botToMove, List<Coord> botsToMove,
            List<Coord> occupiedCoords, List<Move> moves, bool avoidSpawnPoint = true)
        {
            // not finished
            Coord blockingPlayerBot = botsToMove.FirstOrDefault(b => b.SameAs(to));
            if (blockingPlayerBot != null)
            {
                if (AddMove(moves, MoveTowardsCoord(blockingPlayerBot, game.gridData.enemySpawn, game, botsToMove, occupiedCoords, avoidSpawnPoint)))
                {
                    AddMove(moves, MoveTowardsCoord(from, to, game, botsToMove, occupiedCoords, avoidSpawnPoint));
                    return;
                }
            }

        }

        #endregion

        #region search

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

        #endregion

        #region guards

        /// <summary>
        /// this checks to see if an enemy bot is closer to our spawn point than any of our player bots
        /// </summary>
        /// <param name="game"></param>
        /// <param name="playerCoords"></param>
        /// <param name="enemyCoords"></param>
        /// <returns></returns>
        public static bool GuardNeeded(Game game, List<Coord> playerCoords, List<Coord> enemyCoords)
        {
            // find player bot closest to spawn point
            Coord playerBot = FindNearestBot(playerCoords, game.gridData.spawnPoint);
            int playerDistanceFromSpawn = GetDistanceBetweenCoords(playerBot, game.gridData.spawnPoint);

            // find enemy bot closest to spawn point
            Coord enemyBot = FindNearestBot(enemyCoords, game.gridData.spawnPoint);
            int enemyDistanceFromSpawn = GetDistanceBetweenCoords(enemyBot, game.gridData.spawnPoint);


            Log(game.player, "guard needed = " + (enemyDistanceFromSpawn <= playerDistanceFromSpawn) + ".  enemy: " + enemyBot.Print() + ", player: " + playerBot.Print());

            // return true if we need to move to defense position
            return (enemyDistanceFromSpawn <= playerDistanceFromSpawn);
        }

        /// <summary>
        /// Leaves a single bot on the player spawn point
        /// </summary>
        /// <param name="game"></param>
        /// <param name="botsToMove"></param>
        /// <param name="coordsToAvoid"></param>
        /// <returns></returns>
        public static List<Move> CreateSingleBotGuard(Game game, List<Coord> botsToMove, List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();

            if (game.gridData.energy >= 1)
                return moves;


            Coord spawnPoint = game.gridData.spawnPoint;
            attemptGuardBotMove(spawnPoint, game, botsToMove, coordsToAvoid, moves, false);

            foreach (Move move in moves)
                Log(game.player, "CreateSingleBotGuard = " + move.Print(game.state.cols));

            return moves;
        }

        /// <summary>
        /// Leaves a bot on the player spawn point
        /// And another bot in the spot diagonally towards the enemy spawn.
        /// This 2nd bot will kill enemies coming straight on, but not enemies sneaking around the back.
        /// The spawn point guard still blocks those enemies.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="botsToMove"></param>
        /// <param name="coordsToAvoid"></param>
        /// <returns></returns>
        public static List<Move> CreateDoubleBotGuard(Game game, List<Coord> botsToMove, List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();

            Coord spawnPoint = game.gridData.spawnPoint;

            if(game.gridData.energy == 0)
                attemptGuardBotMove(spawnPoint, game, botsToMove, coordsToAvoid, moves, false);

            Coord guardSpot1 = (game.player == "r") ?
                spawnPoint.AdjacentCoord(Direction.DOWN_RIGHT) :
                spawnPoint.AdjacentCoord(Direction.UP_LEFT);
            attemptGuardBotMove(guardSpot1, game, botsToMove, coordsToAvoid, moves, false);

            Coord guardSpot2 = (game.player == "r") ?
                spawnPoint.AdjacentCoord(Direction.DOWN_LEFT) :
                spawnPoint.AdjacentCoord(Direction.UP_RIGHT);
            attemptGuardBotMove(guardSpot2, game, botsToMove, coordsToAvoid, moves, false);

            foreach (Move move in moves)
                Log(game.player, "CreateDoubleBotGuard = " + move.Print(game.state.cols));

            return moves;
        }

        /// <summary>
        /// This leaves a bot on the spawn point, and the 3 diagonal points adjacent to the spawn point.
        /// This allows us to move the spawn point guard right away if we pick up more energy, and kills
        /// enemy bots that come at the spawn point directly.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="botsToMove"></param>
        /// <param name="coordsToAvoid"></param>
        /// <returns></returns>
        public static List<Move> CreateMediumGuard(Game game, List<Coord> botsToMove, List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();

            Coord spawnPoint = game.gridData.spawnPoint;

            if (game.gridData.energy == 0)
                attemptGuardBotMove(spawnPoint, game, botsToMove, coordsToAvoid, moves, false);

            Coord guardSpot1 = (game.player == "r") ?
                spawnPoint.AdjacentCoord(Direction.DOWN_RIGHT) :
                spawnPoint.AdjacentCoord(Direction.UP_LEFT);
            attemptGuardBotMove(guardSpot1, game, botsToMove, coordsToAvoid, moves, false);

            Coord guardSpot2 = (game.player == "r") ?
                spawnPoint.AdjacentCoord(Direction.DOWN_LEFT) :
                spawnPoint.AdjacentCoord(Direction.UP_RIGHT);
            attemptGuardBotMove(guardSpot2, game, botsToMove, coordsToAvoid, moves, false);

            Coord guardSpot3 = (game.player == "r") ?
                spawnPoint.AdjacentCoord(Direction.UP_RIGHT) :
                spawnPoint.AdjacentCoord(Direction.DOWN_LEFT);
            attemptGuardBotMove(guardSpot3, game, botsToMove, coordsToAvoid, moves, false);

            foreach (Move move in moves)
                Log(game.player, "CreateMediumGuard = " + move.Print(game.state.cols));

            return moves;
        }

        private static void attemptGuardBotMove(Coord to, Game game, List<Coord> botsToMove, List<Coord> coordsToAvoid, List<Move> moves,
            bool avoidSpawnPoint)
        {
            Coord bot = FindNearestBot(botsToMove, to);

            if (bot != null)
            {
                Move move = MoveTowardsCoord(bot, to, game, botsToMove, coordsToAvoid, avoidSpawnPoint);
                if (move != null)
                {
                    moves.Add(move);
                    Log(game.player, "attemptGuardBotMove = " + move.Print(game.state.cols));
                }

                // remove the chosen guard bot from the list of bots to move, even if we didn't move it
                // if it was already on the guard spot, then we don't want to move it again
                botsToMove.RemoveAll(c => c.SameAs(bot));
                Log(game.player, "attemptGuardBotMove removing bot: " + bot.Print());
            }
        }

        #endregion

    }
}