using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using MyAccounts.NetStandard.Input;
using MyAccounts.NetStandard.Projections;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Managers.Reports.Models;
using Operations.Classification.WpfUi.Technical.Messages;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly AsyncMessageReceiver _asyncMessageReceiver;
        private readonly List<PlotModelRangeSelectionHandler> _selectionHandlers;

        private PlotModel _dailyOperationsModel;
        private PlotModel _monthlyOperationsModel;

        private IList<AccountViewModel> _accounts;

        private bool _display;
        private List<DashboardOperationModel> _operations;
        private OperationSetContainer _operationsetContainer;

        public DashboardViewModel(BusyIndicatorViewModel busyIndicator)
        {
            _busyIndicator = busyIndicator;

            Filter = new DashboardFilterViewModel();
            Filter.FilterInvalidated += async (sender, arg) =>
            {
                if (sender == Filter.DateRangeFilter)
                {
                    foreach (var handler in _selectionHandlers)
                    {
                        SetRangeFromFilterDateRangeFilter(handler);
                    }
                }

                if (sender == Filter.AccountsFilter)
                {
                    await Refresh();
                }
                else
                {
                    await RefreshFilteredOperations();
                }
            };

            _asyncMessageReceiver = new AsyncMessageReceiver(MessengerInstance);
            _asyncMessageReceiver.RegisterAsync<AccountsViewModelLoaded>(this, OnAccountsViewModelLoaded);

            PlotController = new PlotController();
            // show tooltip on mouse over, instead of on click
            PlotController.UnbindMouseDown(OxyMouseButton.Left);
            PlotController.BindMouseEnter(PlotCommands.HoverSnapTrack);

            _selectionHandlers = new List<PlotModelRangeSelectionHandler>();
        }

        public bool Display
        {
            get => _display;
            set { Set(() => Display, ref _display, value); }
        }

        public DashboardFilterViewModel Filter { get; }

        public PlotController PlotController { get; }

        public PlotModel DailyOperationsModel
        {
            get => _dailyOperationsModel;
            set => Set(nameof(DailyOperationsModel), ref _dailyOperationsModel, value);
        }

        public PlotModel MonthlyOperationsModel
        {
            get => _monthlyOperationsModel;
            private set => Set(nameof(MonthlyOperationsModel), ref _monthlyOperationsModel, value);
        }
        
        public List<DashboardOperationModel> Operations
        {
            get => _operations;
            private set => Set(nameof(Operations), ref _operations, value);
        }

        public async Task ResetAccounts(IList<AccountViewModel> accounts)
        {
            _accounts = accounts;
            Display = _accounts?.Any(a => a.Operations.Any()) == true;
            Filter.AccountsFilter.Initialize(accounts, account => account.Name);
            await Refresh();
        }

        public void OnSettingsUpdated()
        {
            var dailyOperationsModel = DailyOperationsModel;
            var monthyOperationsModel = MonthlyOperationsModel;
            DailyOperationsModel = null;
            MonthlyOperationsModel = null;

            if (dailyOperationsModel != null)
            {
                foreach (var series in dailyOperationsModel.Series)
                {
                    series.TrackerFormatString = GetDailyTrackerFormatString();
                }

                DailyOperationsModel = dailyOperationsModel;
            }

            if (monthyOperationsModel != null)
            {
                foreach (var series in monthyOperationsModel.Series)
                {
                    series.TrackerFormatString = GetMonthTrackerFormatString();
                }

                foreach (var netRevenueSerie in monthyOperationsModel.Series.OfType<LineSeries>()
                    .Where(s => s.DataFieldY == nameof(OperationSet.NetRevenue)))
                {
                    netRevenueSerie.LabelFormatString = GetNetRevenueLabelFormatString();
                }

                MonthlyOperationsModel = monthyOperationsModel;
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _asyncMessageReceiver.Cleanup();
        }

        private static PlotModel CreateDailyOperationsModel(List<OperationSet> operations)
        {
            var operationsModel = new PlotModel { Title = "Daily operations" };

            var dateTimeAxis = new DateTimeAxis
            {
                IntervalType = DateTimeIntervalType.Auto
            };

            if (operations.Count > 0)
            {
                var lastItem = operations[operations.Count - 1];
                dateTimeAxis.Maximum = DateTimeAxis.ToDouble(lastItem.Period);
                dateTimeAxis.Minimum = DateTimeAxis.ToDouble(lastItem.Period.AddMonths(-2));
            }

            operationsModel.Axes.Add(dateTimeAxis);

            var trackerFormatString = GetDailyTrackerFormatString();

            operationsModel.Series.Add(
                CreateLineSeries(operations, "Balance", OxyColors.Blue, nameof(OperationSet.Balance), trackerFormatString));

            operationsModel.Series.Add(
                CreateLineSeries(operations, "Outcome", OxyColors.Red, nameof(OperationSet.Outcome), trackerFormatString));

            operationsModel.Series.Add(
                CreateLineSeries(operations, "Income", OxyColors.Green, nameof(OperationSet.Income), trackerFormatString));

            return operationsModel;
        }

        private static PlotModel CreateMonthlyOperationsModel(List<OperationSet> operations)
        {
            var operationsModel = new PlotModel { Title = "Monthly operations" };

            var dateTimeAxis = new DateTimeAxis
            {
                IntervalType = DateTimeIntervalType.Months
            };
            
            operationsModel.Axes.Add(dateTimeAxis);

            var gridLines = new LinearAxis { Position = AxisPosition.Left, ExtraGridlines = new double[] { 0 }, ExtraGridlineStyle = LineStyle.Dot };
            operationsModel.Axes.Add(gridLines);

            var trackerFormatString = GetMonthTrackerFormatString();

            operationsModel.Series.Add(
                CreateLinearBarSeries(operations, "Income", OxyColors.LightGreen, nameof(OperationSet.Income), trackerFormatString));

            operationsModel.Series.Add(
                CreateLinearBarSeries(operations, "Outcome", OxyColors.OrangeRed, nameof(OperationSet.NegativeOutcome), trackerFormatString));

            operationsModel.Series.Add(
                CreateLineSeries(operations, "Balance", OxyColors.Orange, nameof(OperationSet.Balance), trackerFormatString));

            var netRevenueSeries = CreateLineSeries(operations, "Net revenue", OxyColors.Black, nameof(OperationSet.NetRevenue), trackerFormatString);

            netRevenueSeries.LabelFormatString = GetNetRevenueLabelFormatString();

            operationsModel.Series.Add(netRevenueSeries);

            return operationsModel;
        }
        
        private static string GetNetRevenueLabelFormatString()
        {
            if (!Properties.Settings.Default.HideAmounts)
            {
                return "{1:c2}";
            }

            return string.Empty;
        }

        private static string GetDailyTrackerFormatString()
        {
            return Properties.Settings.Default.HideAmounts ? "{2:d} / 0.01" : "{2:d} / {4:c2}";
        }

        private static string GetMonthTrackerFormatString()
        {
            return Properties.Settings.Default.HideAmounts ? "{2:MMM yyyy} / 0.01" : "{2:MMM yyyy} / {4:c2}";
        }

        private static LineSeries CreateLineSeries(
            IEnumerable<OperationSet> operations,
            string title,
            OxyColor color,
            string dataFieldY,
            string trackerFormatString)
        {
            var series = CreateDataPointSeries<LineSeries>(operations, title, dataFieldY, trackerFormatString);
            series.Color = color;
            series.StrokeThickness = 2;
            series.MarkerSize = 4;
            series.MarkerStroke = color;
            series.MarkerType = MarkerType.Diamond;
            series.MarkerFill = OxyColors.WhiteSmoke;

            return series;
        }

        private static LinearBarSeries CreateLinearBarSeries(
            IEnumerable<OperationSet> operations,
            string title,
            OxyColor fillColor,
            string dataFieldY,
            string trackerFormatString)
        {
            var series = CreateDataPointSeries<LinearBarSeries>(operations, title, dataFieldY, trackerFormatString);
            
            series.FillColor = fillColor;
            series.BarWidth = 30;

            return series;
        }

        private static T CreateDataPointSeries<T>(IEnumerable<OperationSet> operations, string title, string dataFieldY, string trackerFormatString)
            where T : DataPointSeries, new()
        {
            var series = new T
            {
                ItemsSource = operations,
                TrackerFormatString = trackerFormatString,
                Title = title,
                DataFieldX = nameof(OperationSet.Period),
                DataFieldY = dataFieldY
            };

            return series;
        }

        private Task OnAccountsViewModelLoaded(AccountsViewModelLoaded arg)
        {
            return ResetAccounts(arg.Accounts);
        }

        private async Task Refresh()
        {
            OperationSetContainer operationsetContainer = null;

            using (_busyIndicator.EncapsulateActiveJobDescription(this, "Computing reporting index"))
            {
                await Task.Run(() =>
                {
                    var filteredAccounts = Filter.AccountsFilter.Apply(_accounts).ToList();
                    operationsetContainer = OperationSetContainer.Compute(filteredAccounts);
                });
            }

            _selectionHandlers.ForEach(h => h.Cleanup());
            _selectionHandlers.Clear();

            _operationsetContainer = operationsetContainer;
            DailyOperationsModel = CreateDailyOperationsModel(operationsetContainer.DailyOperations);
            AddRangeSelectionHandler(DailyOperationsModel, false);

            MonthlyOperationsModel = CreateMonthlyOperationsModel(operationsetContainer.MonthlyOperations);
            AddRangeSelectionHandler(MonthlyOperationsModel, true);
            
            await RefreshFilteredOperations();
        }

        private void AddRangeSelectionHandler(PlotModel model, bool monthly)
        {
            var handler = new PlotModelRangeSelectionHandler(model);
            SetRangeFromFilterDateRangeFilter(handler);

            handler.RangeChangedFromMouseSelection += (sender, e) =>
            {
                var fromDate = DateTimeAxis.ToDateTime(handler.MinX).Date;
                var toDate = DateTimeAxis.ToDateTime(handler.MaxX).Date;
                if (monthly)
                {
                    fromDate = new DateTime(fromDate.Year, fromDate.Month, 1);
                    toDate = new DateTime(toDate.Year, toDate.Month, 1).AddMonths(1).AddDays(-1);
                }

                Filter.DateRangeFilter.SetPeriod(fromDate, toDate);
            };

            _selectionHandlers.Add(handler);
        }

        private void SetRangeFromFilterDateRangeFilter(PlotModelRangeSelectionHandler handler)
        {
            if (Filter.DateRangeFilter.FromDate.HasValue && Filter.DateRangeFilter.ToDate.HasValue)
            {
                handler.SetXRange(
                    DateTimeAxis.ToDouble(Filter.DateRangeFilter.FromDate.Value),
                    DateTimeAxis.ToDouble(Filter.DateRangeFilter.ToDate.Value));
                handler.DisplaySelection();
            }
            else
            {
                handler.HideAnnotation();
            }
        }

        private async Task RefreshFilteredOperations()
        {
            var filteredOperationsVm = await Task.Run(() =>
            {
                var filteredDailyOperationSets = Filter.DateRangeFilter.Apply(_operationsetContainer.DailyOperations, op => op.Period);
                var filteterdOperations = filteredDailyOperationSets.SelectMany(s => s.Operations);
                filteterdOperations = Filter.NoteFilter.Apply(filteterdOperations, o => o.Note);
                filteterdOperations = Filter.DirectionFilter.Apply(filteterdOperations, o => o.Income, o => o.Outcome);
                var accountById = _accounts.ToDictionary(a => a.Id);

                var result = filteterdOperations
                    .Project()
                    .To<DashboardOperationModel>((source, target) =>
                    {
                        target.Account = accountById[_operationsetContainer.AccountIdByOperationUId[source.UId]];
                    })
                    .OrderByDescending(t => t.ExecutionDate)
                    .ToList();
                return result;
            });
            
            Operations = filteredOperationsVm;
        }
    }
}