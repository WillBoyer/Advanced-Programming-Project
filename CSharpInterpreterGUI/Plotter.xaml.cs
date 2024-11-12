using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSharpInterpreterGUI
{
    public partial class Plotter : Window
    {
        public Plotter(Func<double, double> function)
        {
            InitializeComponent();

            MyModel = new PlotModel { };
            MyModel.Series.Add(new FunctionSeries(function, 0, 10, 0.1));

            this.DataContext = this;
        }

        public PlotModel MyModel { get; set; }
    }
}
