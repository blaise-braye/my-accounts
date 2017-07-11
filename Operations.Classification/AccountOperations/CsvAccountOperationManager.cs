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
using Operations.Classification.AccountOperations.Unified;

namespace Operations.Classification.AccountOperations
{
    public interface ICsvAccountOperationManager
    {
        IList<AccountOperationBase> Read(string file, SourceKind sourceKind);

        IEnumerable<AccountOperationBase> Read(Stream stream, SourceKind sourceKind);

        Task<IList<AccountOperationBase>> ReadAsync(string file, SourceKind sourceKind);

        Task<List<AccountOperationBase>> ReadAsync(Stream stream, SourceKind sourceKind);

        void Write(string targetFile, IList<AccountOperationBase> operations);

        Task WriteAsync(string targetFile, IList<AccountOperationBase> operations);
    }

    public class CsvAccountOperationManager : ICsvAccountOperationManager
    {
        private static readonly Dictionary<SourceKind, CsvConfiguration> _csvConfigurations = CreateCsvConfigurations();

        public static SourceKind DetectFileSourceKindFromFileName(string file)
        {
            var sourceKind = SourceKind.Unknwon;
            var upperCasedFile = file.ToUpper();
            if (upperCasedFile.Contains("FORTIS"))
            {
                if (upperCasedFile.Contains("ARCHIVE"))
                {
                    sourceKind = SourceKind.FortisCsvArchive;
                }
                else if (upperCasedFile.Contains("EXPORT"))
                {
                    sourceKind = SourceKind.FortisCsvExport;
                }
            }
            else if (upperCasedFile.Contains("SODEXO"))
            {
                sourceKind = SourceKind.SodexoCsvExport;
            }

            return sourceKind;
        }

        public static SourceKind DetectSourceKindFromFileContent(string file)
        {
            var supportedSourceKinds = _csvConfigurations.Keys.Except(new[] { SourceKind.Unknwon });

            var result = supportedSourceKinds.Select(
                    sourceKind =>
                    {
                        var config = _csvConfigurations[sourceKind];

                        FileStream fs = null;
                        StreamReader sr = null;
                        try
                        {
                            fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                            sr = new StreamReader(fs);
                            fs = null;
                            using (var reader = new CsvReader(sr, config))
                            {
                                sr = null;

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
                            sr?.Dispose();
                            fs?.Dispose();
                        }

                        return Tuple.Create(false, SourceKind.Unknwon);
                    })
                .Where(t => t.Item1)
                .Select(t => t.Item2)
                .DefaultIfEmpty(SourceKind.Unknwon)
                .FirstOrDefault();

            return result;
        }

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
                throw new InvalidOperationException(
                    "All operations are expected to be of same source kind and same type");
            }

            var config = _csvConfigurations[sourceKind];

            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = File.Open(targetFile, FileMode.Create, FileAccess.Write, FileShare.None);
                sw = new StreamWriter(fs, config.Encoding);
                fs = null;
                using (var reader = new CsvWriter(sw, config))
                {
                    sw = null;
                    reader.WriteHeader(type);
                    reader.WriteRecords(operations);
                }
            }
            finally
            {
                sw?.Dispose();
                fs?.Dispose();
            }
        }

        public Task<List<AccountOperationBase>> ReadAsync(Stream stream, SourceKind sourceKind)
        {
            return Task.Run(() => Read(stream, sourceKind).ToList());
        }

        public IEnumerable<AccountOperationBase> Read(Stream stream, SourceKind sourceKind)
        {
            var config = _csvConfigurations[sourceKind];
            using (var textReader = new StreamReader(stream, config.Encoding))
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
                        case SourceKind.InternalExport:
                            record = reader.GetRecord<UnifiedAccountOperation>();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(sourceKind));
                    }

                    record.SourceKind = sourceKind;

                    yield return record;
                }
            }
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

            var internalExport = new CsvConfiguration();
            sodexoExport.Encoding = Encoding.UTF8;
            internalExport.Encoding = ansiEncoding;
            internalExport.Delimiter = ";";
            internalExport.QuoteAllFields = false;
            internalExport.TrimFields = true;
            internalExport.TrimHeaders = true;
            internalExport.WillThrowOnMissingField = false;

            return new Dictionary<SourceKind, CsvConfiguration>
            {
                { SourceKind.FortisCsvExport, export },
                { SourceKind.FortisCsvArchive, archive },
                { SourceKind.SodexoCsvExport, sodexoExport },
                { SourceKind.InternalExport, internalExport }
            };
        }
    }
}