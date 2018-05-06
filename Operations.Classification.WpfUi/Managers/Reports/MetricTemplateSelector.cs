using System.Windows;
using System.Windows.Controls;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class MetricTemplateSelector : DataTemplateSelector
    {
        public string Focus { get; set; }

        public DataTemplate CompareCellModelBalanceTemplate { get; set; }

        public DataTemplate CompareCellModelOutcomeTemplate { get; set; }

        public DataTemplate CompareCellModelOutcomeEvolutionTemplate { get; set; }

        public DataTemplate CompareCellModelIncomeTemplate { get; set; }

        public DataTemplate CompareCellModelIncomeEvolutionTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (Focus)
            {
                case nameof(CompareCellModel.Balance):
                    return CompareCellModelBalanceTemplate;
                case nameof(CompareCellModel.Income):
                    return CompareCellModelIncomeTemplate;
                case nameof(CompareCellModel.Outcome):
                    return CompareCellModelOutcomeTemplate;
                case nameof(CompareCellModel.IncomeEvolution):
                    return CompareCellModelIncomeEvolutionTemplate;
                case nameof(CompareCellModel.OutcomeEvolution):
                    return CompareCellModelOutcomeEvolutionTemplate;
                default:
                    return base.SelectTemplate(item, container);
            }
        }
    }
}