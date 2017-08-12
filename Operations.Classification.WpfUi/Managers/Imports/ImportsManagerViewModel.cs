using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.WorkingCopyStorage;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Technical.Caching;
using Operations.Classification.WpfUi.Technical.Input;
using Operations.Classification.WpfUi.Technical.Projections;

namespace Operations.Classification.WpfUi.Managers.Imports
{
    public interface IImportsManagerViewModel
    {
        Task<List<ImportCommand>> GetImports(Guid accountId);
    }

    public class ImportsManagerViewModel : ViewModelBase, IImportsManagerViewModel
    {
        private const string ImportsByAccountIdRoute = "/Imports/{0}";
        private readonly BusyIndicatorViewModel _busyIndicator;
        private readonly IImportManager _importManager;
        private readonly OpenFileDialog _ofd;
        private AccountViewModel _currentAccount;
        private List<ImportCommandModel> _imports;
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

            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);

            _ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "csv files (*.csv)|*.csv|All Files (*.*)|*.*"
            };
        }

        public List<ImportCommandModel> Imports
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

        public AsyncCommand<IList<ImportCommandModel>> BeginDownloadCommand { get; set; }

        public AsyncCommand<IList<ImportCommandModel>> DeleteCommand { get; set; }

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

        private FileBase Fb => Fs.File;

        public async Task<List<ImportCommand>> GetImports(Guid accountId)
        {
            var result = await GetCacheEntry(accountId).GetOrAddAsync(
                () => _importManager.GetAll(accountId));
            return result;
        }

        private static ICacheEntry<List<ImportCommand>> GetCacheEntry(Guid accountId)
        {
            return CacheProvider.GetJSonCacheEntry<List<ImportCommand>>(
                string.Format(
                    ImportsByAccountIdRoute,
                    accountId));
        }

        private void OnAccountViewModelReceived(AccountViewModel currentAccount)
        {
            _currentAccount = currentAccount;
            RefreshImports();
        }

        private void RefreshImports()
        {
            var imports = _currentAccount?.Imports?.AsEnumerable();

            Imports = imports.Project().To<ImportCommandModel>()
                .OrderByDescending(i => i.CreationDate).ToList();
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
                        if ((Fb.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            var dirFiles = Directory.GetFiles(path, "*.csv");
                            foreach (var dirFile in dirFiles)
                            {
                                files.Add(dirFile);
                            }
                        }
                        else if (Fb.Exists(path))
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
                            var sourceKind = AccountOperations.Contracts.SourceKind.Unknwon;

                            if (AutoDetectSourceKind)
                            {
                                sourceKind = CsvAccountOperationManager.DetectFileSourceKindFromFileName(file);
                            }
                            else if (SourceKind.HasValue)
                            {
                                sourceKind = SourceKind.Value;
                            }

                            using (var fs = Fb.OpenRead(file))
                            {
                                var importCommand = new ImportCommand(account.Id, Path.GetFileName(file), sourceKind);
                                if (await _importManager.RequestImportExecution(importCommand, fs))
                                {
                                    await GetCacheEntry(account.Id).DeleteAsync();
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
    }
}