using System.Windows;

namespace Operations.Classification.WpfUi.Managers.Settings
{
    /// <summary>
    /// Interaction logic for SettingsEditorView.xaml
    /// </summary>
    public partial class SettingsEditorView
    {
        public SettingsEditorView()
        {
            InitializeComponent();
        }

        private void TxtGmcPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var model = (SettingsModel)TxtGmcPassword.DataContext;
            model.GmcPassword = TxtGmcPassword.Password;
        }
    }
}
