using OxyPlot;
using OxyPlot.Series;
using System;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CSharpInterpreterGUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Evaluate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = inputTextBox.Text;

                // Use double to handle both int and float results
                string result = ArithmeticInterpreter.evaluateExpression(input);

                // Display the result with formatting to trim unnecessary decimal places if it's an integer
                resultTextBlock.Text = $"{result:G}";
            }
            catch (Exception ex)
            {
                resultTextBlock.Text = $"Error: {ex.Message}";
            }
        }
        private void Plot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = inputTextBox.Text;

                // Use double to handle both int and float results
                Func<double, double> function = ArithmeticInterpreter.evaluateFunction(input);

                Plotter plotter = new Plotter(function);
                plotter.Show();
            }
            catch (Exception ex)
            {
                
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            string helpMessage = ArithmeticInterpreter.helpInfo(); // Call the help function
            MessageBox.Show(helpMessage, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
