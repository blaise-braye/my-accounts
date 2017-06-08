using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Operations.Classification.AccountOperations;
using Operations.Classification.WpfUi.Caching;
using Operations.Classification.WpfUi.Data;
using Operations.Classification.WpfUi.Managers.Accounts;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Managers.Integration.GererMesComptes;
using Operations.Classification.WpfUi.Managers.Transactions;
using Operations.Classification.WpfUi.Properties;
using Operations.Classification.WpfUi.Technical.Input;

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
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        ///     Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            var workingCopy = new WorkingCopy(Settings.Default.WorkingFolder);

            var accountsRepository = new AccountsRepository(workingCopy);
            var transactionsRepository = new TransactionsRepository(workingCopy, new CsvAccountOperationManager());
            var cachedTransactionsRepository = new CachedTransactionsRepository(transactionsRepository);
            BusyIndicator = new BusyIndicatorViewModel();
            AccountsManager = new AccountsManager(BusyIndicator, accountsRepository, cachedTransactionsRepository);
            TransactionsManager = new TransactionsManager(BusyIndicator, cachedTransactionsRepository);
            GmgManager = new GmgManager(BusyIndicator);
            LoadCommand = new AsyncCommand(Load);
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

        public AccountsManager AccountsManager { get; }

        public GmgManager GmgManager { get; }

        public TransactionsManager TransactionsManager { get; }

        private async Task Load()
        {
            if (AccountsManager.LoadCommand.CanExecute(null))
            {
                await AccountsManager.LoadCommand.ExecuteAsync(null);
                await GmgManager.InitializeAsync(AccountsManager.Accounts);
            }
        }
    }
}