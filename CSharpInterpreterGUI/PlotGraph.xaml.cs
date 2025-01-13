using System;
using System.Collections.Generic;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace CSharpInterpreterGUI
{
    public partial class PlotGraph : Window
    {
        public PlotGraph(List<double> xValues, List<double> yValues, bool showArea, Func<double, double> derivative = null)
        {
            InitializeComponent();

            // Validate input
            if (xValues == null || yValues == null || xValues.Count != yValues.Count)
            {
                MessageBox.Show("Invalid data for plotting.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create the plot model
            var model = new PlotModel { Title = "Polynomial Function Plot" };

            // Define axes with ranges for all four quadrants
            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "X",
                Minimum = -Math.Max(Math.Abs(xValues.Min()), Math.Abs(xValues.Max())),
                Maximum = Math.Max(Math.Abs(xValues.Min()), Math.Abs(xValues.Max())),
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                AxislineStyle = LineStyle.Solid,
                AxislineThickness = 2,
                MajorGridlineColor = OxyColors.Gray,
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Y",
                Minimum = -Math.Max(Math.Abs(yValues.Min()), Math.Abs(yValues.Max())),
                Maximum = Math.Max(Math.Abs(yValues.Min()), Math.Abs(yValues.Max())),
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                AxislineStyle = LineStyle.Solid,
                AxislineThickness = 2,
                MajorGridlineColor = OxyColors.Gray,
            };

            // Add axes to the model
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            // Create an area series for the shaded region
            if (showArea)
            {
                var areaSeries = new AreaSeries
                {
                    Title = "Area Under Curve",
                    Color = OxyColors.Transparent,
                    Fill = OxyColors.LightBlue,
                    StrokeThickness = 1
                };

                // Add data points to the area series
                for (int i = 0; i < xValues.Count; i++)
                {
                    areaSeries.Points.Add(new DataPoint(xValues[i], yValues[i])); 
                    areaSeries.Points2.Add(new DataPoint(xValues[i], 0));         
                }

                // Add the area series to the plot model
                model.Series.Add(areaSeries);
            }

            // Create a line series for the graph
            var series = new LineSeries
            {
                Title = "f(x)",
                MarkerType = MarkerType.None, 
                StrokeThickness = 2,          
                Color = OxyColors.Red         
            };

            // Add data points to the series
            for (int i = 0; i < xValues.Count; i++)
            {
                series.Points.Add(new DataPoint(xValues[i], yValues[i]));
            }

            // Add the series to the plot model
            model.Series.Add(series);


            // Add tangent lines if derivative is provided
            if (derivative != null)
            {
                foreach (var x in xValues.Where((_, index) => index % 10 == 0)) 
                {
                    double y = yValues[xValues.IndexOf(x)];
                    double slope = derivative(x);

                    double tangentStartX = x - 1;
                    double tangentEndX = x + 1;
                    double tangentStartY = y + slope * (tangentStartX - x);
                    double tangentEndY = y + slope * (tangentEndX - x);

                    var tangentSeries = new LineSeries
                    {
                        Title = $"Tangent at x={x:F2}",
                        Color = OxyColors.Blue,
                        StrokeThickness = 1,
                        LineStyle = LineStyle.Dash
                    };

                    tangentSeries.Points.Add(new DataPoint(tangentStartX, tangentStartY));
                    tangentSeries.Points.Add(new DataPoint(tangentEndX, tangentEndY));

                    model.Series.Add(tangentSeries);
                }
            }

            // Add bold line through the origin (0,0) for X and Y axes
            var zeroLineX = new LineSeries
            {
                Title = "Zero X",
                Color = OxyColors.Black,
                StrokeThickness = 3,
                LineStyle = LineStyle.Solid
            };

            zeroLineX.Points.Add(new DataPoint(xAxis.Minimum, 0));
            zeroLineX.Points.Add(new DataPoint(xAxis.Maximum, 0));

            var zeroLineY = new LineSeries
            {
                Title = "Zero Y",
                Color = OxyColors.Black,
                StrokeThickness = 3,
                LineStyle = LineStyle.Solid
            };

            zeroLineY.Points.Add(new DataPoint(0, yAxis.Minimum));
            zeroLineY.Points.Add(new DataPoint(0, yAxis.Maximum));

            // Add the zero lines to the plot model
            model.Series.Add(zeroLineX);
            model.Series.Add(zeroLineY);

            // Set the model to the plot view
            PlotView.Model = model;
        }


        // Event handler for Plot button click
        private void PlotButton_Click(object sender, RoutedEventArgs e)
        {
            
            // For now, this method just serves as a placeholder since PlotGraph is already passed data during initialization.
        }

        
    }
}
