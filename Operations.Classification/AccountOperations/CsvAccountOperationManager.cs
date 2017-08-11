using System;
using System.Collections.Generic;
using System.Globalization;
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
        FileStructureMetadata GetDefaultFileMetadata(SourceKind sourceKind);

        Task<IList<AccountOperationBase>> ReadAsync(string file, FileStructureMetadata structureMetadata);

        Task<List<AccountOperationBase>> ReadAsync(Stream stream, FileStructureMetadata structureMetadata);

        Task WriteAsync(string targetFile, IList<AccountOperationBase> operations);
    }

    public class CsvAccountOperationManager : ICsvAccountOperationManager
    {
        private static readonly Dictionary<SourceKind, CsvConfiguration> _defaultCsvConfigurations = CreateDefaultCsvConfigurations();

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
            var supportedSourceKinds = _defaultCsvConfigurations.Keys.Except(new[] { SourceKind.Unknwon });

            var result = supportedSourceKinds.Select(
                    sourceKind =>
                    {
                        var config = _defaultCsvConfigurations[sourceKind];

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

        public FileStructureMetadata GetDefaultFileMetadata(SourceKind sourceKind)
        {
            var metadata = new FileStructureMetadata { SourceKind = sourceKind };

            if (_defaultCsvConfigurations.ContainsKey(sourceKind))
            {
                var defaultConfig = _defaultCsvConfigurations[sourceKind];
                metadata.Encoding = defaultConfig.Encoding.WebName;
                metadata.Culture = defaultConfig.CultureInfo.Name;
            }

            return metadata;
        }

        public Task<IList<AccountOperationBase>> ReadAsync(string file, FileStructureMetadata structureMetadata)
        {
            return Task.Run(() => Read(file, structureMetadata));
        }

        public Task<List<AccountOperationBase>> ReadAsync(Stream stream, FileStructureMetadata structureMetadata)
        {
            return Task.Run(() => Read(stream, structureMetadata).ToList());
        }

        public Task WriteAsync(string targetFile, IList<AccountOperationBase> operations)
        {
            return Task.Run(() => Write(targetFile, operations));
        }

        private static void Write(string targetFile, IList<AccountOperationBase> operations)
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

            var config = _defaultCsvConfigurations[sourceKind];

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

        private static IList<AccountOperationBase> Read(string file, FileStructureMetadata structureMetadata)
        {
            using (var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs, structureMetadata).ToList();
            }
        }

        private static IEnumerable<AccountOperationBase> Read(Stream stream, FileStructureMetadata structureMetadata)
        {
            SourceKind sourceKind = structureMetadata.SourceKind;
            var config = CreateCsvConfiguration(structureMetadata);
            using (var textReader = new StreamReader(stream, config.Encoding, true, 1024, true))
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
                        case SourceKind.InternalCsvExport:
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

        private static CsvConfiguration CreateCsvConfiguration(FileStructureMetadata fileStructureMetadata)
        {
            var csvConfiguration = CreateDefaultCsvConfiguration(fileStructureMetadata.SourceKind);
            csvConfiguration.CultureInfo = CultureInfo.GetCultureInfo(fileStructureMetadata.Culture);
            csvConfiguration.Encoding = Encoding.GetEncoding(fileStructureMetadata.Encoding);
            return csvConfiguration;
        }

        private static CsvConfiguration CreateDefaultCsvConfiguration(SourceKind sourceKind)
        {
            var configuration = new CsvConfiguration();
            switch (sourceKind)
            {
                case SourceKind.FortisCsvArchive:
                    configuration.RegisterClassMap<FortisOperationArchiveCsvMap>();
                    configuration.Encoding = Encoding.UTF8;
                    configuration.CultureInfo = CultureInfo.GetCultureInfo("fr-BE");
                    configuration.Delimiter = ";";
                    configuration.QuoteAllFields = true;
                    configuration.TrimFields = true;
                    configuration.TrimHeaders = true;
                    configuration.WillThrowOnMissingField = false;
                    break;
                case SourceKind.FortisCsvExport:
                    configuration.RegisterClassMap<FortisOperationExportCsvMap>();
                    var ansiEncoding = Encoding.GetEncoding(1252);
                    configuration.Encoding = ansiEncoding;
                    configuration.CultureInfo = CultureInfo.GetCultureInfo("fr-BE");
                    configuration.Delimiter = ";";
                    configuration.QuoteAllFields = false;
                    configuration.TrimFields = true;
                    configuration.TrimHeaders = true;
                    configuration.WillThrowOnMissingField = false;
                    break;
                case SourceKind.SodexoCsvExport:
                    configuration.RegisterClassMap<SodexoOperationCsvMap>();
                    configuration.Encoding = Encoding.UTF8;
                    configuration.CultureInfo = CultureInfo.GetCultureInfo("fr-BE");
                    configuration.Delimiter = ";";
                    configuration.QuoteAllFields = true;
                    configuration.TrimFields = true;
                    configuration.TrimHeaders = true;
                    configuration.WillThrowOnMissingField = false;
                    break;
                case SourceKind.InternalCsvExport:
                    configuration.RegisterClassMap<UnifiedAccountOperationCsvMap>();
                    configuration.Encoding = Encoding.UTF8;
                    configuration.CultureInfo = CultureInfo.GetCultureInfo("fr-BE");
                    configuration.Delimiter = ";";
                    configuration.QuoteAllFields = true;
                    configuration.TrimFields = true;
                    configuration.TrimHeaders = true;
                    configuration.WillThrowOnMissingField = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceKind), sourceKind, null);
            }

            return configuration;
        }

        private static Dictionary<SourceKind, CsvConfiguration> CreateDefaultCsvConfigurations()
        {
            var supported = new[]
                {
                    SourceKind.FortisCsvExport,
                    SourceKind.FortisCsvArchive,
                    SourceKind.SodexoCsvExport,
                    SourceKind.InternalCsvExport
                };

            var result = supported.Select(key => new { Key = key, Value = CreateDefaultCsvConfiguration(key) })
                .ToDictionary(i => i.Key, i => i.Value);

            return result;
        }
    }
}