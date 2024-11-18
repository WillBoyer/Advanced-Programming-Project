using System.Windows;

namespace CSharpInterpreterGUI
{
    public partial class InputDialog : Window
    {
        public string InputText { get; private set; }

        public InputDialog()
        {
            InitializeComponent();
        }

        
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = loopInputTextBox.Text;
            DialogResult = true; 
        }
    }
}
