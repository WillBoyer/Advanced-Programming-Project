using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static ArithmeticInterpreter;
using static Microsoft.FSharp.Core.ByRefKinds;

namespace CSharpInterpreterGUI
{
    public partial class MainWindow : Window
    {
        private bool isScientificMode = false;
        private List<string> workspaceHistory = new List<string>();
        private OxyPlot.PlotModel plotModel;

        public bool IsScientificMode
        {
            get => isScientificMode;
            set
            {
                isScientificMode = value;
                this.DataContext = this;
            }
        }



        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string content = button.Content.ToString();
            displayTextBox.Text += content;
        }

        private void Evaluate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            
                // Get the expression from the input box
                string expression = displayTextBox.Text.TrimStart('>', '>').Trim();
                expression = expression.Replace('×', '*').Replace('÷', '/');
                if (expression.Contains("∫"))
                {
                    // Show the Integral input dialog
                    var dialog = new IntegralInputDialog();
                    dialog.Owner = this; // Set owner to the current window
                    if (dialog.ShowDialog() == true && dialog.IsConfirmed)
                    {
                        // Update expression with user inputs
                        string innerExpression = expression.Substring(expression.IndexOf('(') + 1, expression.LastIndexOf(')') - expression.IndexOf('(') - 1);
                        expression = $"∫({dialog.Lower},{dialog.Upper},{innerExpression})";
                        
                    }
                    else
                    {
                        Debug.WriteLine($"######Expression: '{expression}'");
                        return; // User canceled
                    }
                }

                expression = expression.Replace("∫", "Integral");
                string formattedOutput = "";
                Debug.WriteLine($"Processed Expression: '{expression}'");

                if (string.IsNullOrWhiteSpace(expression))
                {
                    return; 
                }

                // Call the F# function to evaluate the expression
                string result = ArithmeticInterpreter.evaluateExpression(expression);

                // Call the F# function to get the variables
                string variables = ArithmeticInterpreter.getAllVariables();
                // Update the variablesTextBox
                variablesTextBox.Text = variables;

                Debug.WriteLine($"All Variables: '{variables}'");
                string displayExpression = expression.Replace("Integral", "∫");

                if (displayExpression.Contains("sin") || displayExpression.Contains("cos") || displayExpression.Contains("tan"))
                {
                    // Append "rad" to indicate the use of radians for trig functions
                    formattedOutput = $">> {displayExpression}\n{result} rad\n";
                }
                else
                {
                    formattedOutput = $">> {displayExpression}\n{result}\n";
                }
                

                // Add the new entry to the top of the history
                workspaceHistory.Insert(0, formattedOutput);

                // Update the workspace history box
                workspaceHistoryBox.Text = string.Join("\n", workspaceHistory);

                // Clear the input box for the next entry
                displayTextBox.Clear();
            }
            catch (Exception ex)
            {
                // Show error in workspace if evaluation fails
                workspaceHistory.Insert(0, $">> {displayTextBox.Text.TrimStart('>', '>').Trim()}\nError: {ex.Message}\n");
                workspaceHistoryBox.Text = string.Join("\n", workspaceHistory);
                displayTextBox.Text = ">>";
            }
        }

        //private void Differential_Click(object sender, RoutedEventArgs e)
        //{
        //    // Get the expression from the input box
        //    string expression = displayTextBox.Text.TrimStart('>', '>').Trim();
        //    string formattedOutput = "";
        //    Debug.WriteLine($"Processed Expression: '{expression}'");

        //    if (string.IsNullOrWhiteSpace(expression))
        //    {
        //        return; 
        //    }

        //    string result = ArithmeticInterpreter.evaluateCalculus(expression);

        //    formattedOutput = $">> {expression}\n{result}\n";

        //    // Add the new entry to the top of the history
        //    workspaceHistory.Insert(0, formattedOutput);

        //    // Update the workspace history box
        //    workspaceHistoryBox.Text = string.Join("\n", workspaceHistory);

        //    // Clear the input box for the next entry
        //    displayTextBox.Clear();
        //}

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            // Clear the history list and update the workspace history box
            workspaceHistory.Clear();
            workspaceHistoryBox.Text = string.Empty;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            displayTextBox.Clear();
            //resultTextBox.Clear();
            //ResetPlotTypeComboBox(0);
        }

        //private void Backspace_Click(object sender, RoutedEventArgs e)
        //{
        //    if (displayTextBox.Text.Length > 0)
        //    {
        //        displayTextBox.Text = displayTextBox.Text.Substring(0, displayTextBox.Text.Length - 1);
        //    }
        //}

        private void ToggleMode_Click(object sender, RoutedEventArgs e)
        {
            isScientificMode = !isScientificMode;

            if (isScientificMode)
            {
                scientificPanel.Visibility = Visibility.Visible;
                ModeButton.Content = "Standard";
            }
            else
            {
                scientificPanel.Visibility = Visibility.Collapsed;
                ModeButton.Content = "Scientific";
            }
        }




        private void ScientificButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string content = button.Content.ToString();
            // Check if the button content is one of the specified scientific functions
            if (content == "sin" || content == "cos" || content == "tan" || content == "ln" || content == "log" || content == "d/dx" || content == "∫")
            {
               
                displayTextBox.Text += content + "()";
            }
            else
            {
                displayTextBox.Text += content;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void DisplayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            const string prompt = ">>";
            var textBox = sender as TextBox;

            // Ensure the text always starts with ">>"
            if (!textBox.Text.StartsWith(prompt))
            {
                textBox.Text = prompt;
                textBox.CaretIndex = textBox.Text.Length; // Move caret to the end
            }
            else
            {
                // Ensure the caret is always after ">>"
                if (textBox.CaretIndex < prompt.Length)
                {
                    textBox.CaretIndex = prompt.Length;
                }
            }
        }

        private void DisplayTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            const int promptLength = 2;
            var textBox = sender as TextBox;

            // Prevent caret from moving before ">>"
            if (textBox.CaretIndex < promptLength)
            {
                textBox.CaretIndex = promptLength;
            }
        }


        private async void PlotGraph()
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    ResetPlotTypeComboBox(1);
                });


                await Task.Delay(10);
                string input = displayTextBox.Text.TrimStart('>', '>').Trim();


                string resultPolynomial = ArithmeticInterpreter.evaluatePolynomial(input);

                if (resultPolynomial != "Valid polynomial.")
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ResetPlotTypeComboBox(0);
                    });

                    MessageBox.Show(resultPolynomial, "Invalid Polynomial", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Enter the for loop expression:", "For Loop Required", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                displayTextBox.Text = $"Error: {ex.Message}";
            }
        }


        private void PlotLinear()
        {

            MessageBox.Show("Plot Linear Interpolation button clicked!");
        }

        private void PlotSpline()
        {

            MessageBox.Show("Plot Linear Interpolation button clicked!");
        }


        //// Handle Help button click
        //private void HelpButton_Click(object sender, RoutedEventArgs e)
        //{
        //    string helpMessage = ArithmeticInterpreter.helpInfo();
        //    MessageBox.Show(helpMessage, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        //}

        // Event handler for the ComboBox selection change
        private void PlotTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var comboBox = sender as System.Windows.Controls.ComboBox;
            var selectedItem = comboBox.SelectedItem as System.Windows.Controls.ComboBoxItem;
            string plotType = selectedItem?.Content.ToString();

            switch (plotType)
            {
                case "Plot Graph":
                    ResetPlotTypeComboBox(1);
                    PlotGraph();
                    break;
                case "Linear Plot":
                    ResetPlotTypeComboBox(2);
                    PlotLinear();
                    break;
                case "Spline Plot":
                    ResetPlotTypeComboBox(3);
                    PlotSpline();
                    break;
                default:
                    break;
            }
        }

        private void ResetPlotTypeComboBox(int index)
        {

            PlotTypeComboBox.SelectedIndex = index;
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the values from the TextBoxes
            string xMinText = XMinTextBox.Text;
            string xMaxText = XMaxTextBox.Text;
            string xStepText = XStepTextBox.Text;
            string loopExpression;
            string input = displayTextBox.Text.TrimStart('>', '>').Trim();

            // Try to parse the values to doubles
            double xMin, xMax, xStep;

            bool isXMinValid = double.TryParse(xMinText, out xMin);
            bool isXMaxValid = double.TryParse(xMaxText, out xMax);
            bool isXStepValid = double.TryParse(xStepText, out xStep);

            // Check if all inputs are valid
            if (!isXMinValid || !isXMaxValid || !isXStepValid)
            {
                MessageBox.Show("Please enter valid numeric values for X Min, X Max, and X Step.");
                return;
            }

            // Generate the loop expression in the format: "for x = startX to endX step stepX"
            loopExpression = $"for x = {xMinText} to {xMaxText} step {xStepText}";

            // Call F# function to evaluate the for-loop expression
            var (xValues, yValues) = ArithmeticInterpreter.evaluatePolynomialForLoop(loopExpression, input);

            // Convert F# lists to C# lists
            var xList = new List<double>(xValues);
            var yList = new List<double>(yValues);

            // Create the plot model (using OxyPlot as an example)
            plotModel = new OxyPlot.PlotModel { Title = "Graph" };

            // Define the X and Y axes with gridlines and styling
            var xAxis = new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = "X-Axis",
                Minimum = xList.Min() - 1, 
                Maximum = xList.Max() + 1, 
                MajorGridlineStyle = OxyPlot.LineStyle.Solid,
                MinorGridlineStyle = OxyPlot.LineStyle.Dot,
                MajorGridlineColor = OxyPlot.OxyColors.Gray,
                AxislineStyle = OxyPlot.LineStyle.Solid,
                AxislineThickness = 2
            };

            var yAxis = new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                Title = "Y-Axis",
                Minimum = yList.Min() - 1, 
                Maximum = yList.Max() + 1, 
                MajorGridlineStyle = OxyPlot.LineStyle.Solid,
                MinorGridlineStyle = OxyPlot.LineStyle.Dot,
                MajorGridlineColor = OxyPlot.OxyColors.Gray,
                AxislineStyle = OxyPlot.LineStyle.Solid,
                AxislineThickness = 2
            };

            // Add axes to the model
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            // Create a line series and add the data points with markers
            var series = new OxyPlot.Series.LineSeries
            {
                Title = "Polynomial",
                ItemsSource = xList.Zip(yList, (x, y) => new OxyPlot.DataPoint(x, y)),
                DataFieldX = "X",
                DataFieldY = "Y",
                MarkerType = OxyPlot.MarkerType.None, // Disable markers (dots)
                StrokeThickness = 2,                  // Adjust the line thickness if needed
                Color = OxyPlot.OxyColors.Red         // Line color
            };

            // Add the series to the plot model
            plotModel.Series.Add(series);

            // Add bold center lines (Zero X and Zero Y)
            var zeroLineX = new OxyPlot.Series.LineSeries
            {
                Title = "Zero X",
                Color = OxyPlot.OxyColors.Black,
                StrokeThickness = 3, 
                LineStyle = OxyPlot.LineStyle.Solid
            };
            zeroLineX.Points.Add(new OxyPlot.DataPoint(xAxis.Minimum, 0)); 
            zeroLineX.Points.Add(new OxyPlot.DataPoint(xAxis.Maximum, 0)); 

            var zeroLineY = new OxyPlot.Series.LineSeries
            {
                Title = "Zero Y",
                Color = OxyPlot.OxyColors.Black,
                StrokeThickness = 3, 
                LineStyle = OxyPlot.LineStyle.Solid
            };
            zeroLineY.Points.Add(new OxyPlot.DataPoint(0, yAxis.Minimum)); 
            zeroLineY.Points.Add(new OxyPlot.DataPoint(0, yAxis.Maximum)); 

            // Add the zero lines to the plot model
            plotModel.Series.Add(zeroLineX);
            plotModel.Series.Add(zeroLineY);

            // Set the plot model to the PlotView control to display it
            plotView.Model = plotModel;

            //resultTextBox.Text = "Graph plotted successfully.";

            // After the plot is done, reset ComboBox selection to index 0
            await Dispatcher.InvokeAsync(() =>
            {
                ResetPlotTypeComboBox(0);
            });
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the input expression
                string input = displayTextBox.Text.TrimStart('>', '>').Trim();

                // Validate the input
                if (string.IsNullOrWhiteSpace(input))
                {
                    MessageBox.Show("Please enter a valid expression.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get the values from the TextBoxes
                string xMinText = XMinTextBox.Text;
                string xMaxText = XMaxTextBox.Text;
                string xStepText = XStepTextBox.Text;
                double xMin = -5.0, xMax = 5.0, step = 0.5;

                // Try to parse the input text values
                if (!double.TryParse(xMinText, out xMin))
                {
                    xMin = -5.0; // Default value if parsing fails
                }

                if (!double.TryParse(xMaxText, out xMax))
                {
                    xMax = 5.0; // Default value if parsing fails
                }

                if (!double.TryParse(xStepText, out step) || step <= 0)
                {
                    step = 0.5; // Default value if parsing fails or invalid step
                }

             

                bool showArea = false;

                if (input.StartsWith("d/dx"))
                {
                    try
                    {
                        

                        // Validate the input
                        if (string.IsNullOrEmpty(input) || !input.StartsWith("d/dx"))
                        {
                            MessageBox.Show("The input must start with 'd/dx' to calculate the derivative.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Extract the function from the input by removing 'd/dx(' and the closing ')'
                        string function = input.Replace("d/dx(", "").TrimEnd(')');
                        Debug.WriteLine($"Extracted Function: {function}");

                        

                       
                        var xValues = Enumerable.Range((int)(xMin / step), (int)((xMax - xMin) / step) + 1)
                                                .Select(i => i * step)
                                                .ToList();

                        // Evaluate the function at each x value
                        var yFunctionValues = xValues
                            .Select(x =>
                            {
                                try
                                {
                                    // Replace 'x' in the function with the current x value
                                    string functionWithX = function.Replace("x", x.ToString("G"));

                                    // Evaluate the function using F#
                                    string evaluatedResult = ArithmeticInterpreter.evaluateExpression(functionWithX);

                                    // Parse the result into a double
                                    return double.Parse(evaluatedResult);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error evaluating function at x={x}: {ex.Message}");
                                    throw;
                                }
                            })
                            .ToList();

                        // Evaluate the derivative at each x value
                        var yDerivativeValues = xValues
                            .Select(x =>
                            {
                                try
                                {
                                    // Evaluate the derivative using F#
                                    return ArithmeticInterpreter.evaluateDerivativeAt(input, x);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error evaluating derivative at x={x}: {ex.Message}");
                                    throw;
                                }
                            })
                            .ToList();

                        // Create the plot model
                        plotModel = new PlotModel { Title = "Function and Derivative Plot" };

                        // Add axes
                        plotModel.Axes.Add(new LinearAxis
                        {
                            Position = AxisPosition.Bottom,
                            Title = "X-Axis",
                            Minimum = xMin,
                            Maximum = xMax,
                            MajorGridlineStyle = LineStyle.Solid,
                            MinorGridlineStyle = LineStyle.Dot,
                            MajorGridlineColor = OxyColors.Gray
                        });

                        plotModel.Axes.Add(new LinearAxis
                        {
                            Position = AxisPosition.Left,
                            Title = "Y-Axis",
                            Minimum = Math.Min(yFunctionValues.Min(), yDerivativeValues.Min()) - 1,
                            Maximum = Math.Max(yFunctionValues.Max(), yDerivativeValues.Max()) + 1,
                            MajorGridlineStyle = LineStyle.Solid,
                            MinorGridlineStyle = LineStyle.Dot,
                            MajorGridlineColor = OxyColors.Gray
                        });

                        // Add function series
                        var functionSeries = new LineSeries
                        {
                            Title = "f(x)",
                            StrokeThickness = 2,
                            Color = OxyColors.Blue
                        };
                        functionSeries.Points.AddRange(xValues.Zip(yFunctionValues, (x, y) => new DataPoint(x, y)));

                        // Add derivative series
                        var derivativeSeries = new LineSeries
                        {
                            Title = "f'(x)",
                            StrokeThickness = 2,
                            Color = OxyColors.Red,
                            LineStyle = LineStyle.Dash
                        };
                        derivativeSeries.Points.AddRange(xValues.Zip(yDerivativeValues, (x, y) => new DataPoint(x, y)));

                        // Add series to plot model
                        plotModel.Series.Add(functionSeries);
                        plotModel.Series.Add(derivativeSeries);

                        // Create a new window to display the plot
                        var plotView = new PlotView
                        {
                            Model = plotModel,
                            
                        };

                        var plotWindow = new Window
                        {
                            Title = "Function and Derivative Plot",
                            Content = plotView,
                            WindowState = WindowState.Maximized,
                            Width = 800,
                            Height = 600
                        };

                        // Show the plot window maximized
                        plotWindow.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                else if (input.StartsWith("∫"))
                {
                    try
                    {
                        

                        // Try to parse the values to doubles
                        if (!double.TryParse(xMinText, out double lower) ||
                            !double.TryParse(xMaxText, out double upper) ||
                            !double.TryParse(xStepText, out double interval) || interval <= 0)
                        {
                            MessageBox.Show("Please enter valid numeric values for X Min, X Max, and X Step.");
                            return;
                        }

                        // Replace ∫ with Integral in the expression
                        input = input.Replace("∫", "Integral");
                        input = $"Integral({lower},{upper},{input.Substring(input.IndexOf('(') + 1)}";

                        // Call the F# function to evaluate the area under the curve
                        var (xValues, yValues) = ArithmeticInterpreter.evaluateNumericalIntegration(input, lower, upper, interval);

                        // Convert F# lists to C# lists
                        var xList = new List<double>(xValues);
                        var yList = new List<double>(yValues);

                        

                        // Add axes
                        plotModel.Axes.Add(new LinearAxis
                        {
                            Position = AxisPosition.Bottom,
                            Title = "X-Axis",
                            Minimum = lower,
                            Maximum = upper,
                            MajorGridlineStyle = LineStyle.Solid,
                            MinorGridlineStyle = LineStyle.Dot,
                            MajorGridlineColor = OxyColors.Gray
                        });

                        plotModel.Axes.Add(new LinearAxis
                        {
                            Position = AxisPosition.Left,
                            Title = "Y-Axis",
                            Minimum = yValues.Min() - 1,
                            Maximum = yValues.Max() + 1,
                            MajorGridlineStyle = LineStyle.Solid,
                            MinorGridlineStyle = LineStyle.Dot,
                            MajorGridlineColor = OxyColors.Gray
                        });

                        // Add an area series for the shaded region
                        var areaSeries = new AreaSeries
                        {
                            Title = "Area Under Curve",
                            Color = OxyColors.Transparent,
                            Fill = OxyColors.LightBlue,
                            StrokeThickness = 1
                        };

                        // Add points to the area series
                        for (int i = 0; i < xList.Count; i++)
                        {
                            areaSeries.Points.Add(new DataPoint(xValues[i], yValues[i])); // Curve points
                            areaSeries.Points2.Add(new DataPoint(xValues[i], 0));        // Baseline (y = 0)
                        }

                        // Add the area series to the plot model
                        plotModel.Series.Add(areaSeries);

                        // Add a line series for the function curve
                        var lineSeries = new LineSeries
                        {
                            Title = "f(x)",
                            StrokeThickness = 2,
                            Color = OxyColors.Red
                        };
                        lineSeries.Points.AddRange(xValues.Zip(yValues, (x, y) => new DataPoint(x, y)));

                        // Add the line series to the plot model
                        plotModel.Series.Add(lineSeries);

                       

                        // Open the PlotGraph window and pass the x and y values
                        PlotGraph plotWindow = new PlotGraph(xList, yList, true);
                        plotWindow.WindowState = System.Windows.WindowState.Maximized;
                        plotWindow.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {

                    // Try to parse the values to doubles
                    if (!double.TryParse(xMinText, out double lower) ||
                        !double.TryParse(xMaxText, out double upper) ||
                        !double.TryParse(xStepText, out double interval) || interval <= 0)
                    {
                        MessageBox.Show("Please enter valid numeric values for X Min, X Max, and X Step.");
                        return;
                    }

                    // Generate the loop expression in the format: "for x = startX to endX step stepX"
                    String loopExpression = $"for x = {xMinText} to {xMaxText} step {xStepText}";

                    // Call F# function to evaluate the for-loop expression
                    var (xValues, yValues) = ArithmeticInterpreter.evaluatePolynomialForLoop(loopExpression, input);

                    // Convert F# lists to C# lists
                    var xList = new List<double>(xValues);
                    var yList = new List<double>(yValues);

                    // Pass the x and y values to the PlotGraph window
                    PlotGraph plotWindow = new PlotGraph(xList, yList, false);

                    // Set the window state to Maximized
                    plotWindow.WindowState = System.Windows.WindowState.Maximized;
                    plotWindow.ShowDialog();
                }

               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DefiniteIntegral_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the values from the TextBoxes
                string xMinText = XMinTextBox.Text;
                string xMaxText = XMaxTextBox.Text;
                string xStepText = XStepTextBox.Text;
                string input = displayTextBox.Text.TrimStart('>', '>').Trim();

                // Try to parse the values to doubles
                if (!double.TryParse(xMinText, out double lower) ||
                    !double.TryParse(xMaxText, out double upper) ||
                    !double.TryParse(xStepText, out double interval) || interval <= 0)
                {
                    MessageBox.Show("Please enter valid numeric values for X Min, X Max, and X Step.");
                    return;
                }

                // Replace ∫ with Integral in the expression
                input = input.Replace("∫", "Integral");
                input = $"Integral({lower},{upper},{input.Substring(input.IndexOf('(') + 1)}";

                // Call the F# function to evaluate the area under the curve
                var (xValues, yValues) = ArithmeticInterpreter.evaluateNumericalIntegration(input, lower, upper, interval);

                // Convert F# lists to C# lists
                var xList = new List<double>(xValues);
                var yList = new List<double>(yValues);

                // Create the plot model
                plotModel = new PlotModel { Title = "Definite Integral Plot" };

                // Add axes
                plotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "X-Axis",
                    Minimum = lower,
                    Maximum = upper,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColors.Gray
                });

                plotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Y-Axis",
                    Minimum = yValues.Min() - 1,
                    Maximum = yValues.Max() + 1,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColors.Gray
                });

                // Add an area series for the shaded region
                var areaSeries = new AreaSeries
                {
                    Title = "Area Under Curve",
                    Color = OxyColors.Transparent,
                    Fill = OxyColors.LightBlue,
                    StrokeThickness = 1
                };

                // Add points to the area series
                for (int i = 0; i < xList.Count; i++)
                {
                    areaSeries.Points.Add(new DataPoint(xValues[i], yValues[i])); // Curve points
                    areaSeries.Points2.Add(new DataPoint(xValues[i], 0));        // Baseline (y = 0)
                }

                // Add the area series to the plot model
                plotModel.Series.Add(areaSeries);

                // Add a line series for the function curve
                var lineSeries = new LineSeries
                {
                    Title = "f(x)",
                    StrokeThickness = 2,
                    Color = OxyColors.Red
                };
                lineSeries.Points.AddRange(xValues.Zip(yValues, (x, y) => new DataPoint(x, y)));

                // Add the line series to the plot model
                plotModel.Series.Add(lineSeries);

                // Set the plot model to the PlotView control
                plotView.Model = plotModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void Differential_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the expression from the input box
                string input = displayTextBox.Text.TrimStart('>', '>').Trim();

                // Validate the input
                if (string.IsNullOrEmpty(input) || !input.StartsWith("d/dx"))
                {
                    MessageBox.Show("The input must start with 'd/dx' to calculate the derivative.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Extract the function from the input by removing 'd/dx(' and the closing ')'
                string function = input.Replace("d/dx(", "").TrimEnd(')');
                Debug.WriteLine($"Extracted Function: {function}");

                // Define the range and step size for x values
                // Get the values from the TextBoxes
                string xMinText = XMinTextBox.Text;
                string xMaxText = XMaxTextBox.Text;
                string xStepText = XStepTextBox.Text;
                double xMin = -5.0, xMax = 5.0, step = 0.5;

                // Try to parse the input text values
                if (!double.TryParse(xMinText, out xMin))
                {
                    xMin = -5.0; // Default value if parsing fails
                }

                if (!double.TryParse(xMaxText, out xMax))
                {
                    xMax = 5.0; // Default value if parsing fails
                }

                if (!double.TryParse(xStepText, out step) || step <= 0)
                {
                    step = 0.5; // Default value if parsing fails or invalid step
                }
                var xValues = Enumerable.Range((int)(xMin / step), (int)((xMax - xMin) / step) + 1)
                                        .Select(i => i * step)
                                        .ToList();

                // Evaluate the function at each x value
                var yFunctionValues = xValues
                    .Select(x =>
                    {
                        try
                        {
                            // Replace 'x' in the function with the current x value
                            string functionWithX = function.Replace("x", x.ToString("G"));

                            // Evaluate the function using F#
                            string evaluatedResult = ArithmeticInterpreter.evaluateExpression(functionWithX);

                            // Parse the result into a double
                            return double.Parse(evaluatedResult);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error evaluating function at x={x}: {ex.Message}");
                            throw;
                        }
                    })
                    .ToList();

                // Evaluate the derivative at each x value
                var yDerivativeValues = xValues
                    .Select(x =>
                    {
                        try
                        {
                            // Evaluate the derivative using F#
                            return ArithmeticInterpreter.evaluateDerivativeAt(input, x);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error evaluating derivative at x={x}: {ex.Message}");
                            throw;
                        }
                    })
                    .ToList();

                // Create the plot model
                plotModel = new PlotModel { Title = "Function and Derivative Plot" };

                // Add axes
                plotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "X-Axis",
                    Minimum = xMin,
                    Maximum = xMax,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColors.Gray
                });

                plotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Y-Axis",
                    Minimum = Math.Min(yFunctionValues.Min(), yDerivativeValues.Min()) - 1,
                    Maximum = Math.Max(yFunctionValues.Max(), yDerivativeValues.Max()) + 1,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColors.Gray
                });

                // Add function series
                var functionSeries = new LineSeries
                {
                    Title = "f(x)",
                    StrokeThickness = 2,
                    Color = OxyColors.Blue
                };
                functionSeries.Points.AddRange(xValues.Zip(yFunctionValues, (x, y) => new DataPoint(x, y)));

                // Add derivative series
                var derivativeSeries = new LineSeries
                {
                    Title = "f'(x)",
                    StrokeThickness = 2,
                    Color = OxyColors.Red,
                    LineStyle = LineStyle.Dash
                };
                derivativeSeries.Points.AddRange(xValues.Zip(yDerivativeValues, (x, y) => new DataPoint(x, y)));

                // Add series to plot model
                plotModel.Series.Add(functionSeries);
                plotModel.Series.Add(derivativeSeries);

                // Set the plot model to the PlotView control
                plotView.Model = plotModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }











        //private void DefiniteIntegral_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        // Prompt for the for-loop expression
        //        MessageBox.Show("Enter the for-loop expression:", "For Loop Required", MessageBoxButton.OK, MessageBoxImage.Information);

        //        // Get the values from the TextBoxes
        //        string xMinText = XMinTextBox.Text;
        //        string xMaxText = XMaxTextBox.Text;
        //        string xStepText = XStepTextBox.Text;
        //        string input = displayTextBox.Text.TrimStart('>', '>').Trim();

        //        // Try to parse the values to doubles
        //        if (!double.TryParse(xMinText, out double lower) ||
        //            !double.TryParse(xMaxText, out double upper) ||
        //            !double.TryParse(xStepText, out double interval))
        //        {
        //            MessageBox.Show("Please enter valid numeric values for X Min, X Max, and X Step.");
        //            return;
        //        }

        //        // Replace ∫ with Integral in the expression
        //        input = input.Replace('∫', "Integral");
        //        input = $"Integral({lower},{upper},{input.Substring(input.IndexOf('(') + 1)})";

        //        // Call the F# function to evaluate the area under the curve
        //        var plotModel = ArithmeticInterpreter.displayAreaUnderCurve(input, lower, upper, interval);

        //        // Display the plot
        //        PlotView plotView = new PlotView
        //        {
        //            Model = plotModel,
        //            Width = 600,
        //            Height = 400
        //        };

        //        Window plotWindow = new Window
        //        {
        //            Title = "Area Under Curve",
        //            Content = plotView,
        //            Width = 800,
        //            Height = 600
        //        };
        //        plotWindow.ShowDialog();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}



    }


}
