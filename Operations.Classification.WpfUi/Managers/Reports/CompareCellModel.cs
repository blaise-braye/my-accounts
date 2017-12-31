using GalaSoft.MvvmLight;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class CompareCellModel : ObservableObject
    {
        private decimal _outcome;
        private decimal _income;

        private decimal _incomeEvolution;

        private decimal _outcomeEvolution;

        public CompareCellModel(string title)
        {
            Title = title;
        }

        public string Title { get; }

        public decimal Outcome
        {
            get => _outcome;
            set => Set(nameof(Outcome), ref _outcome, value);
        }

        public decimal Income
        {
            get => _income;
            set => Set(nameof(Income), ref _income, value);
        }

        public decimal IncomeEvolution
        {
            get => _incomeEvolution;
            set => Set(nameof(IncomeEvolution), ref _incomeEvolution, value);
        }

        public decimal OutcomeEvolution
        {
            get => _outcomeEvolution;
            set => Set(nameof(OutcomeEvolution), ref _outcomeEvolution, value);
        }
    }
}