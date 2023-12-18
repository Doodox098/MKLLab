using System.Text;
using System.Text.Json;

namespace Lab1;

public class V2DataArray : V2Data
{
    public double[] X { get; set; }
    public double[,] Y { get; set; }
    class MyEnumerator : IEnumerator<DataItem>
    {
        private int current = -1;

        private V2DataArray array;

        public bool MoveNext()
        {
            if (current < array.X.Length - 1)
            {
                ++current;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            current = -1;
        }

        DataItem IEnumerator<DataItem>.Current => (DataItem)Current;

        public Object Current
        {
            get
            {
                return new DataItem(array.X[current], array.Y[0, current], array.Y[1, current]);
            }
        }

        public MyEnumerator(V2DataArray a)
        {
            array = a;
        }
        
        public void Dispose()
        {
        }
    }
    public override IEnumerator<DataItem> GetEnumerator()
    {
        return new MyEnumerator(this);
    }
    public override double MinField
    {
        get
        {
            if (X.Length == 0)
                return 0;
            double min = Double.MaxValue;
            foreach (var y in Y)
            {
                min = Math.Min(Math.Abs(y), min);
            }
            return min;
        }
    }
    public override DataItem xMaxItem
    {
        get
        {
            DataItem max = new DataItem( Double.MinValue,  Double.MinValue,  Double.MinValue);
            for (int i = 0; i < X.Length; ++i)
            {
                if (max.X < X[i])
                {
                    max = new DataItem(X[i], Y[0, i], Y[1, i]);
                }
            }

            return max;
        }
    }
    public double[] this[int k]
    {
        get
        {
            double[] y = new double[X.Length];
            for (int i = 0; i < X.Length; i++)
            {
                y[i] = Y[k, i];
            }

            return y;
        }
    }
    public V2DataArray(string key, DateTime date) : base(key, date)
    {
        X = new double[0];
        Y = new double[2,0];
    }
    public V2DataArray(string key, DateTime date, double[] x, FValues F) : base(key, date)
    {
        X = x;
        Y = new double[2, X.Length];
        for (int i = 0; i < X.Length; ++i)
        {
            F(X[i], ref Y[0, i], ref Y[1, i]);
        }
    }
    public V2DataArray(string key, DateTime date, int nX, double xL, double xR, FValues F) : base(key, date)
    {
        double dX = (xR - xL) / nX;
        X = new double[nX];
        Y= new double[nX, nX];
        for (int i = 0; i < nX; ++i)
        {
            X[i] = xL + i * dX;
            F(X[i], ref Y[0, i], ref Y[1, i]);
        }
    }
    public static explicit operator V2DataList(V2DataArray source)
    {
        V2DataList list = new V2DataList(source.Key, source.Date);
        for (int i = 0; i < source.X.Length; i++)
        {
            list.Data.Add(new DataItem(source.X[i], source.Y[0, i], source.Y[1, i]));
        }

        return list;
    }
    public override string ToString()
    {
        return "V2DataArray " + Key.ToString() + " " + Date.ToString();
    }
    public override string ToLongString(string format)
    {
        string s = ToString() + "\n";
        for (int i = 0; i < X.Length; i++)
        {
            s += string.Format(format, X[i]) + " " + string.Format(format, Y[0, i]) + " " + string.Format(format, Y[1, i]) + "\n";
        }

        return s;
    }
    public bool Save(string filename)
    {
        bool stat = true;
        try
        {
            string jsonDate = JsonSerializer.Serialize(base.Date) + "\n";
            string jsonX = JsonSerializer.Serialize(X) + "\n";
            string jsonY1 = JsonSerializer.Serialize(this[0]) + "\n";
            string jsonY2 = JsonSerializer.Serialize(this[1]) + "\n";
            
            File.WriteAllText(filename, jsonX + jsonY1 + jsonY2 + Key + "\n" + jsonDate);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            stat = false;
        }

        return stat;
    }
    public static bool Load(string filename, ref V2DataArray array)
    {
        bool stat = true;
        try
        {
            StreamReader sr = new StreamReader(filename);
            array.X = JsonSerializer.Deserialize<double[]>(sr.ReadLine());
            double[] y1 = JsonSerializer.Deserialize<double[]>(sr.ReadLine());
            double[] y2 = JsonSerializer.Deserialize<double[]>(sr.ReadLine());
            array.Y = new double[2, array.X.Length];
            for (int i = 0; i < array.X.Length; i++)
            {
                array.Y[0, i] = y1[i];
                array.Y[1, i] = y2[i];
            }
            array.Key = sr.ReadLine();
            array.Date = JsonSerializer.Deserialize<DateTime>(sr.ReadLine());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            stat = false;
        }

        return stat;
    }
}