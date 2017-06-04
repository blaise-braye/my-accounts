using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight;

namespace Operations.Classification.WpfUi.Technical.Controls
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

        public string Header
        {
            get { return _header; }
            set { Set(nameof(Header), ref _header, value); }
        }

        public bool StaysOpenOnClick
        {
            get { return _staysOpenOnClick; }
            set { Set(nameof(StaysOpenOnClick), ref _staysOpenOnClick, value); }
        }

        public bool IsCheckable
        {
            get { return _isCheckable; }
            set { Set(nameof(IsCheckable), ref _isCheckable, value); }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set { Set(nameof(IsChecked), ref _isChecked, value); }
        }

        public ICommand Command
        {
            get { return _command; }
            set { Set(nameof(Command), ref _command, value); }
        }

        public object CommandParameter
        {
            get { return _commandParameter; }
            set { Set(nameof(CommandParameter), ref _commandParameter, value); }
        }

        public List<MenuItemViewModel> Items
        {
            get { return _items; }
            set { Set(nameof(Items), ref _items, value); }
        }
    }
}