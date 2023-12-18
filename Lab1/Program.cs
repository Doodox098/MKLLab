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

    public static void IOCheck()
    {
        Random rnd = new Random();
        double[] x = new[] { 0.0, 1.0, 2.0};
        
        V2DataArray v2da = new V2DataArray(rnd.Next(1000000, 10000000).ToString(), DateTime.Now, x, G);
        
        string filename = "C:/Users/doodo/Desktop/Contest/5Sem/1Lec/Lab1/v2da.json";
        v2da.Save(filename);
        
        V2DataArray v2da2 = new V2DataArray("test", DateTime.Now);
        V2DataArray.Load(filename, ref v2da2);
        
        System.Console.Write(v2da.ToLongString("{0:f2}"));
        Console.WriteLine("");
        System.Console.Write(v2da2.ToLongString("{0:f2}"));
        Console.WriteLine("");
        Console.Write(File.ReadAllText(filename));
        Console.WriteLine("");
    }

    public static void LINQCheck()
    {
        Random rnd = new Random();
        
        double[] x = new[] { 1.0, 1.0, 2.0};
        V2DataArray v2da = new V2DataArray(rnd.Next(1000000, 10000000).ToString(), DateTime.Now, x, G);
        
        x = new[] { 2.0, 7.0, 8.5};
        V2DataList v2dl = new V2DataList(rnd.Next(1000000, 10000000).ToString(), DateTime.Now, x, G);
        
        V2MainCollection v2mc = new V2MainCollection(0, 0);
        
        v2mc.Add(v2da);
        v2mc.Add(v2dl);
        v2mc.Add(new V2DataArray(rnd.Next(1000000, 10000000).ToString(), DateTime.Now));
        v2mc.Add(new V2DataList(rnd.Next(1000000, 10000000).ToString(), DateTime.Now));
        
        System.Console.Write(v2mc.ToLongString("{0:f2}"));
        Console.WriteLine("");
        System.Console.Write("Max number of zero-fields in V2Data's:    ");
        System.Console.WriteLine(v2mc.MaxZeros);
        System.Console.Write("Max Field in V2Data's:                    ");
        System.Console.WriteLine(((DataItem)v2mc.MaxField).ToLongString("{0:f2}"));
        System.Console.Write("All Unique X's:                           ");
        foreach (var X in v2mc.Grid)
        {
            System.Console.Write(String.Format("{0:f2} ", X));
        }
        Console.WriteLine("");
    }

   static int Main()
    {
        Random rnd = new Random();

        double[] x = new[] { 1.0, 1.5, 2.0 };
        V2DataArray v2da = new V2DataArray(rnd.Next(1000000, 10000000).ToString(), DateTime.Now, x, G);
        System.Console.WriteLine(v2da.ToLongString("{0:f2}"));
        SplineData sd = new SplineData(ref v2da, 5, 5);
        sd.CalculateSpline();
        System.Console.WriteLine(sd.ToLongString("{0:f2}"));
        return 0;
    }
}

