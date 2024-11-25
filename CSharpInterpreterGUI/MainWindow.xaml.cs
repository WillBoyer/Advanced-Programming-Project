using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSharpInterpreterGUI
{
    public partial class MainWindow : Window
    {
        private bool isScientificMode = false;

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
                string expression = displayTextBox.Text;
                string result = ArithmeticInterpreter.evaluateExpression(expression);

                if (expression.Contains("sin") || expression.Contains("cos") || expression.Contains("tan"))
                {
                    // Append "rad" to indicate the use of radians for trig functions
                    resultTextBox.Text = $"{result} rad";
                }
                else
                {
                    resultTextBox.Text = $"{result}";
                }
                
            }
            catch (Exception ex)
            {
                resultTextBox.Text = $"Error: {ex.Message}";
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            displayTextBox.Clear();
            resultTextBox.Clear();
            ResetPlotTypeComboBox(0);
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (displayTextBox.Text.Length > 0)
            {
                displayTextBox.Text = displayTextBox.Text.Substring(0, displayTextBox.Text.Length - 1);
            }
        }

        private void ToggleMode_Click(object sender, RoutedEventArgs e)
        {

            isScientificMode = !isScientificMode;

            if (isScientificMode)
            {
                scientificPanel.Visibility = Visibility.Visible;
                PlotButtonsPanel.Visibility = Visibility.Visible;
                ModeButton.Content = "Standard";
            }
            else
            {
                scientificPanel.Visibility = Visibility.Collapsed;
                PlotButtonsPanel.Visibility = Visibility.Collapsed;
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

        private async void PlotGraph()
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    ResetPlotTypeComboBox(1);
                });

                
                await Task.Delay(10);
                string input = displayTextBox.Text;

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

                // Show input prompt for the user to enter the loop expression
                var inputDialog = new InputDialog();
                if (inputDialog.ShowDialog() == true)
                {
                    string loopExpression = inputDialog.InputText;

                    // Call F# function to evaluate the for-loop expression
                    var (xValues, yValues) = ArithmeticInterpreter.evaluatePolynomialForLoop(loopExpression, input);

                    // Convert F# lists to C# lists
                    var xList = new List<double>(xValues);
                    var yList = new List<double>(yValues);

                    // Pass the x and y values to the PlotGraph window
                    PlotGraph plotWindow = new PlotGraph(xList, yList);
                    plotWindow.ShowDialog();

                    resultTextBox.Text = "Graph plotted successfully.";

                    // After the plot is done, reset ComboBox selection to index 0
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ResetPlotTypeComboBox(0);
                    });

                }
                else
                {
                    // If the dialog was canceled (i.e., user did nothing), reset the ComboBox to index 0.
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ResetPlotTypeComboBox(0);
                    });
                }
            }
            catch (Exception ex)
            {
                resultTextBox.Text = $"Error: {ex.Message}";
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


        // Handle Help button click
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            string helpMessage = ArithmeticInterpreter.helpInfo();
            MessageBox.Show(helpMessage, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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
    }


}
