using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Operations.Classification.WorkingCopyStorage;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Technical.Caching;
using Operations.Classification.WpfUi.Technical.Input;
using Operations.Classification.WpfUi.Technical.Projections;

namespace Operations.Classification.WpfUi.Managers.Imports
{
    public interface IImportsManager
    {
        Task<List<ImportCommand>> GetImports(Guid accountId);
    }

    public class ImportsManagerViewModel : ViewModelBase, IImportsManager
    {
        private const string ImportsByAccountIdRoute = "/Imports/{0}";
        private readonly IImportManager _importManager;
        private AccountViewModel _currentAccount;
        private List<ImportCommandModel> _imports;

        public ImportsManagerViewModel(IImportManager importManager)
        {
            _importManager = importManager;
            MessengerInstance.Register<AccountViewModel>(this, OnAccountViewModelReceived);
        }

        public List<ImportCommandModel> Imports
        {
            get => _imports;
            private set => Set(nameof(Imports), ref _imports, value);
        }

        public RelayCommand BeginImportCommand { get; set; }

        public RelayCommand BeginEditCommand { get; set; }

        public AsyncCommand<IList<ImportCommandModel>> BeginDownloadCommand { get; set; }

        public AsyncCommand<IList<ImportCommandModel>> DeleteCommand { get; set; }

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
    }
}