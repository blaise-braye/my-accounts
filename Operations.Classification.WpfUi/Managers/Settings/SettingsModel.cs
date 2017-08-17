using GalaSoft.MvvmLight;

namespace Operations.Classification.WpfUi.Managers.Settings
{
    public class SettingsModel : ObservableObject
    {
        private string _culture;
        private string _gmcPassword;
        private string _gmcUserName;
        private bool _hideAmounts;
        private string _uiCulture;
        private string _workingFolder;

        public string WorkingFolder
        {
            get => _workingFolder;
            set => Set(nameof(WorkingFolder), ref _workingFolder, value);
        }

        public bool HideAmounts
        {
            get => _hideAmounts;
            set => Set(nameof(HideAmounts), ref _hideAmounts, value);
        }

        public string Culture
        {
            get => _culture;
            set => Set(nameof(Culture), ref _culture, value);
        }

        public string UiCulture
        {
            get => _uiCulture;
            set => Set(nameof(UiCulture), ref _uiCulture, value);
        }

        public string GmcUserName
        {
            get => _gmcUserName;
            set { Set(() => GmcUserName, ref _gmcUserName, value); }
        }

        public string GmcPassword
        {
            get => _gmcPassword;
            set { Set(() => GmcPassword, ref _gmcPassword, value); }
        }
    }
}