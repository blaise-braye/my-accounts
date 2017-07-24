using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Operations.Classification.AccountOperations;
using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Fortis;
using Operations.Classification.AccountOperations.Unified;
using Operations.Classification.GeoLoc;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Operations.Classification.Tests.Steps
{
    [Binding]
    public class ClassificationSteps
    {
        private readonly CsvAccountOperationManager _csvAccountOperationManager = new CsvAccountOperationManager();

        private readonly UnifiedAccountOperationPatternTransformer _transformer =
            new UnifiedAccountOperationPatternTransformer(
                new PlaceInfoResolver(PlaceProvider.Load(new PlacesRepository())));

        private List<AccountOperationBase> _readOperations;

        private List<UnifiedAccountOperation> _unifiedOperations;
        
        [Given(@"I have read the following fortis operations from archive files")]
        public void GivenIHaveReadTheFollowingFortisOperationsFromArchiveFiles(Table table)
        {
            _readOperations = table.CreateSet<FortisOperation>().Select(
                o =>
                {
                    o.SourceKind = SourceKind.FortisCsvArchive;
                    return o;
                }).Cast<AccountOperationBase>().ToList();
        }

        [Given(@"I have read the following fortis operations from export files")]
        public void GivenIHaveReadTheFollowingFortisOperationsFromExportFiles(Table table)
        {
            _readOperations = table.CreateSet<FortisOperation>().Select(
                o =>
                {
                    o.SourceKind = SourceKind.FortisCsvExport;
                    return o;
                }).Cast<AccountOperationBase>().ToList();
        }

        [Given(@"I have read the following sodexo operations")]
        public async Task GivenIHaveReadTheFollowingSodexoOperations(string multilineText)
        {
            var rawStream = new MemoryStream(Encoding.UTF8.GetBytes(multilineText));
            var operationManager = new CsvAccountOperationManager();
            _readOperations = await operationManager.ReadAsync(rawStream, SourceKind.SodexoCsvExport);
        }

        [Then(@"File '(.*)' exists")]
        public void ThenFileExists(string filePath)
        {
            var fileExist = File.Exists(filePath);
            Assert.IsTrue(fileExist, "File should exist");
        }

        [Then(@"pattern detection accuracy is higher that (.*) %")]
        public void ThenPatternDetectionAccuracyIsHigherThat(double minAccuracy)
        {
            var patternDetected = _unifiedOperations.Where(p => !string.IsNullOrEmpty(p.PatternName)).ToList();
            var detectedCount = patternDetected.Count;
            var total = _unifiedOperations.Count;
            var detectedPercentage = 100.0 * detectedCount / total;
            foreach (var op in _unifiedOperations.Where(p => string.IsNullOrEmpty(p.PatternName)))
                Console.WriteLine($@"{op.SourceKind} {op.OperationId} {op.Note}");

            Assert.That(detectedPercentage, Is.GreaterThanOrEqualTo(minAccuracy));
        }

        [Then(@"the operations data is")]
        public void ThenTheOperationsDataIs(Table table)
        {
            table.CompareToSet(_unifiedOperations);
        }

        [Then(@"the read fortis operations are")]
        public void ThenTheReadFortisOperationsAre(Table table)
        {
            table.CompareToSet(_readOperations.Cast<FortisOperation>());
        }
        
        [When(@"I Filter the details where number is higher than '(.*)'")]
        public void WhenIFilterTheDetailsWhereNumberIsHigherThan(string number)
        {
            if (!string.IsNullOrEmpty(number))
            {
                _unifiedOperations = _unifiedOperations.Where(t => string.CompareOrdinal(t.OperationId, number) > 0).ToList();
            }
        }

        [When(@"I parse the details of the files '(.*)'")]
        public void WhenIParseTheDetailsOfTheFiles(string filePath)
        {
            string[] files = { filePath };

            var attr = File.GetAttributes(filePath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                files = Directory.GetFiles(filePath, "*.csv");
            }

            _unifiedOperations =
                files.SelectMany(s => _csvAccountOperationManager.Read(s, CsvAccountOperationManager.DetectFileSourceKindFromFileName(s)))
                    .Select(operation => _transformer.Apply(operation))
                    .SortByOperationIdDesc()
                    .ToList();
        }

        [When(@"I read the following fortis operations from an export csv file")]
        public async Task WhenIReadTheFollowingFortisOperationsFromAnExportCsvFile(string multilineText)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(multilineText);
            var asciiBytes = Encoding.Convert(
                Encoding.UTF8,
                Encoding.GetEncoding("windows-1252"),
                utf8Bytes);

            var rawStream = new MemoryStream(asciiBytes);
            var operationManager = new CsvAccountOperationManager();
            _readOperations = await operationManager.ReadAsync(rawStream, SourceKind.FortisCsvExport);
        }

        [When(@"I store the operation details in file '(.*)'")]
        public void WhenIStoreTheOperationDetailsInFile(string targetFilePath)
        {
            new UnifiedAccountOperationWriter().WriteCsv(targetFilePath, _unifiedOperations);
        }

        [When(@"I store the operation details in qif file '(.*)'")]
        public async Task WhenIStoreTheOperationDetailsInQifFile(string targetFilePath)
        {
            await new UnifiedAccountOperationWriter().WriteQif(targetFilePath, _unifiedOperations);
        }

        [When(@"I unify and transform the read operations")]
        public void WhenIUnifyAndTransformTheReadOperations()
        {
            _unifiedOperations = _readOperations.Select(o => _transformer.Apply(o)).SortByOperationIdDesc().ToList();
        }
    }
}