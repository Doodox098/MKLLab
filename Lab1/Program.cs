using System.Runtime.InteropServices;

namespace Lab1;

class Program
{
    public static DataItem F(double x)
    {
        return new DataItem(x, Math.Sin(Math.PI * x), Math.Sin(Math.PI * x + Math.PI / 2));
    }

    public static DataItem G(double x)
    {
        return new DataItem(x, Math.Sin(Math.PI * x), Math.Sin(Math.PI * x + Math.PI));
    }
    
    public static void F(double x, ref double y1, ref double y2)
    {
        y1 = Math.Cos(Math.PI * x);
        y2 = Math.Sin(Math.PI * x + Math.PI);
    }
    
    public static void G(double x, ref double y1, ref double y2)
    {
        y1 = 2 * Math.Cos(Math.PI * x);
        y2 = Math.Cos(Math.PI * x + Math.PI);
    }


   static int Main()
    {
        Random rnd = new Random();

        double[] x = new[] { 1.0, 1.5, 2.0, 3.0, 4.0, 4.5, 5.0, 6.0, 6.5};
        V2DataArray v2da = new V2DataArray(rnd.Next(1000000, 10000000).ToString(), DateTime.Now, x, G);
        SplineData sd = new SplineData(v2da, 6, 10000, 10, 1e-12);
        // sd.MakeSpline(false);
        sd.Save("save.txt", "{0:F2}");
        return 0;
    }
}

