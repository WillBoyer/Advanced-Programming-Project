using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
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

                if (expression.Contains("sin") || expression.Contains("cos") || expression.Contains("tan"))
                {
                    // Append "rad" to indicate the use of radians for trig functions
                    formattedOutput = $">> {expression}\n{result} rad\n";
                }
                else
                {
                    formattedOutput = $">> {expression}\n{result}\n";
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

        private void Differential_Click(object sender, RoutedEventArgs e)
        {
            // Get the expression from the input box
            string expression = displayTextBox.Text.TrimStart('>', '>').Trim();
            string formattedOutput = "";
            Debug.WriteLine($"Processed Expression: '{expression}'");

            if (string.IsNullOrWhiteSpace(expression))
            {
                return; 
            }

            string result = ArithmeticInterpreter.evaluateCalculus(expression);

            formattedOutput = $">> {expression}\n{result}\n";

            // Add the new entry to the top of the history
            workspaceHistory.Insert(0, formattedOutput);

            // Update the workspace history box
            workspaceHistoryBox.Text = string.Join("\n", workspaceHistory);

            // Clear the input box for the next entry
            displayTextBox.Clear();
        }

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
            if (content == "sin" || content == "cos" || content == "tan" || content == "ln" || content == "log" || content == "d/dx")
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
                MarkerType = OxyPlot.MarkerType.Circle,
                MarkerSize = 5,
                MarkerStroke = OxyPlot.OxyColors.Red 
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

            // Pass the x and y values to the PlotGraph window
            PlotGraph plotWindow = new PlotGraph(xList, yList);

            // Set the window state to Maximized
            plotWindow.WindowState = System.Windows.WindowState.Maximized;
            plotWindow.ShowDialog();


        }


    }


}
