using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

using Operations.Classification.AccountOperations.Contracts;
using Operations.Classification.AccountOperations.Fortis;
using Operations.Classification.AccountOperations.Sodexo;

namespace Operations.Classification.AccountOperations
{
    public class AccountOperationReader
    {
        private static readonly Dictionary<SourceKind, CsvConfiguration> _csvConfigurations = CreateCsvConfigurations();

        public IEnumerable<AccountOperationBase> Read(params string[] files)
        {
            foreach (var file in files)
            {
                SourceKind sourceKind = GetSourceKindFromFileName(file);

                var config = _csvConfigurations[sourceKind];

                using (var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var sr = new StreamReader(fs, config.Encoding))
                {
                    using (var reader = new CsvReader(sr, config))
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

                            record.SourceKind = sourceKind;

                            yield return record;
                        }
                    }
                }
            }
        }

        private static SourceKind GetSourceKindFromFileName(string file)
        {
            file = file.ToLower();
            var fileName = Path.GetFileName(file);
            Debug.Assert(fileName != null, "fileName != null");

            SourceKind sourceKind = SourceKind.Unknwon;

            if (file.Contains("fortis"))
            {
                if (fileName.Contains("archive"))
                {
                    sourceKind = SourceKind.FortisCsvArchive;
                }
                else if (fileName.Contains("export"))
                {
                    sourceKind = SourceKind.FortisCsvExport;
                }
            }
            else if (file.Contains("sodexo"))
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
