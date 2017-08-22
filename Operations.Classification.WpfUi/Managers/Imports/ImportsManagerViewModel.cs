using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using MyAccounts.Business.AccountOperations;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.IO;
using MyAccounts.Business.Managers;
using MyAccounts.Business.Managers.Imports;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Technical.Input;

namespace Operations.Classification.WpfUi.Managers.Imports
{
    public class ImportsManagerViewModel : ViewModelBase
    {
        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly IImportManager _importManager;
        private readonly OpenFileDialog _ofd;
        private AccountViewModel _currentAccount;
        private List<ImportCommandGridModel> _imports;
        private bool _isImporting;
        private bool _autoDetectSourceKind;
        private string _filePaths;
        private SourceKind? _sourceKind;

        public ImportsManagerViewModel(BusyIndicatorViewModel busyIndicator, IFileSystem fileSystem, IImportManager importManager)
        {
            _busyIndicator = busyIndicator;
            _importManager = importManager;
            Fs = fileSystem;

            BeginImportCommand = new RelayCommand(BeginImport);
            CommitImportCommand = new AsyncCommand(CommitImport);
            SelectFilesToImportCommand = new RelayCommand(SelectFilesToImport);
            DeleteImportsCommand = new AsyncCommand<IEnumerable>(DeleteImports);
            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);

            _ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "csv files (*.csv)|*.csv|All Files (*.*)|*.*"
            };
        }

        public List<ImportCommandGridModel> Imports
        {
            get => _imports;
            private set => Set(nameof(Imports), ref _imports, value);
        }

        public bool IsImporting
        {
            get => _isImporting;
            set => Set(nameof(IsImporting), ref _isImporting, value);
        }

        public RelayCommand BeginImportCommand { get; set; }

        public RelayCommand CommitImportCommand { get; }

        public RelayCommand SelectFilesToImportCommand { get; }

        public RelayCommand BeginEditCommand { get; set; }

        public AsyncCommand<IList<ImportCommandGridModel>> BeginDownloadCommand { get; set; }

        public AsyncCommand<IEnumerable> DeleteImportsCommand { get; }

        public string FilePaths
        {
            get => _filePaths;
            set => Set(nameof(FilePaths), ref _filePaths, value);
        }

        public IEnumerable<SourceKind> SourceKinds => Enum.GetValues(typeof(SourceKind)).Cast<SourceKind>();

        public bool AutoDetectSourceKind
        {
            get => _autoDetectSourceKind;
            set
            {
                if (Set(nameof(AutoDetectSourceKind), ref _autoDetectSourceKind, value))
                {
                    if (value)
                    {
                        SourceKind = null;
                    }
                }
            }
        }

        public SourceKind? SourceKind
        {
            get => _sourceKind;
            set => Set(nameof(SourceKind), ref _sourceKind, value);
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

        private void BeginImport()
        {
            IsImporting = true;
        }

        private async Task CommitImport()
        {
            if (IsImporting)
            {
                using (_busyIndicator.EncapsulateActiveJobDescription(this, "finalizing import"))
                {
                    var paths = FilePaths.Split(',');
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
                    if (account != null)
                    {
                        foreach (var file in files)
                        {
                            var sourceKind = MyAccounts.Business.AccountOperations.Contracts.SourceKind.Unknwon;

                            if (AutoDetectSourceKind)
                            {
                                sourceKind = CsvAccountOperationManager.DetectFileSourceKindFromFileName(file);
                            }
                            else if (SourceKind.HasValue)
                            {
                                sourceKind = SourceKind.Value;
                            }

                            using (var fs = Fs.FileOpenRead(file))
                            {
                                var importCommand = new ImportCommand(account.Id, Path.GetFileName(file), sourceKind);
                                if (await _importManager.RequestImportExecution(importCommand, fs))
                                {
                                    someImportSucceeded = true;
                                }
                            }
                        }
                    }

                    if (someImportSucceeded)
                    {
                        MessengerInstance.Send(new AccountDataInvalidated());
                        OnAccountViewModelReceived(_currentAccount);
                    }
                }

                IsImporting = false;
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
                FilePaths = string.Join(",", _ofd.FileNames);
            }
        }

        private async Task DeleteImports(IEnumerable arg)
        {
            var lst = arg?.OfType<ImportCommandGridModel>().ToList();
            if (lst?.Count > 0)
            {
                var idSet = new HashSet<Guid>(lst.Select(a => a.Id));
                await _importManager.DeleteImports(lst[0].AccountId, idSet);

                var imports = _currentAccount.Imports.Where(i => !idSet.Contains(i.Id)).ToList();
                _currentAccount.Imports = imports;

                RefreshImports();
            }
        }
    }
}