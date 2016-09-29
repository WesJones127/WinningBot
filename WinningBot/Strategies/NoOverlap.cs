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
            List<Move> tempMoves = MoveBotFromSpawnPoint(game, ref botsToMove, ref coordsToAvoid);
            if (tempMoves.Any())
                moves.AddRange(tempMoves);


            // IF WE HAVE ENOUGH BOTS, MOVE TO GUARD POSITIONS
            tempMoves = GuardSpawnPoint(game, ref botsToMove, ref coordsToAvoid);
            if (tempMoves.Any())
                moves.AddRange(tempMoves);


            // ATTEMPT TO RAZE ENEMY SPAWN
            tempMoves = MoveBotsTowardsEnemySpawn(game, ref botsToMove, ref coordsToAvoid);
            if (tempMoves.Any())
                moves.AddRange(tempMoves);


            // CHASE ENERGY WITH CLOSEST BOT
            tempMoves = MoveBotsTowardsEnergy(game, energyCoords, ref  botsToMove, ref  coordsToAvoid);
            if (tempMoves.Any())
                moves.AddRange(tempMoves);




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
        private List<Move> MoveBotFromSpawnPoint(Game game, ref List<Coord> botsToMove, ref List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();
            int moveCount = 0;
            int cols = game.state.cols;
            Coord spawnPoint = game.gridData.spawnPoint;
            Coord spawnPointBlocker = botsToMove.FirstOrDefault(c => c.EqualTo(spawnPoint));
            bool hasEnergy = game.gridData.energy >= 1;

            if (spawnPointBlocker == null)
                return moves;

            if (Util.GuardNeeded(game, botsToMove, game.gridData.enemyCoords) &&
                !hasEnergy &&
                botsToMove.Count >= 2)
                return moves;


            Direction direction1 = (spawnPoint.Y > game.state.rows / 2) ? Direction.UP : Direction.DOWN;
            Direction direction2 = (spawnPoint.Y > game.state.rows / 2) ? Direction.LEFT : Direction.RIGHT;
            Coord desiredSpot1 = spawnPoint.AdjacentCoord(direction1);
            Coord desiredSpot2 = spawnPoint.AdjacentCoord(direction2);
            Coord desiredSpot3 = spawnPoint.AdjacentCoord(direction1.Opposite());
            Coord desiredSpot4 = spawnPoint.AdjacentCoord(direction2.Opposite());
            // List<Coord> occupiedSpots = playerCoords.Concat(coordsToAvoid).ToList();

            //while (moveCount <= 0)
            //{
            if (!Util.CoordOccupied(desiredSpot1, cols, coordsToAvoid))
            {
                moveCount++;
                moves.Add(Util.MoveTowardsCoord(spawnPoint, desiredSpot1, game, ref botsToMove, ref coordsToAvoid));

                foreach (Move move in moves)
                {
                    Util.Log(game.player, "MoveBotFromSpawnPoint = " + move.Print());
                }
                return moves;
            }

            if (!Util.CoordOccupied(desiredSpot2, cols, coordsToAvoid))
            {
                moveCount++;
                moves.Add(Util.MoveTowardsCoord(spawnPoint, desiredSpot2, game, ref  botsToMove, ref coordsToAvoid));

                foreach (Move move in moves)
                {
                    Util.Log(game.player, "MoveBotFromSpawnPoint = " + move.Print());
                }
                return moves;
            }

            if (!Util.CoordOccupied(desiredSpot3, cols, coordsToAvoid))
            {
                moveCount++;
                moves.Add(Util.MoveTowardsCoord(spawnPoint, desiredSpot3, game, ref  botsToMove, ref  coordsToAvoid));

                foreach (Move move in moves)
                {
                    Util.Log(game.player, "MoveBotFromSpawnPoint = " + move.Print());
                }
                return moves;
            }

            if (!Util.CoordOccupied(desiredSpot4, cols, coordsToAvoid))
            {
                moveCount++;
                moves.Add(Util.MoveTowardsCoord(spawnPoint, desiredSpot4, game, ref botsToMove, ref  coordsToAvoid));

                foreach (Move move in moves)
                {
                    Util.Log(game.player, "MoveBotFromSpawnPoint = " + move.Print());
                }
                return moves;
            }
            // }

            foreach (Move move in moves)
            {
                Util.Log(game.player, "MoveBotFromSpawnPoint = " + move.Print());
            }

            // if we got here, all direct moves are blocked
            //   moves.AddRange(recursivelyMoveBlockedBots(spawnPoint, desiredSpot1, game, playerCoords, coordsToAvoid));
            return moves;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="botsToMove">This is the list of bots that need to be moved, NOT the list of all player bots</param>
        /// <param name="coordsToAvoid">This includes all player bots, enemy bots, and possible new locations for enemy bots</param>
        /// <returns></returns>
        private List<Move> GuardSpawnPoint(Game game, ref List<Coord> botsToMove, ref List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();

            if (botsToMove.Count < 2)
                return moves;

            if (botsToMove.Count <= 4)
            {
                if (Util.GuardNeeded(game, botsToMove, game.gridData.enemyCoords))
                    moves.AddRange(Util.CreateSmallGuard(game, ref  botsToMove, ref  coordsToAvoid));
            }
            else if (botsToMove.Count <= 6)
                moves.AddRange(Util.CreateSmallGuard(game, ref botsToMove, ref coordsToAvoid));
            else
                moves.AddRange(Util.CreateMediumGuard(game, ref  botsToMove, ref coordsToAvoid));


            foreach (Move move in moves)
            {
                Util.Log(game.player, "GuardSpawnPoint = " + move.Print());
            }
            return moves;
        }

        private List<Move> MoveBotsTowardsEnemySpawn(Game game, ref List<Coord> botsToMove, ref List<Coord> coordsToAvoid)
        {
            Util.Log(game.player, "MoveBotsTowardsEnemySpawn start");
            Coord enemySpawn = game.gridData.enemySpawn;
            List<Move> moves = new List<Move>();
            int razingBots = 0;

            if (botsToMove.Count <= 2)
                return moves;
            if (botsToMove.Count <= 5)
                razingBots = 1;
            else
                razingBots = botsToMove.Count / 3;


            //TODO: FIX THIS TO CHECK IF THE SPOT HAS BEEN RAZED BEFORE, THEN WE DON'T NEED TO LEAVE THE BOT ON THE RAZE POINT
            // exit if we already have their spawn point razed
            // and remove the razing bot from the list of bots to be moved
            Coord razingBot = botsToMove.FirstOrDefault(c => c.EqualTo(enemySpawn));
            if (razingBot != null)
            {
                botsToMove.RemoveAll(c => c.EqualTo(razingBot));
                return moves;
            }

            while (razingBots > 0)
            {
                for (int x = botsToMove.Count - 1; x >= 0; x--)
                {
                    Coord playerCoord = botsToMove[x];
                    Move move = Util.MoveTowardsCoord(playerCoord, enemySpawn, game, ref  botsToMove, ref coordsToAvoid);

                    if (move != null)
                    {
                        moves.Add(move);
                        razingBots--;
                    }
                }
                razingBots = 0;
            }


            foreach (Move move in moves)
            {
                Util.Log(game.player, "MoveBotsTowardsEnemySpawn = " + move.Print());
            }

            Util.Log(game.player, "MoveBotsTowardsEnemySpawn end. moves=" + moves.Count);
            return moves;
        }

        private List<Move> MoveBotsTowardsEnergy(Game game, List<Coord> energyPoints, ref List<Coord> botsToMove,
           ref List<Coord> coordsToAvoid)
        {
            Util.Log(game.player, "MoveBotsTowardsEnergy start. botsToMove=" + botsToMove.Count);
            List<Move> moves = new List<Move>();

            for (int x = botsToMove.Count - 1; x > 0; x--)
            {
                Coord bot = botsToMove[x];
                Coord nearestEnergy = Util.FindNearestEnergy(energyPoints, bot);
                Move move = Util.MoveTowardsCoord(bot, nearestEnergy, game, ref  botsToMove, ref  coordsToAvoid);
                if (move != null)
                {
                    Util.Log(game.player, "MoveBotsTowardsEnergy = " + move.Print() + ".  Energy=" + nearestEnergy.Print() + " ChasingBot=" + bot.Print());
                    moves.Add(move);
                }
                else
                {
                    Util.Log(game.player, "move null");
                }
            }

            //foreach (Coord energyPoint in energyPoints)
            //{
            //    Coord chasingBot = Util.FindNearestBot(botsToMove, energyPoint);

            //    if (chasingBot == null)
            //        continue;

            //    Move move = Util.MoveTowardsCoord(chasingBot, energyPoint, game, botsToMove, coordsToAvoid);
            //    if (move != null)
            //    {
            //        Util.Log(game.player, "MoveBotsTowardsEnergy = " + move.Print() + ".  Energy=" + energyPoint.Print() + " ChasingBot=" + chasingBot.Print());
            //        moves.Add(move);
            //    }
            //}

            Util.Log(game.player, "MoveBotsTowardsEnergy end. moves=" + moves.Count + ". botsToMove=" + botsToMove.Count);
            return moves;
        }
    }
}