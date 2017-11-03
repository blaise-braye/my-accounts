using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight;

namespace MyAccounts.NetStandard.Controls
{
    public class MenuItemViewModel : ObservableObject
    {
        private ICommand _command;

        private object _commandParameter;

        private string _header;

        private bool _isCheckable;

        private bool _isChecked;

        private List<MenuItemViewModel> _items;

        private bool _staysOpenOnClick;

        private object _tag;

        public string Header
        {
            get => _header;
            set => Set(nameof(Header), ref _header, value);
        }

        public bool StaysOpenOnClick
        {
            get => _staysOpenOnClick;
            set => Set(nameof(StaysOpenOnClick), ref _staysOpenOnClick, value);
        }

        public bool IsCheckable
        {
            get => _isCheckable;
            set => Set(nameof(IsCheckable), ref _isCheckable, value);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => Set(nameof(IsChecked), ref _isChecked, value);
        }

        public ICommand Command
        {
            get => _command;
            set => Set(nameof(Command), ref _command, value);
        }

        public object CommandParameter
        {
            get => _commandParameter;
            set => Set(nameof(CommandParameter), ref _commandParameter, value);
        }

        public List<MenuItemViewModel> Items
        {
            get => _items;
            set => Set(nameof(Items), ref _items, value);
        }
        
        public object Tag
        {
            get => _tag;
            set { Set(() => Tag, ref _tag, value); }
        }

        public void Uncheck()
        {
            IsChecked = false;
            Command.Execute(CommandParameter);
        }

        public void Check()
        {
            IsChecked = true;
            Command.Execute(CommandParameter);
        }
    }
}