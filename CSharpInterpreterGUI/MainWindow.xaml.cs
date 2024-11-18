using System;
using System.Linq.Expressions;
using System.Windows;
using static Microsoft.FSharp.Core.ByRefKinds;


namespace CSharpInterpreterGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Evaluate Button Click
        private void Evaluate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = inputTextBox.Text;

                // Evaluate the expression using the interpreter
                string result = ArithmeticInterpreter.evaluateExpression(input);
                resultTextBlock.Text = $"Result: {result}";

                // If it's a polynomial, ask for the loop expression
                //if (result.Contains("0"))
                //{
                //    //PromptForLoopExpression();
                //}
                //else
                //{
                //    resultTextBlock.Text = $"Result: {result}";
                //}
            }
            catch (Exception ex)
            {
                resultTextBlock.Text = $"Error: {ex.Message}";
            }
        }

        // Handle Help button click
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            string helpMessage = ArithmeticInterpreter.helpInfo(); 
            MessageBox.Show(helpMessage, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }



        private void PlotGraph_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = inputTextBox.Text;
                // string resultPolynomial = ArithmeticInterpreter.evaluatePolynomial(input);
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

                    resultTextBlock.Text = "Graph plotted successfully.";
                }
            }
            catch (Exception ex)
            {
                resultTextBlock.Text = $"Error: {ex.Message}";
            }
        }

        private void PlotLinear_Click(object sender, RoutedEventArgs e)
        {
            // Logic for plotting spline interpolation
            MessageBox.Show("Plotting Spline Interpolation...");
            // Call your spline interpolation plotting function here
        }

        private void PlotSpline_Click(object sender, RoutedEventArgs e)
        {
            // Logic for plotting spline interpolation
            MessageBox.Show("Plotting Spline Interpolation...");
            // Call your spline interpolation plotting function here
        }

        // Prompt the user to enter a for-loop expression
        //private void PromptForLoopExpression()
        //{
        //    MessageBox.Show("Enter the for loop expression:", "For Loop Required", MessageBoxButton.OK, MessageBoxImage.Information);

        //    // Show input prompt for the user to enter the loop expression
        //    var inputDialog = new InputDialog();
        //    if (inputDialog.ShowDialog() == true)
        //    {
        //        string loopExpression = inputDialog.InputText;
        //        // Call F# function to evaluate the for-loop expression, without expecting a string result
        //        ArithmeticInterpreter.evaluatePolynomialForLoop(loopExpression);
        //        // Optionally, update the UI to reflect that the loop expression was handled
        //        resultTextBlock.Text = "For-loop processed.";
        //    }
        //}
    }
}
