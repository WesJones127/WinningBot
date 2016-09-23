using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace skookumbot.Models
{
    public class State
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public Player P1 { get; set; }
        public Player P2 { get; set; }
        public string Grid { get; set; }
        public int MaxTurns { get; set; }
        public int TurnsElapsed { get; set; }
    }
}