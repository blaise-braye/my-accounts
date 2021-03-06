﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.2.0.0
//      SpecFlow Generator Version:2.2.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace MyAccounts.Tests.Features.Imports
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.2.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Import operations file")]
    [NUnit.Framework.CategoryAttribute("UnitTest")]
    public partial class ImportOperationsFileFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ImportOperationsFile.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Import operations file", null, ProgrammingLanguage.CSharp, new string[] {
                        "UnitTest"});
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
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("import two times the same datasource and fetch imported operations")]
        public virtual void ImportTwoTimesTheSameDatasourceAndFetchImportedOperations()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("import two times the same datasource and fetch imported operations", ((string[])(null)));
#line 5
this.ScenarioSetup(scenarioInfo);
#line 6
    testRunner.Given("I have an empty operations repository", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table1.AddRow(new string[] {
                        "SourceKind",
                        "FortisCsvExport"});
            table1.AddRow(new string[] {
                        "Encoding",
                        "windows-1252"});
            table1.AddRow(new string[] {
                        "DecimalSeparator",
                        ","});
#line 8
    testRunner.Given("I am working with operations coming from a file having the following structure me" +
                    "tadata", ((string)(null)), table1, "Given ");
#line hidden
#line 14
 testRunner.Given("I have an operations file with the following \'windows-1252\' content", "Numéro de séquence;Date d\'exécution;Date valeur;Montant;Devise du compte;Détails;" +
                    "Numéro de compte\r\n   2017-0050;05/02/2017;05/02/2017;-1,11;EUR;AVEC LA CARTE 670" +
                    "3 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/201" +
                    "7;BE02275045085140\r\n   2017-0051;05/02/2017;05/02/2017;-1,11;EUR;AVEC LA CARTE 6" +
                    "703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2" +
                    "017;BE02275045085140\r\n   2017-0052;05/02/2017;05/02/2017;-1,11;EUR;AVEC LA CARTE" +
                    " 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02" +
                    "/2017;BE02275045085140\r\n   2017-0053;05/02/2017;05/02/2017;-1,11;EUR;AVEC LA CAR" +
                    "TE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/" +
                    "02/2017;BE02275045085140\r\n   2017-0060;06/02/2017;06/02/2017;-1,11;EUR;AVEC LA C" +
                    "ARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 0" +
                    "4/02/2017;BE02275045085140\r\n   2017-0060;06/02/2017;06/02/2017;-1,11;EUR;AVEC LA" +
                    " CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR :" +
                    " 04/02/2017;BE02275045085140\r\n   2017-0061;06/02/2017;06/02/2017;-1,11;EUR;AVEC " +
                    "LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR" +
                    " : 04/02/2017;BE02275045085140\r\n   2017-0062;06/02/2017;06/02/2017;-1,11;EUR;AVE" +
                    "C LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALE" +
                    "UR : 04/02/2017;BE02275045085140\r\n   2017-0063;06/02/2017;06/02/2017;-1,11;EUR;A" +
                    "VEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VA" +
                    "LEUR : 04/02/2017;BE02275045085140", ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 28
 testRunner.When("I import the operations file with the current structure metadata", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 30
    testRunner.And("I import the operations file with the current structure metadata", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "OperationId",
                        "ExecutionDate"});
            table2.AddRow(new string[] {
                        "2017-0050",
                        "2017-02-05"});
            table2.AddRow(new string[] {
                        "2017-0051",
                        "2017-02-05"});
            table2.AddRow(new string[] {
                        "2017-0052",
                        "2017-02-05"});
            table2.AddRow(new string[] {
                        "2017-0053",
                        "2017-02-05"});
            table2.AddRow(new string[] {
                        "2017-0060",
                        "2017-02-06"});
            table2.AddRow(new string[] {
                        "2017-0061",
                        "2017-02-06"});
            table2.AddRow(new string[] {
                        "2017-0062",
                        "2017-02-06"});
            table2.AddRow(new string[] {
                        "2017-0063",
                        "2017-02-06"});
#line 32
    testRunner.Then("the imported operation data is", ((string)(null)), table2, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("custom encoding is taken into account during import")]
        public virtual void CustomEncodingIsTakenIntoAccountDuringImport()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("custom encoding is taken into account during import", ((string[])(null)));
#line 44
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table3.AddRow(new string[] {
                        "SourceKind",
                        "FortisCsvExport"});
            table3.AddRow(new string[] {
                        "Encoding",
                        "UTF-8"});
            table3.AddRow(new string[] {
                        "DecimalSeparator",
                        ","});
#line 48
    testRunner.Given("I am working with operations coming from a file having the following structure me" +
                    "tadata", ((string)(null)), table3, "Given ");
#line hidden
#line 54
 testRunner.Given("I have an operations file with the following \'windows-1252\' content", @"Numéro de séquence;Date d'exécution;Date valeur;Montant;Devise du compte;Détails;Numéro de compte
2017-0049;04/02/2017;04/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140", ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 60
 testRunner.And("I have an empty operations repository", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 62
 testRunner.When("I import the operations file with the current structure metadata", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "OperationId",
                        "ExecutionDate"});
#line 64
 testRunner.Then("the imported operation data is", ((string)(null)), table4, "Then ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Key",
                        "Value"});
            table5.AddRow(new string[] {
                        "Encoding",
                        "windows-1252"});
#line 68
 testRunner.When("I change the last import command such that", ((string)(null)), table5, "When ");
#line 72
 testRunner.And("I replay the entire reflog of operations", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "OperationId",
                        "ExecutionDate"});
            table6.AddRow(new string[] {
                        "2017-0049",
                        "2017-02-04"});
#line 74
 testRunner.Then("the imported operation data is", ((string)(null)), table6, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
