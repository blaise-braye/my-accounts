using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using GalaSoft.MvvmLight;
using MyAccounts.Business.AccountOperations;
using MyAccounts.Business.AccountOperations.Unified;
using MyAccounts.Business.Caching;
using MyAccounts.Business.GeoLoc;
using MyAccounts.Business.Managers.Accounts;
using MyAccounts.Business.Managers.Imports;
using MyAccounts.Business.Managers.Operations;
using MyAccounts.Business.Managers.Persistence;
using Operations.Classification.WpfUi.Managers.Accounts;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Managers.Imports;
using Operations.Classification.WpfUi.Managers.Integration.GererMesComptes;
using Operations.Classification.WpfUi.Managers.Settings;
using Operations.Classification.WpfUi.Managers.Transactions;
using Operations.Classification.WpfUi.Technical.Controls;
using Operations.Classification.WpfUi.Technical.Input;
using Operations.Classification.WpfUi.Technical.Localization;

namespace Operations.Classification.WpfUi
{
    /// <summary>
    ///     This class contains properties that the main View can data bind to.
    ///     <para>
    ///         Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    ///     </para>
    ///     <para>
    ///         You can also use Blend to data bind with the tool's support.
    ///     </para>
    ///     <para>
    ///         See http://www.galasoft.ch/mvvm
    ///     </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly SettingsManager _settingsManager;

        public MainViewModel()
        {
            // Initialize Data layer
            var fs = new MyAccounts.Business.IO.FileSystem();
            var workingCopy = new WorkingCopy(fs, Properties.Settings.Default.WorkingFolder);
            var accountsRepository = new AccountsRepository(workingCopy);
            var accountCommandRepository = new AccountCommandRepository(workingCopy);
            var placesRepository = new PlacesRepository();
            var placeProvider = PlaceProvider.Load(placesRepository);
            var placeInfoResolver = new PlaceInfoResolver(placeProvider);
            var operationPatternTransformer = new UnifiedAccountOperationPatternTransformer(placeInfoResolver);
            var operationsRepository = new OperationsRepository(workingCopy, new CsvAccountOperationManager(), operationPatternTransformer);

            // Initialize Managers
            var importManager = new ImportManager(accountCommandRepository, operationsRepository);
            var operationsManager = new OperationsManager(operationsRepository);
            var accountsManager = new AccountsManager(accountsRepository);

            // Initialize View Models
            BusyIndicator = new BusyIndicatorViewModel();

            ImportsManagerViewModel = new ImportsManagerViewModel(BusyIndicator, fs, importManager);

            OperationsManagerViewModel = new OperationsManagerViewModel(BusyIndicator, operationsManager, importManager);
            AccountsManagerViewModel = new AccountsManagerViewModel(BusyIndicator, accountsManager, operationsManager, importManager);
            GmcManager = new GmcManager(BusyIndicator);
            _settingsManager = new SettingsManager();

            MessengerInstance.Register<Properties.Settings>(this, OnSettingsUpdated);
            MessengerInstance.Register<AccountDataInvalidated>(this, OnCurrentAccountDataInvalidated);

            if (!IsInDesignMode)
            {
                LoadCommand = new AsyncCommand(Load);
                RefreshCommand = new AsyncCommand(Refresh);
            }
            else
            {
                AccountsManagerViewModel.Accounts.Add(
                    new AccountViewModel
                    {
                        Name = "Blaise CC",
                        Status =
                        {
                            Operations = 7,
                            Balance = 2541.7345M,
                            LastImportedOperation = "2012-0001"
                        }
                    });

                AccountsManagerViewModel.CurrentAccount = AccountsManagerViewModel.Accounts.First();
            }
        }

        public BusyIndicatorViewModel BusyIndicator { get; }

        public IAsyncCommand LoadCommand { get; }

        public IAsyncCommand RefreshCommand { get; }

        public AccountsManagerViewModel AccountsManagerViewModel { get; }

        public GmcManager GmcManager { get; }

        public ImportsManagerViewModel ImportsManagerViewModel { get; }

        public OperationsManagerViewModel OperationsManagerViewModel { get; }

        public SettingsManager SettingsManager => _settingsManager;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _settingsManager.Dispose();
            }
        }

        private void OnSettingsUpdated(Properties.Settings obj)
        {
            ApplicationCulture.ResetCulture();
            AccountsManagerViewModel.UnifiedOperationsReporter.OnSettingsUpdated();
            Application.Current.UpdateBindingTargets();
        }

        private async Task Load()
        {
            if (AccountsManagerViewModel.LoadCommand.CanExecute(null))
            {
                await AccountsManagerViewModel.LoadCommand.ExecuteAsync(null);
                await GmcManager.InitializeAsync(AccountsManagerViewModel.Accounts);
            }
        }

        private void OnCurrentAccountDataInvalidated(AccountDataInvalidated obj)
        {
            RefreshCommand.Execute(null);
        }

        private async Task Refresh()
        {
            await OperationsManagerViewModel.ReplayImports(AccountsManagerViewModel.Accounts);
            await CacheProvider.ClearCache();
            await Load();
        }
    }
}