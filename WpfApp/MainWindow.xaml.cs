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
using ViewModel;
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
    public class BoundariesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string res = "";
            if (value == null)
                return res;
            foreach (var elem in (double[])value)
            {
                res += elem.ToString() + " ";
            }
            return res; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "") return null;
            var LR = ((string)value).Split([',', ';', ' ', ':']).Where(val => val != "").ToArray();

            double[] Boundaries = new double[LR.Length];
            try
            {
                for (int i = 0; i < LR.Length; i++) 
                {
                    Boundaries[i] = double.Parse(LR[i].Replace('.', ','));
                }
            }
            catch
            {
                return null;
            }
            return Boundaries;
        }
    }
    public partial class MainWindow : Window, IUIServices
    {
        public void PlotSeries(double[][] UniformGridValues, double[][] ApproximationResults)
        {
            Values.ItemsSource = UniformGridValues;
            Spline.ItemsSource = ApproximationResults;

            PlotModel plot = new PlotModel();
            ScatterSeries ScatterSeries = new ScatterSeries();
            LineSeries lineSeries = new LineSeries();
            foreach (var elem in UniformGridValues)
            {
                lineSeries.Points.Add(new DataPoint(elem[0], elem[1]));
            }
            foreach (var elem in ApproximationResults)
            {
                ScatterSeries.Points.Add(new ScatterPoint(elem[0], elem[1]));
                ScatterSeries.LabelFormatString = "{1:0.00}";
            }
            ScatterSeries.MarkerType = MarkerType.Circle;
            plot.Series.Add(lineSeries);
            plot.Series.Add(ScatterSeries);
            Plot.Model = plot;
        }
        public void ErrorReport(string message)
        {
            MessageBox.Show(message);
        }
        public string SaveFile()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                return filename;
            }
            return null;
        }
        public string LoadFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                return filename;
            }
            return null;
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this);

            Binding boundaries = new Binding();
            boundaries.Source = DataContext;
            boundaries.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            boundaries.Mode = BindingMode.TwoWay;
            boundaries.Path = new PropertyPath("Boundaries");
            boundaries.Converter = new BoundariesConverter();
            Boundaries.SetBinding(TextBox.TextProperty, boundaries);
        }
    }
}