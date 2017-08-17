using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Operations.Classification.Caching;
using Operations.Classification.WpfUi.Technical.Input;
using Operations.Classification.WpfUi.Technical.Projections;

namespace Operations.Classification.WpfUi.Managers.Settings
{
    public class SettingsManager : ViewModelBase, IDisposable
    {
        private readonly RelayCommand[] _commands;
        private readonly FolderBrowserDialog _fbd;
        private bool _isEditing;

        private SettingsModel _settings;

        public SettingsManager()
        {
            BeginEditCommand = new RelayCommand(BeginEdit, () => !IsEditing);
            CommitEditCommand = new RelayCommand(CommitEdit, () => IsEditing);
            ResetDefaultCommand = new AsyncCommand(ResetDefault, () => IsEditing);
            CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditing);

            _fbd = new FolderBrowserDialog();
            SelectWorkingFolderCommand = new RelayCommand(SelectWorkingFolder);
            _commands = new[] { BeginEditCommand, CommitEditCommand, ResetDefaultCommand, CancelEditCommand };
            Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures).ToList();
        }

        public bool IsEditing
        {
            get => _isEditing;
            private set
            {
                if (Set(nameof(IsEditing), ref _isEditing, value))
                {
                    if (value)
                    {
                        BeginEdit();
                    }

                    InvalidateCommands();
                }
            }
        }

        public SettingsModel Settings
        {
            get => _settings;
            private set => Set(nameof(Settings), ref _settings, value);
        }

        public List<CultureInfo> Cultures { get; }

        public RelayCommand ResetDefaultCommand { get; }

        public RelayCommand BeginEditCommand { get; }

        public RelayCommand CommitEditCommand { get; }

        public RelayCommand CancelEditCommand { get; }

        public RelayCommand SelectWorkingFolderCommand { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fbd.Dispose();
            }
        }

        private void SelectWorkingFolder()
        {
            _fbd.SelectedPath = Settings.WorkingFolder;
            if (_fbd.ShowDialog() == DialogResult.OK)
            {
                Settings.WorkingFolder = _fbd.SelectedPath;
            }
        }

        private void InvalidateCommands()
        {
            foreach (var command in _commands)
            {
                command.RaiseCanExecuteChanged();
            }
        }

        private void BeginEdit()
        {
            IsEditing = true;
            Settings = Properties.Settings.Default.Map().To<SettingsModel>();
        }

        private void CommitEdit()
        {
            Settings.Map().To(Properties.Settings.Default);
            Properties.Settings.Default.Save();
            OnSettingsPersisted();
            IsEditing = false;
        }

        private void CancelEdit()
        {
            Settings = null;
            IsEditing = false;
        }

        private async Task ResetDefault()
        {
            Properties.Settings.Default.Reset();
            await CacheProvider.ClearCache();
            OnSettingsPersisted();
            Settings = Properties.Settings.Default.Map().To<SettingsModel>();
        }

        private void OnSettingsPersisted()
        {
            MessengerInstance.Send(Properties.Settings.Default);
        }
    }
}