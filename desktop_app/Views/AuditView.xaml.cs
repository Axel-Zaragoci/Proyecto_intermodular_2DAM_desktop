using System.Windows;
using System.Windows.Controls;

namespace desktop_app.Views;

public partial class AuditView : UserControl
{
    public AuditView()
    {
        InitializeComponent();
    }
    
    private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not ListView listView || listView.View is not GridView gridView)
            return;

        int fixedColumns = 4;
        double fixedWidth = 0;

        for (int i = 0; i < fixedColumns && i < gridView.Columns.Count; i++)
        {
            fixedWidth += gridView.Columns[i].Width;
        }

        double availableWidth = listView.ActualWidth - fixedWidth - 10;
        double autoWidth = availableWidth / (gridView.Columns.Count - fixedColumns);

        for (int i = fixedColumns; i < gridView.Columns.Count; i++)
        {
            gridView.Columns[i].Width = autoWidth;
        }
    }
}