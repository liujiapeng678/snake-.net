namespace SnakeClient.GameClass
{
    public class Cell
    {
        public int R { get; set; }
        public int C { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public Cell()
        {
        }
        public Cell(int r, int c)
        {
            R = r;
            C = c;
            X = c + 0.5;
            Y = r + 0.5;
        }

        public Cell(Cell other)
        {
            R = other.R;
            C = other.C;
            X = other.X;
            Y = other.Y;
        }
    }
}
