﻿namespace SnakeServer.Models
{
    public class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Cell()
        {
        }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}