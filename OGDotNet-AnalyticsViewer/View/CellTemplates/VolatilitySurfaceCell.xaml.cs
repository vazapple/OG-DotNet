//-----------------------------------------------------------------------
// <copyright file="VolatilitySurfaceCell.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using OGDotNet.AnalyticsViewer.Properties;
using OGDotNet.AnalyticsViewer.View.Charts;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.Surface;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.WPFUtils;

namespace OGDotNet.AnalyticsViewer.View.CellTemplates
{
    /// <summary>
    /// TODO work out how to present this
    /// </summary>
    public partial class VolatilitySurfaceCell : UserControl
    {
        private static readonly bool ToScale = Settings.Default.ShowVolatilityCurveToScale;
        static readonly Point3D Center = new Point3D(0.5, 0.5, 0.5);

        const double GraphOffset = 0.4;
        const int ProjectedCurveSize = 400;

        private readonly DispatcherTimer _timer;

        private bool _haveInitedData;
        double _zRange = 100.0;

        public VolatilitySurfaceCell()
        {
            InitializeComponent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80.0) };

            double speed = 0.02;

            double t = Math.PI / 4.0;
            const double maxT = Math.PI / 2.0;
            const double minT = 0;
            SetCamera(t);
            _timer.Start();

            _timer.Tick += delegate
                               {
                                   if (t > maxT)
                                   {
                                       speed = -Math.Abs(speed);
                                   }

                                   if (t < minT)
                                   {
                                       speed = Math.Abs(speed);
                                   }

                                   t += speed;

                                   SetCamera(t);
                                   UpdateToolTip(Mouse.GetPosition(mainViewport));
                               };
        }

        private void SetCamera(double t)
        {
            const double circleRadius = 3.8;

            camera.Position = Center + (new Vector3D(Math.Sin(t), Math.Cos(t), 0) * circleRadius);
            camera.LookDirection = Center - camera.Position;
        }

        public double ZRange
        {
            get { return _zRange; }
            set { _zRange = value; }
        }

        private void InitTableData()
        {
            if (_haveInitedData) return;
            _haveInitedData = true;

            var data = (VolatilitySurfaceData)DataContext;

            foreach (var x in data.Xs)
            {
                var textBlockStyle = new Style(typeof(FrameworkElement));
                var binding = BindingUtils.GetIndexerBinding(x.ToString());
                binding.Converter = new ValueToColorConverter(_zRange);
                textBlockStyle.Setters.Add(new Setter(BackgroundProperty, binding));

                detailsList.Columns.Add(new DataGridTextColumn
                                     {
                                         Width = 60,
                                         Header = x,
                                         CellStyle = textBlockStyle,
                                         Binding = BindingUtils.GetIndexerBinding(x.ToString())
                                     });
            }

            var rows = new List<Dictionary<string, object>>();

            foreach (var y in data.Ys)
            {
                var row = new Dictionary<string, object>();
                row["Length"] = y;

                foreach (var x in data.Xs)
                {
                    row[x.ToString()] = data[x, y];
                }

                rows.Add(row);
            }

            detailsList.ItemsSource = rows;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            _timer.IsEnabled = false;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            toolTip.IsOpen = false;
            _timer.IsEnabled = true;
        }

        private void detailsButton_Checked(object sender, RoutedEventArgs e)
        {
            InitTableData();
            detailsPopup.IsOpen = true;
            _timer.IsEnabled = false;
        }

        private void detailsButton_Unchecked(object sender, RoutedEventArgs e)
        {
            detailsPopup.IsOpen = false;
            _timer.IsEnabled = true;
        }

        private void detailsPopup_Closed(object sender, EventArgs e)
        {
            detailsButton.IsChecked = false;
        }

        private void BuildModel()
        {
            var surfaceModel = BuildSurfaceModel();

            var models = new Model3DCollection
                            {
                                new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)), 
                                BuildBaseModel(surfaceModel), 
                                BuildGraphModel(), 
                                surfaceModel, 
                            };

            var groupModel = new ModelVisual3D { Content = new Model3DGroup { Children = models } };

            mainViewport.Children.Clear();
            mainViewport.Children.Add(groupModel);
        }

        /// <summary>
        /// NOTE: these graphs are not on the same axis as the surface, since they are to scale
        /// </summary>
        private Model3DGroup BuildGraphModel()
        {
            return new Model3DGroup
                       {
                           Children = new Model3DCollection
                                        {
                                            BuildXSliceGraphModel(), 
                                            BuildYSliceGraphModel(), 
                                        }
                       };
        }

        private CurveControl _xSliceCurveControl;
        private GeometryModel3D BuildXSliceGraphModel()
        {
            _xSliceCurveControl = new CurveControl { Width = ProjectedCurveSize, Height = ProjectedCurveSize, YMin = 0, YMax = _zRange, StrokeThickness = 5.0, ShowName = false };
            var brush = new VisualBrush(_xSliceCurveControl);

            var material = new DiffuseMaterial(brush);

            var normal = new Vector3D(1, 0, 0);

            var mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                                               {
                                                   new Point3D(-GraphOffset, 0, 0), 
                                                   new Point3D(-GraphOffset, 1, 0), 
                                                   new Point3D(-GraphOffset, 0, 1), 
                                                   new Point3D(-GraphOffset, 1, 1)
                                               },
                Normals = new Vector3DCollection
                                             {
                                                 normal, 
                                                 normal, 
                                                 normal, 
                                                 normal
                                             },
                TriangleIndices = new Int32Collection
                                                     {
                                                         0, 1, 3, 
                                                         0, 3, 2
                                                     },
                TextureCoordinates = new PointCollection
                                                        {
                                                                 new Point(0, ProjectedCurveSize), 
                                                                 new Point(ProjectedCurveSize, ProjectedCurveSize), 
                                                                 new Point(0, 0), 
                                                                 new Point(ProjectedCurveSize, 0), 
                                                        }
            };

            return new GeometryModel3D(mesh, material) { BackMaterial = material };
        }

        private CurveControl _ySliceCurveControl;
        private GeometryModel3D BuildYSliceGraphModel()
        {
            _ySliceCurveControl = new CurveControl { Width = ProjectedCurveSize, Height = ProjectedCurveSize, YMin = 0, YMax = _zRange, StrokeThickness = 5.0, ShowName = false };
            var brush = new VisualBrush(_ySliceCurveControl);

            var material = new DiffuseMaterial(brush);

            var normal = new Vector3D(0, -1, 0);

            var mesh = new MeshGeometry3D
                           {
                               Positions = new Point3DCollection
                                               {
                                                   new Point3D(0, -GraphOffset, 0), 
                                                   new Point3D(1, -GraphOffset, 0), 
                                                   new Point3D(0, -GraphOffset, 1), 
                                                   new Point3D(1, -GraphOffset, 1)
                                               },
                               Normals = new Vector3DCollection
                                             {
                                                 normal, 
                                                 normal, 
                                                 normal, 
                                                 normal
                                             },
                               TriangleIndices = new Int32Collection
                                                     {
                                                         0, 1, 3, 
                                                         0, 3, 2
                                                     },
                               TextureCoordinates = new PointCollection
                                                        {
                                                                 new Point(0, ProjectedCurveSize), 
                                                                 new Point(ProjectedCurveSize, ProjectedCurveSize), 
                                                                 new Point(0, 0), 
                                                                 new Point(ProjectedCurveSize, 0), 
                                                        }
                           };

            return new GeometryModel3D(mesh, material) { BackMaterial = material };
        }

        private static GeometryModel3D BuildBaseModel(GeometryModel3D buildSurfaceModel)
        {
            var geometryModel3D = new GeometryModel3D(buildSurfaceModel.Geometry, buildSurfaceModel.Material) { BackMaterial = buildSurfaceModel.BackMaterial };
            var transform = new Matrix3D(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 1);
            geometryModel3D.Transform = new MatrixTransform3D(transform);
            return geometryModel3D;
        }

        private class ValueToColorConverter : IValueConverter
        {
            private readonly double _zRange;

            public ValueToColorConverter(double zRange)
            {
                _zRange = zRange;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (!(value is double))
                    throw new NotImplementedException();

                if (targetType == typeof(Color))
                {
                    var colorQuotient = (double)value;
                    return GetColor(colorQuotient / _zRange);
                }

                if (targetType == typeof(Brush))
                {
                    var color = (Color)Convert(value, typeof(Color), parameter, culture);
                    return new SolidColorBrush(color);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            public static Color GetColor(double colorQuotientDouble)
            {
                var colorQuotient = (float)colorQuotientDouble;
                return Colors.Yellow * (1 - colorQuotient) + Colors.Red * colorQuotient;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private GeometryModel3D BuildSurfaceModel()
        {
            double zScale = 1.0 / _zRange;

            var mesh = new MeshGeometry3D();

            var xKeys = Surface.Xs;
            var yKeys = Surface.Ys;

            // TODO this should really be a texture generated by interpolating the surface
            var linearGradientBrush = new LinearGradientBrush
                                          {
                                              StartPoint = new Point(0, 0),
                                              EndPoint = new Point(1, 0)
                                          };

            const int bands = 2;
            for (int i = 0; i < bands; i++)
            {
                var offset = i / (float)bands;
                Color color = ValueToColorConverter.GetColor(offset * _zRange);
                linearGradientBrush.GradientStops.Add(new GradientStop(color - Color.FromArgb(10, 0, 0, 0), offset));
            }

            var diffuseMaterial = new DiffuseMaterial(linearGradientBrush);

            // Points
            if (ToScale)
            {
                var xMax = Surface.Xs.Select(GetScaledValue).Max();
                var xMin = Surface.Xs.Select(GetScaledValue).Min();
                double xScale = 1.0 / (xMax - xMin);
                var yMax = Surface.Ys.Select(GetScaledValue).Max();
                var yMin = Surface.Ys.Select(GetScaledValue).Min();
                double yScale = 1.0 / (yMax - yMin);

                var scaleMatrix = new Matrix3D(
                    xScale, 0, 0, 0,
                    0, yScale, 0, 0,
                    0, 0, zScale, 0,

                    -xMin * xScale, -yMin * yScale, 0, 1);

                foreach (Tenor yKey in yKeys)
                {
                    foreach (Tenor xKey in xKeys)
                    {
                        var xValue = GetScaledValue(xKey);
                        var yValue = GetScaledValue(yKey);
                        var zValue = Surface[xKey, yKey];
                        var point3D = new Point3D(xValue, yValue, zValue) * scaleMatrix;

                        mesh.Positions.Add(point3D);
                    }
                }
            }
            else
            {
                double xScale = 1.0 / (xKeys.Count - 1);
                double yScale = 1.0 / (yKeys.Count - 1);

                var scaleMatrix = new Matrix3D(
                    xScale, 0, 0, 0,
                    0, yScale, 0, 0,
                    0, 0, zScale, 0,

                    0, 0, 0, 1);

                for (int yi = 0; yi < yKeys.Count; yi++)
                {
                    for (int xi = 0; xi < xKeys.Count; xi++)
                    {
                        var zValue = Surface[xKeys[xi], yKeys[yi]];

                        mesh.Positions.Add(new Point3D(xi, yi, zValue) * scaleMatrix);
                    }
                }
            }

            // Triangles and normals
            for (int yi = 0; yi < yKeys.Count; yi++)
            {
                for (int xi = 0; xi < xKeys.Count; xi++)
                {
                    var normals = new List<Vector3D>(4);

                    if (yi < yKeys.Count - 1 && xi < xKeys.Count - 1)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi + 1 + (yi * xKeys.Count);
                        var p2 = xi + ((yi + 1) * xKeys.Count);

                        mesh.TriangleIndices.Add(p0);
                        mesh.TriangleIndices.Add(p1);
                        mesh.TriangleIndices.Add(p2);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }

                    if (yi > 0 && xi > 0)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi - 1 + (yi * xKeys.Count);
                        var p2 = xi + ((yi - 1) * xKeys.Count);

                        mesh.TriangleIndices.Add(p0);
                        mesh.TriangleIndices.Add(p1);
                        mesh.TriangleIndices.Add(p2);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }

                    // We don't need triangles here, but we need normals
                    if (yi > 0 && xi < xKeys.Count - 1)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi + ((yi - 1) * xKeys.Count);
                        var p2 = xi + 1 + (yi * xKeys.Count);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }

                    if (yi < yKeys.Count - 1 && xi > 0)
                    {
                        var p0 = xi + (yi * xKeys.Count);
                        var p1 = xi + ((yi + 1) * xKeys.Count);
                        var p2 = xi - 1 + (yi * xKeys.Count);

                        normals.Add(CalculateNormal(mesh.Positions[p0], mesh.Positions[p1], mesh.Positions[p2]));
                    }

                    mesh.Normals.Add(normals.Aggregate(new Vector3D(0, 0, 0), (a, b) => a + b));
                }
            }

            // Texture co-ordinates
            double colorScale = 1 / _zRange;
            for (int yi = 0; yi < yKeys.Count; yi++)
            {
                for (int xi = 0; xi < xKeys.Count; xi++)
                {
                    var zValue = Surface[xKeys[xi], yKeys[yi]];

                    // Try and make sure all the triangles are real in texture space
                    var fakeYValue = (xi / (float)xKeys.Count + yi / (float)yKeys.Count) * 0.5;

                    var point = new Point(zValue * colorScale, fakeYValue);
                    mesh.TextureCoordinates.Add(point);
                }
            }

            return new GeometryModel3D(mesh, diffuseMaterial) { BackMaterial = diffuseMaterial };
        }

        private static Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            var v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            var v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private static double GetScaledValue(Tenor t)
        {
            return Math.Log(t.TimeSpan.TotalDays);
        }

        private VolatilitySurfaceData Surface
        {
            get { return (VolatilitySurfaceData)DataContext; }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is VolatilitySurfaceData)
            {
                BuildModel();
            }
            else
            {
                mainViewport.Children.Clear();
            }
        }

        private void mainViewport_MouseMove(object sender, MouseEventArgs e)
        {
            _timer.IsEnabled = false;
            Point position = e.GetPosition(mainViewport);
            UpdateToolTip(position);
        }

        private void UpdateToolTip(Point position)
        {
            if (Surface == null || ToScale)
            {
                // TODO this isn't right if we're 'ToScale'ing
                toolTip.IsOpen = false;
                return;
            }

            var xs = Surface.Xs.ToList();
            var ys = Surface.Ys.ToList();

            double xScale = 1.0 / (xs.Count - 1);
            double yScale = 1.0 / (ys.Count - 1);

            HitTestResult hitTestResult = VisualTreeHelper.HitTest(mainViewport, position);
            if (hitTestResult != null && hitTestResult.VisualHit != null)
            {
                if (!(hitTestResult is RayMeshGeometry3DHitTestResult))
                    throw new ArgumentException();
                var result = (RayMeshGeometry3DHitTestResult)hitTestResult;

                var point = result.PointHit;

                var yFloor = (int)Math.Round(point.Y / yScale);
                var xFloor = (int)Math.Round(point.X / xScale);

                if (xFloor < 0 || yFloor < 0)
                {// The graph slices
                    toolTip.IsOpen = false;
                    return;
                }

                var x = xs[xFloor];
                var y = ys[yFloor];

                UpdateToolTip(x, y);
                UpdateCellSelection(x, y);
                SetXSliceGraph(x);
                SetYSliceGraph(y);
            }
            else
            {
                toolTip.IsOpen = false;
            }
        }

        private void UpdateCellSelection(Tenor x, Tenor y)
        {
            InitTableData();

            Func<Dictionary<string, object>, bool> rowPredicate = row => row["Length"].Equals(y);

            Func<DataGridColumn, bool> columnPredicate = c => c.Header.Equals(x);

            DataGridColumn myColumn = detailsList.Columns.Where(columnPredicate).First();
            Dictionary<string, object> myRow = detailsList.Items.Cast<Dictionary<string, object>>().Where(rowPredicate).First();

            detailsList.SelectedCells.Clear();
            detailsList.SelectedCells.Add(new DataGridCellInfo(myRow, myColumn));
        }

        private void UpdateToolTip(Tenor x, Tenor y)
        {
            toolTipBox.Text = string.Format("{0},{1},{2}", x, y, Surface[x, y]);
            toolTip.IsOpen = true;
        }

        private void SetYSliceGraph(Tenor y)
        {
            var slice = Surface.GetYSlice(y);
            _ySliceCurveControl.DataContext = slice;
            leftCurveControl.DataContext = slice;
        }

        private void SetXSliceGraph(Tenor x)
        {
            var slice = Surface.GetXSlice(x);
            _xSliceCurveControl.DataContext = slice;
            rightCurveControl.DataContext = slice;
        }

        private void detailsList_CurrentCellChanged(object sender, EventArgs e)
        {
            var dataGridCellInfo = detailsList.CurrentCell;

            var x = dataGridCellInfo.Column == null ? null : dataGridCellInfo.Column.Header as Tenor;
            var y = dataGridCellInfo.Item as Dictionary<string, object>;

            if (x != null)
            {
                SetXSliceGraph(x);
            }

            if (y != null)
            {
                SetYSliceGraph((Tenor)y["Length"]);
            }
        }
    }
}
