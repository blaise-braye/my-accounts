using System;
using Newtonsoft.Json;

namespace Operations.Classification.Managers.Imports
{
    public class ImportExecutionImpact
    {
        public ImportExecutionImpact()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        public Guid Id { get; set; }

        public Guid CommandId { get; set; }

        public DateTime CreationDate { get; set; }

        [JsonIgnore]
        public bool Success => string.IsNullOrEmpty(Error);

        [JsonIgnore]
        public int Total => NewOperations + AlreadyKnown + NotCompliant;

        public int NewOperations { get; set; }

        public int AlreadyKnown { get; set; }

        public int NotCompliant { get; set; }

        public string Error { get; set; }
    }
}