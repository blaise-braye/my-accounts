using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using GalaSoft.MvvmLight;

using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.GeoLoc;
using Operations.Classification.WpfUi.Data;
using Operations.Classification.WpfUi.Managers.Accounts;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Managers.Integration.GererMesComptes;
using Operations.Classification.WpfUi.Managers.Settings;
using Operations.Classification.WpfUi.Managers.Transactions;
using Operations.Classification.WpfUi.Technical.Caching;
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
            var placesRepository = new PlacesRepository();
            var placeProvider = PlaceProvider.Load(placesRepository);
            var placeInfoResolver = new PlaceInfoResolver(placeProvider);
            var operationPatternTransformer = new UnifiedAccountOperationPatternTransformer(placeInfoResolver);

            IFileSystem fs = new FileSystem();
            var workingCopy = new WorkingCopy(fs, Properties.Settings.Default.WorkingFolder);

            var transactionsRepository = new TransactionsRepository(workingCopy, new CsvAccountOperationManager(), operationPatternTransformer);
            var accountsRepository = new AccountsRepository(workingCopy);

            BusyIndicator = new BusyIndicatorViewModel();
            TransactionsManager = new TransactionsManager(BusyIndicator, fs, transactionsRepository);
            AccountsManager = new AccountsManager(BusyIndicator, accountsRepository, TransactionsManager);

            GmcManager = new GmcManager(BusyIndicator);
            _settingsManager = new SettingsManager();
            LoadCommand = new AsyncCommand(Load);
            RefreshCommand = new AsyncCommand(Refresh);
            MessengerInstance.Register<Properties.Settings>(this, OnSettingsUpdated);

            if (IsInDesignMode)
            {
                AccountsManager.Accounts.Add(
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

                AccountsManager.CurrentAccount = AccountsManager.Accounts.First();
            }
        }

        public BusyIndicatorViewModel BusyIndicator { get; }

        public IAsyncCommand LoadCommand { get; }

        public IAsyncCommand RefreshCommand { get; }

        public AccountsManager AccountsManager { get; }

        public GmcManager GmcManager { get; }

        public TransactionsManager TransactionsManager { get; }

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
            AccountsManager.UnifiedOperationsReporter.OnSettingsUpdated();
            Application.Current.UpdateBindingTargets();
        }

        private async Task Load()
        {
            if (AccountsManager.LoadCommand.CanExecute(null))
            {
                await AccountsManager.LoadCommand.ExecuteAsync(null);
                await GmcManager.InitializeAsync(AccountsManager.Accounts);
            }
        }

        private async Task Refresh()
        {
            await CacheProvider.ClearCache();
            await Load();
        }
    }
}