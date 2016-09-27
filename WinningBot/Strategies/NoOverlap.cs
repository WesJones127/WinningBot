using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
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

            if (myPlayerCoords.Count >= 5)
            {
                moves.AddRange(GuardSpawnPoint(game, ref myPlayerCoords, ref coordsToAvoid));
            }

            //todo: move newly spawned bots away from Guard Bots
            foreach (Coord coord in game.gridData.playerCoords)
            {
                Coord nearestEnergy = Util.findNearestEnergy(game.gridData.energyCoords, coord);

                if (nearestEnergy != null)
                {
                    //todo: probably don't want to use occupiedCoords here
                    Move move = Util.moveTowardsCoord(coord,
                        nearestEnergy,
                        game.state.cols,
                        game.gridData.enemyCoordsIncludingPossibleMoves.Concat(game.gridData.occupiedCoords)
                            .ToList());
                    Coord newPlayerCoord = Util.ConvertIndexToCoord(move.to, game.state.rows,
                        game.state.cols);

                    moves.Add(move);
                    game.gridData.occupiedCoords.RemoveAll(c => c.X == coord.X && c.Y == coord.Y);
                    game.gridData.occupiedCoords.Add(new Coord(newPlayerCoord.X, newPlayerCoord.Y));
                }
            }

            return moves;
        }

        private List<Move> GuardSpawnPoint(Game game, ref List<Coord> playerCoords, ref List<Coord> coordsToAvoid)
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

            // this does specifically check if a bot is already on the guard spot, but I think it will work that way by finding the closest bot
            Coord guardBot1 = Util.findNearestBot(playerCoords, guardSpot1);
            if (guardBot1 != null)
            {
                moves.Add(Util.moveTowardsCoord(guardBot1, guardSpot1, game.state.cols, coordsToAvoid));
                playerCoords.RemoveAll(c => c.X == guardBot1.X && c.Y == guardBot1.Y);
                //playerCoords.Add(new Coord(guardBot1.X, guardBot1.Y));
                coordsToAvoid.RemoveAll(c => c.X == guardBot1.X && c.Y == guardBot1.Y);
                coordsToAvoid.Add(new Coord(guardBot1.X, guardBot1.Y));
            }

            Coord guardBot2 = Util.findNearestBot(playerCoords, guardSpot2);
            if (guardBot2 != null)
            {
                moves.Add(Util.moveTowardsCoord(guardBot2, guardSpot2, game.state.cols, coordsToAvoid));
                playerCoords.RemoveAll(c => c.X == guardBot2.X && c.Y == guardBot2.Y);
                //playerCoords.Add(new Coord(guardBot2.X, guardBot2.Y));
                coordsToAvoid.RemoveAll(c => c.X == guardBot2.X && c.Y == guardBot2.Y);
                coordsToAvoid.Add(new Coord(guardBot2.X, guardBot2.Y));
            }

            return moves;
        }
    }


}