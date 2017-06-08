using System.Windows;
using System.Windows.Controls;

namespace Operations.Classification.WpfUi.Technical.Controls
{
    public class PopupContent : ContentControl
    {
        static PopupContent()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupContent), new FrameworkPropertyMetadata(typeof(PopupContent)));
        }
    }
}