﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.2.0.0
//      SpecFlow Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Operations.Classification.Tests.Features.GererMesComptes
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.2.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("ImportBankOperations")]
    public partial class ImportBankOperationsFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ImportBankOperations.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ImportBankOperations", "\tUser must be able to import bank operations manually", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 4
#line 5
 testRunner.Given("I connect on GererMesComptes with email \'blaisemail@gmail.com\' and password \'kT5X" +
                    "eI!I9AI9\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
 testRunner.And("I create the bank account \'Automated Test Account\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Import an operation")]
        public virtual void ImportAnOperation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Import an operation", ((string[])(null)));
#line 8
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
#line 9
 testRunner.When("I import the qif data on account \'Automated Test Account\'", "!Type:Bank\r\nD11/04/2016\r\nT0.01\r\nMPARTENA - MUTUALITE LIBRE - BE74 2100 0818 2307 " +
                    "- BIC GEBABEBB - COMMUNICATION /C/ PAIE /0201733339803 DU 10/04/2017 POUR 001 PR" +
                    "ESTATIONS CHEZ M.BAYET BEN 0201733339803 711041707696 - BankTransfert\r\nN2017-014" +
                    "8\r\n^", ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 18
  testRunner.Then("the last qif data import succeeded", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Export imported operations")]
        public virtual void ExportImportedOperations()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Export imported operations", ((string[])(null)));
#line 20
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
#line 21
 testRunner.Given("I import the qif data on account \'Automated Test Account\'", @"!Type:Bank
D11/04/2016
T0.01
MData Too Long Is Truncated To 129 Bytes You Should Not Be Able To Read Something After triple letter ooooooooooooooooooooooo SSS while there is something as you can read it :)
N2017-0148
^
D09/28/2016
T0.02
Mdates must be formatted in mm/dd/yyyy
N2017-0148
^", ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 36
 testRunner.When("I export the qif data from account \'Automated Test Account\', between \'2016-04-11T" +
                    "00:00:00\' and \'2016-11-05T00:00:00\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Number",
                        "Date",
                        "Amount",
                        "Memo"});
            table1.AddRow(new string[] {
                        "",
                        "2016-11-04T00:00:00",
                        "0.01",
                        "Data Too Long Is Truncated To 129 Bytes You Should Not Be Able To Read Something " +
                            "After Triple Letter Ooooooooooooooooooooooo Sss"});
            table1.AddRow(new string[] {
                        "",
                        "2016-09-28T00:00:00",
                        "0.02",
                        "Dates Must Be Formatted In Mm Dd Yyyy"});
#line 38
 testRunner.Then("the last exported qif data are the following operations", ((string)(null)), table1, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Execute two successive Imports")]
        public virtual void ExecuteTwoSuccessiveImports()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Execute two successive Imports", ((string[])(null)));
#line 43
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
#line 46
 testRunner.Given("I import the qif data on account \'Automated Test Account\'", "!Type:Bank\r\nD09/28/2016\r\nT0.01\r\nMSome Memo\r\nN2017-0148\r\n^", ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 55
 testRunner.And("I wait that last imported qifdata in account \'Automated Test Account\' is availabl" +
                    "e in export", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 57
 testRunner.When("I import the qif data on account \'Automated Test Account\'", "!Type:Bank\r\nD09/28/2016\r\nT0.01\r\nMSome Updated Memo\r\nN2017-0148\r\n^\r\n!Type:Bank\r\nD0" +
                    "4/01/2016\r\nT0.02\r\nMSome Added Memo\r\nN2017-0149\r\n^", ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 73
 testRunner.And("I wait that last imported qifdata in account \'Automated Test Account\' is availabl" +
                    "e in export", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 75
 testRunner.Then("the last qif data import succeeded", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Number",
                        "Date",
                        "Amount",
                        "Memo"});
            table2.AddRow(new string[] {
                        "",
                        "2016-09-28T00:00:00",
                        "0.01",
                        "Some Updated Memo"});
            table2.AddRow(new string[] {
                        "",
                        "2016-04-01T00:00:00",
                        "0.02",
                        "Some Added Memo"});
#line 76
 testRunner.And("the last exported qif data are the following operations", ((string)(null)), table2, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Identify delta between imported qif data and new available qif data")]
        public virtual void IdentifyDeltaBetweenImportedQifDataAndNewAvailableQifData()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Identify delta between imported qif data and new available qif data", ((string[])(null)));
#line 83
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
#line 84
 testRunner.Given("I import the qif data on account \'Automated Test Account\'", "!Type:Bank\r\nD09/27/2013\r\nT1.00\r\nMUnchanged\r\n^\r\n!Type:Bank\r\nD09/28/2013\r\nT0.01\r\nMR" +
                    "emove\r\n^\r\n!Type:Bank\r\nD09/29/2013\r\nT0.02\r\nMSome Memo\r\n^", ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 103
 testRunner.Given("I have an update of the qif data file to import", "!Type:Bank\r\nD09/27/2013\r\nT1.00\r\nMUnchanged\r\n^\r\n!Type:Bank\r\nD09/29/2013\r\nT0.02\r\nMU" +
                    "pdated Memo\r\n^\r\n!Type:Bank\r\nD01/11/2014\r\nT0.01\r\nMAdded Memo\r\n^", ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 122
 testRunner.And("I wait that last imported qifdata in account \'Automated Test Account\' is availabl" +
                    "e in export", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "DeltaKey",
                        "Action"});
            table3.AddRow(new string[] {
                        "2013-09-27$1.00",
                        "Nothing"});
            table3.AddRow(new string[] {
                        "2013-09-29$0.02",
                        "UpdateMemo"});
            table3.AddRow(new string[] {
                        "2014-01-11$0.01",
                        "Add"});
            table3.AddRow(new string[] {
                        "2013-09-28$0.01",
                        "Remove"});
#line 124
 testRunner.Then("dry run import available qif data to account \'Automated Test Account\' produces th" +
                    "e following delta report", ((string)(null)), table3, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
