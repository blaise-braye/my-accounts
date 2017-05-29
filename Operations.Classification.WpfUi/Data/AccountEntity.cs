using System;

namespace Operations.Classification.WpfUi.Data
{
    public class AccountEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string GmgAccountName { get; set; }

        public decimal InitialBalance { get; set; }
    }
}