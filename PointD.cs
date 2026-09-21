namespace LissajousLab.Models
{
    public readonly struct PointD
    {
        public double X { get; }
        public double Y { get; }

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}