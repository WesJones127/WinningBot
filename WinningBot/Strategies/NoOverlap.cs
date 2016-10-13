using System.Collections.Generic;
using System.Linq;
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

            // CHASE EACH ENERGY WITH CLOSEST BOT
            MoveBotsTowardsEnergy(moves, game, energyCoords, botsToMove, coordsToAvoid);

            // ATTEMPT TO RAZE ENEMY SPAWN
            MoveBotsTowardsEnemySpawn(moves, game, botsToMove, coordsToAvoid);

            // MOVE REMAINING BOTS TOWARDS ENERGY
            MoveBotsTowardsEnergy(moves, game, energyCoords, botsToMove, coordsToAvoid, true);

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
            List<Move> newMoves = new List<Move>();
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

            bool moveMade = false;
            if (!Util.CoordOccupied(desiredSpot1, cols, coordsToAvoid))
                moveMade = Util.AddMove(newMoves, Util.MoveTowardsCoord(spawnPoint, desiredSpot1, game, botsToMove, coordsToAvoid));

            if (!moveMade && !Util.CoordOccupied(desiredSpot2, cols, coordsToAvoid))
                moveMade = Util.AddMove(newMoves, Util.MoveTowardsCoord(spawnPoint, desiredSpot2, game, botsToMove, coordsToAvoid));

            if (!moveMade && !Util.CoordOccupied(desiredSpot3, cols, coordsToAvoid))
                moveMade = Util.AddMove(newMoves, Util.MoveTowardsCoord(spawnPoint, desiredSpot3, game, botsToMove, coordsToAvoid));

            if (!moveMade && !Util.CoordOccupied(desiredSpot4, cols, coordsToAvoid))
                moveMade = Util.AddMove(newMoves, Util.MoveTowardsCoord(spawnPoint, desiredSpot4, game, botsToMove, coordsToAvoid));

            foreach (Move move in newMoves)
            {
                Util.Log(game.player, "MoveBotFromSpawnPoint = " + move.Print(cols));
                moves.Add(move);
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
            if (game.gridData.spawnDisabled)
                return;

            if (botsToMove.Count < 2)
                return;

            List<Move> newMoves = new List<Move>();

            if (botsToMove.Count <= 3)
            {
                if (Util.GuardNeeded(game, botsToMove, game.gridData.enemyCoords))
                    Util.AddMoves(newMoves, Util.CreateSingleBotGuard(game, botsToMove, coordsToAvoid));
            }
            else if (botsToMove.Count <= 5)
                Util.AddMoves(newMoves, Util.CreateSingleBotGuard(game, botsToMove, coordsToAvoid));
            else if (botsToMove.Count <= 7)
                Util.AddMoves(newMoves, Util.CreateDoubleBotGuard(game, botsToMove, coordsToAvoid));
            else
                Util.AddMoves(newMoves, Util.CreateMediumGuard(game, botsToMove, coordsToAvoid));


            foreach (Move move in newMoves)
            {
                Util.Log(game.player, "GuardSpawnPoint = " + move.Print(game.state.cols));
                moves.Add(move);
            }
        }

        private void MoveBotsTowardsEnemySpawn(List<Move> moves, Game game, List<Coord> botsToMove, List<Coord> coordsToAvoid)
        {
            if (game.gridData.enemySpawnDisabled)
                return;

            Coord enemySpawn = game.gridData.enemySpawn;
            int razingBots;
            List<Move> newMoves = new List<Move>();

            if (botsToMove.Count <= 2)
                return;

            if (botsToMove.Count <= 5)
                razingBots = 1;
            else
                razingBots = botsToMove.Count / 3;

            while (razingBots > 0)
            {
                Coord nearestBot = Util.FindNearestBot(botsToMove, enemySpawn);
                if (nearestBot != null)
                    Util.AddMove(newMoves, Util.MoveTowardsCoord(nearestBot, enemySpawn, game, botsToMove, coordsToAvoid));
                razingBots--;
            }

            foreach (Move move in newMoves)
            {
                Util.Log(game.player, "MoveBotsTowardsEnemySpawn = " + move.Print(game.state.cols));
                moves.Add(move);
            }
        }

        private void MoveBotsTowardsEnergy(List<Move> moves, Game game, List<Coord> energyPoints, List<Coord> botsToMove,
           List<Coord> coordsToAvoid, bool moveAllRemainingTroops = false)
        {
            int cols = game.state.cols;
            List<Move> newMoves = new List<Move>();

            // sort each energy point by its distance to the nearest player bot
            // this way we don't pass up a close energy by moving towards an energy that's far away
            List<KeyValuePair<int, int>> energyWithClosestBot = new List<KeyValuePair<int, int>>(); // Int1 = index of energy point, Int2 = distance to nearest bot
            foreach (Coord energyPoint in energyPoints)
            {
                Coord closestBot = Util.FindNearestBot(botsToMove, energyPoint);
                if (closestBot != null)
                {
                    energyWithClosestBot.Add(new KeyValuePair<int, int>(energyPoint.ToIndex(cols), Util.GetDistanceBetweenCoords(energyPoint, closestBot)));
                }
            }

            energyWithClosestBot.Sort(CompareKVP);
            foreach (KeyValuePair<int, int> energyPoint in energyWithClosestBot)
            {
                Coord energy = energyPoints.First(e => e.ToIndex(cols) == energyPoint.Key);
                Coord nearestBot = Util.FindNearestBot(botsToMove, energy);
                if (nearestBot != null)
                    Util.AddMove(newMoves, Util.MoveTowardsCoord(nearestBot, energy, game, botsToMove, coordsToAvoid));
            }


            if (moveAllRemainingTroops)
            {
                botsToMove = game.player == "r"
                    ? botsToMove.OrderBy(b => b.ToIndex(cols)).ToList()
                    : botsToMove.OrderByDescending(b => b.ToIndex(cols)).ToList();

                for (int x = botsToMove.Count - 1; x >= 0; x--)
                {
                    Coord bot = botsToMove[x];
                    Coord nearestEnergy = Util.FindNearestEnergy(energyPoints, bot);

                    if (nearestEnergy != null)
                        Util.AddMove(newMoves, Util.MoveTowardsCoord(bot, nearestEnergy, game, botsToMove, coordsToAvoid));
                }
            }

            foreach (Move move in newMoves)
            {
                Util.Log(game.player, "MoveBotsTowardsEnergy = " + move.Print(cols));
                moves.Add(move);
            }
        }

        private int CompareKVP(KeyValuePair<int, int> a, KeyValuePair<int, int> b)
        {
            return a.Value.CompareTo(b.Value);
        }
    }
}