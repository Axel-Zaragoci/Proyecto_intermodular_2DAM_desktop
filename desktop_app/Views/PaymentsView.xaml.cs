using System.Windows;
using System.Windows.Controls;

namespace desktop_app.Views;

public partial class PaymentsView : UserControl
{
    public PaymentsView()
    {
        InitializeComponent();
    }
    
    private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not ListView listView || listView.View is not GridView gridView)
            return;

        int fixedColumns = 1;
        double fixedWidth = gridView.Columns[^1].Width;

        double availableWidth = listView.ActualWidth - fixedWidth - 10;
        double autoWidth = availableWidth / (gridView.Columns.Count - fixedColumns);

        for (int i = 0; i < gridView.Columns.Count - fixedColumns; i++)
        {
            gridView.Columns[i].Width = autoWidth;
        }
    }
}