using System.Windows;

namespace Operations.Classification.WpfUi.Managers.Transactions
{
    /// <summary>
    ///     Interaction logic for TransactionsFileManagerView.xaml
    /// </summary>
    public partial class TransactionsFileManagerView
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