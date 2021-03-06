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

using TechTalk.SpecFlow;

#pragma warning disable
namespace MyAccounts.Tests.Features
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.2.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("ParseTransactionFiles")]
    [NUnit.Framework.CategoryAttribute("IntegrationTest")]
    [NUnit.Framework.CategoryAttribute("UserPcHelperTest")]
    public partial class ParseTransactionFilesFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ParseAndUnifyOperations.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ParseTransactionFiles", "\tIn order to classify my personal operations,\r\n\tI want a more structure operation" +
                    " detail for any transactions coming for any csv file kind", ProgrammingLanguage.CSharp, new string[] {
                        "IntegrationTest",
                        "UserPcHelperTest"});
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
        [NUnit.Framework.DescriptionAttribute("merge fortis operations and parse details")]
        [NUnit.Framework.TestCaseAttribute("C:\\Users\\BBraye\\OneDrive\\Gestion\\Data2\\Blaise - Sodexo\\operations", "", "100", null)]
        [NUnit.Framework.TestCaseAttribute("C:\\Users\\BBraye\\OneDrive\\Gestion\\Data2\\Blaise - Compte courant\\operations", "", "99.59", null)]
        [NUnit.Framework.TestCaseAttribute("C:\\Users\\BBraye\\OneDrive\\Gestion\\Data2\\Sylvie - Compte courant\\operations", "", "99.48", null)]
        public virtual void MergeFortisOperationsAndParseDetails(string path, string latestSynchronised, string minAccuracy, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("merge fortis operations and parse details", exampleTags);
#line 7
this.ScenarioSetup(scenarioInfo);
#line 9
 testRunner.When(string.Format("I parse the details of the files \'{0}\\\'", path), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 10
 testRunner.And(string.Format("I Filter the details where number is higher than \'{0}\'", latestSynchronised), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
 testRunner.And(string.Format("I store the operation details in file \'{0}.csv\'", path), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
 testRunner.And(string.Format("I store the operation details in qif file \'{0}.qif\'", path), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 14
 testRunner.Then(string.Format("File \'{0}.csv\' exists", path), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 15
 testRunner.And(string.Format("File \'{0}.qif\' exists", path), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 16
 testRunner.And(string.Format("pattern detection accuracy is higher that {0} %", minAccuracy), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
