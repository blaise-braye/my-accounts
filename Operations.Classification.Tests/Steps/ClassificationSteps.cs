using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAccounts.Business.AccountOperations;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Fortis;
using MyAccounts.Business.AccountOperations.Unified;
using MyAccounts.Business.GererMesComptes;
using MyAccounts.Business.Managers.Imports;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Operations.Classification.Tests.Steps
{
    [Binding]
    public class ClassificationSteps
    {
        private readonly ClassificationStepsContext _context;

        public ClassificationSteps(ClassificationStepsContext context)
        {
            _context = context;
        }

        [Given(@"I have an operations file with the following '(.*)' content")]
        public void GivenIHaveAnOperationsFileWithTheFollowingContent(string encoding, string content)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(content);
            var encodingBytes = Encoding.Convert(
                Encoding.UTF8, Encoding.GetEncoding(encoding), utf8Bytes);
            _context.AnOperationsFile = encodingBytes;
        }

        [Given(@"I have an empty operations repository")]
        public async Task GivenIHaveAnEmptyOperationsRepository()
        {
            await _context.OperationsManager.Clear(_context.AccountId);
        }

        [When(@"I import the operations file with following parameters")]
        public async Task WhenIImportTheOperationsFileWithFollowingParameters(Table table)
        {
            var importCommand = new ImportCommand(_context.AccountId);
            table.FillInstance(importCommand);
            
            await _context.ImportManager.RequestImportExecution(
                importCommand,
                new MemoryStream(_context.AnOperationsFile));
        }

        [When(@"I change the last import command such that")]
        public async Task WhenIChangeTheLastImportCommandSuchThat(Table table)
        {
            var imports = await _context.AccountCommandRepository.GetAll(_context.AccountId);
            var lastImportCommand = imports.Last();
            table.FillInstance(lastImportCommand);
            await _context.AccountCommandRepository.Replace(lastImportCommand);
        }

        [When(@"I replay the entire reflog of operations")]
        public async Task WhenIReplayTheEntireReflogOfOperations()
        {
            await _context.ImportManager.ReplayCommands(_context.AccountId);
        }
        
        [Given(@"I have read the following fortis operations from archive files")]
        public void GivenIHaveReadTheFollowingFortisOperationsFromArchiveFiles(Table table)
        {
            _context.ReadOperations = table.CreateSet<FortisOperation>().Select(
                o =>
                {
                    o.SourceKind = SourceKind.FortisCsvArchive;
                    return o;
                }).Cast<AccountOperationBase>().ToList();
        }

        [Given(@"I have read the following fortis operations from export files")]
        public void GivenIHaveReadTheFollowingFortisOperationsFromExportFiles(Table table)
        {
            _context.ReadOperations = table.CreateSet<FortisOperation>().Select(
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
            _context.ReadOperations = await operationManager.ReadAsync(rawStream, operationManager.GetDefaultFileMetadata(SourceKind.SodexoCsvExport));
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
            var patternDetected = _context.UnifiedOperations.Where(p => !string.IsNullOrEmpty(p.PatternName)).ToList();
            var detectedCount = patternDetected.Count;
            var total = _context.UnifiedOperations.Count;
            var detectedPercentage = 100.0 * detectedCount / total;
            foreach (var op in _context.UnifiedOperations.Where(p => string.IsNullOrEmpty(p.PatternName)))
                Console.WriteLine($@"{op.SourceKind} {op.OperationId} {op.Note}");

            Assert.That(detectedPercentage, Is.GreaterThanOrEqualTo(minAccuracy));
        }

        [Then(@"the operations data is")]
        public void ThenTheOperationsDataIs(Table table)
        {
            table.CompareToSet(_context.UnifiedOperations);
        }

        [Then(@"the imported operation data is")]
        public async Task ThenTheImportedOperationDataIs(Table table)
        {
            var operations = await _context.OperationsManager.GetTransformedUnifiedOperations(_context.AccountId);
            table.CompareToSet(operations);
        }

        [Then(@"the read fortis operations are")]
        public void ThenTheReadFortisOperationsAre(Table table)
        {
            table.CompareToSet(_context.ReadOperations.Cast<FortisOperation>());
        }
        
        [When(@"I Filter the details where number is higher than '(.*)'")]
        public void WhenIFilterTheDetailsWhereNumberIsHigherThan(string number)
        {
            if (!string.IsNullOrEmpty(number))
            {
                _context.UnifiedOperations = _context.UnifiedOperations.Where(t => string.CompareOrdinal(t.OperationId, number) > 0).ToList();
            }
        }

        [When(@"I parse the details of the files '(.*)'")]
        public async Task WhenIParseTheDetailsOfTheFiles(string filePath)
        {
            string[] files = { filePath };

            var attr = File.GetAttributes(filePath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                files = Directory.GetFiles(filePath, "*.csv");
            }

            var readTasks = files.Select(s =>
            {
                var sourceKind = CsvAccountOperationManager.DetectFileSourceKindFromFileName(s);
                var fmd = _context.CsvAccountOperationManager.GetDefaultFileMetadata(sourceKind);
                using (var fs = File.OpenRead(s))
                {
                    return _context.CsvAccountOperationManager.ReadAsync(fs, fmd);
                }
            });

            var operationsBatches = await Task.WhenAll(readTasks);
            _context.UnifiedOperations =
                operationsBatches.SelectMany(operations => operations)
                    .Select(operation => _context.Transformer.Apply(operation))
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
            _context.ReadOperations = await operationManager.ReadAsync(rawStream, operationManager.GetDefaultFileMetadata(SourceKind.FortisCsvExport));
        }

        [When(@"I store the operation details in file '(.*)'")]
        public async Task WhenIStoreTheOperationDetailsInFile(string targetFilePath)
        {
            using (var fs = File.Create(targetFilePath))
            {
                await _context.CsvAccountOperationManager.WriteAsync(fs, _context.UnifiedOperations.Cast<AccountOperationBase>().ToList());
            }
        }

        [When(@"I store the operation details in qif file '(.*)'")]
        public async Task WhenIStoreTheOperationDetailsInQifFile(string targetFilePath)
        {
            var qifData = _context.UnifiedOperations.ToQifData();
            using (var fs = File.Open(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var sw = new StreamWriter(fs))
            {
                await sw.WriteAsync(qifData);
            }
        }

        [When(@"I unify and transform the read operations")]
        public void WhenIUnifyAndTransformTheReadOperations()
        {
            _context.UnifiedOperations = _context.ReadOperations.Select(o => _context.Transformer.Apply(o)).SortByOperationIdDesc().ToList();
        }
    }
}