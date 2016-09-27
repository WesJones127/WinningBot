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

            Coord spawnPoint = (game.player == "r") ?
                game.state.p1.spawn.ToCoord(game.state.rows, game.state.cols) :
                game.state.p2.spawn.ToCoord(game.state.rows, game.state.cols);
            Coord spawnedBot = myPlayerCoords.FirstOrDefault(c => c.EqualTo(spawnPoint));

            if (spawnedBot != null)
                moves.AddRange(MoveNewlySpawnedBot(game, spawnPoint, myPlayerCoords, coordsToAvoid));

            if (myPlayerCoords.Count >= 5)
                moves.AddRange(GuardSpawnPoint(game, myPlayerCoords, coordsToAvoid));



            foreach (Coord coord in myPlayerCoords)
            {
                Coord nearestEnergy = Util.FindNearestEnergy(game.gridData.energyCoords, coord);

                if (nearestEnergy != null)
                {
                    Move move = Util.MoveTowardsCoord(coord, nearestEnergy, game.state.cols, coordsToAvoid.ToList());

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

        private List<Move> GuardSpawnPoint(Game game, List<Coord> playerCoords, List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();

            Coord guardSpot1 =
                Util.ConvertIndexToCoord(
                (game.player == "r") ? game.state.p1.spawn + game.state.cols : game.state.p2.spawn - game.state.cols,
                game.state.rows,
                game.state.cols);

            Coord guardSpot2 =
                Util.ConvertIndexToCoord(
                (game.player == "r") ? game.state.p1.spawn + 1 : game.state.p2.spawn - 1,
                game.state.rows,
                game.state.cols);


            Coord guardBot1 = Util.FindNearestBot(playerCoords, guardSpot1);
            if (guardBot1 != null)
            {
                Move move = Util.MoveTowardsCoord(guardBot1, guardSpot1, game.state.cols, coordsToAvoid);
                if (move != null)
                {
                    Debug.WriteLine(move.Print());
                    moves.Add(move);

                    // remove the From location from the list of spots to avoid
                    coordsToAvoid.RemoveAll(c => c.EqualTo(new Coord(move, false, game.state.cols)));
                    // add the To location to the list of spots to avoid
                    coordsToAvoid.Add(new Coord(move, true, game.state.cols));
                }

                // remove the chosen guard bot from the list of bots to move
                playerCoords.RemoveAll(c => c.EqualTo(guardBot1));
            }

            Coord guardBot2 = Util.FindNearestBot(playerCoords, guardSpot2);
            if (guardBot2 != null)
            {
                Move move = Util.MoveTowardsCoord(guardBot2, guardSpot2, game.state.cols, coordsToAvoid);
                if (move != null)
                {
                    Debug.WriteLine(move.Print());
                    moves.Add(move);

                    // remove the From location from the list of spots to avoid
                    coordsToAvoid.RemoveAll(c => c.Equals(new Coord(move, false, game.state.cols)));
                    // add the To location to the list of spots to avoid
                    coordsToAvoid.Add(new Coord(move, true, game.state.cols));
                }

                // remove the chosen guard bot from the list of bots to move
                playerCoords.RemoveAll(c => c.Equals(guardBot2));
            }
            if (moves.Count == 0)
                Util.Log(game.player, "adding 0 moves");
            return moves;
        }

        private List<Move> MoveNewlySpawnedBot(Game game, Coord spawnPoint, List<Coord> playerCoords, List<Coord> coordsToAvoid)
        {
            List<Move> moves = new List<Move>();
            Direction direction1 = (spawnPoint.Y > game.state.rows / 2) ? Direction.UP : Direction.DOWN;
            Direction direction2 = (spawnPoint.Y > game.state.rows / 2) ? Direction.LEFT : Direction.RIGHT;
            int cols = game.state.cols;
            Coord desiredSpot = spawnPoint.MoveTo(direction1);
            List<Coord> occupiedSpots = playerCoords.Concat(coordsToAvoid).ToList();

            if (!Util.CoordOccupied(desiredSpot, cols, occupiedSpots))
                return new List<Move>()
                {
                    new Move(spawnPoint.ToIndex(cols), desiredSpot.ToIndex(cols))
                };

            desiredSpot = spawnPoint.MoveTo(direction2);
            if (!Util.CoordOccupied(desiredSpot, cols, occupiedSpots))
                return new List<Move>()
                {
                 new Move(spawnPoint.ToIndex(cols), desiredSpot.ToIndex(cols))
                };

            desiredSpot = spawnPoint.MoveTo(direction1.Opposite());
            if (!Util.CoordOccupied(desiredSpot, cols, occupiedSpots))
                return new List<Move>()
                {
                 new Move(spawnPoint.ToIndex(cols), desiredSpot.ToIndex(cols))
                };

            desiredSpot = spawnPoint.MoveTo(direction2.Opposite());
            if (!Util.CoordOccupied(desiredSpot, cols, occupiedSpots))
                return new List<Move>()
                {
                 new Move(spawnPoint.ToIndex(cols), desiredSpot.ToIndex(cols))
                };

            // if we got here, all direct moves are blocked
            //todo: bump player bot out of the way
            return moves;
        }
    }


}