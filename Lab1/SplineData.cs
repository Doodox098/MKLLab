using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Lab1
{
    public class SplineData
    {
        public V2DataArray Array { get; set; }
        public int M { get; set; }
        public double[] Spline { get; set; }
        public int MaxIterations { get; set; }
        public int NumIterations { get; set; } // Возможно лишнее
        public int ErrorCode { get; set; }
        public double MinDiscrepancy { get; set; }
        public List<SplineDataItem> ApproxResults { get; set; }

        public SplineData(ref V2DataArray a, int m, int max)
        {
            Array = a;
            M = m;
            MaxIterations = max;
            NumIterations = 0;
            Spline = new double[a.Count()];
            ApproxResults = new List<SplineDataItem>();
        }

        public void CalculateSpline()
        {
            ErrorCode = CubicSpline(Array.Count(), Array.X, 1, Array[0], Array.Count(), , , Spline);
            System.Console.WriteLine("ErrorCode " + ErrorCode.ToString());
            foreach (var elem in Spline)
            {
                System.Console.WriteLine(string.Format("{0:f2}", elem));
            }
        }

        [DllImport("\\..\\..\\..\\..\\x64\\Debug\\MYDLL.dll",
        CallingConvention = CallingConvention.Cdecl)]
        public static extern int CubicSpline(int nX, double[] X, int nY, double[] Y,
                                            int nS, double sL, double sR, double[] splineValues);

        public string ToLongString(string format)
        {
            string res = Array.ToLongString(format) + '\n';
            
            for (int i = 0; i < Array.Count(); ++i)
            {
                res += string.Format(format, Array.X[i]) + ' ';
                res += string.Format(format, Array.Y[0, i]) + ' ';
                res += string.Format(format, Spline[i]) + '\n';
            }

            res += "Minimum discrepancy: " + string.Format(format, MinDiscrepancy) + '\n';
            res += "ErrorCode: " + string.Format(format, ErrorCode) + '\n';
            res += "Number of iterations: " + string.Format(format, NumIterations) + '\n';

            return res;
        }

        public void Save(string filename, string format)
        {
            File.WriteAllText(filename, ToLongString(format));
        }
    }
}
 