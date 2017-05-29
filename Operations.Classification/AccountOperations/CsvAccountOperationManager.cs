using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Fortis;
using Operations.Classification.AccountOperations.Sodexo;

namespace Operations.Classification.AccountOperations
{
    public interface ICsvAccountOperationManager
    {
        Task<IList<AccountOperationBase>> ReadAsync(string file, SourceKind sourceKind);
        IList<AccountOperationBase> Read(string file, SourceKind sourceKind);

        Task<List<AccountOperationBase>> ReadAsync(Stream stream, SourceKind sourceKind);
        IEnumerable<AccountOperationBase> Read(Stream stream, SourceKind sourceKind);

        Task WriteAsync(string targetFile, IList<AccountOperationBase> operations);
        void Write(string targetFile, IList<AccountOperationBase> operations);
    }

    public class CsvAccountOperationManager : ICsvAccountOperationManager
    {
        private static readonly Dictionary<SourceKind, CsvConfiguration> _csvConfigurations = CreateCsvConfigurations();
        
        public Task<IList<AccountOperationBase>> ReadAsync(string file, SourceKind sourceKind)
        {
            return Task.Run(() => Read(file, sourceKind));
        }

        public IList<AccountOperationBase> Read(string file, SourceKind sourceKind)
        {
            using (var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs, sourceKind).ToList();
            }
        }

        public Task WriteAsync(string targetFile, IList<AccountOperationBase> operations)
        {
            return Task.Run(() => Write(targetFile, operations));
        }

        public void Write(string targetFile, IList<AccountOperationBase> operations)
        {
            if (operations.Count == 0)
            {
                return;
            }

            var type = operations[0].GetType();
            var sourceKind = operations[0].SourceKind;
            if (!operations.All(o => o.SourceKind == sourceKind && o.GetType() == type))
            {
                throw new InvalidOperationException("All operations are expected to be of same source kind and same type");
            }

            var config = _csvConfigurations[sourceKind];

            using(var fs = File.Open(targetFile, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var sw = new StreamWriter(fs, config.Encoding))
            {
                using (var reader = new CsvWriter(sw, config))
                {
                    reader.WriteHeader(type);
                    reader.WriteRecords(operations);
                }
            }
        }
        
        public Task<List<AccountOperationBase>> ReadAsync(Stream stream, SourceKind sourceKind)
        {
            return Task.Run(() => Read(stream, sourceKind).ToList());
        }

        public IEnumerable<AccountOperationBase> Read(Stream stream, SourceKind sourceKind)
        {
            var config = _csvConfigurations[sourceKind];
            using(var textReader = new StreamReader(stream, config.Encoding))
            using (var reader = new CsvReader(textReader, config))
            {
                while (reader.Read())
                {
                    AccountOperationBase record;

                    switch (sourceKind)
                    {
                        case SourceKind.FortisCsvArchive:
                        case SourceKind.FortisCsvExport:
                            record = reader.GetRecord<FortisOperation>();
                            break;
                        case SourceKind.SodexoCsvExport:
                            record = reader.GetRecord<SodexoOperation>();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(sourceKind));
                    }

                    var forisop = record as FortisOperation;
                    if (forisop != null && String.IsNullOrEmpty(forisop.Reference))
                    {

                    }

                    record.SourceKind = sourceKind;

                    yield return record;
                }
            }
        }

        public static SourceKind DetectSourceKindFromFileContent(string file)
        {
            var supportedSourceKinds = _csvConfigurations.Keys.Except(new[] { SourceKind.Unknwon });

            var result = supportedSourceKinds.Select(
                    sourceKind =>
                    {
                        var config = _csvConfigurations[sourceKind];
                        try
                        {
                            using (var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                            using (var sr = new StreamReader(fs))
                            using (var reader = new CsvReader(sr, config))
                            {
                                if (reader.Read())
                                {
                                    sourceKind = reader.GetField<SourceKind>(nameof(SourceKind));
                                    if (sourceKind != SourceKind.Unknwon)
                                    {
                                        return Tuple.Create(true, sourceKind);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        return Tuple.Create(false, SourceKind.Unknwon);
                    })
                .Where(t => t.Item1)
                .Select(t => t.Item2)
                .DefaultIfEmpty(SourceKind.Unknwon)
                .FirstOrDefault();
            
            return result;
        }

        public static SourceKind DetectFileSourceKindFromFileName(string file)
        {
            var sourceKind = SourceKind.Unknwon;
            var lFile = file.ToUpper();
            if (lFile.Contains("FORTIS"))
            {
                if (lFile.Contains("ARCHIVE"))
                    sourceKind = SourceKind.FortisCsvArchive;
                else if (lFile.Contains("EXPORT"))
                    sourceKind = SourceKind.FortisCsvExport;
            }
            else if (lFile.Contains("SODEXO"))
            {
                sourceKind = SourceKind.SodexoCsvExport;
            }

            return sourceKind;
        }

        private static Dictionary<SourceKind, CsvConfiguration> CreateCsvConfigurations()
        {
            var export = new CsvConfiguration();
            export.RegisterClassMap<FortisOperationExportCsvMap>();
            var ansiEncoding = Encoding.GetEncoding(1252);
            export.Encoding = ansiEncoding;
            export.Delimiter = ";";
            export.QuoteAllFields = false;
            export.TrimFields = true;
            export.TrimHeaders = true;
            export.WillThrowOnMissingField = false;

            var archive = new CsvConfiguration();
            archive.RegisterClassMap<FortisOperationArchiveCsvMap>();
            archive.Encoding = Encoding.UTF8;
            archive.Delimiter = ";";
            archive.QuoteAllFields = true;
            archive.TrimFields = true;
            archive.TrimHeaders = true;
            archive.WillThrowOnMissingField = false;

            var sodexoExport = new CsvConfiguration();
            sodexoExport.RegisterClassMap<SodexoOperationCsvMap>();
            sodexoExport.Encoding = Encoding.UTF8;
            sodexoExport.Delimiter = ";";
            sodexoExport.QuoteAllFields = true;
            sodexoExport.TrimFields = true;
            sodexoExport.TrimHeaders = true;
            sodexoExport.WillThrowOnMissingField = false;

            return new Dictionary<SourceKind, CsvConfiguration>
            {
                           { SourceKind.FortisCsvExport, export },
                           { SourceKind.FortisCsvArchive, archive },
                           { SourceKind.SodexoCsvExport, sodexoExport }
                       };
        }
    }
}
