using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Managers.Reports.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class UnifiedOperationsReporter : ViewModelBase
    {
        private PlotModel _dailyOperationsModel;

        private PlotModel _monthyOperationsModel;

        public PlotModel DailyOperationsModel
        {
            get => _dailyOperationsModel;
            set => Set(nameof(DailyOperationsModel), ref _dailyOperationsModel, value);
        }

        public PlotModel MonthyOperationsModel
        {
            get => _monthyOperationsModel;
            private set => Set(nameof(MonthyOperationsModel), ref _monthyOperationsModel, value);
        }

        public async Task UpdateAccountSelection(List<AccountViewModel> selection)
        {
            var operations = await Task.Run(
                () =>
                {
                    var mixedAccountOperationOperations = selection
                        .Where(a => a.Operations?.Any() == true)
                        .SelectMany(account => ComputeOperationsPerDay(account.InitialBalance, account.Operations))
                        .OrderBy(op => op.Day).ToList();

                    var aggregattedDailyOperations = GroupDailyOperations(mixedAccountOperationOperations);
                    var aggregattedMonthlyOperations = ComputeMonthlyOperations(aggregattedDailyOperations);

                    return new
                    {
                        dailyOperations = aggregattedDailyOperations,
                        monthlyOperations = aggregattedMonthlyOperations
                    };
                });

            var dailyOperations = operations.dailyOperations;
            var monthlyOperations = operations.monthlyOperations;

            DailyOperationsModel = CreateDailyOperationsModel(dailyOperations);
            MonthyOperationsModel = CreateMonthlyOperationsModel(monthlyOperations);
        }

        private static PlotModel CreateDailyOperationsModel(List<OperationSet> operations)
        {
            var operationsModel = new PlotModel();
            operationsModel.Title = "Daily operations";

            var dateTimeAxis = new DateTimeAxis
            {
                IntervalType = DateTimeIntervalType.Auto
            };

            if (operations.Count > 0)
            {
                var lastItem = operations[operations.Count - 1];
                dateTimeAxis.Maximum = DateTimeAxis.ToDouble(lastItem.Day);
                dateTimeAxis.Minimum = DateTimeAxis.ToDouble(lastItem.Day.AddMonths(-2));
            }

            operationsModel.Axes.Add(dateTimeAxis);

            var trackerFormatString = GetDailyTrackerFormatString();

            operationsModel.Series.Add(
                CreateLineSeries(operations, "Balance", OxyColors.Blue, nameof(OperationSet.Balance), trackerFormatString)
            );

            operationsModel.Series.Add(
                CreateLineSeries(operations, "Outcome", OxyColors.Red, nameof(OperationSet.Outcome), trackerFormatString)
            );

            operationsModel.Series.Add(
                CreateLineSeries(operations, "Income", OxyColors.Green, nameof(OperationSet.Income), trackerFormatString)
            );

            return operationsModel;
        }

        private static PlotModel CreateMonthlyOperationsModel(List<OperationSet> operations)
        {
            var operationsModel = new PlotModel();
            operationsModel.Title = "Monthly operations";
            
            var dateTimeAxis = new DateTimeAxis
            {
                IntervalType = DateTimeIntervalType.Months
            };

            if (operations.Count > 0)
            {
                var lastItem = operations[operations.Count - 1];
                var maxDay = lastItem.Day.AddDays(15);
                dateTimeAxis.Maximum = DateTimeAxis.ToDouble(maxDay);
                dateTimeAxis.Minimum = DateTimeAxis.ToDouble(maxDay.AddMonths(-7));
            }

            operationsModel.Axes.Add(dateTimeAxis);

            var gridLines = new LinearAxis { Position = AxisPosition.Left, ExtraGridlines = new double[] { 0 }, ExtraGridlineStyle = LineStyle.Dot };
            operationsModel.Axes.Add(gridLines);

            var trackerFormatString = GetMonthTrackerFormatString();

            operationsModel.Series.Add(
                CreateLinearBarSeries(operations, "Income", OxyColors.LightGreen, nameof(OperationSet.Income), trackerFormatString)
            );

            operationsModel.Series.Add(
                CreateLinearBarSeries(operations, "Outcome", OxyColors.OrangeRed, nameof(OperationSet.NegativeOutcome), trackerFormatString)
            );

            operationsModel.Series.Add(
                CreateLineSeries(operations, "Balance", OxyColors.Orange, nameof(OperationSet.Balance), trackerFormatString)
            );

            var netRevenueSeries = CreateLineSeries(operations, "Net revenue", OxyColors.Black, nameof(OperationSet.NetRevenue), trackerFormatString);

            netRevenueSeries.LabelFormatString = GetNetRevenueLabelFormatString();

            operationsModel.Series.Add(netRevenueSeries);

            return operationsModel;
        }

        private static string GetNetRevenueLabelFormatString()
        {
            if (!Properties.Settings.Default.HideAmounts)
                return "{1:c2}";
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
            string yPropertyName,
            string trackerFormatString)
        {
            var series = CreateDataPointSeries<LineSeries>(operations, title, yPropertyName, trackerFormatString);
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
            string yPropertyName,
            string trackerFormatString)
        {
            var series = CreateDataPointSeries<LinearBarSeries>(operations, title, yPropertyName, trackerFormatString);

            series.FillColor = fillColor;
            series.BarWidth = 30;

            return series;
        }

        private static T CreateDataPointSeries<T>(IEnumerable<OperationSet> operations, string title, string yPropertyName, string trackerFormatString)
            where T : DataPointSeries, new()
        {
            var series = new T
            {
                ItemsSource = operations,
                TrackerFormatString = trackerFormatString,
                Title = title,
                DataFieldX = nameof(OperationSet.Day),
                DataFieldY = yPropertyName
            };

            return series;
        }

        private List<OperationSet> GroupDailyOperations(List<OperationSet> operations)
        {
            var groupedByDay = operations.GroupBy(op => op.Day);

            var flattifiedDailyOperations = groupedByDay.Select(
                    grp =>
                    {
                        var bpd = new OperationSet(grp.Key, grp.Sum(pd => pd.InitialBalance));
                        var grpOperations = grp.SelectMany(p => p.Operations);
                        bpd.AddRange(grpOperations);
                        return bpd;
                    }).ToList();

            return flattifiedDailyOperations;
        }

        private List<OperationSet> ComputeMonthlyOperations(List<OperationSet> dailyOperations)
        {
            var result = new List<OperationSet>();

            var groupedByMonth = dailyOperations.OrderBy(db => db.Day).GroupBy(db => new DateTime(db.Day.Year, db.Day.Month, 1));
            foreach (var grp in groupedByMonth)
            {
                var initialBalance = grp.First().InitialBalance;
                var operations = grp.SelectMany(b => b.Operations);
                var osb = new OperationSet(grp.Key, initialBalance).AddRange(operations);
                result.Add(osb);
            }

            return result;
        }

        private static List<OperationSet> ComputeOperationsPerDay(decimal initialBalance, List<UnifiedAccountOperation> operations)
        {
            operations = operations.OrderBy(o => o.ExecutionDate).ToList();

            var seedBpd = new OperationSet(operations[0].ExecutionDate, initialBalance);

            var result = new List<OperationSet> { seedBpd };

            if (operations.Any())
                operations
                    .Aggregate(
                        seedBpd,
                        (currentBpd, operation) =>
                        {
                            var operationDay = operation.ExecutionDate;

                            while (currentBpd.Day < operationDay.AddDays(-1))
                            {
                                currentBpd = OperationSet.CreateForNextDay(currentBpd);
                                result.Add(currentBpd);
                            }

                            OperationSet nextBpd;

                            if (currentBpd.Day.Equals(operation.ExecutionDate))
                            {
                                nextBpd = currentBpd;
                                currentBpd.Add(operation);
                            }
                            else
                            {
                                nextBpd = OperationSet.CreateForNextDay(currentBpd);
                                nextBpd.Add(operation);
                                result.Add(nextBpd);
                            }

                            return nextBpd;
                        });

            return result;
        }

        public void OnSettingsUpdated()
        {
            var dailyOperationsModel = DailyOperationsModel;
            var monthyOperationsModel = MonthyOperationsModel;
            DailyOperationsModel = null;
            MonthyOperationsModel = null;

            if (dailyOperationsModel != null)
            {
                
                foreach (var series in dailyOperationsModel.Series)
                    series.TrackerFormatString = GetDailyTrackerFormatString();
                DailyOperationsModel = dailyOperationsModel;
            }

            if (monthyOperationsModel != null)
            {
                foreach (var series in monthyOperationsModel.Series)
                    series.TrackerFormatString = GetMonthTrackerFormatString();

                foreach (var netRevenueSerie in monthyOperationsModel.Series.OfType<LineSeries>()
                    .Where(s => s.DataFieldY == nameof(OperationSet.NetRevenue)))
                {
                    netRevenueSerie.LabelFormatString = GetNetRevenueLabelFormatString();
                }

                MonthyOperationsModel = monthyOperationsModel;
            }
        }
    }
}