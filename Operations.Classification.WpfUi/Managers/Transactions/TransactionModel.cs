using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GalaSoft.MvvmLight;
using MyAccounts.Business.AccountOperations.Contracts;

namespace Operations.Classification.WpfUi.Managers.Transactions
{
    public partial class TransactionModel : ObservableObject
    {
        private string _category;

        [ReadOnly(true)]
        public string OperationId { get; set; }

        [ReadOnly(true)]
        public string PatternName { get; set; }

        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "d")]
        public DateTime ExecutionDate { get; set; }

        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "d")]
        public DateTime ValueDate { get; set; }

        [ReadOnly(true)]
        [Display(AutoGenerateField = false)]
        public string Currency { get; set; }
        
        public string Category
        {
            get => _category;
            set { Set(() => Category, ref _category, value); }
        }

        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Income { get; set; }

        [ReadOnly(true)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Outcome { get; set; }

        [ReadOnly(true)]
        public string ThirdParty { get; set; }

        [ReadOnly(true)]
        public string IBAN { get; set; }

        [ReadOnly(true)]
        public string BIC { get; set; }

        [ReadOnly(true)]
        public string Address { get; set; }

        [ReadOnly(true)]
        public string City { get; set; }

        [ReadOnly(true)]
        public string Mandat { get; set; }

        [ReadOnly(true)]
        public string ThirdPartyOperationRef { get; set; }

        [ReadOnly(true)]
        public string Communication { get; set; }

        [ReadOnly(true)]
        public string Note { get; set; }

        [ReadOnly(true)]
        public SourceKind SourceKind { get; set; }

        [Display(AutoGenerateField = false)]
        public string Account { get; set; }
    }
}