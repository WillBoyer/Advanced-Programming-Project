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
        public PlotGraph(List<double> xValues, List<double> yValues)
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

            // Create a line series for the graph
            var series = new LineSeries
            {
                Title = "f(x)",
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Red
            };

            // Add data points to the series
            for (int i = 0; i < xValues.Count; i++)
            {
                series.Points.Add(new DataPoint(xValues[i], yValues[i]));
            }

            // Add the series to the plot model
            model.Series.Add(series);

            // Add bold line through the origin (0,0) for X and Y axes
            var zeroLineX = new LineSeries
            {
                Title = "Zero X",
                Color = OxyColors.Black, 
                StrokeThickness = 3, 
                LineStyle = LineStyle.Solid
            };

            // Add points for the X-axis line (crossing 0 on the Y-axis)
            zeroLineX.Points.Add(new DataPoint(xAxis.Minimum, 0));  
            zeroLineX.Points.Add(new DataPoint(xAxis.Maximum, 0));  

            var zeroLineY = new LineSeries
            {
                Title = "Zero Y",
                Color = OxyColors.Black, 
                StrokeThickness = 3, 
                LineStyle = LineStyle.Solid
            };

            // Add points for the Y-axis line (crossing 0 on the X-axis)
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
