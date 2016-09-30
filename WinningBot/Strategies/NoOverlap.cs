using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Helpers;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public class NoOverlap : IStrategy
    {
        List<Move> IStrategy.getMoves(Game game)
        {
            List<Move> moves = new List<Move>();

            // myPlayerCoords is used to track the remaining bots that need to be moved
            // make a copy here so we don't edit the original
            List<Coord> botsToMove = new List<Coord>(game.gridData.playerCoords);

            // coordsToAvoid includes existing player bots, existing enemy bots, and possible enemy moves
            List<Coord> coordsToAvoid = new List<Coord>(
                game.gridData.enemyCoordsIncludingPossibleMoves
                .Concat(botsToMove));

            List<Coord> energyCoords = new List<Coord>(game.gridData.energyCoords);


            // MOVE BOT FROM SPAWN POINT IF WE HAVE ANOTHER BOT READY TO SPAWN
            MoveBotFromSpawnPoint(moves, game, botsToMove, coordsToAvoid);

            // IF WE HAVE ENOUGH BOTS, MOVE TO GUARD POSITIONS
            GuardSpawnPoint(moves, game, botsToMove, coordsToAvoid);

            // ATTEMPT TO RAZE ENEMY SPAWN
            MoveBotsTowardsEnemySpawn(moves, game, botsToMove, coordsToAvoid);

            // CHASE ENERGY WITH CLOSEST BOT
            MoveBotsTowardsEnergy(moves, game, energyCoords, botsToMove, coordsToAvoid);


            return moves;
        }

        /// <summary>
        /// This function returns a list just to stay consistent with other functions
        /// </summary>
        /// <param name="game"></param>
        /// <param name="spawnPoint"></param>
        /// <param name="playerCoords"></param>
        /// <param name="coordsToAvoid"></param>
        /// <returns></returns>
        private void MoveBotFromSpawnPoint(List<Move> moves, Game game, List<Coord> botsToMove, List<Coord> coordsToAvoid)
        {
            int originalMoveCount = moves.Count;
            int cols = game.state.cols;
            Coord spawnPoint = game.gridData.spawnPoint;
            Coord spawnPointBlocker = botsToMove.FirstOrDefault(c => c.SameAs(spawnPoint));
            bool hasEnergy = game.gridData.energy >= 1;

            if (spawnPointBlocker == null)
                return;

            if (Util.GuardNeeded(game, botsToMove, game.gridData.enemyCoords) &&
                !hasEnergy &&
                botsToMove.Count >= 2)
                return;


            Direction direction1 = (spawnPoint.Y > game.state.rows / 2) ? Direction.UP : Direction.DOWN;
            Direction direction2 = (spawnPoint.Y > game.state.rows / 2) ? Direction.LEFT : Direction.RIGHT;
            Coord desiredSpot1 = spawnPoint.AdjacentCoord(direction1);
            Coord desiredSpot2 = spawnPoint.AdjacentCoord(direction2);
            Coord desiredSpot3 = spawnPoint.AdjacentCoord(direction1.Opposite());
            Coord desiredSpot4 = spawnPoint.AdjacentCoord(direction2.Opposite());

            while (moves.Count == originalMoveCount)
            {
                if (!Util.CoordOccupied(desiredSpot1, cols, coordsToAvoid))
                    Util.AddMove(moves, Util.MoveTowardsCoord(spawnPoint, desiredSpot1, game, botsToMove, coordsToAvoid));

                if (!Util.CoordOccupied(desiredSpot2, cols, coordsToAvoid))
                    Util.AddMove(moves, Util.MoveTowardsCoord(spawnPoint, desiredSpot2, game, botsToMove, coordsToAvoid));

                if (!Util.CoordOccupied(desiredSpot3, cols, coordsToAvoid))
                    Util.AddMove(moves, Util.MoveTowardsCoord(spawnPoint, desiredSpot3, game, botsToMove, coordsToAvoid));

                if (!Util.CoordOccupied(desiredSpot4, cols, coordsToAvoid))
                    Util.AddMove(moves, Util.MoveTowardsCoord(spawnPoint, desiredSpot4, game, botsToMove, coordsToAvoid));

                originalMoveCount++;
            }

            foreach (Move move in moves)
            {
                Util.Log(game.player, "MoveBotFromSpawnPoint = " + move.Print());
            }

            // if we got here, all direct moves are blocked
            //   moves.AddRange(recursivelyMoveBlockedBots(spawnPoint, desiredSpot1, game, playerCoords, coordsToAvoid));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="botsToMove">This is the list of bots that need to be moved, NOT the list of all player bots</param>
        /// <param name="coordsToAvoid">This includes all player bots, enemy bots, and possible new locations for enemy bots</param>
        /// <returns></returns>
        private void GuardSpawnPoint(List<Move> moves, Game game, List<Coord> botsToMove, List<Coord> coordsToAvoid)
        {
            if (botsToMove.Count < 2)
                return;

            if (botsToMove.Count <= 3)
            {
                if (Util.GuardNeeded(game, botsToMove, game.gridData.enemyCoords))
                    Util.AddMoves(moves, Util.CreateSingleBotGuard(game, botsToMove, coordsToAvoid));
            }
            else if (botsToMove.Count <= 6)
                Util.AddMoves(moves, Util.CreateDoubleBotGuard(game, botsToMove, coordsToAvoid));
            else
                Util.AddMoves(moves, Util.CreateMediumGuard(game, botsToMove, coordsToAvoid));

            foreach (Move move in moves)
            {
                Util.Log(game.player, "GuardSpawnPoint = " + move.Print());
            }
        }

        private void MoveBotsTowardsEnemySpawn(List<Move> moves, Game game, List<Coord> botsToMove, List<Coord> coordsToAvoid)
        {
            Coord enemySpawn = game.gridData.enemySpawn;
            int razingBots;

            if (botsToMove.Count <= 2)
                return;

            if (botsToMove.Count <= 5)
                razingBots = 1;
            else
                razingBots = botsToMove.Count / 3;


            //TODO: FIX THIS TO CHECK IF THE SPOT HAS BEEN RAZED BEFORE, THEN WE DON'T NEED TO LEAVE THE BOT ON THE RAZE POINT
            // exit if we already have their spawn point razed
            // and remove the razing bot from the list of bots to be moved
            Coord razingBot = botsToMove.FirstOrDefault(c => c.SameAs(enemySpawn));
            if (razingBot != null)
            {
                botsToMove.RemoveAll(c => c.SameAs(razingBot));
                return;
            }

            while (razingBots > 0)
            {
                for (int x = botsToMove.Count - 1; x >= 0; x--)
                {
                    Coord playerCoord = botsToMove[x];
                    if (Util.AddMove(moves, Util.MoveTowardsCoord(playerCoord, enemySpawn, game, botsToMove, coordsToAvoid)))
                        razingBots--;
                }
                razingBots = 0;
            }

            foreach (Move move in moves)
            {
                Util.Log(game.player, "MoveBotsTowardsEnemySpawn = " + move.Print());
            }
        }

        private void MoveBotsTowardsEnergy(List<Move> moves, Game game, List<Coord> energyPoints, List<Coord> botsToMove,
           List<Coord> coordsToAvoid)
        {
            for (int x = botsToMove.Count - 1; x >= 0; x--)
            {
                Coord bot = botsToMove[x];
                Coord nearestEnergy = Util.FindNearestEnergy(energyPoints, bot);

                Util.AddMove(moves, Util.MoveTowardsCoord(bot, nearestEnergy, game, botsToMove, coordsToAvoid));
            }
        }
    }
}