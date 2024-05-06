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
        public int NumIterations { get; set; } 
        public int Stop { get; set; }
        public double MinDiscrepancy { get; set; }
        public List<SplineDataItem> ApproximationResults { get; set; }

        // 4.3 
        public int NumUniformNodes { get; set; }
        public List<double[]> UniformGridValues { get; set; }
        public double StopDiscrepancy { get; set; }

        public SplineData(V2DataArray a, int m, int max, int num_uniform_nodes, double stop)
        {
            Array = a;
            M = m;
            MaxIterations = max;
            NumIterations = 0;
            Spline = new double[a.Count()];
            ApproximationResults = new List<SplineDataItem>();
            UniformGridValues = new List<double[]>();
            NumUniformNodes = num_uniform_nodes;
            StopDiscrepancy = stop;
        }

        public void MakeSpline(bool coeff = false)
        {
            int NumIterations = 0;
            int stop = 0;
            double MinDiscrepancy = 0;
            double[] SplineCoeff = new double[(M - 1) * 4];
            double[] Values = new double[NumUniformNodes];
            SplineOptimization(Array.X, Array[0], M, Array.X.Length, Spline, MaxIterations, StopDiscrepancy,
                NumUniformNodes, Values,
                ref NumIterations, ref stop, ref MinDiscrepancy, SplineCoeff);
            

            for (int i = 0; i < Array.X.Length; i++)
            {
                ApproximationResults.Add(new SplineDataItem(Array.X[i], Array.Y[0, i], Spline[i])); 
            }

            if (coeff)
            {
                for (int i = 0; i < M - 1; ++i)
                {
                    Console.WriteLine(SplineCoeff[4 * i] + " " + SplineCoeff[4 * i + 1] +
                         " " + SplineCoeff[4 * i + 2] + " " + SplineCoeff[4 * i + 3]);
                }
            }
            double Coord = Array.X[0];
            double Step = (Array.X[Array.X.Length - 1] - Array.X[0]) / (NumUniformNodes - 1);
            for (int i = 0; i < NumUniformNodes; ++i)
            {
                UniformGridValues.Add(new double[2] { Coord + i * Step, Values[i] });
            }
            this.NumIterations = NumIterations;
            this.Stop = stop;
            this.MinDiscrepancy = MinDiscrepancy;
        }

        [DllImport("\\..\\..\\..\\..\\x64\\Debug\\SplineOptimization.dll",
        CallingConvention = CallingConvention.Cdecl)]
        public static extern void SplineOptimization(double[] X, double[] Y, int M,
                                            int N, double[] spline, int maxiter, double stopdis,
                                            int n_small_grid, double[] values,
                                            ref int numiter, ref int errorcode, ref double mindis, double[] scoeff);

        public string ToLongString(string format)
        {
            string res = Array.ToLongString(format) + '\n';
            
            for (int i = 0; i < Array.Count(); ++i)
            {
                res += ApproximationResults[i].ToLongString(format) + '\n';
            }

            res += "Number of iterations: " + NumIterations + '\n';
            res += "StopCriteria: " + Stop + '\n';
            res += "Minimum discrepancy: " + string.Format(format, MinDiscrepancy) + '\n';

            return res;
        }

        public void Save(string filename, string format)
        {
            File.WriteAllText(filename, ToLongString(format));
        }
    }
}
 