using System;
using System.ComponentModel.DataAnnotations;
using QifApi.Transactions;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    public class BasicTransactionModel
    {
        [DisplayFormat(DataFormatString = "d")]
        public DateTime Date { get; set; }

        public string Number { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Amount { get; set; }

        public string Memo { get; set; }

        public string Payee { get; set; }

        public string Category { get; set; }

        [Display(AutoGenerateField = false)]
        public BasicTransaction SourceItem { get; set; }
    }
}