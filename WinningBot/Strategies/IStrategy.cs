using System.Collections.Generic;
using WinningBot.Models;

namespace WinningBot.Strategies
{
    public interface IStrategy
    {
        List<Move> getMoves(Game game);
    }
}