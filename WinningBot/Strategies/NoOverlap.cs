using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
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
            List<Coord> myPlayerCoords = new List<Coord>(game.gridData.playerCoords);

            // coordsToAvoid includes existing player bots, existing enemy bots, possible enemy moves, and the player spawn point
            List<Coord> coordsToAvoid = new List<Coord>(game.gridData.enemyCoordsIncludingPossibleMoves.Concat(myPlayerCoords));
            coordsToAvoid.Add(game.gridData.spawn.ToCoord(game.state.rows, game.state.cols));

            List<Coord> energyCoords = new List<Coord>(game.gridData.energyCoords);



            // STEP 1: ALWAYS MOVE NEWLY SPAWNED BOTS FROM SPAWN POINT
            moves.AddRange(MoveNewlySpawnedBot(game, myPlayerCoords, coordsToAvoid));


            // STEP 2: IF WE HAVE ENOUGH BOTS, MOVE TO GUARD POSITIONS
            moves.AddRange(GuardSpawnPoint(game, myPlayerCoords, coordsToAvoid));


            // STEP 3: CHASE ENERGY WITH CLOSEST BOT
            moves.AddRange(MoveBotsTowardsEnergy(game, energyCoords, myPlayerCoords, coordsToAvoid));


            // STEP 4: USE REMAINING BOTS TO RAZE ENEMY SPAWN
            moves.AddRange(MoveBotsTowardsEnemySpawn(game, myPlayerCoords));

            // STEP 5: CHASE ENEMY BOTS
            moves.AddRange(ChaseEnemyBots(game, myPlayerCoords, game.gridData.enemyCoords, coordsToAvoid));
           
            return moves;
        }

        private IEnumerable<Move> ChaseEnemyBots(Game game, List<Coord> playerCoords, List<Coord> enemyBotCoords, List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();
            int rows = game.state.rows;
            int cols = game.state.cols;

            for (int x = playerCoords.Count - 1; x >= 0; x--)
            {
                Coord bot = playerCoords[x];
                Coord enemyBot = Util.FindNearestTarget(enemyBotCoords, bot);
                Move move = Util.MoveTowardsCoord(bot, enemyBot, game, coordsToAvoid);
                if (move != null)
                {
                    moves.Add(move);
                    playerCoords.RemoveAt(x);
                    coordsToAvoid.RemoveAll(c => c.EqualTo(move.ToCoord(true, rows, cols)));
                    coordsToAvoid.Add(move.ToCoord(false, rows, cols));
                }
            }
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
        private List<Move> MoveNewlySpawnedBot(Game game, List<Coord> playerCoords, List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();
            int cols = game.state.cols;
            int rows = game.state.rows;
            Coord spawnPoint = Util.FindSpawnPoint(game);
            Coord spawnedBot = playerCoords.FirstOrDefault(c => c.EqualTo(spawnPoint));
            bool moveMade = false;

            if (spawnedBot == null)
                return moves;


            Direction direction1 = (spawnPoint.Y > game.state.rows / 2) ? Direction.UP : Direction.DOWN;
            Direction direction2 = (spawnPoint.Y > game.state.rows / 2) ? Direction.LEFT : Direction.RIGHT;
            Coord desiredSpot1, desiredSpot2, desiredSpot3, desiredSpot4;
            desiredSpot1 = spawnPoint.MoveTo(direction1);
            desiredSpot2 = spawnPoint.MoveTo(direction2);
            desiredSpot3 = spawnPoint.MoveTo(direction1.Opposite());
            desiredSpot4 = spawnPoint.MoveTo(direction2.Opposite());
            List<Coord> occupiedSpots = playerCoords.Concat(coordsToAvoid).ToList();

            while (moveMade == false)
            {
                if (!Util.CoordOccupied(desiredSpot1, cols, occupiedSpots))
                {
                    moveMade = true;
                    moves.Add(new Move(spawnPoint.ToIndex(cols), desiredSpot1.ToIndex(cols)));
                }

                if (!Util.CoordOccupied(desiredSpot2, cols, occupiedSpots))
                {
                    moveMade = true;
                    moves.Add(new Move(spawnPoint.ToIndex(cols), desiredSpot2.ToIndex(cols)));
                }

                if (!Util.CoordOccupied(desiredSpot3, cols, occupiedSpots))
                {
                    moveMade = true;
                    moves.Add(new Move(spawnPoint.ToIndex(cols), desiredSpot3.ToIndex(cols)));
                }

                if (!Util.CoordOccupied(desiredSpot4, cols, occupiedSpots))
                {
                    moveMade = true;
                    moves.Add(new Move(spawnPoint.ToIndex(cols), desiredSpot4.ToIndex(cols)));
                }
            }

            foreach (Move move in moves)
            {
                coordsToAvoid.RemoveAll(c => c.EqualTo(move.from.ToCoord(rows, cols)));
                coordsToAvoid.Add(move.to.ToCoord(rows, cols));

            }

            // if we got here, all direct moves are blocked
            //   moves.AddRange(recursivelyMoveBlockedBots(spawnPoint, desiredSpot1, game, playerCoords, coordsToAvoid));
            return moves;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="playerCoords">This is the list of bots that need to be moved, NOT the list of all player bots</param>
        /// <param name="coordsToAvoid">This includes all player bots, enemy bots, and possible new locations for enemy bots</param>
        /// <returns></returns>
        private List<Move> GuardSpawnPoint(Game game, List<Coord> playerCoords, List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();

            if (playerCoords.Count < 6)
                return moves;

            int cols = game.state.cols;
            int rows = game.state.rows;

            Coord guardSpot1 =
                Util.ConvertIndexToCoord(
                (game.player == "r") ? game.state.p1.spawn + cols - 1 : game.state.p2.spawn - cols + 1, rows, cols);

            Coord guardSpot2 =
                Util.ConvertIndexToCoord(
                (game.player == "r") ? game.state.p1.spawn + cols + 1 : game.state.p2.spawn - cols - 1, rows, cols);

            Coord guardSpot3 =
                Util.ConvertIndexToCoord(
                (game.player == "r") ? game.state.p1.spawn - cols + 1 : game.state.p2.spawn + cols - 1, rows, cols);

            Coord guardBot1 = Util.FindNearestBot(playerCoords, guardSpot1);
            if (guardBot1 != null)
            {
                Move move = Util.MoveTowardsCoord(guardBot1, guardSpot1, game, coordsToAvoid);
                if (move != null)
                {
                    moves.Add(move);
                    coordsToAvoid.RemoveAll(c => c.EqualTo(new Coord(move, false, cols)));
                    coordsToAvoid.Add(new Coord(move, true, cols));
                }

                // remove the chosen guard bot from the list of bots to move
                playerCoords.RemoveAll(c => c.EqualTo(guardBot1));
            }

            Coord guardBot2 = Util.FindNearestBot(playerCoords, guardSpot2);
            if (guardBot2 != null)
            {
                Move move = Util.MoveTowardsCoord(guardBot2, guardSpot2, game, coordsToAvoid);
                if (move != null)
                {
                    moves.Add(move);
                    coordsToAvoid.RemoveAll(c => c.Equals(new Coord(move, false, cols)));
                    coordsToAvoid.Add(new Coord(move, true, cols));
                }

                // remove the chosen guard bot from the list of bots to move
                playerCoords.RemoveAll(c => c.Equals(guardBot2));
            }

            Coord guardBot3 = Util.FindNearestBot(playerCoords, guardSpot3);
            if (guardBot3 != null)
            {
                Move move = Util.MoveTowardsCoord(guardBot3, guardSpot3, game, coordsToAvoid);
                if (move != null)
                {
                    moves.Add(move);
                    coordsToAvoid.RemoveAll(c => c.Equals(new Coord(move, false, cols)));
                    coordsToAvoid.Add(new Coord(move, true, cols));
                }

                // remove the chosen guard bot from the list of bots to move
                playerCoords.RemoveAll(c => c.Equals(guardBot3));
            }

            return moves;
        }

        private List<Move> MoveBotsTowardsEnergy(Game game, List<Coord> energyPoints, List<Coord> playerCoords,
            List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();
            int rows = game.state.rows;
            int cols = game.state.cols;

            foreach (Coord energyPoint in energyPoints)
            {
                Coord chasingBot = Util.FindNearestBot(playerCoords, energyPoint);

                if (chasingBot == null)
                    continue;

                Move move = Util.MoveTowardsCoord(chasingBot, energyPoint, game, coordsToAvoid, false);
                if (move != null)
                {
                    moves.Add(move);
                    playerCoords.RemoveAll(c => c.EqualTo(move.ToCoord(true, rows, cols)));
                    coordsToAvoid.RemoveAll(c => c.EqualTo(move.ToCoord(true, rows, cols)));
                    coordsToAvoid.Add(move.ToCoord(false, rows, cols));
                }

            }

            return moves;
        }

        private List<Move> MoveBotsTowardsEnemySpawn(Game game, List<Coord> playerCoords)
        {
            int rows = game.state.rows;
            int cols = game.state.cols;
            Coord enemySpawn = game.gridData.enemySpawn.ToCoord(rows, cols);
            List<Move> moves = new List<Move>();
            List<Coord> coordsToAvoid = playerCoords.Concat(game.gridData.enemyCoords).ToList();

            // exit if we already have their spawn point razed
            // and remove the razing bot from the list of bots to be moved
            Coord razingBot = playerCoords.FirstOrDefault(c => c.EqualTo(enemySpawn));
            if (razingBot != null)
            {
                playerCoords.RemoveAll(c => c.EqualTo(razingBot));
                return moves;
            }

            for (int x = playerCoords.Count - 1; x >= 0; x--)
            {
                Coord playerCoord = playerCoords[x];
                Move move = Util.MoveTowardsCoord(playerCoord, enemySpawn, game, coordsToAvoid, true);

                if (move != null)
                {
                    moves.Add(move);
                    playerCoords.RemoveAt(x);
                    coordsToAvoid.RemoveAll(c => c.EqualTo(move.ToCoord(true, rows, cols)));
                    coordsToAvoid.Add(move.ToCoord(false, rows, cols));
                }
            }

            return moves;
        }

    }
}