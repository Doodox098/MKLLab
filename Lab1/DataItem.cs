namespace Lab1;

public struct DataItem
{
    public double X { get; }
    public double[] Y { get; }

    public DataItem(double x, double y1, double y2)
    {
        this.X = x;
        this.Y = new [] { y1, y2 };
    }

    public string ToLongString(string format)
    {
        return string.Format(format, X) + " " + string.Format(format, Y[0]) + " " + string.Format(format, Y[1]);
    }
    public override string ToString()
    {
        return ToLongString("{0:F2}");
    }
}