using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace desktop_app.Views;

public partial class FormBookingView : UserControl
{
    public FormBookingView()
    {
        InitializeComponent();
    }
    
    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.IsOpen = true;
    }
}