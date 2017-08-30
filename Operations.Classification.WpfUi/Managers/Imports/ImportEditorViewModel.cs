using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyAccounts.Business.AccountOperations.Contracts;
using MyAccounts.Business.Managers.Imports;

namespace Operations.Classification.WpfUi.Managers.Imports
{
    public class ImportEditorViewModel : ViewModelBase
    {
        private readonly List<Action<ImportCommand>> _changeSet = new List<Action<ImportCommand>>();

        private SourceKind _sourceKind;

        private string _filePaths;

        private string _sourceName;

        private string _encoding;

        private string _culture;

        public Guid? Id { get; set; }

        public bool IsNew => !Id.HasValue;

        public SourceKind SourceKind
        {
            get => _sourceKind;
            set
            {
                if (Set(() => SourceKind, ref _sourceKind, value))
                {
                    ResetDefaultSourceKindMetadataCommand?.Execute(null);
                }
            }
        }

        public string FilePaths
        {
            get => _filePaths;
            set => Set(nameof(FilePaths), ref _filePaths, value);
        }

        public bool DisplayFilePaths => string.IsNullOrEmpty(SourceName);

        public bool DisplaySourceName => !string.IsNullOrEmpty(SourceName);

        public string SourceName
        {
            get => _sourceName;
            set
            {
                if (Set(() => SourceName, ref _sourceName, value))
                {
                    _changeSet.Add(c => c.SourceName = value);
                }
            }
        }

        public string Encoding
        {
            get => _encoding;
            set
            {
                if (Set(() => Encoding, ref _encoding, value))
                {
                    _changeSet.Add(c => c.Encoding = value);
                }
            }
        }

        public string Culture
        {
            get => _culture;
            set
            {
                if (Set(() => Culture, ref _culture, value))
                {
                    _changeSet.Add(c => c.Culture = value);
                }
            }
        }

        public ICommand CommitCommand { get; set; }

        public ICommand SelectFilesToImportCommand { get; set; }

        public RelayCommand ResetDefaultSourceKindMetadataCommand { get; set; }

        public IEnumerable<SourceKind> SourceKinds => Enum.GetValues(typeof(SourceKind)).Cast<SourceKind>();

        public IEnumerable<CultureInfo> Cultures => CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures).ToList();

        public void FillMetadataFromChangeSet(ImportCommand importCommand)
        {
            foreach (var action in _changeSet)
            {
                action(importCommand);
            }
        }

        public ImportEditorViewModel ResetChangeSet()
        {
            _changeSet.Clear();
            return this;
        }

        public bool IsDirty()
        {
            return _changeSet.Any();
        }
    }
}
