namespace Lab1;

public class V2DataList : V2Data
{
    public List<DataItem> Data { get; set; }

    public override IEnumerator<DataItem> GetEnumerator()
    {
        return Data.GetEnumerator();
    }
    
    public override double MinField
    {
        get
        {
            if (Data.Count == 0)
                return 0;
            double min = Double.MaxValue;
            foreach (var y in Data)
            {
                min = Math.Min(Math.Min(Math.Abs(y.Y[0]), Math.Abs(y.Y[1])), min);
            }
            return min;
        }
    }

    public override DataItem xMaxItem
    {
        get
        {
            DataItem max = new DataItem( Double.MinValue,  Double.MinValue,  Double.MinValue);
            foreach (var item in Data)
            {
                if (max.X < item.X)
                {
                    max = item;
                }
            }

            return max;
        }
    }

    public V2DataArray Array
    {
        get
        {
            V2DataArray array = new V2DataArray(Key, Date);
            array.X = new double[Data.Count];
            array.Y = new double[Data.Count, Data.Count];
            for (int i = 0; i < Data.Count; ++i)
            {
                array.X[i] = Data[i].X;
                array.Y[0, i] = Data[i].Y[0];
                array.Y[1, i] = Data[i].Y[1];
            }

            return array;
        }
    }

    public V2DataList(string key, DateTime date) : base(key, date)
    {
        Data = new List<DataItem>();
    }

    public V2DataList(string key, DateTime date, double[] x, FDI F) : base(key, date)
    {
        Data = new List<DataItem>();
        List<double> usedX = new List<double>();
        foreach (double it in x)
        {
            if (usedX.Contains(it)) continue;
            Data.Add(F(it));
            usedX.Add(it);
        }
    }

    public override string ToString()
    {
        return  "V2DataList " + Key.ToString() + " " + Date.ToString() + " " + Data.Count.ToString();
    }

    public override string ToLongString(string format)
    {
        string s = ToString() + "\n";
        foreach (var Item in Data)
        {
            s += Item.ToLongString(format) + "\n";
        }

        return s;
    }
}