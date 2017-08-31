using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyAccounts.Business.AccountOperations.Contracts;
using Operations.Classification.WpfUi.Technical.ChangeTracking;

namespace Operations.Classification.WpfUi.Managers.Imports
{
    public class ImportEditorViewModel : ObservableObject, IEditableObject
    {
        private readonly DataTracker _dataTracker = new DataTracker();

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
            set => Set(() => SourceName, ref _sourceName, value);
        }

        public string Encoding
        {
            get => _encoding;
            set => Set(() => Encoding, ref _encoding, value);
        }

        public string Culture
        {
            get => _culture;
            set => Set(() => Culture, ref _culture, value);
        }

        public ICommand CommitCommand { get; set; }

        public ICommand SelectFilesToImportCommand { get; set; }

        public RelayCommand ResetDefaultSourceKindMetadataCommand { get; set; }

        public IEnumerable<SourceKind> SourceKinds => Enum.GetValues(typeof(SourceKind)).Cast<SourceKind>();

        public IEnumerable<CultureInfo> Cultures => CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures).ToList();

        public void FillFromDirtyProperties(object targetData)
        {
            _dataTracker.FillFromDirtyProperties(targetData);
        }

        public void BeginEdit()
        {
            _dataTracker.StartTracking(this);
        }

        public void EndEdit()
        {
            _dataTracker.StopTracking();
        }

        public void CancelEdit()
        {
            _dataTracker.StopTracking();
        }

        public bool IsDirty()
        {
            return _dataTracker.IsDirty;
        }
    }
}
