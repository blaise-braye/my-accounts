using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Unified;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Operations.Classification.Tests.Steps
{
    [Binding]
    public class ParseOperationDetailsSteps
    {
        private List<UnifiedAccountOperation> _unifiedOperations;

        [Given(@"I have read the following account operations from source of kind (\w+)")]
        public void GivenIHaveReadTheFollowingAccountOperationsFromSourceOfKindFortisCsvArchive(SourceKind sourceKind, Table table)
        {
            _unifiedOperations = table.CreateSet<UnifiedAccountOperation>().Select(
                o =>
                    {
                        o.SourceKind = sourceKind;
                        return o;
                    }).ToList();
        }
        
        [When(@"I parse the details of the files '(.*)'")]
        public void WhenIParseTheDetailsOfTheFiles(string filePath)
        {
            string[] files = { filePath };
            
            FileAttributes attr = File.GetAttributes(filePath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                files = Directory.GetFiles(filePath, "*.csv");
            }

            var transactionPatternMapper = new UnifiedAccountOperationPatternTransformer();
            var accountToUnifiedOperationMapper = new AccountToUnifiedOperationMapper();
            var repository = new AccountOperationReader();
            _unifiedOperations =
                repository.Read(files)
                    .Select(operation => accountToUnifiedOperationMapper.Map(operation))
                    .Select(unifiedOperation => transactionPatternMapper.Apply(unifiedOperation))
                    .OrderByDescending(t => t.OperationId, StringComparer.OrdinalIgnoreCase)
                    .ToList();
        }

        [When(@"I Filter the details where number is higher than '(.*)'")]
        public void WhenIFilterTheDetailsWhereNumberIsHigherThan(string number)
        {
            if (!string.IsNullOrEmpty(number))
            {
                _unifiedOperations = _unifiedOperations.Where(t => string.CompareOrdinal(t.OperationId, number) > 0).ToList();
            }
        }

        [Then(@"pattern detection accuracy is higher that (.*) %")]
        public void ThenPatternDetectionAccuracyIsHigherThat(double minAccuracy)
        {
            var patternDetected = _unifiedOperations.Where(p => !string.IsNullOrEmpty(p.PatternName)).ToList();
            var detectedCount = patternDetected.Count;
            var total = _unifiedOperations.Count;
            var detectedPercentage = 100.0 * detectedCount / total;
            foreach (var note in _unifiedOperations.Where(p => string.IsNullOrEmpty(p.PatternName)).Select(p => p.Note))
            {
                Console.WriteLine(note);
            }

            Assert.That(detectedPercentage, Is.GreaterThanOrEqualTo(minAccuracy));
        }
        
        [When(@"I store the operation details in file '(.*)'")]
        public void WhenIStoreTheOperationDetailsInFile(string targetFilePath)
        {
            new UnifiedAccountOperationWriter().WriteCsv(targetFilePath, _unifiedOperations);
        }

        [When(@"I store the operation details in qif file '(.*)'")]
        public void WhenIStoreTheOperationDetailsInQifFile(string targetFilePath)
        {
            new UnifiedAccountOperationWriter().WriteQif(targetFilePath, _unifiedOperations);
        }
        
        [Then(@"File '(.*)' exists")]
        public void ThenFileExists(string filePath)
        {
            var fileExist = File.Exists(filePath);
            Assert.IsTrue(fileExist, "File should exist");
        }

        [When(@"I apply the cleanup transformation on unified operations")]
        public void WhenIApplyTheCleanupTransformationOnUnifiedOperations()
        {
            var transformer = new UnifiedAccountOperationPatternTransformer();

            foreach (var operation in _unifiedOperations)
            {
                transformer.Apply(operation);
            }
        }
        
        [Then(@"the operations data is")]
        public void ThenTheOperationsDataIs(Table table)
        {
            table.CompareToSet(_unifiedOperations);
        }
    }
}
