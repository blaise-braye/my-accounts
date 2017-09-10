using System;
using OxyPlot;
using OxyPlot.Annotations;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class PlotModelRangeSelectionHandler
    {
        private readonly RectangleAnnotation _range;

        private PlotModel _model;

        private double _startx;

        public PlotModelRangeSelectionHandler(PlotModel model)
        {
            _model = model;
            _range = new RectangleAnnotation { Fill = OxyColor.FromAColor(120, OxyColors.SkyBlue) };
            // Create and add the annotation to the plot
            _model.InvalidatePlot(true);

            _startx = double.NaN;

            _model.MouseDown += OnModelOnMouseDown;
            _model.MouseMove += OnModelOnMouseMove;
            _model.MouseUp += OnModelOnMouseUp;
        }
        
        public event EventHandler RangeChangedFromMouseSelection;

        public double MinX => _range.MinimumX;

        public double MaxX => _range.MaximumX;

        public void SetXRange(double minX, double maxX)
        {
            if (Math.Abs(_range.MinimumX - minX) > 0 || Math.Abs(_range.MaximumX - maxX) > 0)
            {
                _range.MinimumX = minX;
                _range.MaximumX = maxX;
                _model.InvalidatePlot(true);
            }
        }
        
        public void DisplaySelection()
        {
            if (!_model.Annotations.Contains(_range))
            {
                _model.Annotations.Add(_range);
                _model.InvalidatePlot(true);
            }
        }

        public void HideAnnotation()
        {
            if (_model.Annotations.Remove(_range))
            {
                _model.InvalidatePlot(true);
            }
        }

        public void Cleanup()
        {
            _model.MouseDown -= OnModelOnMouseDown;
            _model.MouseMove -= OnModelOnMouseMove;
            _model.MouseUp -= OnModelOnMouseUp;
            HideAnnotation();
            _model.Annotations.Remove(_range);
            _model = null;
        }

        private void OnModelOnMouseDown(object s, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton == OxyMouseButton.Left)
            {
                DisplaySelection();

                _startx = _range.InverseTransform(e.Position).X;
                _range.MinimumX = _startx;
                _range.MaximumX = _startx;
                _model.InvalidatePlot(true);
                e.Handled = true;
            }
        }

        private void OnModelOnMouseUp(object s, OxyMouseEventArgs e)
        {
            if (!double.IsNaN(_startx))
            {
                _startx = double.NaN;
                RangeChangedFromMouseSelection?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnModelOnMouseMove(object s, OxyMouseEventArgs e)
        {
            if (!double.IsNaN(_startx))
            {
                var x = _range.InverseTransform(e.Position).X;
                _range.MinimumX = Math.Min(x, _startx);
                _range.MaximumX = Math.Max(x, _startx);
                //range.Text = string.Format("? cos(x) dx =  {0:0.00}", Math.Sin(range.MaximumX) - Math.Sin(range.MinimumX));
                _model.InvalidatePlot(true);
                e.Handled = true;
            }
        }
    }
}