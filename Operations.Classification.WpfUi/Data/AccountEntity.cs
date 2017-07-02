using System;

namespace Operations.Classification.WpfUi.Data
{
    public class AccountEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string GmcAccountName { get; set; }

        public decimal InitialBalance { get; set; }
    }
}