using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace skookumbot.Models
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int ToIndex(int columns)
        {
            return columns*X + Y;
        }

        public Point() { }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}