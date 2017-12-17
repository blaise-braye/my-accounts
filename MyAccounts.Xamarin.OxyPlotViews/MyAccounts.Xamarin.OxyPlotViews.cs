using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxyPlot;
using OxyPlot.Xamarin.Forms;
using Xamarin.Forms;

namespace MyAccounts.Xamarin.OxyPlotViews
{
    public class OxyPlotViewBuilder
    {
        public object CreateView(PlotModel model)
        {
            return new PlotView();
        }
    }
}
