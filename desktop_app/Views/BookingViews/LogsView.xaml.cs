using System.Windows;
using System.Windows.Controls;

namespace desktop_app.Views.BookingViews;

public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
    }
    
    private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not ListView listView || listView.View is not GridView gridView)
            return;

        double availableWidth = listView.ActualWidth - 10;

        for (int i = 0; i < gridView.Columns.Count; i++)
        {
            gridView.Columns[i].Width = (availableWidth / gridView.Columns.Count);
        }
    }
}