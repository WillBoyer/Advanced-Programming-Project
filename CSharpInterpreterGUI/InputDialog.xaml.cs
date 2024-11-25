using System.Windows;

namespace CSharpInterpreterGUI
{
    public partial class InputDialog : Window
    {
        public string StartX { get; private set; }
        public string EndX { get; private set; }
        public string StepX { get; private set; }
        public string InputText { get; private set; }

        public InputDialog()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the input values for startX, endX, and stepX from the textboxes
            StartX = startXTextBox.Text;
            EndX = endXTextBox.Text;
            StepX = stepXTextBox.Text;

            // Generate the loop expression in the format: "for x = startX to endX step stepX"
            InputText = $"for x = {StartX} to {EndX} step {StepX}";

            // Close the dialog and return true
            DialogResult = true; 
        }
    }
}
