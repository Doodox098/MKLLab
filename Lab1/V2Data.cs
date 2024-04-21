using System.Collections;

namespace Lab1;

public abstract class V2Data : IEnumerable<DataItem>
{
    public string Key { get; set; }
    public DateTime Date { get; set; }

    public V2Data(string key, DateTime date)
    {
        Key = key;
        Date = date;
    }
    public abstract double MinField { get; }
    public abstract DataItem xMaxItem { get; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public abstract IEnumerator<DataItem> GetEnumerator();

    public abstract string ToLongString(string format);
    public override string ToString()
    {
        return "V2Data " + Key.ToString() + " " + Date.ToString();
    }
}