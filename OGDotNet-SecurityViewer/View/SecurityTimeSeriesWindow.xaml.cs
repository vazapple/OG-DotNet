﻿//-----------------------------------------------------------------------
// <copyright file="SecurityTimeSeriesWindow.xaml.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using OGDotNet.Mappedtypes.Core.Security;
using OGDotNet.Model.Resources;
using OGDotNet.WPFUtils.Windsor;

namespace OGDotNet.SecurityViewer.View
{
    /// <summary>
    /// Interaction logic for SecurityTimeSeriesWindow.xaml
    /// </summary>
    public partial class SecurityTimeSeriesWindow : OGDotNetWindow
    {
        public static void ShowDialog(IEnumerable<ISecurity> securities, Window owner)
        {
            var securityTimeSeriesWindow = new SecurityTimeSeriesWindow
            {
                DataContext = securities,
                Owner = owner,
            };
            securityTimeSeriesWindow.ShowDialog();
        }

        public SecurityTimeSeriesWindow()
        {
            InitializeComponent();
        }

        private RemoteHistoricalTimeSeriesSource TimeSeriesSource
        {
            get { return OGContext.HistoricalTimeSeriesSource; }
        }

        private IEnumerable<ISecurity> Securities
        {
            get { return (IEnumerable<ISecurity>)DataContext; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = string.Join(",", Securities.Select(s => s.Name));
            chart.Title = Title;
            UpdateDetailsBlock();
            BeginInvokeOnIdle(delegate
                                  {
                                      UpdateChart();
                                      BeginInvokeOnIdle(() => Cursor = null);
                                  });
        }

        private void UpdateDetailsBlock()
        {
            var sb = new StringBuilder();
            foreach (var security in Securities)
            {
                sb.AppendLine(security.Name);
                foreach (var propertyInfo in security.GetType().GetProperties())
                {
                    sb.AppendFormat(string.Format("{0}: {1}", propertyInfo.Name, propertyInfo.GetGetMethod().Invoke(security, null)));
                    sb.Append(Environment.NewLine);
                }
                sb.AppendLine(new string('-', 12));
            }

            detailsBlock.Text = sb.ToString();
        }

        private void UpdateChart()
        {
            foreach (var tuple in Securities.Zip(chart.StylePalette, Tuple.Create))
            {
                var security = tuple.Item1;
                var style = tuple.Item2;

                var seriesTuple = TimeSeriesSource.GetHistoricalTimeSeries(security.Identifiers, DateTimeOffset.Now, "BLOOMBERG", null, "PX_LAST");
                if (seriesTuple.Item1 == null && seriesTuple.Item2 == null)
                    continue;

                var timeSeries = seriesTuple.Item2;
                //NOTE: the chart understands DateTime, but not DateTime Offset
                var tuples = timeSeries.Values.Select(t => Tuple.Create(t.Item1.LocalDateTime, t.Item2)).ToList();

                if (tuples.Count == 0)
                    continue;

                var lineSeries = new LineSeries
                                     {
                                         IndependentValueBinding = new Binding("Item1"),
                                         DependentValueBinding = new Binding("Item2"),
                                         ItemsSource = tuples,
                                         Title = security.Name,
                                         DataPointStyle = new Style(),

                                         PolylineStyle = new Style(),
                                         LegendItemStyle = new Style()
                                     };
                //Data points make us even slower, and there's too many of them to be useful
                lineSeries.DataPointStyle.Setters.Add(new Setter(VisibilityProperty, Visibility.Hidden));

                //The background is set from the style palette, which sets the line color?! http://wpf.codeplex.com/discussions/207938?ProjectName=wpf
                var backgroundSetter = style.Setters.Where(s => s is Setter).Cast<Setter>().Where(s => s.Property == BackgroundProperty).First();
                lineSeries.DataPointStyle.Setters.Add(backgroundSetter);

                chart.Series.Add(lineSeries);
            }
            if (chart.Series.Count == 0)
            {
                chart.Title = "No time series found";
            }
        }

        private void BeginInvokeOnIdle(Action act)
        {
            Dispatcher.BeginInvoke(act, DispatcherPriority.ContextIdle);
        }

        #region zooming

        private Point _startDragPosition;

        private void chart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startDragPosition = e.GetPosition(canvas);

            bool moving = (Keyboard.Modifiers & ModifierKeys.Control) == 0;

            if (moving)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                zoomRectangle.Visibility = Visibility.Visible;
            }

            chart_MouseMove(sender, e);
        }

        private void chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                ResetDrag();
                return;
            }

            var endDragPosition = e.GetPosition(canvas);

            bool moving = (Keyboard.Modifiers & ModifierKeys.Control) == 0;
            if (moving)
            {
                if (Cursor != Cursors.Hand)
                    return;

                var delta = _startDragPosition - endDragPosition;
                chart.RenderTransform = new TranslateTransform(-delta.X, -delta.Y);
            }
            else
            {
                if (zoomRectangle.Visibility != Visibility.Visible)
                    return;

                Canvas.SetLeft(zoomRectangle, Math.Min(endDragPosition.X, _startDragPosition.X));
                Canvas.SetTop(zoomRectangle, Math.Min(endDragPosition.Y, _startDragPosition.Y));
                zoomRectangle.Width = Math.Abs(endDragPosition.X - _startDragPosition.X);
                zoomRectangle.Height = Math.Abs(endDragPosition.Y - _startDragPosition.Y);
            }
        }

        private void chart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var endDragPosition = e.GetPosition(canvas);

            bool moving = (Keyboard.Modifiers & ModifierKeys.Control) == 0;

            if (moving)
            {
                if (Cursor != Cursors.Hand)
                    return;
                var dx = GetChartPosition(_startDragPosition).Item1 - GetChartPosition(endDragPosition).Item1;
                var dy = GetChartPosition(_startDragPosition).Item2 - GetChartPosition(endDragPosition).Item2;
                Cursor = null;
                XAxis.Maximum = XAxis.ActualMaximum.Value + dx;
                XAxis.Minimum = XAxis.ActualMinimum.Value + dx;

                YAxis.Maximum = YAxis.ActualMaximum.Value + dy;
                YAxis.Minimum = YAxis.ActualMinimum.Value + dy;
            }
            else
            {
                if (zoomRectangle.Visibility != Visibility.Visible)
                    return;

                SetXRange(_startDragPosition, endDragPosition);
                SetYRange(_startDragPosition, endDragPosition);
            }

            ResetDrag();
        }

        private void chart_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResetDrag();
        }

        private void chart_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetDrag();

            YAxis.Minimum = null;
            YAxis.Maximum = null;

            XAxis.Minimum = null;
            XAxis.Maximum = null;
        }

        private void ResetDrag()
        {
            zoomRectangle.Visibility = Visibility.Hidden;
            Cursor = null;
            chart.RenderTransform = null;
        }

        private void chart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Released || e.RightButton != MouseButtonState.Released)
                return;
            if ((Keyboard.Modifiers & ~ModifierKeys.Control) != 0)
                return;

            bool moving = (Keyboard.Modifiers & ModifierKeys.Control) == 0;

            var centrePoint = e.GetPosition(canvas);
            var centre = GetChartPosition(centrePoint);

            if (moving)
            {
                double mult = e.Delta / 601.0;

                var range = YAxis.ActualMaximum.Value - YAxis.ActualMinimum.Value;
                YAxis.Minimum = YAxis.ActualMinimum.Value + range * mult;
                YAxis.Maximum = YAxis.ActualMaximum.Value + range * mult;
            }
            else
            {
                double mult = e.Delta / 361.0;
                XAxis.Minimum = XAxis.ActualMinimum.Value + Mult(centre.Item1 - XAxis.ActualMinimum.Value, mult);
                XAxis.Maximum = XAxis.ActualMaximum.Value - Mult(XAxis.ActualMaximum.Value - centre.Item1, mult);

                YAxis.Minimum = YAxis.ActualMinimum.Value + (centre.Item2 - YAxis.ActualMinimum.Value) * mult;
                YAxis.Maximum = YAxis.ActualMaximum.Value - (YAxis.ActualMaximum.Value - centre.Item2) * mult;
            }
        }

        private void SetXRange(Point startPosition, Point endPosition)
        {
            var x1 = GetChartPosition(startPosition).Item1;
            var x2 = GetChartPosition(endPosition).Item1;

            if (x1 == x2)
            {
                XAxis.Minimum = x1;
                XAxis.Maximum = x1;
            }
            else
            {
                XAxis.Minimum = Min(x1, x2);
                XAxis.Maximum = Max(x1, x2);
            }
        }

        private void SetYRange(Point startPosition, Point endPosition)
        {
            var y1 = GetChartPosition(startPosition).Item2;
            var y2 = GetChartPosition(endPosition).Item2;

            if (y1 == y2)
            {
                YAxis.Minimum = y1;
                YAxis.Maximum = y1;
            }
            else
            {
                YAxis.Minimum = Math.Min(y1, y2);
                YAxis.Maximum = Math.Max(y1, y2);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="p">relative to <see cref="canvas"/></param>
        /// <returns></returns>
        private Tuple<DateTime, double> GetChartPosition(Point p)
        {
            var yAxisY = canvas.TranslatePoint(p, YAxis).Y;
            var yMax = YAxis.ActualMaximum.Value;
            var yMin = YAxis.ActualMinimum.Value;
            var yRange = yMax - yMin;
            var yScale = yRange * 1.0 / YAxis.ActualHeight;

            var yScaled = yMax - (yScale * yAxisY);

            var xAxisX = canvas.TranslatePoint(p, XAxis).X;
            var xMax = XAxis.ActualMaximum.Value;
            var xMin = XAxis.ActualMinimum.Value;
            var xRange = xMax - xMin;
            var xScale = Mult(xRange, 1.0 / XAxis.ActualWidth);

            var xScaled = xMin + Mult(xScale, xAxisX);

            return Tuple.Create(xScaled, yScaled);
        }

        private DateTimeAxis XAxis
        {
            get { return (DateTimeAxis)((LineSeries)chart.Series[0]).ActualIndependentAxis; }
        }

        private NumericAxis YAxis
        {
            get { return (NumericAxis)((LineSeries)chart.Series[0]).ActualDependentRangeAxis; }
        }

        private static TimeSpan Mult(TimeSpan timeSpan, double factor)
        {
            return TimeSpan.FromMilliseconds(timeSpan.TotalMilliseconds * factor);
        }
        private static DateTime Max(DateTime x1, DateTime x2)
        {
            return x1 > x2 ? x1 : x2;
        }

        private static DateTime Min(DateTime x1, DateTime x2)
        {
            return x1 > x2 ? x2 : x1;
        }

        #endregion
    }
}
