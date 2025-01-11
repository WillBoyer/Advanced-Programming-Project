using System.Windows;

namespace CSharpInterpreterGUI
{
    public partial class IntegralInputDialog : Window
    {
        public double Lower { get; private set; }
        public double Upper { get; private set; }
        public int Interval { get; private set; }
        public bool IsConfirmed { get; private set; }

        public IntegralInputDialog()
        {
            InitializeComponent();
            IsConfirmed = false;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Lower = double.Parse(LowerInput.Text);
                Upper = double.Parse(UpperInput.Text);
                Interval = int.Parse(IntervalInput.Text);
                IsConfirmed = true;
                DialogResult = true;
                Close();
            }
            catch
            {
                MessageBox.Show("Invalid input. Please enter valid numeric values.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
