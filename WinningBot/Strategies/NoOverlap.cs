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
            List<Coord> myPlayerCoords = new List<Coord>(game.gridData.playerCoords);
            List<Coord> coordsToAvoid = new List<Coord>(game.gridData.enemyCoordsIncludingPossibleMoves.Concat(myPlayerCoords));

            Coord spawnPoint = Util.FindSpawnPoint(game);
            Coord spawnedBot = myPlayerCoords.FirstOrDefault(c => c.EqualTo(spawnPoint));

            if (spawnedBot != null)
            {
                Move move = MoveNewlySpawnedBot(game, spawnPoint, myPlayerCoords, coordsToAvoid);
                if (move != null)
                    moves.Add(move);
            }
            if (myPlayerCoords.Count >= 6)
                moves.AddRange(GuardSpawnPoint(game, myPlayerCoords, coordsToAvoid));


            foreach (Coord coord in myPlayerCoords)
            {
                Coord nearestEnergy = Util.FindNearestEnergy(game.gridData.energyCoords, coord);

                if (nearestEnergy != null)
                {
                    Move move = Util.MoveTowardsCoord(coord, nearestEnergy, game, coordsToAvoid.ToList());

                    if (move != null)
                    {
                        Coord newPlayerCoord = new Coord(move, true, game.state.cols);

                        moves.Add(move);
                        coordsToAvoid.RemoveAll(c => c.EqualTo(coord));
                        coordsToAvoid.Add(newPlayerCoord.Copy());
                    }
                }
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="spawnPoint"></param>
        /// <param name="playerCoords"></param>
        /// <param name="coordsToAvoid"></param>
        /// <returns></returns>
        private Move MoveNewlySpawnedBot(Game game, Coord spawnPoint, List<Coord> playerCoords, List<Coord> coordsToAvoid)
        {
            Move move = null;
            Direction direction1 = (spawnPoint.Y > game.state.rows / 2) ? Direction.UP : Direction.DOWN;
            Direction direction2 = (spawnPoint.Y > game.state.rows / 2) ? Direction.LEFT : Direction.RIGHT;
            int cols = game.state.cols;
            int rows = game.state.rows;
            Coord desiredSpot1, desiredSpot2, desiredSpot3, desiredSpot4;
            desiredSpot1 = spawnPoint.MoveTo(direction1);
            desiredSpot2 = spawnPoint.MoveTo(direction2);
            desiredSpot3 = spawnPoint.MoveTo(direction1.Opposite());
            desiredSpot4 = spawnPoint.MoveTo(direction2.Opposite());
            List<Coord> occupiedSpots = playerCoords.Concat(coordsToAvoid).ToList();

            if (!Util.CoordOccupied(desiredSpot1, cols, occupiedSpots))
                move = new Move(spawnPoint.ToIndex(cols), desiredSpot1.ToIndex(cols));

            if (!Util.CoordOccupied(desiredSpot2, cols, occupiedSpots))
                move = new Move(spawnPoint.ToIndex(cols), desiredSpot2.ToIndex(cols));

            if (!Util.CoordOccupied(desiredSpot3, cols, occupiedSpots))
                move = new Move(spawnPoint.ToIndex(cols), desiredSpot3.ToIndex(cols));

            if (!Util.CoordOccupied(desiredSpot4, cols, occupiedSpots))
                move = new Move(spawnPoint.ToIndex(cols), desiredSpot4.ToIndex(cols));

            if (move != null)
            {
                coordsToAvoid.RemoveAll(c => c.EqualTo(move.from.ToCoord(rows, cols)));
                coordsToAvoid.Add(move.to.ToCoord(rows, cols));
            }

            // if we got here, all direct moves are blocked
            //   moves.AddRange(recursivelyMoveBlockedBots(spawnPoint, desiredSpot1, game, playerCoords, coordsToAvoid));
            return move;
        }

        private List<Move> recursivelyMoveBlockedBots(Coord from, Coord to, Game game, List<Coord> playerCoords,
            List<Coord> coordsToAvoid)
        {
            // if dest is open move to it
            // if dest is taken by player, recursive move dest




            List<Move> moves = new List<Move>();
            int cols = game.state.cols;

            // if desired spot is open, take it
            if (coordsToAvoid.FirstOrDefault(c => c.EqualTo(to)) == null)
            {
                moves.Add(new Move(from.ToIndex(cols), to.ToIndex(cols)));
                playerCoords.RemoveAll(c => c.EqualTo(from));
                coordsToAvoid.RemoveAll(c => c.EqualTo(from));
                coordsToAvoid.Add(to.Copy());
                return moves;
            }

            if (playerCoords.FirstOrDefault(c => c.EqualTo(to)) != null)
            {
                List<Coord> possibleMoves = Util.GetAdjacentCoords(to, game);

                foreach (Coord possibleMove in possibleMoves)
                {

                }

            }

            return moves;

        }

    }
}