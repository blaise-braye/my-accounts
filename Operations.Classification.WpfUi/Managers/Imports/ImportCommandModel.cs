using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GalaSoft.MvvmLight;
using Operations.Classification.AccountOperations.Contracts;

namespace Operations.Classification.WpfUi.Managers.Imports
{
    public class ImportCommandModel : ObservableObject
    {
        private string _culture;

        private string _encoding;

        private SourceKind _sourceKind;

        [Display(AutoGenerateField = false)]
        [ReadOnly(true)]
        public Guid Id { get; set; }

        [Display(AutoGenerateField = false)]
        [ReadOnly(true)]
        public Guid AccountId { get; set; }

        [ReadOnly(true)]
        public string SourceName { get; set; }

        [ReadOnly(true)]
        public DateTime CreationDate { get; set; }

        [Required]
        public SourceKind SourceKind
        {
            get => _sourceKind;
            set => Set(nameof(SourceKind), ref _sourceKind, value);
        }

        public string Encoding
        {
            get => _encoding;
            set => Set(nameof(Encoding), ref _encoding, value);
        }

        public string Culture
        {
            get => _culture;
            set => Set(nameof(Culture), ref _culture, value);
        }
    }
}