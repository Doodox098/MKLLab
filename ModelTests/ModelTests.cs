using FluentAssertions.Equivalency;
using FluentAssertions.Common;
using FluentAssertions.Execution;
using FsCheck;
using FsCheck.Xunit;
using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ModelTests
{
    public class ModelTests
    {
        class DataItemComparer : IEqualityComparer<DataItem>
        {
            public bool Equals(DataItem x, DataItem y) { return x.X == y.X & x.Y == x.Y; }
            public static bool Equal(DataItem x, DataItem y) { return x.X == y.X & x.Y == x.Y; }

            public int GetHashCode([DisallowNull] DataItem obj)
            {
                throw new NotImplementedException();
            }
        }
        public static void F(double x, ref double y1, ref double y2)
        {
            y1 = x * 2;
            y2 = -x + 1;
        }
        public static DataItem F(double x)
        {
            return new DataItem(x, x * 2, -x + 1);
        }
        [Fact]
        public void DataItemTest()
        {
            DataItem di = new DataItem(0, 1, 2);
            di.ToLongString("{0:F2}").Should().Be("0,00 1,00 2,00");
        }
        [Fact]
        public void SplineDataItemTest()
        {
            SplineDataItem di = new SplineDataItem(0, 1, 2);
            di.ToLongString("{0:F2}").Should().Be("0,00 1,00 2,00");
        }
        [Fact]
        public void DataArrayTest()
        {
            string key = "Key";
            DateTime dateTime= DateTime.Now;
            V2DataArray first_constructor = new V2DataArray(key, dateTime);
            using (new AssertionScope())
            {
                first_constructor.Key.Should().Be(key);
                first_constructor.Date.Should().Be(dateTime);
            }
            double[] x = new double[] { 1, 2, 3, };
            double[] y1 = new double[] { 2, 4, 6 };
            double[] y2 = new double[] { 0, -1, -2 };
            V2DataArray second_constructor = new V2DataArray(key, dateTime, x, F);
            using (new AssertionScope())
            {
                second_constructor.Key.Should().Be(key);
                second_constructor.Date.Should().Be(dateTime);
                second_constructor.X.Should().Equal(x);
                second_constructor[0].Should().Equal(y1);
                second_constructor[1].Should().Equal(y2);
                second_constructor.Y.GetLength(0).Should().Be(2);
                second_constructor.Y.GetLength(1).Should().Be(3);
                second_constructor.X.Length.Should().Be(3);
            }
            int n = 2;
            double l = 0;
            double r = 1;
            V2DataArray third_constructor = new V2DataArray(key, dateTime, n, l, r, F);
            x = new double[] { 0, 1 };
            y1 = new double[] { 0, 2 };
            y2 = new double[] { 1, 0 };
            using (new AssertionScope())
            {
                third_constructor.Key.Should().Be(key);
                third_constructor.Date.Should().Be(dateTime);
                third_constructor.X.Should().Equal(x);
                third_constructor[0].Should().Equal(y1);
                third_constructor[1].Should().Equal(y2);
                third_constructor.Y.GetLength(0).Should().Be(2);
                third_constructor.Y.GetLength(1).Should().Be(n);
                third_constructor.X.Length.Should().Be(n);
            }
            string filename = $"{System.Random.Shared.Next()}file.da";
            third_constructor.Save(filename);
            V2DataArray load = new V2DataArray(filename, DateTime.Now);
            V2DataArray.Load(filename, ref load);
            using (new AssertionScope())
            {
                load.Key.Should().Be(third_constructor.Key);
                load.Date.Should().Be(third_constructor.Date);
                load.X.Should().Equal(third_constructor.X);
                load[0].Should().Equal(third_constructor[0]);
                load[1].Should().Equal(third_constructor[1]);

            }
            V2DataList list_convertion = (V2DataList)third_constructor;
            using (new AssertionScope())
            {
                list_convertion.Key.Should().Be(third_constructor.Key);
                list_convertion.Date.Should().Be(third_constructor.Date);
                list_convertion.Data.Should().HaveCount(n);
                list_convertion.Data.Should().Equal<DataItem>(new DataItem[2] { new DataItem(0, 0, 1), new DataItem(1, 2, 0) }, DataItemComparer.Equal);
            }
            third_constructor.MinField.Should().Be(0);

            third_constructor.xMaxItem.X.Should().Be(1);
            third_constructor.xMaxItem.Y.Should().Equal(new double[2] {2, 0});
        }
        [Fact]

        public void DataListTest()
        {
            string key = "Key";
            DateTime dateTime = DateTime.Now;
            V2DataList first_constructor = new V2DataList(key, dateTime);
            using (new AssertionScope())
            {
                first_constructor.Key.Should().Be(key);
                first_constructor.Date.Should().Be(dateTime);
            }
            double[] x = new double[] { 1, 2, 3, };
            double[] y1 = new double[] { 2, 4, 6 };
            double[] y2 = new double[] { 0, -1, -2 };
            V2DataList second_constructor = new V2DataList(key, dateTime, x, F);
            using (new AssertionScope())
            {
                second_constructor.Key.Should().Be(key);
                second_constructor.Date.Should().Be(dateTime);
                second_constructor.Data.Should().HaveCount(3);
                second_constructor.Data.Should().Equal<DataItem>(new DataItem[3] { new DataItem(1, 2, 0), new DataItem(2, 4, -1), new DataItem(3, 6, -2) }, DataItemComparer.Equal);
            }
            second_constructor.MinField.Should().Be(0);

            second_constructor.xMaxItem.X.Should().Be(3);
            second_constructor.xMaxItem.Y.Should().Equal(new double[2] { 6, -2 });
        }
        [Property]
        public FsCheck.Property MainCollectionConstructorTest() =>
            Prop.ForAll(Arb.From<Tuple<int, int>>().Filter(tuple => tuple.Item1 >= 0 && tuple.Item2 >= 0 && tuple.Item1 <= 25 && tuple.Item2 <= 25), (a) =>
            {
                V2MainCollection main_collection = new V2MainCollection(a.Item1, a.Item2);
                return main_collection.Count == a.Item1 + a.Item2;
            });
        [Fact]
        public void MainCollectionTest()
        {
            
            V2MainCollection main_collection = new V2MainCollection(0, 0);
            main_collection.Add(new V2DataArray("arr1", DateTime.Now, new double[1] { 1 }, F));
            using (new AssertionScope())
            {
                main_collection.Should().HaveCount(1);
                main_collection.xMaxItems.Should().Equal<DataItem>(new DataItem[1] { new DataItem(1, 2, 0) }, DataItemComparer.Equal);
                main_collection.MaxZeros.Should().Be(0);
                main_collection.MaxField.Should().Be(new DataItem(1, 2, 0), new DataItemComparer());
            }
            main_collection.Add(new V2DataArray("arr2", DateTime.Now, new double[2] { 2, 3 }, F));
            using (new AssertionScope())
            {
                main_collection.Should().HaveCount(2);
                main_collection.xMaxItems.Should().Equal<DataItem>(new DataItem[2] { new DataItem(1, 2, 0), new DataItem(3, 6, -2) }, DataItemComparer.Equal);
                main_collection.MaxZeros.Should().Be(0);
                main_collection.MaxField.Should().Be(new DataItem(3, 6, -2), new DataItemComparer());
            }
            main_collection.Add(new V2DataArray("arr3", DateTime.Now, new double[2] { 2, 3 }, F));
            main_collection.Grid.Should().Equal(new double[3] {1, 2, 3 }, "All values should present one time");
            main_collection.Add(new V2DataArray("arr4", DateTime.Now, new double[2] { 4, 4 }, F));
            main_collection.Grid.Should().Equal(new double[3] { 1, 2, 3 }, "All values should present one time except 4");
            for (int i = 1; i <= 4; ++i)
            {
                main_collection.Contains($"arr{i}").Should().BeTrue();
            }
            for (int i = 5; i <= 10; ++i)
            {
                main_collection.Contains($"arr{i}").Should().BeFalse();
            }
        }
        [Fact]
        public void SplineDataTest()
        {
            double[] x = new double[] { 1, 2, 3, };
            V2DataArray array = new V2DataArray("key", DateTime.Now, x, F);
            SplineData spline_data = new SplineData(array, 2, 100, 21, 0.001);
            spline_data.MakeSpline();

            spline_data.ApproximationResults.Should().Equal(new SplineDataItem[3] { new SplineDataItem(1, 2, 2), new SplineDataItem(2, 4, 4), new SplineDataItem(3, 6, 6), },
                (x, y) => { return x.ToString() == y.ToString(); });
            using (new AssertionScope())
            {
                double step = 0.1;
                for (int i = 0; i < 21; ++i)
                {
                    spline_data.UniformGridValues[i].Should().Equal(new double[2] { 1 + i * step, 2 * (1 + i * step) }, (x, y) => { double eps = 1e-5; return Math.Abs(x - y) < eps; });
                }
            }
        }
    }
}