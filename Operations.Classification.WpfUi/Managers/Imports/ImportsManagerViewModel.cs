using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using MyAccounts.Business.AccountOperations;
using MyAccounts.Business.IO;
using MyAccounts.Business.Managers;
using MyAccounts.Business.Managers.Imports;
using MyAccounts.NetStandard.Input;
using MyAccounts.NetStandard.Projections;
using Operations.Classification.WpfUi.Managers.Accounts.Models;

namespace Operations.Classification.WpfUi.Managers.Imports
{
    public class ImportsManagerViewModel : ViewModelBase
    {
        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly IImportManager _importManager;
        private readonly OpenFileDialog _ofd;
        private AccountViewModel _currentAccount;

        private ImportEditorViewModel _editor;
        private List<ImportCommandGridModel> _imports;

        public ImportsManagerViewModel(BusyIndicatorViewModel busyIndicator, IFileSystem fileSystem, IImportManager importManager)
        {
            _busyIndicator = busyIndicator;
            _importManager = importManager;
            Fs = fileSystem;

            ReplayImportsCommand = new RelayCommand(ReplayImports);
            BeginImportCommand = new RelayCommand(BeginNewImport);
            BeginEditImportsCommand = new AsyncCommand(BeginEditImport);
            CommitEditCommand = new AsyncCommand(CommitEdit);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SelectFilesToImportCommand = new RelayCommand(SelectFilesToImport);
            DeleteImportsCommand = new AsyncCommand(DeleteImports);
            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);
            SelectedImports = new ObservableCollection<ImportCommandGridModel>();

            _ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "csv files (*.csv)|*.csv|All Files (*.*)|*.*"
            };
        }

        public List<ImportCommandGridModel> Imports
        {
            get => _imports;
            private set
            {
                if (Set(nameof(Imports), ref _imports, value))
                {
                    SelectedImports.Clear();
                }
            }
        }

        public ObservableCollection<ImportCommandGridModel> SelectedImports { get; }

        public bool IsEditing => Editor != null;

        public bool IsNew => Editor?.IsNew == true;

        public RelayCommand BeginImportCommand { get; }

        public RelayCommand SelectFilesToImportCommand { get; }

        public AsyncCommand BeginEditImportsCommand { get; }

        public RelayCommand ReplayImportsCommand { get; }

        public AsyncCommand BeginDownloadCommand { get; set; }

        public AsyncCommand CommitEditCommand { get; }

        public RelayCommand CancelEditCommand { get; }

        public AsyncCommand DeleteImportsCommand { get; }

        public ImportEditorViewModel Editor
        {
            get => _editor;
            set
            {
                if (Set(() => Editor, ref _editor, value))
                {
                    RaisePropertyChanged(() => IsEditing);
                    RaisePropertyChanged(() => IsNew);
                }
            }
        }

        private IFileSystem Fs { get; }

        private void OnAccountViewModelReceived(AccountViewModel currentAccount)
        {
            _currentAccount = currentAccount;
            RefreshImports();
        }

        private void RefreshImports()
        {
            Imports = _currentAccount?.Imports;
        }

        private void BeginNewImport()
        {
            if (IsEditing)
            {
                throw new InvalidOperationException("An import is already in progress");
            }

            var editor = new ImportEditorViewModel();
            SetupEditorCommands(editor);
            Editor = editor;
        }

        private async Task BeginEditImport()
        {
            if (IsEditing)
            {
                throw new InvalidOperationException("An import found to be in progress");
            }

            var selection = SelectedImports.Select(i => i.Id).ToArray();
            if (selection.Length > 0)
            {
                var importCommands = await _importManager.Get(_currentAccount.Id, selection);
                var editor = importCommands[0].Map().To<ImportEditorViewModel>();
                editor.Ids = selection;

                editor.BeginEdit();

                if (importCommands.Count > 1)
                {
                    editor.SourceName = string.Join(";", SelectedImports.Select(i => i.SourceName));
                    editor.IsSourceNameReadOnly = true;
                    foreach (var importCommand in importCommands)
                    {
                        editor.UpdateMixedState(importCommand, nameof(importCommand.SourceName));
                    }
                }

                SetupEditorCommands(editor);
                Editor = editor;
            }
        }

        private async Task CommitEdit()
        {
            if (!IsEditing)
            {
                throw new InvalidOperationException("no edit in progress, nothing to commit");
            }

            bool datachanged = false;
            if (IsNew)
            {
                using (_busyIndicator.EncapsulateActiveJobDescription(this, "finalizing import"))
                {
                    datachanged = await SaveNewImport();
                }
            }
            else if (Editor.IsDirty())
            {
                using (_busyIndicator.EncapsulateActiveJobDescription(this, "committing changes"))
                {
                    datachanged = await SaveExistingImport();
                }
            }

            Editor.EndEdit();
            Editor = null;

            if (datachanged)
            {
                MessengerInstance.Send(new AccountImportDataChanged(_currentAccount.Id));
            }
        }

        private void ReplayImports()
        {
            MessengerInstance.Send(new AccountImportDataChanged(_currentAccount.Id));
        }

        private async Task<bool> SaveExistingImport()
        {
            if (Editor.IsNew)
            {
                throw new InvalidOperationException("import id must be known");
            }

            var importId = Editor.Ids;
            var importCommands = await _importManager.Get(_currentAccount.Id, importId);
            foreach (var importCommand in importCommands)
            {
                var toSkip = Editor.IsSourceNameReadOnly ? new[] { nameof(ImportCommand.SourceName) } : new string[0];
                Editor.FillFromDirtyProperties(importCommand, toSkip);
            }

            var saved = await _importManager.Replace(importCommands);
            return saved;
        }

        private async Task<bool> SaveNewImport()
        {
            var paths = Editor.FilePaths.Split(',');
            var files = new HashSet<string>();
            foreach (var path in paths)
            {
                if (Fs.IsDirectoy(path))
                {
                    var dirFiles = Fs.DirectoryGetFiles(path, "*.csv");
                    foreach (var dirFile in dirFiles)
                    {
                        files.Add(dirFile);
                    }
                }
                else if (Fs.FileExists(path))
                {
                    files.Add(path);
                }
            }

            var account = _currentAccount;
            var someImportSucceeded = false;
            var sourceKind = Editor.SourceKind;
            if (account != null)
            {
                foreach (var file in files)
                {
                    using (var fs = Fs.FileOpenRead(file))
                    {
                        var importCommand = new ImportCommand(account.Id, Path.GetFileName(file), sourceKind);
                        Editor.FillFromDirtyProperties(importCommand);
                        if (await _importManager.RequestImportExecution(importCommand, fs))
                        {
                            someImportSucceeded = true;
                        }
                    }
                }
            }

            return someImportSucceeded;
        }

        private void CancelEdit()
        {
            if (!IsEditing)
            {
                throw new InvalidOperationException("no edit in progress, nothing to cancel");
            }

            Editor.CancelEdit();
            Editor = null;
        }

        private async Task DeleteImports()
        {
            var selection = SelectedImports;
            if (selection.Count > 0)
            {
                var idSet = new HashSet<Guid>(selection.Select(a => a.Id));
                await _importManager.DeleteImports(_currentAccount.Id, idSet);

                var imports = _currentAccount.Imports.Where(i => !idSet.Contains(i.Id)).ToList();
                _currentAccount.Imports = imports;

                RefreshImports();
            }
        }

        private void SelectFilesToImport()
        {
            if (string.IsNullOrEmpty(_ofd.InitialDirectory))
            {
                _ofd.InitialDirectory = Properties.Settings.Default.WorkingFolder;
            }

            if (_ofd.ShowDialog() == true)
            {
                Editor.FilePaths = string.Join(",", _ofd.FileNames);
            }
        }

        private void SetupEditorCommands(ImportEditorViewModel editor)
        {
            editor.CommitCommand = new AsyncCommand(CommitEdit);
            editor.ResetDefaultSourceKindMetadataCommand = new RelayCommand(ResetDefaultSourceKindMetadataCommand);

            if (editor.IsNew)
            {
                editor.SelectFilesToImportCommand = SelectFilesToImportCommand;
            }
        }

        private void ResetDefaultSourceKindMetadataCommand()
        {
            var sourceKind = Editor.SourceKind;
            var fsmd = FileStructureMetadataFactory.CreateDefault(sourceKind);
            fsmd.Map().To(Editor);
        }
    }
}