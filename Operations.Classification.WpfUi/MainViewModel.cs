using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using GalaSoft.MvvmLight;
using MyAccounts.Business.AccountOperations;
using MyAccounts.Business.AccountOperations.Unified;
using MyAccounts.Business.GeoLoc;
using MyAccounts.Business.Managers.Accounts;
using MyAccounts.Business.Managers.Imports;
using MyAccounts.Business.Managers.Operations;
using MyAccounts.Business.Managers.Persistence;
using MyAccounts.NetStandard.Input;
using Operations.Classification.WpfUi.Managers.Accounts;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Managers.Imports;
using Operations.Classification.WpfUi.Managers.Integration.GererMesComptes;
using Operations.Classification.WpfUi.Managers.Reports;
using Operations.Classification.WpfUi.Managers.Settings;
using Operations.Classification.WpfUi.Managers.Transactions;
using Operations.Classification.WpfUi.Technical.Controls;
using Operations.Classification.WpfUi.Technical.Localization;
using Operations.Classification.WpfUi.Technical.Messages;

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
        private readonly AsyncMessageReceiver _asyncMessageReceiver;

        public MainViewModel()
        {
            // Initialize Data layer
            var fs = new MyAccounts.Business.IO.WindowsFileSystem();
            var workingCopy = new WorkingCopy(fs, Properties.Settings.Default.WorkingFolder);
            var accountsRepository = new AccountsRepository(workingCopy);
            var accountCommandRepository = new AccountCommandRepository(workingCopy);
            var placesRepository = new PlacesRepository();
            var placeProvider = PlaceProvider.Load(placesRepository);
            var placeInfoResolver = new PlaceInfoResolver(placeProvider);
            var operationPatternTransformer = new UnifiedAccountOperationPatternTransformer(placeInfoResolver);
            var operationsRepository = new OperationsRepository(workingCopy, new CsvAccountOperationManager(), operationPatternTransformer);

            // Initialize Managers
            var operationsManager = new OperationsManager(App.CacheManager, operationsRepository);
            var accountsManager = new AccountsManager(App.CacheManager, accountsRepository);
            var importManager = new ImportManager(App.CacheManager, accountCommandRepository, operationsManager);

            // Initialize View Models

            BusyIndicator = new BusyIndicatorViewModel();

            ImportsManagerViewModel = new ImportsManagerViewModel(BusyIndicator, fs, importManager);

            OperationsManagerViewModel = new OperationsManagerViewModel(BusyIndicator, operationsManager, importManager);
            AccountsManagerViewModel = new AccountsManagerViewModel(BusyIndicator, accountsManager, operationsManager, importManager);
            DashboardViewModel = new DashboardViewModel(BusyIndicator);
            GmcManager = new GmcManager(BusyIndicator, App.CacheManager, operationsManager);
            _settingsManager = new SettingsManager(App.CacheManager);

            MessengerInstance.Register<Properties.Settings>(this, OnSettingsUpdated);
            _asyncMessageReceiver = new AsyncMessageReceiver(MessengerInstance);
            _asyncMessageReceiver.RegisterAsync<AccountDataInvalidated>(this, data => Refresh());

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

        public DashboardViewModel DashboardViewModel { get; }

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

        public override void Cleanup()
        {
            base.Cleanup();
            _asyncMessageReceiver.Cleanup();
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
            DashboardViewModel.OnSettingsUpdated();
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

        private async Task Refresh()
        {
            await App.CacheManager.ClearCache();
            await Load();
        }
    }
}