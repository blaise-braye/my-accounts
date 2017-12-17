using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using log4net;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.AccountOperations.Fortis;
using MyAccounts.Business.AccountOperations.Sodexo;
using MyAccounts.Business.AccountOperations.Unified;

namespace MyAccounts.Business.AccountOperations
{
    public interface ICsvAccountOperationManager
    {
        Task<List<AccountOperationBase>> ReadAsync(Stream sourceStream, FileStructureMetadata structureMetadata);

        Task WriteAsync(Stream targetStream, IList<AccountOperationBase> operations);
    }

    public class CsvAccountOperationManager : ICsvAccountOperationManager
    {
        internal static readonly Dictionary<SourceKind, Configuration> DefaultCsvConfigurations = CreateDefaultCsvConfigurations();

        private static readonly ILog _logger = LogManager.GetLogger(typeof(CsvAccountOperationManager));

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

        public async Task<List<AccountOperationBase>> ReadAsync(Stream sourceStream, FileStructureMetadata structureMetadata)
        {
            var result = new List<AccountOperationBase>();
            SourceKind sourceKind = structureMetadata.SourceKind;
            var config = CreateCsvConfiguration(structureMetadata);
            using (var textReader = new StreamReader(sourceStream, config.Encoding, true, 1024, true))
            using (var reader = new CsvReader(textReader, config))
            {
                while (await reader.ReadAsync())
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
                    result.Add(record);
                }
            }

            return result;
        }

        public async Task WriteAsync(Stream targetStream, IList<AccountOperationBase> operations)
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

            var config = DefaultCsvConfigurations[sourceKind];

            var sw = new StreamWriter(targetStream, config.Encoding);
            var writer = new CsvWriter(sw, config);
            writer.WriteHeader(type);
            await writer.NextRecordAsync();
            writer.WriteRecords((IEnumerable)operations);
            await sw.FlushAsync();
        }
        
        private static Configuration CreateCsvConfiguration(FileStructureMetadata fileStructureMetadata)
        {
            var csvConfiguration = CreateDefaultCsvConfiguration(fileStructureMetadata.SourceKind);
            csvConfiguration.CultureInfo = fileStructureMetadata.GetCultureInfo();
            csvConfiguration.Encoding = Encoding.GetEncoding(fileStructureMetadata.Encoding);
            return csvConfiguration;
        }

        private static Configuration CreateDefaultCsvConfiguration(SourceKind sourceKind)
        {
            var configuration = new Configuration();
            switch (sourceKind)
            {
                case SourceKind.FortisCsvArchive:
                    configuration.RegisterClassMap<FortisOperationArchiveCsvMap>();
                    configuration.Encoding = Encoding.UTF8;
                    configuration.CultureInfo = FileStructureMetadata.GetCultureInfo("fr-BE", ",");
                    configuration.Delimiter = ";";
                    configuration.QuoteAllFields = true;
                    configuration.TrimOptions = TrimOptions.InsideQuotes;
                    configuration.MissingFieldFound = LogMissingFieldFound;
                    configuration.HeaderValidated = LogHeaderValidatationFailed;
                    break;
                case SourceKind.FortisCsvExport:
                    configuration.RegisterClassMap<FortisOperationExportCsvMap>();
                    var ansiEncoding = Encoding.GetEncoding(1252);
                    configuration.Encoding = ansiEncoding;
                    configuration.CultureInfo = FileStructureMetadata.GetCultureInfo("fr-BE", ".");
                    configuration.Delimiter = ";";
                    configuration.QuoteAllFields = false;
                    configuration.TrimOptions = TrimOptions.Trim;
                    configuration.MissingFieldFound = LogMissingFieldFound;
                    configuration.HeaderValidated = LogHeaderValidatationFailed;
                    break;
                case SourceKind.SodexoCsvExport:
                    configuration.RegisterClassMap<SodexoOperationCsvMap>();
                    configuration.Encoding = Encoding.UTF8;
                    configuration.CultureInfo = FileStructureMetadata.GetCultureInfo("fr-BE", ".");
                    configuration.Delimiter = ";";
                    configuration.QuoteAllFields = true;
                    configuration.TrimOptions = TrimOptions.InsideQuotes;
                    configuration.MissingFieldFound = LogMissingFieldFound;
                    configuration.HeaderValidated = LogHeaderValidatationFailed;
                    break;
                case SourceKind.InternalCsvExport:
                    configuration.RegisterClassMap<UnifiedAccountOperationCsvMap>();
                    configuration.Encoding = Encoding.UTF8;
                    configuration.CultureInfo = FileStructureMetadata.GetCultureInfo("fr-BE", ".");
                    configuration.Delimiter = ";";
                    configuration.QuoteAllFields = true;
                    configuration.TrimOptions = TrimOptions.InsideQuotes;
                    configuration.MissingFieldFound = LogMissingFieldFound;
                    configuration.HeaderValidated = LogHeaderValidatationFailed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceKind), sourceKind, null);
            }

            return configuration;
        }
        
        private static void LogMissingFieldFound(string[] headerNames, int index, IReadingContext context)
        {
            if (headerNames != null && headerNames.Length != 0)
            {
                var message =
                    $"Field with names ['{string.Join("', '", headerNames)}'] at index '{index}' does not exist.";
                _logger.Warn(message);
            }
            else
            {
                var msg = $"Field at index '{index}' does not exist";
                _logger.Warn(msg);
            }
        }

        private static void LogHeaderValidatationFailed(bool isValid, string[] headerNames, int headerNameIndex, IReadingContext context)
        {
            if (!isValid)
            {
                string message = string.Format("Header matching ['{0}'] names at index {1} was not found. ", string.Join("', '", headerNames), headerNameIndex);
                _logger.Warn(message);
            }
        }

        private static Dictionary<SourceKind, Configuration> CreateDefaultCsvConfigurations()
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