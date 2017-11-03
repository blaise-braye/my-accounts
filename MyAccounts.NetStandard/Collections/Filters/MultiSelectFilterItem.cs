using GalaSoft.MvvmLight;

namespace MyAccounts.NetStandard.Collections.Filters
{
    public class MultiSelectFilterItem : ObservableObject
    {
        private bool _isSelected;

        private string _label;

        private object _data;

        public object Data
        {
            get => _data;
            set { Set(() => Data, ref _data, value); }
        }

        public string Label
        {
            get => _label;
            set { Set(() => Label, ref _label, value); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { Set(() => IsSelected, ref _isSelected, value); }
        }
    }
}