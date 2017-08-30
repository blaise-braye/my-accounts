using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
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
        internal static readonly Dictionary<SourceKind, CsvConfiguration> DefaultCsvConfigurations = CreateDefaultCsvConfigurations();

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

        public Task<List<AccountOperationBase>> ReadAsync(Stream sourceStream, FileStructureMetadata structureMetadata)
        {
            return Task.Run(() => Read(sourceStream, structureMetadata).ToList());
        }

        public Task WriteAsync(Stream targetStream, IList<AccountOperationBase> operations)
        {
            return Task.Run(() => Write(targetStream, operations));
        }

        private static void Write(Stream targetStream, IList<AccountOperationBase> operations)
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
            writer.WriteRecords(operations);
            sw.Flush();
        }

        private static IEnumerable<AccountOperationBase> Read(Stream sourceStream, FileStructureMetadata structureMetadata)
        {
            SourceKind sourceKind = structureMetadata.SourceKind;
            var config = CreateCsvConfiguration(structureMetadata);
            using (var textReader = new StreamReader(sourceStream, config.Encoding, true, 1024, true))
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