using System;
using Operations.Classification.AccountOperations.Contracts;

namespace Operations.Classification.AccountOperations.Sodexo
{
    public class SodexoOperation : AccountOperationBase
    {
        public DateTime Date { get; set; }

        public string Detail { get; set; }

        public string Amount { get; set; }
    }
}