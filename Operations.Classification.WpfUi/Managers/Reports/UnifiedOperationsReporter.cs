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
        private PlotModel _dailyBalancesModel;

        private PlotModel _monthyBalancesModel;

        public PlotModel DailyBalancesModel
        {
            get => _dailyBalancesModel;
            set => Set(nameof(DailyBalancesModel), ref _dailyBalancesModel, value);
        }

        public PlotModel MonthyBalancesModel
        {
            get => _monthyBalancesModel;
            private set => Set(nameof(MonthyBalancesModel), ref _monthyBalancesModel, value);
        }

        public async Task UpdateAccountSelection(List<AccountViewModel> selection)
        {
            var balances = await Task.Run(
                () =>
                {
                    var mixedAccountOperationBalances = selection.SelectMany(account => ComputeBalancesPerDay(account.InitialBalance, account.Operations))
                        .OrderBy(op => op.Day).ToList();

                    var aggregattedDailyBalances = GroupDailyBalances(mixedAccountOperationBalances);
                    var aggregattedMonthlyBalances = ComputeMonthlyBalances(aggregattedDailyBalances);

                    return new
                    {
                        dailyBalances = aggregattedDailyBalances,
                        monthlyBalances = aggregattedMonthlyBalances
                    };
                });

            var dailyBalances = balances.dailyBalances;
            var monthlyBalances = balances.monthlyBalances;

            DailyBalancesModel = CreateDailyBalancesModel(dailyBalances);
            MonthyBalancesModel = CreateMonthlyBalancesModel(monthlyBalances);
        }

        private static PlotModel CreateDailyBalancesModel(List<OperationSetBalance> balances)
        {
            var balancesModel = new PlotModel();

            var dateTimeAxis = new DateTimeAxis
            {
                IntervalType = DateTimeIntervalType.Auto
            };

            if (balances.Count > 0)
            {
                var lastItem = balances[balances.Count - 1];
                dateTimeAxis.Maximum = DateTimeAxis.ToDouble(lastItem.Day);
                dateTimeAxis.Minimum = DateTimeAxis.ToDouble(lastItem.Day.AddMonths(-2));
            }

            balancesModel.Axes.Add(dateTimeAxis);

            var trackerFormatString = GetMonthTrackerFormatString();

            balancesModel.Series.Add(
                CreateLineSeries(balances, "Balance", OxyColors.Blue, nameof(OperationSetBalance.Balance), trackerFormatString)
            );

            return balancesModel;
        }

        private static PlotModel CreateMonthlyBalancesModel(List<OperationSetBalance> balances)
        {
            var balancesModel = new PlotModel();

            var dateTimeAxis = new DateTimeAxis
            {
                IntervalType = DateTimeIntervalType.Months
            };

            if (balances.Count > 0)
            {
                var lastItem = balances[balances.Count - 1];
                var maxDay = lastItem.Day.AddDays(15);
                dateTimeAxis.Maximum = DateTimeAxis.ToDouble(maxDay);
                dateTimeAxis.Minimum = DateTimeAxis.ToDouble(maxDay.AddMonths(-7));
            }

            balancesModel.Axes.Add(dateTimeAxis);

            var gridLines = new LinearAxis { Position = AxisPosition.Left, ExtraGridlines = new double[] { 0 }, ExtraGridlineStyle = LineStyle.Dot };
            balancesModel.Axes.Add(gridLines);

            var trackerFormatString = GetDailyTrackerFormatString();

            balancesModel.Series.Add(
                CreateLinearBarSeries(balances, "Income", OxyColors.LightGreen, nameof(OperationSetBalance.Income), trackerFormatString)
            );

            balancesModel.Series.Add(
                CreateLinearBarSeries(balances, "Outcome", OxyColors.OrangeRed, nameof(OperationSetBalance.NegativeOutcome), trackerFormatString)
            );

            balancesModel.Series.Add(
                CreateLineSeries(balances, "Balance", OxyColors.Orange, nameof(OperationSetBalance.Balance), trackerFormatString)
            );

            var netRevenueSeries = CreateLineSeries(balances, "Net revenue", OxyColors.Black, nameof(OperationSetBalance.NetRevenue), trackerFormatString);

            netRevenueSeries.LabelFormatString = GetNetRevenueLabelFormatString();

            balancesModel.Series.Add(netRevenueSeries);

            return balancesModel;
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
            IEnumerable<OperationSetBalance> balances,
            string title,
            OxyColor color,
            string yPropertyName,
            string trackerFormatString)
        {
            var series = CreateDataPointSeries<LineSeries>(balances, title, yPropertyName, trackerFormatString);
            series.Color = color;
            series.StrokeThickness = 2;
            series.MarkerSize = 4;
            series.MarkerStroke = color;
            series.MarkerType = MarkerType.Diamond;
            series.MarkerFill = OxyColors.WhiteSmoke;
            return series;
        }


        private static LinearBarSeries CreateLinearBarSeries(
            IEnumerable<OperationSetBalance> balances,
            string title,
            OxyColor fillColor,
            string yPropertyName,
            string trackerFormatString)
        {
            var series = CreateDataPointSeries<LinearBarSeries>(balances, title, yPropertyName, trackerFormatString);

            series.FillColor = fillColor;
            series.BarWidth = 30;

            return series;
        }

        private static T CreateDataPointSeries<T>(IEnumerable<OperationSetBalance> balances, string title, string yPropertyName, string trackerFormatString)
            where T : DataPointSeries, new()
        {
            var series = new T
            {
                ItemsSource = balances,
                TrackerFormatString = trackerFormatString,
                Title = title,
                DataFieldX = nameof(OperationSetBalance.Day),
                DataFieldY = yPropertyName
            };

            return series;
        }

        private List<OperationSetBalance> GroupDailyBalances(List<OperationSetBalance> balances)
        {
            var groupedByDay = balances.GroupBy(op => op.Day);

            var flattifiedDailyBalances = groupedByDay.Select(OperationSetBalance.Flattify)
                .Where(b => b.InitialBalance != b.Balance).ToList();

            return flattifiedDailyBalances;
        }

        private List<OperationSetBalance> ComputeMonthlyBalances(List<OperationSetBalance> dailyBalances)
        {
            var result = new List<OperationSetBalance>();

            var groupedByMonth = dailyBalances.OrderBy(db => db.Day).GroupBy(db => new DateTime(db.Day.Year, db.Day.Month, 1));
            foreach (var grp in groupedByMonth)
            {
                var initialBalance = grp.Min(b => b.InitialBalance);
                var operations = grp.SelectMany(b => b.Operations);
                var osb = new OperationSetBalance(grp.Key, initialBalance).AddRange(operations);
                result.Add(osb);
            }

            return result;
        }

        private static List<OperationSetBalance> ComputeBalancesPerDay(decimal initialBalance, List<UnifiedAccountOperation> operations)
        {
            operations = operations.OrderBy(o => o.ValueDate).ToList();

            var seedBpd = new OperationSetBalance(operations[0].ValueDate, initialBalance);

            var result = new List<OperationSetBalance> { seedBpd };

            if (operations.Any())
                operations
                    .Aggregate(
                        seedBpd,
                        (currentBpd, operation) =>
                        {
                            var operationDay = operation.ValueDate;

                            while (currentBpd.Day < operationDay.AddDays(-1))
                            {
                                currentBpd = OperationSetBalance.CreateForNextDay(currentBpd);
                                result.Add(currentBpd);
                            }

                            OperationSetBalance nextBpd;

                            if (currentBpd.Day.Equals(operation.ValueDate))
                            {
                                nextBpd = currentBpd;
                                currentBpd.Add(operation);
                            }
                            else
                            {
                                nextBpd = OperationSetBalance.CreateForNextDay(currentBpd);
                                nextBpd.Add(operation);
                                result.Add(nextBpd);
                            }

                            return nextBpd;
                        });

            return result;
        }

        public void OnSettingsUpdated()
        {
            var dailyBalancesModel = DailyBalancesModel;
            var monthyBalancesModel = MonthyBalancesModel;
            DailyBalancesModel = null;
            MonthyBalancesModel = null;

            if (dailyBalancesModel != null)
            {
                
                foreach (var series in dailyBalancesModel.Series)
                    series.TrackerFormatString = GetDailyTrackerFormatString();
                DailyBalancesModel = dailyBalancesModel;
            }

            if (monthyBalancesModel != null)
            {
                foreach (var series in monthyBalancesModel.Series)
                    series.TrackerFormatString = GetMonthTrackerFormatString();

                foreach (var netRevenueSerie in monthyBalancesModel.Series.OfType<LineSeries>()
                    .Where(s => s.DataFieldY == nameof(OperationSetBalance.NetRevenue)))
                {
                    netRevenueSerie.LabelFormatString = GetNetRevenueLabelFormatString();
                }

                MonthyBalancesModel = monthyBalancesModel;
            }
        }
    }
}