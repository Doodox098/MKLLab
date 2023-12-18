using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    public struct SplineDataItem
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double YSpline { get; set; }

        public SplineDataItem(double x, double y, double yspline)
        {
            X = x;
            Y = y;
            YSpline = yspline;
        }

        public string ToString(string format)
        {
            return "SplineDataItem " + string.Format(format, X) + ' ' + string.Format(format, Y) + ' ' + string.Format(format, YSpline) + '\n';
        }

        public override string ToString()
        {
            return "SplineDataItem";
        }
    }
 
}
