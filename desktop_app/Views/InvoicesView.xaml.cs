using System.Windows;
using System.Windows.Controls;

namespace desktop_app.Views;

public partial class InvoicesView : UserControl
{
    public InvoicesView()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// Método que calcula el ancho de las columnas con datos de la tabla
    /// </summary>
    /// 
    /// <param name="sender">
    /// </param>
    /// <param name="e">
    /// </param>
    private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        Console.WriteLine("Size Changed ejecutado");
        
        if (sender is not ListView listView || listView.View is not GridView gridView)
            return;

        int fixedColumns = 2;
        double buttonColumnWidth = 75;

        double availableWidth = listView.ActualWidth - (buttonColumnWidth * fixedColumns) - 10;

        if (availableWidth > 0 && !double.IsNaN(availableWidth))
        {
            int dataColumns = gridView.Columns.Count - fixedColumns;
            double columnWidth = availableWidth / dataColumns;

            for (int i = 0; i < dataColumns; i++)
            {
                gridView.Columns[i].Width = columnWidth;
            }

            for (int i = dataColumns; i < gridView.Columns.Count; i++)
            {
                gridView.Columns[i].Width = buttonColumnWidth;
            }
        }
    }
}