using System;
using System.ComponentModel.DataAnnotations;
using MyAccounts.Business.AccountOperations.Contracts;
using Operations.Classification.WpfUi.Managers.Accounts.Models;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    public class DashboardOperationModel
    {
        [Display(AutoGenerateField = false)]
        public Guid UId { get; set; }

        [Display(AutoGenerateField = false)]
        public AccountViewModel Account { get; set; }

        public string OperationId { get; set; }

        public string PatternName { get; set; }

        [DisplayFormat(DataFormatString = "d")]
        public DateTime ExecutionDate { get; set; }

        [DisplayFormat(DataFormatString = "d")]
        public DateTime ValueDate { get; set; }

        [Display(AutoGenerateField = false)]
        public string Currency { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Income { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Outcome { get; set; }

        public string ThirdParty { get; set; }

        public string IBAN { get; set; }

        public string BIC { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Mandat { get; set; }

        public string ThirdPartyOperationRef { get; set; }

        public string Communication { get; set; }

        public string Note { get; set; }

        public SourceKind SourceKind { get; set; }
    }
}