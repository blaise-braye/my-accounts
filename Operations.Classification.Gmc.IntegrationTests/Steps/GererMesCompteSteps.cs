using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using NUnit.Framework;
using Operations.Classification.GererMesComptes;
using QifApi;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Operations.Classification.Gmc.IntegrationTests.Steps
{
    [Binding]
    public class GererMesCompteSteps
    {
        private readonly AccountInfoRepository _accounts;
        private readonly PostScenarioCleaner _cleaner;

        private readonly GererMesComptesClient _client;

        private readonly OperationsRepository _operations;

        private string _lastExportedQifData;

        private QifDom _lastExportedQifDom;

        private RunImportResult _lastQifImportResult;

        private string _toImportQifData;

        public GererMesCompteSteps(PostScenarioCleaner cleaner)
        {
            _cleaner = cleaner;

            _client = new GererMesComptesClient();
            _accounts = new AccountInfoRepository(_client);
            _operations = new OperationsRepository(_client);
        }

        [Given(@"I am disconnected")]
        public async Task GivenIAmDisconnected()
        {
            await _client.Disconnect();
        }

        [Given(@"I have an update of the qif data file to import")]
        public void GivenIHaveAnUpdateOfTheQifDataFileToImport(string qifData)
        {
            _toImportQifData = qifData;
        }
        
        [Given(@"I import the qif data on account '(.*)'")]
        [When(@"I import the qif data on account '(.*)'")]
        public async Task GivenIImportTheQifDataOnAccount(string accountName, string qifData)
        {
            var account = await _accounts.GetByName(accountName);
            _lastQifImportResult = await _operations.Import(account.Id, qifData);
        }

        [Then(@"the last qif data import succeeded")]
        public void ThenTheLastQifDataImportSucceeded()
        {
            _lastQifImportResult.Success.Should().BeTrue();
        }


        [Then(@"dry run import available qif data to account '(.*)' produces the following delta report")]
        public async Task ThenDryRunImportAvailableQifDataToAccountProducesTheFollowingDeltaReport(string accountName, Table expectedQifDataDelta)
        {
            var account = await _accounts.GetByName(accountName);
            var operationsDelta = await _operations.DryRunImport(account.Id, _toImportQifData);
            var comparableOperationsDelta = operationsDelta.ToList().Select(
                d => new
                {
                    d.DeltaKey,
                    d.Action,
                    SourceMemo = d.Source?.Memo,
                    TargetMemo = d.Target?.Memo
                }).ToList();

            var csvConfiguration = new CsvConfiguration();
            using (var fs = File.Open($"c:\\temp\\delta-{account.Id}.csv", FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    using (var cw = new CsvWriter(sw, csvConfiguration))
                    {
                        cw.WriteHeader(comparableOperationsDelta.First().GetType());
                        cw.WriteRecords(comparableOperationsDelta);
                    }
                }
            }

            //expectedQifDataDelta.CompareToSet(comparableOperationsDelta);
        }

        [Then(@"I am (not )?connected")]
        public async Task ThenIAmConnected(string notConnected)
        {
            var isConnected = await _client.IsConnected();
            var expectedConnectState = string.IsNullOrEmpty(notConnected);
            Assert.That(isConnected, Is.EqualTo(expectedConnectState));
        }

        [Then(@"the account information of the bank account '(.*)' are")]
        public async Task ThenTheAccountInformationOfTheBankAccountAre(string accountName, Table table)
        {
            var account = await _accounts.GetByName(accountName);
            var expectedKeys = table.Rows.Select(r => new { Key = r["Key"], Value = account.GetValue(r["Key"], "KeyNotFound") }).ToList();
            table.CompareToSet(expectedKeys);
        }

        [Then(@"the bank account '(.*)' (exists|does not exist)")]
        public async Task ThenTheBankAccountExists(string accountName, string existsString)
        {
            var expectedExists = existsString.Equals("exists");
            var account = await _accounts.GetByName(accountName);
            var accualExist = account != null;

            accualExist.Should().Be(expectedExists);
        }

        [Then(@"the last exported qif data are the following operations")]
        public void ThenTheLastExportedQifDataContainsTheFollowingOperations(Table table)
        {
            table.CompareToSet(_lastExportedQifDom.BankTransactions);
        }
        
        [When(@"I connect on GererMesComptes with email '(.*)' and password '(.*)'")]
        [Given(@"I connect on GererMesComptes with email '(.*)' and password '(.*)'")]
        public async Task WhenIConnectOnGererMesComptesWithEmailAndPassword(Wrapper<string> userName, Wrapper<string> password)
        {
            await _client.Connect(userName, password);
        }

        [Given(@"I create the bank account '(.*)'")]
        [When(@"I create the bank account '(.*)'")]
        public async Task WhenICreateTheBankAccount(string accountName)
        {
            var values = new { name = accountName };
            if (await _accounts.Create(values))
            {
                _cleaner.Add(() => _accounts.Delete(accountName));
            }
        }

        [When(@"I delete the bank account '(.*)'")]
        public async Task WhenIDeleteTheBankAccount(string accountName)
        {
            await _accounts.Delete(accountName);
        }

        [When(@"I export the qif data from account '(.*)', between '(.*)' and '(.*)'")]
        public async Task WhenIExportTheQifDataFromAccount(string accountName, DateTime startDate, DateTime endDate)
        {
            var account = await _accounts.GetByName(accountName);
            _lastExportedQifData = await _operations.ExportQif(account.Id, startDate, endDate);
            _lastExportedQifDom = QifMapper.ParseQifDom(_lastExportedQifData);
        }
        
        [Given(@"I wait that last imported qifdata in account '(.*)' is available in export")]
        [When(@"I wait that last imported qifdata in account '(.*)' is available in export")]
        public async Task WhenIWaitThatLastImportedQifdataIsAvailableInExport(string accountName)
        {
            var account = await _accounts.GetByName(accountName);
            _lastExportedQifData = await _operations.WaitExportAvailability(account.Id, _lastQifImportResult.ImportedQifData);
            _lastExportedQifDom = QifMapper.ParseQifDom(_lastExportedQifData);
        }
    }
}