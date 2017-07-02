using System.Windows;
using System.Windows.Controls;

namespace Operations.Classification.WpfUi.Managers.Transactions
{
    /// <summary>
    ///     Interaction logic for TransactionsFileManagerView.xaml
    /// </summary>
    public partial class TransactionsFileManagerView : UserControl
    {
        public TransactionsFileManagerView()
        {
            InitializeComponent();
        }

        private void ShowPopup(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = true;
        }
    }
}