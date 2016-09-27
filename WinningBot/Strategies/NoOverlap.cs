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

            foreach (Coord coord in game.gridData.playerCoords)
            {
                Coord nearestEnergy = Util.findNearestEnergy(game.gridData.energyCoords, coord);

                if (nearestEnergy != null)
                {
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



    }

}