namespace CSharpInterpreterGUI
{
    using System;

    using OxyPlot;
    using OxyPlot.Series;

    public class PlotViewModel
    {
        public PlotViewModel()
        {
            MyModel = new PlotModel { };
            MyModel.Series.Add(new FunctionSeries(Math.Sin, 0, 10, 0.1));
        }

        public PlotModel MyModel { get; private set; }
    }
}