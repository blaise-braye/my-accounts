using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.WpfUi.Data;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Properties;
using Operations.Classification.WpfUi.Technical.Input;

namespace Operations.Classification.WpfUi.Managers.Transactions
{
    public class TransactionsManager : ViewModelBase
    {
        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly OpenFileDialog _ofd;
        private readonly ITransactionsRepository _transactionsRepository;

        private bool _autoDetectSourceKind;
        private AccountViewModel _currentAccount;

        private string _filePaths;
        private bool _isImporting;
        private List<UnifiedAccountOperation> _operations;
        private SourceKind? _sourceKind;

        public TransactionsManager(BusyIndicatorViewModel busyIndicator, ITransactionsRepository transactionsRepository)
        {
            _busyIndicator = busyIndicator;
            _transactionsRepository = transactionsRepository;
            BeginImportCommand = new RelayCommand(BeginImport);
            CommitImportCommand = new AsyncCommand(CommitImport);

            _ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "csv files (*.csv)|*.csv|All Files (*.*)|*.*",
                InitialDirectory = Settings.Default.WorkingFolder
            };
            SelectFilesToImportCommand = new RelayCommand(SelectFilesToImport);
            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);
        }

        public RelayCommand CommitImportCommand { get; }

        public RelayCommand BeginImportCommand { get; }

        public List<UnifiedAccountOperation> Operations
        {
            get { return _operations; }
            private set { Set(nameof(Operations), ref _operations, value); }
        }

        public bool IsImporting
        {
            get { return _isImporting; }
            set { Set(nameof(IsImporting), ref _isImporting, value); }
        }

        public string FilePaths
        {
            get { return _filePaths; }
            set { Set(nameof(FilePaths), ref _filePaths, value); }
        }

        public SourceKind? SourceKind
        {
            get { return _sourceKind; }
            set { Set(nameof(SourceKind), ref _sourceKind, value); }
        }

        public IEnumerable<SourceKind> SourceKinds => Enum.GetValues(typeof(SourceKind)).Cast<SourceKind>();

        public AccountViewModel CurrentAccount
        {
            get { return _currentAccount; }
            set { Set(nameof(CurrentAccount), ref _currentAccount, value); }
        }

        public ICommand SelectFilesToImportCommand { get; }

        public bool AutoDetectSourceKind
        {
            get { return _autoDetectSourceKind; }
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
                        if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            var dirFiles = Directory.GetFiles(path, "*.csv");
                            foreach (var dirFile in dirFiles)
                                files.Add(dirFile);
                        }
                        else if (File.Exists(path))
                        {
                            files.Add(path);
                        }

                    var account = CurrentAccount;
                    var someImportSucceeded = false;
                    if (account != null)
                    {
                        foreach (var file in files)
                        {
                            var sourceKind = AccountOperations.Contracts.SourceKind.Unknwon;

                            if (AutoDetectSourceKind)
                            {
                                sourceKind = CsvAccountOperationManager.DetectFileSourceKindFromFileName(file);
                            }
                            else if (SourceKind.HasValue)
                            {
                                sourceKind = SourceKind.Value;
                            }

                            using (var fs = File.OpenRead(file))
                            {
                                if (await _transactionsRepository.Import(account.Name, fs, sourceKind))
                                {
                                    someImportSucceeded = true;
                                }
                            }
                        }
                    }

                    if (someImportSucceeded)
                    {
                        var operations = await _transactionsRepository.GetTransformedUnifiedOperations(CurrentAccount.Name);
                        CurrentAccount.Operations = operations;
                        OnAccountViewModelReceived(CurrentAccount);
                    }
                }

                IsImporting = false;
            }
        }

        private void OnAccountViewModelReceived(AccountViewModel currentAccount)
        {
            CurrentAccount = currentAccount;
            Operations = currentAccount?.Operations;
        }

        private void SelectFilesToImport()
        {
            if (_ofd.ShowDialog() == true)
            {
                FilePaths = string.Join(",", _ofd.FileNames);
            }
        }
    }
}