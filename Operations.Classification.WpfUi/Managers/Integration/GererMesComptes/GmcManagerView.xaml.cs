using System.Windows;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    /// <summary>
    ///     Interaction logic for GmcManagerView.xaml
    /// </summary>
    public partial class GmcManagerView
    {
        public GmcManagerView()
        {
            InitializeComponent();
        }

        private void ShowPopup(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = true;
        }
    }
}