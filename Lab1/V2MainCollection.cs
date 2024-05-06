namespace Lab1;

public class V2MainCollection : System.Collections.ObjectModel.ObservableCollection<V2Data>
{
    public List<DataItem> xMaxItems
    {
        get
        {
            List<DataItem> max = new List<DataItem>();
            foreach (var v2d in Items)
            {
                max.Add(v2d.xMaxItem);
            }

            return max;
        }
    }

    public int MaxZeros
    {
        get
        {
            double eps = 1e-10;
            if (Items.Count == 0) return -1;
            return Items.Max(data => data.Count(item => Math.Abs(item.Y[0]) < eps && Math.Abs(item.Y[1]) < eps));
        }
    }
    public DataItem? MaxField
    {
        get
        {
            if (Items.Count == 0) return null;
            Func<DataItem, double> Magnitude = item => Math.Pow(item.Y[0], 2) + Math.Pow(item.Y[1], 2);
            return Items.Select(data => 
                data.DefaultIfEmpty(new DataItem(0, 0, 0)).MaxBy(Magnitude))
                .MaxBy(Magnitude);
        }
    }
    public IEnumerable<double> Grid
    {   
        get
        {
            if (Items.Count == 0) return null;
            return Items.SelectMany(data =>
                data.Select(item => item.X)
                .GroupBy(X => X)
                .Where(group => group.Count() == 1)
                    )
                .SelectMany(group => group.AsEnumerable()).Distinct();
        }
    }
    public bool Contains(string key)
    {
        foreach (var item in base.Items)
        {
            if (item.Key == key)
                return true;
        }

        return false;
    }
    public bool Add(V2Data v2Data)
    {
        if (!this.Contains(v2Data.Key))
        {
            base.Add(v2Data);
            return true;
        }

        return false;
    }
    
    public V2MainCollection(int nV2DataArray, int nV2DataList)
    {
        int i = 1;
        for (; i <= nV2DataArray; ++i)
        {
            double[] x = new[] { i * 1.25, i * 2.125, i * 3.0625 };
            this.Add(new V2DataArray(i.ToString(), DateTime.Now, x, Program.F));
        }
        for (; i <= nV2DataList + nV2DataArray; ++i)
        {
            double[] x = new[] { i * 1.0, i * 2.25, i * 3.5 };
            this.Add(new V2DataList(i.ToString(), DateTime.Now, x, Program.F));
        }
    }

    public string ToLongString(string format)
    {
        string s = "V2MainCollection \n";
        foreach (var item in Items)
        {
            s += item.ToLongString(format);
        }

        return s;
    }
    
    public override string ToString()
    {
        string s = "V2MainCollection \n";
        foreach (var item in Items)
        {
            s += item.ToString();
        }

        return s;
    }
}