using System.Windows;
using System.Windows.Controls;

namespace Operations.Classification.WpfUi.Managers.Integration.GererMesComptes
{
    /// <summary>
    ///     Interaction logic for GmgManagerView.xaml
    /// </summary>
    public partial class GmgManagerView : UserControl
    {
        public GmgManagerView()
        {
            InitializeComponent();
        }

        private void ShowPopup(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = true;
        }
    }
}