using System.Windows;
using System.Windows.Controls;

namespace Operations.Classification.WpfUi.Managers.Reports
{
    /// <summary>
    /// Interaction logic for DatshboardView.xaml
    /// </summary>
    public partial class DatshboardView : UserControl
    {
        public DatshboardView()
        {
            InitializeComponent();
        }

        private void ShowPopup(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = true;
        }

        private void CmdRecurrenceFocus_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mts = (MetricTemplateSelector)Resources["MetricTemplateSelector"];
            mts.Focus = CmdRecurrenceFocus.SelectedItem?.ToString();
            RecurrenceGrid.Items.Refresh();
        }
    }
}
