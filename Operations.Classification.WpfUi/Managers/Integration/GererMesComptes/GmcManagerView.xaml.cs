using System.Windows;
using System.Windows.Controls;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    /// <summary>
    ///     Interaction logic for GmcManagerView.xaml
    /// </summary>
    public partial class GmcManagerView : UserControl
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