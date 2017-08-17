using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Operations.Classification.AccountOperations.Contracts;

namespace Operations.Classification.Managers.Imports
{
    public class ImportCommand
    {
        public ImportCommand()
        {
            Id = Guid.NewGuid();
        }

        public ImportCommand(Guid accountId)
            : this()
        {
            AccountId = accountId;
        }

        public ImportCommand(Guid accountId, string sourceName, SourceKind sourceKind)
            : this(accountId)
        {
            SourceName = sourceName;
            SourceKind = sourceKind;
            CreationDate = DateTime.UtcNow;
        }

        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public DateTime CreationDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SourceKind SourceKind { get; set; }

        public string SourceName { get; set; }

        public string Encoding { get; set; }

        public string Culture { get; set; }
    }
}