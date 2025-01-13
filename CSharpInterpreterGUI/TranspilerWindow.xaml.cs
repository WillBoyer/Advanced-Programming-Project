using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace CSharpInterpreterGUI
{
    public partial class TranspilerWindow : Window
    {
        public TranspilerWindow(string interpreterCode)
        {
            InitializeComponent();
            InterpreterCodeBox.Text = interpreterCode; // Populate the interpreter code box
        }

        private void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string interpreterCode = InterpreterCodeBox.Text.TrimStart('>', '>').Trim();
                

                if (string.IsNullOrWhiteSpace(interpreterCode))
                {
                    MessageBox.Show("Please enter valid interpreter code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Use evaluateExpression to process the input and check for errors
                string result = ArithmeticInterpreter.evaluateExpressionForCompiler(interpreterCode);

                // Generate Python code
                string pythonCode = GeneratePythonCode(interpreterCode);

                // Display Python code in the output box
                PythonCodeBox.Text = pythonCode;

                // Save Python code to file
                string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "generated_code.py");
                File.WriteAllText(outputPath, pythonCode);

                MessageBox.Show($"Compilation successful!\nPython code saved to: {outputPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Compilation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "generated_code.py");

                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show("Generated Python script not found. Please compile the code first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string output = RunPythonScript(scriptPath);
                MessageBox.Show($"Output:\n{output}", "Execution Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Execution failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GeneratePythonCode(string interpreterCode)
        {
            return interpreterCode.Replace("^", "**").Replace(";", "\n");
        }

        private string RunPythonScript(string scriptPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new Exception(error);
            }

            return output;
        }
    }
}
