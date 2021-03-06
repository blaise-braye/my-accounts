﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MyAccounts.Business.GererMesComptes;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace MyAccounts.Tests.Gmc.Steps
{
    [Binding]
    public class GererMesCompteSteps
    {
        public GererMesCompteSteps(GererMesCompteTestContext context)
        {
            Context = context;
        }

        private GererMesCompteTestContext Context { get; }

        [Given(@"I am disconnected")]
        public async Task GivenIAmDisconnected()
        {
            await Context.GmcClient.Disconnect();
        }
        
        [Given(@"I import the qif data on account '(.*)'")]
        [When(@"I import the qif data on account '(.*)'")]
        public async Task GivenIImportTheQifDataOnAccount(Wrapper<string> accountName, string qifData)
        {
            var account = await Context.GmcAccounts.GetByName(accountName);
            Context.LastQifImportResult = await Context.GmcOperations.Import(account.Id, qifData);
        }

        [Then(@"the last qif data import succeeded")]
        public void ThenTheLastQifDataImportSucceeded()
        {
            Context.LastQifImportResult.Success.Should().BeTrue();
        }
        
        [When(@"I apply a dry run for the available qif data to account '(.*)'")]
        public async Task WhenIApplyADryRunForTheAvailableQifDataToAccount(Wrapper<string> accountName, string qifData)
        {
            var account = await Context.GmcAccounts.GetByName(accountName);
            Context.LastOperationsDelta = await Context.GmcOperations.DryRunImport(account.Id, qifData);
        }

        [Then(@"the last dry run result produces the following delta report")]
        public void ThenTheLastDryRunResultProducesTheFollowingDeltaReport(Table expectedQifDataDelta)
        {
            var comparableOperationsDelta = Context.LastOperationsDelta.ToList().Select(
                d => new
                {
                    d.DeltaKey,
                    d.Action,
                    SourceMemo = d.Source?.Memo,
                    TargetMemo = d.Target?.Memo
                }).ToList();
            
            expectedQifDataDelta.CompareToSet(comparableOperationsDelta);
        }

        [Then(@"I am (not )?connected")]
        public async Task ThenIAmConnected(string notConnected)
        {
            var isConnected = await Context.GmcClient.IsConnected();
            var expectedConnectState = string.IsNullOrEmpty(notConnected);
            Assert.That(isConnected, Is.EqualTo(expectedConnectState));
        }

        [Then(@"the account information of the bank account '(.*)' are")]
        public async Task ThenTheAccountInformationOfTheBankAccountAre(Wrapper<string> accountName, Table table)
        {
            var account = await Context.GmcAccounts.GetByName(accountName);
            var expectedKeys = table.Rows.Select(r => new { Key = r["Key"], Value = account.GetValue(r["Key"], "KeyNotFound") }).ToList();
            table.CompareToSet(expectedKeys);
        }

        [Then(@"the bank account '(.*)' (exists|does not exist)")]
        [Given(@"the bank account '(.*)' (exists|does not exist)")]
        public async Task ThenTheBankAccountExists(Wrapper<string> accountName, string existsString)
        {
            var expectedExists = existsString.Equals("exists");
            var account = await Context.GmcAccounts.GetByName(accountName);
            var accualExist = account != null;

            accualExist.Should().Be(expectedExists);
        }
        
        [Then(@"the last exported qif data are the following operations")]
        public void ThenTheLastExportedQifDataContainsTheFollowingOperations(Table table)
        {
            table.CompareToSet(Context.LastExportedQifDom.BankTransactions);
        }
        
        [When(@"I connect on GererMesComptes with email '(.*)' and password '(.*)'")]
        [Given(@"I connect on GererMesComptes with email '(.*)' and password '(.*)'")]
        public async Task WhenIConnectOnGererMesComptesWithEmailAndPassword(Wrapper<string> userName, PasswordWrapper password)
        {
            await Context.GmcClient.Connect(userName, password);
        }

        [Given(@"I create the bank account '(.*)'")]
        [When(@"I create the bank account '(.*)'")]
        public async Task WhenICreateTheBankAccount(Wrapper<string> accountName)
        {
            var values = new { name = accountName };
            if (await Context.GmcAccounts.Create(values))
            {
                Context.Cleaner.AddTask(async () =>
                {
                    var accountInfo = await Context.GmcAccounts.GetByName(accountName);
                    
                    if (accountInfo != null)
                    {
                        await Context.GmcAccounts.Delete(accountInfo);
                    }
                });
            }
        }
        
        [When(@"I delete the bank account '(.*)'")]
        public async Task WhenIDeleteTheBankAccount(Wrapper<string> accountName)
        {
            await Context.GmcAccounts.Delete(accountName);
        }

        [When(@"I export the qif data from account '(.*)', between '(.*)' and '(.*)'")]
        public async Task WhenIExportTheQifDataFromAccount(Wrapper<string> accountName, DateTime startDate, DateTime endDate)
        {
            var account = await Context.GmcAccounts.GetByName(accountName);
            Context.LastExportedQifData = await Context.GmcOperations.ExportQif(account.Id, startDate, endDate);
            Context.LastExportedQifDom = QifMapper.ParseQifDom(Context.LastExportedQifData);
        }
        
        [Given(@"I wait that last imported qifdata in account '(.*)' is available in export")]
        [When(@"I wait that last imported qifdata in account '(.*)' is available in export")]
        public async Task WhenIWaitThatLastImportedQifdataIsAvailableInExport(Wrapper<string> accountName)
        {
            var account = await Context.GmcAccounts.GetByName(accountName);
            Context.LastExportedQifData = await Context.GmcOperations.WaitExportAvailability(account.Id, Context.LastQifImportResult.ImportedQifData);
            Context.LastExportedQifDom = QifMapper.ParseQifDom(Context.LastExportedQifData);
        }
    }
}