using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.FSharp.Collections;


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
                
                int result = ArithmeticInterpreter.evaluateExpression(input);
                
                resultTextBlock.Text = $"Result: {result}";
            }
            catch (Exception ex)
            {
                resultTextBlock.Text = $"Error: {ex.Message}";
            }
        }
    }
}
