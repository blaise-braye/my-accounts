using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Newtonsoft.Json.Linq;

using Operations.Classification.Extensions;

namespace Operations.Classification.GererMesComptes
{
    public class AccountInfoRepository
    {
        private readonly GererMesComptesClient _client;

        private List<AccountInfo> _cachedAccounts;

        public AccountInfoRepository(GererMesComptesClient client)
        {
            _client = client;
        }

        private HttpClient Transport => _client.Transport;

        public Task<AccountInfo> PrepareNew(object initialValues)
        {
            var rawMembersDictionary = initialValues.ToRawMembersDictionary();
            return PrepareNew(rawMembersDictionary);
        }

        public async Task<bool> Create(AccountInfo accountInfo)
        {
            var fields = accountInfo.ToDictionnary();

            var postResponse = await Transport.PostAsync("/system/requests/user/parameters/accounts.html?action_form=saveAccount", new FormUrlEncodedContent(fields));
            postResponse.EnsureSuccessStatusCode();
            var json = await postResponse.Content.ReadAsStringAsync();
            var succeeded = (bool)JObject.Parse(json)["response"];
            if (succeeded)
            {
                ClearCache();
            }

            return succeeded;
        }

        public async Task<bool> Create(object initialValues)
        {
            var info = initialValues as AccountInfo ?? await PrepareNew(initialValues);
            var succeeded = await Create(info);
            return succeeded;
        }

        public async Task<AccountInfo> GetByName(string accountName)
        {
            if (string.IsNullOrEmpty(accountName))
            {
                throw new ArgumentNullException(nameof(accountName));
            }

            var accounts = await GetAll();
            var info = accounts.FirstOrDefault(a => a.Name == accountName);
            return info;
        }

        public async Task<AccountInfo> GetById(string accountId)
        {
            if (string.IsNullOrEmpty(accountId))
            {
                throw new ArgumentNullException(nameof(accountId));
            }

            var accounts = await GetAll();
            var info = accounts.FirstOrDefault(a => a.Id == accountId);
            return info;
        }

        public async Task<List<AccountInfo>> GetAll()
        {
            if (_cachedAccounts == null)
            {
                var result = new List<AccountInfo>();
                var identifers = await GetIdentifiers();
                foreach (var identifer in identifers)
                {
                    var modificationFormUrl = $"/fr/u/parametres/mes-comptes/compte-{identifer}.html";
                    var getResponse = await Transport.GetAsync(modificationFormUrl);
                    var html = await getResponse.Content.ReadAsStringAsync();

                    var fields = HtmlParser.ParseFieldsToDictionnary(html, "//form[@id='account-edition']");
                    var accountInfo = AccountInfo.Create(fields);

                    result.Add(accountInfo);
                }

                _cachedAccounts = result;
            }

            return _cachedAccounts;
        }

        public async Task<List<string>> GetIdentifiers()
        {
            List<string> result;
            if (_cachedAccounts == null)
            {
                result = new List<string>();

                var getResponse = await Transport.GetAsync("/fr/u/finances/");
                getResponse.EnsureSuccessStatusCode();

                var document = new HtmlDocument();
                var html = await getResponse.Content.ReadAsStreamAsync();
                document.Load(html);
                var tableRows = document.DocumentNode.SelectNodes("//table/tbody/tr");
                foreach (var tableRow in tableRows)
                {
                    var accountId = tableRow.GetAttributeValue("data-account", string.Empty);
                    result.Add(accountId);
                }
            }
            else
            {
                result = _cachedAccounts.Select(a => a.Id).ToList();
            }

            return result;
        }

        public async Task<bool> Delete(AccountInfo account)
        {
            var input = new { id_account = account.Id }.ToRawMembersDictionary();
            var postResponse = await Transport.PostAsync("/system/requests/user/parameters/accounts.html?action_form=deleteAccount", new FormUrlEncodedContent(input));
            postResponse.EnsureSuccessStatusCode();
            var json = await postResponse.Content.ReadAsStringAsync();
            var succeeded = (bool)JObject.Parse(json)["response"];
            if (succeeded)
            {
                ClearCache();
            }

            return succeeded;
        }

        public async Task<bool> Delete(string accountName)
        {
            var account = await GetByName(accountName);
            return await Delete(account);
        }

        private async Task<AccountInfo> PrepareNew(IDictionary<string, string> initialValues)
        {
            var createFormUrl = "/system/requests/user/parameters/accounts.html?action_form=createManualAccount_html";
            var getResponse = await Transport.GetAsync(createFormUrl);
            var jsonPopup = await getResponse.Content.ReadAsStringAsync();
            var obj = JObject.Parse(jsonPopup);
            var popupContent = (string)obj["popup"]["html"];

            var fields = HtmlParser.ParseFieldsToDictionnary(popupContent, "//form[@id='f_account_creation']");
            var account = AccountInfo.Create(fields);

            foreach (var initialValue in initialValues)
            {
                account.SetValue(initialValue.Key, initialValue.Value);
            }

            return account;
        }

        private void ClearCache()
        {
            _cachedAccounts = null;
        }
    }
}
