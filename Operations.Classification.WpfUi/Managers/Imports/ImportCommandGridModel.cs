using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GalaSoft.MvvmLight;
using MyAccounts.Business.AccountOperations.Contracts;

namespace Operations.Classification.WpfUi.Managers.Imports
{
    public class ImportCommandGridModel : ObservableObject
    {
        [Display(AutoGenerateField = false)]
        [ReadOnly(true)]
        public Guid Id { get; set; }

        [ReadOnly(true)]
        public string SourceName { get; set; }

        [ReadOnly(true)]
        public DateTime CreationDate { get; set; }

        [ReadOnly(true)]
        public SourceKind SourceKind { get; set; }

        [ReadOnly(true)]
        public DateTime LastExecution { get; set; }

        [ReadOnly(true)]
        public bool Success { get; set; }

        [ReadOnly(true)]
        public int NewOperations { get; set; }

        [ReadOnly(true)]
        public int AlreadyKnown { get; set; }

        [ReadOnly(true)]
        public int NotCompliant { get; set; }
    }
}