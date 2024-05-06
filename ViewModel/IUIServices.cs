using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public interface IUIServices
    {
        void ErrorReport(string message);
        void PlotSeries(double[][] x, double[][] y);
        string SaveFile();
        string LoadFile();
    }
}
