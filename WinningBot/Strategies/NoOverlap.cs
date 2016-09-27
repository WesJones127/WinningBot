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

            Util.Log(game.player, " myPlayerCoords count = " + myPlayerCoords.Count);
            Util.Log(game.player, "coordsToAvoid count = " + coordsToAvoid.Count);

            if (myPlayerCoords.Count >= 5)
            {
                moves.AddRange(GuardSpawnPoint(game, myPlayerCoords, coordsToAvoid));
            }
            Util.Log(game.player, "myPlayerCoords count = " + myPlayerCoords.Count);
            Util.Log(game.player, "coordsToAvoid count = " + coordsToAvoid.Count);
            Util.Log(game.player, "Guard moves count = " + moves.Count);
        

            foreach (Coord coord in myPlayerCoords)
            {
                Coord nearestEnergy = Util.findNearestEnergy(game.gridData.energyCoords, coord);

                if (nearestEnergy != null)
                {
                    Move move = Util.moveTowardsCoord(coord, nearestEnergy, game.state.cols, coordsToAvoid.ToList());

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
            Util.Log(game.player, "beginning GuardSpawnPoint");
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

            Util.Log(game.player, "guardSpot1/guardSpot2 = " + guardSpot1.Print() + "/" + guardSpot2.Print());

            Coord guardBot1 = Util.findNearestBot(playerCoords, guardSpot1);
            if (guardBot1 != null)
            {
                Move move = Util.moveTowardsCoord(guardBot1, guardSpot1, game.state.cols, coordsToAvoid);
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

            Coord guardBot2 = Util.findNearestBot(playerCoords, guardSpot2);
            if (guardBot2 != null)
            {
                Move move = Util.moveTowardsCoord(guardBot2, guardSpot2, game.state.cols, coordsToAvoid);
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

            if (guardBot1 != null)
                Util.Log(game.player, "guardBot1 = " + guardBot1.Print());
            if (guardBot2 != null)
                Util.Log(game.player, "guardBot2 = " + guardSpot2.Print());


            Util.Log(game.player, "ending GuardSpawnPoint");
            return moves;
        }
    }


}