using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Lab1;

namespace ViewModel
{
    public class MainViewModel : INotifyDataErrorInfo
    {
        Dictionary<string, List<string>> Errors = new Dictionary<string, List<string>>();

        public bool HasErrors => Errors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (Errors.ContainsKey(propertyName))
            {
                return Errors[propertyName];

            }
            else
            {
               return Enumerable.Empty<string>();
            }

        }

        public void Validate(string propertyName)
        {
            string results = this[propertyName];

            if (results != "")
            {
                if (Errors.ContainsKey(propertyName))
                    Errors[propertyName][0] = results;
                else 
                    Errors.Add(propertyName, new List<string>() { results });
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
            else
            {
                Errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }

            DataFromControlsCommand.RaiseCanExecuteChanged(); ;
            DataFromFileCommand.RaiseCanExecuteChanged(); ;
            SaveCommand.RaiseCanExecuteChanged(); ;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] String propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static void Linear(double x, ref double y1, ref double y2)
        {
            y1 = 5 * x + 2;
            y2 = Math.Tan(x);
        }
        public static void XCubed(double x, ref double y1, ref double y2)
        {
            y1 = x * x * x;
            y2 = Math.Tan(x);
        }
        public static void XSin(double x, ref double y1, ref double y2)
        {
            y1 = Math.Sin(x);
            y2 = Math.Tan(x);
        }

        public List<FValues> Functions;
        private IUIServices UI;
        private double[] _Boundaries;

        public double[] Boundaries
        {
            get { return _Boundaries; }
            set
            {
                _Boundaries = value;
                Validate("Boundaries");
            }
        }
        private int _NumNodes;
        public int NumNodes
        {
            get { return _NumNodes; }
            set
            {
                _NumNodes = value;
                Validate("NumNodes");
            }
        }
        private int _isUniform = 0;
        public int isUniform
        {
            get { return _isUniform; }
            set
            {
                _isUniform = value;
                Validate("isUniform");
            }
        }
        private int _Function = 0;
        public int Function
        {
            get { return _Function; }
            set
            {
                _Function = value;
                Validate("Function");
            }
        }
        private int _SplineNumNodes;
        public int SplineNumNodes
        {
            get { return _SplineNumNodes; }
            set
            {
                _SplineNumNodes = value;
                Validate("SplineNumNodes");
            }
        }
        private int _UniformGridNumNodes;
        public int UniformGridNumNodes
        {
            get { return _UniformGridNumNodes; }
            set
            {
                _UniformGridNumNodes = value;
                Validate("UniformGridNumNodes");
            }
        }
        private double _StopDiscrepancy;
        public double StopDiscrepancy
        {
            get { return _StopDiscrepancy; }
            set
            {
                _StopDiscrepancy = value;
                Validate("StopDiscrepancy");
            }
        }
        private int _MaxIterations;
        public int MaxIterations
        {
            get { return _MaxIterations; }
            set
            {
                _MaxIterations = value;
                Validate("MaxIterations");
            }
        }

        public V2DataArray Data;
        public SplineData Spline;
        public ActionCommand DataFromControlsCommand { get; private set; }
        public ActionCommand DataFromFileCommand { get; private set; }
        public ActionCommand SaveCommand { get; private set; }
        public MainViewModel(IUIServices UI)
        {
            Functions = new List<FValues>();
            Functions.Add(XCubed);
            Functions.Add(XSin);
            Functions.Add(Linear);
            this.UI = UI;
            DataFromControlsCommand = new ActionCommand(DataFromControls, DataFromControlsCanExecute);
            DataFromFileCommand = new ActionCommand(DataFromFile, DataFromFileCanExecute);
            SaveCommand = new ActionCommand(Save, SaveCanExecute);
            Boundaries = null;
            NumNodes = 0;
            UniformGridNumNodes = 0;
            SplineNumNodes = 0;
            MaxIterations = 0;
            StopDiscrepancy = 0;
        }

        public string this[string propertyName]
        {
            get
            {
                string error = string.Empty;
                switch (propertyName)
                {
                    case "NumNodes":
                        if (NumNodes < 3)
                        {
                            error = "Число узллов сетки должно быть больше 2";
                        }
                        break;
                    case "UniformGridNumNodes":
                        if (UniformGridNumNodes < 3)
                        {
                            error = "Число узллов равномерной сетки должно быть больше 2";
                        }
                        break;
                    case "Boundaries":
                        if (Boundaries == null || Boundaries.Length < 2 || Boundaries[0] > Boundaries[1])
                        {
                            error = "Левая граница отрезка должны быть меньше правой";
                        }
                        break;
                    case "SplineNumNodes":
                        if ((SplineNumNodes < 2) || (SplineNumNodes > NumNodes))
                        {
                            error = "Число узллов сшлаживающего сплайна должно быть больше 2 и не больше числа узлов сетки";
                        }
                        break;
                }
                return error;
            }
        }
        public string Error
        {
            get { return Error; }
        }

        private void PlotData()
        {
            double[][] ApproximationResults = new double[Spline.ApproximationResults.Count][];
            double[][] UniformGridValues = new double[Spline.UniformGridValues.Count][];
            int i = 0;
            foreach (var elem in Spline.ApproximationResults) {
                ApproximationResults[i] = new double[3] {elem.X, elem.Y, elem.YSpline};
                ++i;
            }
            i = 0;
            foreach (var elem in Spline.UniformGridValues)
            {
                UniformGridValues[i] = elem;
                ++i;
            }
            UI.PlotSeries(UniformGridValues, ApproximationResults);
        }
        public void DataFromControls(object? o)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            try
            {
                FValues F;
                F = Functions[Function];
                if (isUniform == 0)
                {
                    Data = new V2DataArray($"{rnd.Next()}", DateTime.Now, NumNodes, Boundaries[0], Boundaries[Boundaries.Length - 1], F);
                }
                else
                {
                    Data = new V2DataArray($"{rnd.Next()}", DateTime.Now, Boundaries, F);
                }
                Spline = new SplineData(Data, SplineNumNodes, MaxIterations, UniformGridNumNodes, StopDiscrepancy);
                Spline.MakeSpline();
                PlotData();
            }
            catch (Exception ex)
            {
                UI.ErrorReport($"Some Error Happened\n{ex.Message}");
            }
        }
        private bool DataFromControlsCanExecute(object? o)
        {
            bool res = true;
            string error_list = "";
            string[] ItemsToValidate = new string[4] { "Boundaries", "NumNodes", "SplineNumNodes", "UniformGridNumNodes" };
            foreach (string child in ItemsToValidate)
            {
                if (Errors.ContainsKey(child))
                {
                    foreach (string error in Errors[child])
                        error_list += child + ": " + error + "\n";
                    res = false;
                }
            }
            return res;
        }
        public void DataFromFile(object? o)
        {
            string filename = UI.LoadFile();
            if (filename == null)
                return;
            Random rnd = new Random((int)DateTime.Now.Ticks);
            Data = new V2DataArray($"{rnd.Next()}", DateTime.Now);
            V2DataArray.Load(filename, ref Data);
            Spline = new SplineData(Data, SplineNumNodes, MaxIterations, UniformGridNumNodes, StopDiscrepancy);
            Spline.MakeSpline();
            PlotData();
        }
        private bool DataFromFileCanExecute(object? o)
        {
            bool res = true;
            string error_list = "";
            string[] ItemsToValidate = new string[2] { "SplineNumNodes", "UniformGridNumNodes" };
            foreach (string child in ItemsToValidate)
            {
                if (Errors.ContainsKey(child))
                {
                    foreach (string error in Errors[child])
                        error_list += child + ": " + error + "\n";
                    res = false;
                }
            }
            return res;
        }
        public void Save(object? o)
        {
            string filename = UI.SaveFile();
            if (filename == null)
                return;
            FValues F;
            F = Functions[Function];

            Random rnd = new Random((int)DateTime.Now.Ticks);
            if (isUniform == 0)
            {
                Data = new V2DataArray($"{rnd.Next()}", DateTime.Now, NumNodes, Boundaries[0], Boundaries[Boundaries.Length - 1], F);
            }
            else
            {
                Data = new V2DataArray($"{rnd.Next()}", DateTime.Now, Boundaries, F);
            }
            try
            {
                Data.Save(filename);
            }
            catch (Exception e)
            {
                UI.ErrorReport(e.Message);
            }
        }
        private bool SaveCanExecute(object? o)
        {
            bool res = true;
            string error_list = "";
            string[] ItemsToValidate = new string[2] { "Boundaries", "NumNodes" };
            foreach (string child in ItemsToValidate)
            {
                if (Errors.ContainsKey(child))
                {
                    foreach (string error in Errors[child])
                        error_list += child + ": " + error + "\n";
                    res = false;
                }
            }
            return res;
        }
    }
}
