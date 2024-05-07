using FluentAssertions.Execution;
using System.Collections;

namespace ViewModelTests
{
    public class ViewModelTests
    {
        public class MyUI : IUIServices
        {
            public double[][] x;
            public double[][] y;
            public string s;
            public string Message;
            public int load_calls = 0;
            public int save_calls = 0;
            public void ErrorReport(string message)
            {
                this.s = s; Message = "1";
            }

            public string LoadFile()
            {
                load_calls++;
                Message = "2";
                return "file.txt";
            }

            public void PlotSeries(double[][] x, double[][] y)
            {
                this.x = x; this.y = y; Message = "0";
            }

            public string SaveFile()
            {
                save_calls++;
                Message = "3";
                return "file.txt";
            }
        }
        MyUI UI = new MyUI();
        private int DataFromControls = 0;
        private int DataFromFile = 0;
        private int Save = 0;
        private void DataFromControlsCanExecuteChanged(object? sender, EventArgs e)
        {
            DataFromControls += 1;
        }
        private void DataFromFileCanExecuteChanged(object? sender, EventArgs e)
        {
            DataFromFile += 1;
        }
        private void SaveCanExecuteChanged(object? sender, EventArgs e)
        {
            Save += 1;
        }

        private List<string> Transform(IEnumerable arr)
        {
            List<string> result = new List<string>();
            foreach(string s in arr) { result.Add(s); }
            return result;
        }

        [Fact]
        public void ValidationTest()
        {
            MainViewModel MVM = new MainViewModel(UI);
            MVM.DataFromControlsCommand.CanExecuteChanged += DataFromControlsCanExecuteChanged;
            MVM.DataFromFileCommand.CanExecuteChanged += DataFromFileCanExecuteChanged;
            MVM.SaveCommand.CanExecuteChanged += SaveCanExecuteChanged;
            using (new AssertionScope())
            {
                MVM.Boundaries = new double[1] { 0 };
                Transform(MVM.GetErrors("Boundaries")).Should().HaveCount(1);
                MVM.Boundaries = new double[2] { 1, 0 };
                Transform(MVM.GetErrors("Boundaries")).Should().HaveCount(1);
                MVM.Boundaries = new double[2] { 0, 1 };
                Transform(MVM.GetErrors("Boundaries")).Should().HaveCount(0);

                MVM.NumNodes = -1;
                Transform(MVM.GetErrors("NumNodes")).Should().HaveCount(1);
                MVM.NumNodes = 2;
                Transform(MVM.GetErrors("NumNodes")).Should().HaveCount(1);
                MVM.NumNodes = 10;
                Transform(MVM.GetErrors("NumNodes")).Should().HaveCount(0);


                MVM.UniformGridNumNodes = -1;
                Transform(MVM.GetErrors("UniformGridNumNodes")).Should().HaveCount(1);
                MVM.UniformGridNumNodes = 2;
                Transform(MVM.GetErrors("UniformGridNumNodes")).Should().HaveCount(1);
                MVM.UniformGridNumNodes = 10;
                Transform(MVM.GetErrors("UniformGridNumNodes")).Should().HaveCount(0);

                MVM.SplineNumNodes = 1;
                Transform(MVM.GetErrors("SplineNumNodes")).Should().HaveCount(1);
                MVM.SplineNumNodes = 2;
                Transform(MVM.GetErrors("SplineNumNodes")).Should().HaveCount(0);
                MVM.SplineNumNodes = 12;
                Transform(MVM.GetErrors("SplineNumNodes")).Should().HaveCount(1);
                MVM.SplineNumNodes = 5;
                Transform(MVM.GetErrors("SplineNumNodes")).Should().HaveCount(0);
            }
            DataFromControls.Should().Be(13);
            DataFromFile.Should().Be(13);
            Save.Should().Be(13);
        }
        [Fact]
        public void UITest()
        {
            MainViewModel MVM = new MainViewModel(UI);
            MVM.DataFromControls(null);
            UI.Message.Should().Be("1");
        }
        [Fact]
        public void MakeSplineTest()
        {
            MainViewModel MVM = new MainViewModel(UI);
            MVM.Boundaries = new double[2] { 0, 1 };
            MVM.NumNodes = 3;
            MVM.UniformGridNumNodes = 5;
            MVM.Function = 2;
            MVM.isUniform = 0;
            MVM.SplineNumNodes = 2;
            MVM.MaxIterations = 100;
            MVM.StopDiscrepancy = 0.0001;

            double[][] x = new double[5][] { new double[2] { 0, 2 }, new double[2] { 0.25, 3.25 }, new double[2] { 0.5, 4.5 }, new double[2] { 0.75, 5.75 }, new double[2] { 1, 7 } };
            double[][] y = new double[3][] { new double[3] { 0, 2, 2 }, new double[3] { 0.5, 4.5, 4.5 }, new double[3] { 1, 7, 7 } };

            MVM.DataFromControls(null);
            UI.Message.Should().Be("0");
            UI.x.Should().Equal(x, (x, y) => { double eps = 1e-5; return Math.Abs(x[0] - y[0]) < eps && Math.Abs(x[1] - y[1]) < eps; });
            UI.y.Should().Equal(y, (x, y) => { double eps = 1e-5; return Math.Abs(x[0] - y[0]) < eps && Math.Abs(x[1] - y[1]) < eps && Math.Abs(x[2] - y[2]) < eps; });
        }
        [Fact]
        public void SaveLoadTest()
        {
            MainViewModel MVM = new MainViewModel(UI);
            MVM.Boundaries = new double[2] { 0, 1 };
            MVM.NumNodes = 3;
            MVM.Function = 2;
            MVM.isUniform = 0;

            MVM.Save(null);
            UI.save_calls.Should().Be(1);
            UI.Message.Should().Be("3");

            MVM.Boundaries = new double[2] { 1, 0 };

            MVM.UniformGridNumNodes = 5;
            MVM.SplineNumNodes = 2;
            MVM.MaxIterations = 100;
            MVM.StopDiscrepancy = 0.0001;

            double[][] x = new double[5][] { new double[2] { 0, 2 }, new double[2] { 0.25, 3.25 }, new double[2] { 0.5, 4.5 }, new double[2] { 0.75, 5.75 }, new double[2] { 1, 7 } };
            double[][] y = new double[3][] { new double[3] { 0, 2, 2 }, new double[3] { 0.5, 4.5, 4.5 }, new double[3] { 1, 7, 7 } };

            MVM.DataFromFile(null);
            UI.load_calls.Should().Be(1);
            UI.Message.Should().Be("0");
            UI.x.Should().Equal(x, (x, y) => { double eps = 1e-5; return Math.Abs(x[0] - y[0]) < eps && Math.Abs(x[1] - y[1]) < eps; });
            UI.y.Should().Equal(y, (x, y) => { double eps = 1e-5; return Math.Abs(x[0] - y[0]) < eps && Math.Abs(x[1] - y[1]) < eps && Math.Abs(x[2] - y[2]) < eps; });
        }
    }
}