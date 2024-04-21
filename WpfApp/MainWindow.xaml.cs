using System;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Lab1;
using Microsoft.VisualBasic;
using OxyPlot.Series;
using OxyPlot;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class ViewData : IDataErrorInfo
    {
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
        public Dictionary<string, FValues> Functions;

        public double[] Boundaries { get; set; }
        public int NumNodes { get; set; }
        public bool isUniform { get; set; }
        public string Function { get; set; }
        public int SplineNumNodes { get; set; }
        public int UniformGridNumNodes { get; set; }
        public double StopDiscrepancy { get; set; }
        public int MaxIterations { get; set; }

        public V2DataArray Data;
        public SplineData Spline;
        public ViewData() 
        {
            Functions = new Dictionary<string, FValues>();
            Functions.Add("X^3", XCubed);
            Functions.Add("Sin(X)", XSin);
            Functions.Add("Linear", Linear);
        }
        public delegate void ShowHandler(ViewData vd);
        public event ShowHandler Update;

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
                        if (Boundaries == null || Boundaries[0] > Boundaries[1])
                        {
                            error = "Левая граница отрезка должны быть меньше правой";
                        }
                        break;
                    case "SplineNumNodes":
                        if ((SplineNumNodes < 2) || (SplineNumNodes > SplineNumNodes))
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

        public void DataFromControls(object sender, ExecutedRoutedEventArgs e)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            if (Function == null)
            {
                MessageBox.Show($"Choose Function");
                return;
            }
            try
            {
                FValues F;
                if (Functions.ContainsKey(Function))
                {
                    F = Functions[Function];
                }
                else
                {
                    MessageBox.Show("Choose Function");
                    return;
                }
                if (isUniform)
                {
                    if (NumNodes <= 0)
                    {
                        MessageBox.Show("Enter positive number of nodes");
                        return;
                    }
                    if (Boundaries == null)
                    {
                        MessageBox.Show("Enter boundaries");
                        return;
                    }
                    Data = new V2DataArray($"{rnd.Next()}", DateTime.Now, NumNodes, Boundaries[0], Boundaries[Boundaries.Length - 1], F);
                }
                else
                {
                    if (Boundaries == null)
                    {
                        MessageBox.Show("Enter boundaries");
                        return;
                    }
                    Data = new V2DataArray($"{rnd.Next()}", DateTime.Now, Boundaries, F);
                }
                if (SplineNumNodes <= 0)
                {
                    MessageBox.Show("Enter positive number of spline nodes");
                    return;
                }
                if (MaxIterations <= 0)
                {
                    MessageBox.Show("Enter positive number of iterations");
                    return;
                }
                if (UniformGridNumNodes <= 0)
                {
                    MessageBox.Show("Enter positive number of uniform grid nodes");
                    return;
                }
                if (StopDiscrepancy <= 0)
                {
                    MessageBox.Show("Enter positive number for stop discrepancy");
                    return;
                }
                Spline = new SplineData(Data, SplineNumNodes, MaxIterations, UniformGridNumNodes, StopDiscrepancy);
                Spline.MakeSpline();
                Update(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Some Error Happened\n{ex.Message}");
            }
        }
        public void DataFromFile(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                if (filename == null) return;
                try
                {
                    Load(filename);
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                    return;
                }
            }
            else
            {
                return;
            }
            Random rnd = new Random((int)DateTime.Now.Ticks);

            if (SplineNumNodes <= 0)
            {
                MessageBox.Show("Enter positive number of spline nodes");
                return;
            }
            if (MaxIterations <= 0)
            {
                MessageBox.Show("Enter positive number of iterations");
                return;
            }
            if (UniformGridNumNodes <= 0)
            {
                MessageBox.Show("Enter positive number of uniform grid nodes");
                return;
            }
            if (StopDiscrepancy <= 0)
            {
                MessageBox.Show("Enter positive number for stop discrepancy");
                return;
            }
            Spline = new SplineData(Data, SplineNumNodes, MaxIterations, UniformGridNumNodes, StopDiscrepancy);
            Spline.MakeSpline();
            Update(this);
        }
        public void Load(string filename)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            Data = new V2DataArray($"{rnd.Next()}", DateTime.Now);
            V2DataArray.Load(filename, ref Data);
            
        }
        public void SaveHandler(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                Save(filename);
            }
        }
        public void Save(string filename)
        {
            FValues F;
            if (Functions.ContainsKey(Function))
            {
                F = Functions[Function];
            }
            else
            {
                MessageBox.Show("ChooseFunction");
                return;
            }

            Random rnd = new Random((int)DateTime.Now.Ticks);
            if (isUniform)
            {
                if (NumNodes <= 0)
                {
                    MessageBox.Show("Enter positive number of nodes");
                    return;
                }
                if (Boundaries == null)
                {
                    MessageBox.Show("Enter boundaries");
                    return;
                }
                Data = new V2DataArray($"{rnd.Next()}", DateTime.Now, NumNodes, Boundaries[0], Boundaries[Boundaries.Length - 1], F);
            }
            else
            {
                if (Boundaries == null)
                {
                    MessageBox.Show("Enter boundaries");
                    return;
                }
                Data = new V2DataArray($"{rnd.Next()}", DateTime.Now, Boundaries, F);
            }
            try
            {
                Data.Save(filename);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
    public class StringToIntDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "") return null;
            try
            {
                if (targetType == typeof(int))
                {
                    return int.Parse((string)value);
                }
                if (targetType == typeof(double))
                {
                    return double.Parse(((string)value).Replace('.', ','));
                }
            }
            catch
            {
                MessageBox.Show($"Wrong Input:\nNot a number");
            }
            throw new NotImplementedException("No Such Type");
        }
    }
    public class ComboBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if ((string)((ComboBoxItem)value).Content == "Равномерная")
            {
                return true;
            }
            return false;
            throw new NotImplementedException("No Such Type");
        }
    }
    public class FuncConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return (string)((ComboBoxItem)value).Content;
        }
    }
    public class BoundariesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "") return null;
            var LR = ((string)value).Split([',', ';', ' ', ':']).Where(val => val != "").ToArray();

            double[] Boundaries = new double[LR.Length];
            if (LR.Length < 2)
            {
                MessageBox.Show($"Wrong Input:\nIncorrect number of arguments");
                return null;
            }
            try
            {
                for (int i = 0; i < LR.Length; i++) 
                {
                    Boundaries[i] = double.Parse(LR[i].Replace('.', ','));
                }
            }
            catch
            {
                MessageBox.Show($"Wrong Input:\nNot a number");
                return null;
            }
            // System.Array.Sort(Boundaries);
            return Boundaries;
        }
    }
    public static class MyCommands
    {
        public static RoutedCommand DataFromControls = new RoutedCommand("DataFromControls", typeof(WpfApp.MyCommands));
    }
    public partial class MainWindow : Window
    {
        public void UpdateHandler(ViewData viewData)
        {
            Spline.ItemsSource = viewData.Spline.ApproximationResults;
            Values.ItemsSource = viewData.Spline.UniformGridValues;

            PlotModel plot = new PlotModel();
            ScatterSeries ScatterSeries = new ScatterSeries();
            LineSeries lineSeries = new LineSeries();
            foreach (var elem in viewData.Spline.UniformGridValues)
            {
                lineSeries.Points.Add(new DataPoint(elem[0], elem[1]));
            }
            foreach (var elem in viewData.Spline.ApproximationResults)
            {
                ScatterSeries.Points.Add(new ScatterPoint(elem.X, elem.Y));
                ScatterSeries.LabelFormatString = "{1:0.00}";
            }
            ScatterSeries.MarkerType = MarkerType.Circle;
            plot.Series.Add(lineSeries);
            plot.Series.Add(ScatterSeries);
            Plot.Model = plot;
        }

        ViewData viewData;
        public MainWindow()
        {
            InitializeComponent();

            viewData = new ViewData();

            Binding boundaries = new Binding();
            boundaries.Source = viewData;
            boundaries.ValidatesOnDataErrors = true;
            boundaries.Mode = BindingMode.OneWayToSource;
            boundaries.Path = new PropertyPath("Boundaries");
            boundaries.Converter = new BoundariesConverter();
            Boundaries.SetBinding(TextBox.TextProperty, boundaries);

            Binding n_nodes = new Binding();
            n_nodes.Source = viewData;
            n_nodes.ValidatesOnDataErrors = true;
            n_nodes.Mode = BindingMode.OneWayToSource;
            n_nodes.Path = new PropertyPath("NumNodes");
            n_nodes.Converter = new StringToIntDoubleConverter();
            NumNodes.SetBinding(TextBox.TextProperty, n_nodes);

            Binding grid_type = new Binding();
            grid_type.Source = viewData;
            grid_type.ValidatesOnDataErrors = true;
            grid_type.Mode = BindingMode.OneWayToSource;
            grid_type.Path = new PropertyPath("isUniform");
            grid_type.Converter = new ComboBoxConverter();
            GridType.SetBinding(ComboBox.SelectedValueProperty, grid_type);

            Binding field_function = new Binding();
            field_function.Source = viewData;
            field_function.ValidatesOnDataErrors = true;
            field_function.Mode = BindingMode.OneWayToSource;
            field_function.Path = new PropertyPath("Function");
            field_function.Converter = new FuncConverter();
            FieldFunction.SetBinding(ComboBox.SelectedValueProperty, field_function);

            Binding spline_num_nodes = new Binding();
            spline_num_nodes.Source = viewData;
            spline_num_nodes.ValidatesOnDataErrors = true;
            spline_num_nodes.Mode = BindingMode.OneWayToSource;
            spline_num_nodes.Path = new PropertyPath("SplineNumNodes");
            spline_num_nodes.Converter = new StringToIntDoubleConverter();
            SplineNumNodes.SetBinding(TextBox.TextProperty, spline_num_nodes);

            Binding uniform_grid_num_nodes = new Binding();
            uniform_grid_num_nodes.Source = viewData;
            uniform_grid_num_nodes.ValidatesOnDataErrors = true;
            uniform_grid_num_nodes.Mode = BindingMode.OneWayToSource;
            uniform_grid_num_nodes.Path = new PropertyPath("UniformGridNumNodes");
            uniform_grid_num_nodes.Converter = new StringToIntDoubleConverter();
            UniformGridNumNodes.SetBinding(TextBox.TextProperty, uniform_grid_num_nodes);

            Binding stop_discrepancy = new Binding();
            stop_discrepancy.Source = viewData;
            stop_discrepancy.ValidatesOnDataErrors = true;
            stop_discrepancy.Mode = BindingMode.OneWayToSource;
            stop_discrepancy.Path = new PropertyPath("StopDiscrepancy");
            stop_discrepancy.Converter = new StringToIntDoubleConverter();
            StopDiscrepancy.SetBinding(TextBox.TextProperty, stop_discrepancy);

            Binding max_iterations = new Binding();
            max_iterations.Source = viewData;
            max_iterations.ValidatesOnDataErrors = true;
            max_iterations.Mode = BindingMode.OneWayToSource;
            max_iterations.Path = new PropertyPath("MaxIterations");
            max_iterations.Converter = new StringToIntDoubleConverter();
            MaxIterations.SetBinding(TextBox.TextProperty, max_iterations);

            CommandBinding data_from_controls = new CommandBinding();
            data_from_controls.Command = MyCommands.DataFromControls;
            data_from_controls.Executed += viewData.DataFromControls;
            data_from_controls.CanExecute += ControlsCanExecuteHandler;

            DataFromControls.Command = MyCommands.DataFromControls;
            DataFromControls.CommandBindings.Add(data_from_controls);
            DataFromControlsButton.Command = MyCommands.DataFromControls;
            DataFromControlsButton.CommandBindings.Add(data_from_controls);

            CommandBinding save = new CommandBinding();
            save.Command = System.Windows.Input.ApplicationCommands.Save;
            save.Executed += SaveExecutedHandler;
            save.CanExecute += SaveCanExecuteHandler;

            Save.Command = System.Windows.Input.ApplicationCommands.Save;
            Save.CommandBindings.Add(save);
            SaveButton.Command = System.Windows.Input.ApplicationCommands.Save;
            SaveButton.CommandBindings.Add(save);

            DataFromFile.Click += viewData.DataFromFile;
            DataFromFileButton.Click += viewData.DataFromFile;

            viewData.Update += UpdateHandler;

        }
        private void ControlsCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (viewData != null)
            {
                string error_list = "";
                e.CanExecute = true;
                FrameworkElement[] ItemsToValidate = new FrameworkElement[4] { Boundaries, NumNodes, SplineNumNodes, UniformGridNumNodes };
                foreach (FrameworkElement child in ItemsToValidate)
                {
                    if (Validation.GetHasError(child) == true)
                    {
                        e.CanExecute = false;
                        foreach (var item in Validation.GetErrors(child))
                            error_list += child.Name + ": " + item.ErrorContent.ToString() + "\n";
                    }
                }

            }

            else e.CanExecute = false;
        }
        private void SaveCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (viewData != null)
            {
                string error_list = "";
                e.CanExecute = true;
                FrameworkElement[] ItemsToValidate = new FrameworkElement[2] { Boundaries, NumNodes };
                foreach (FrameworkElement child in ItemsToValidate)
                {
                    if (Validation.GetHasError(child) == true)
                    {
                        e.CanExecute = false;
                        foreach (var item in Validation.GetErrors(child))
                            error_list += child.Name + ": " + item.ErrorContent.ToString() + "\n";
                    }
                }

            }   
            else e.CanExecute = false;
        }
        private void SaveExecutedHandler(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                viewData.Save(filename);
            }
        }
    }
}