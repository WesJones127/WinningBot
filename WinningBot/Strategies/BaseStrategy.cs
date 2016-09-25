using System;
using System.Collections.Generic;
using System.Linq;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public class BaseStrategy : IStrategy
    {
        public List<Move> getMoves(Game game)
        {
            List<Move> moves = new List<Move>();
            return moves;
        }

    }
}