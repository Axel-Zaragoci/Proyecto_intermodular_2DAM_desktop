using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using desktop_app.ViewModels;

namespace desktop_app.Views
{
    /// <summary>
    /// Lógica de interacción para UserView.xaml
    /// </summary>
    public partial class UserView : UserControl
    {
        public UserView()
        {
            InitializeComponent();
            DataContext = new UserViewModel();
        }
        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not ListView listView || listView.View is not GridView gv)
                return;

            double fixedWidth = gv.Columns[0].Width + gv.Columns[^1].Width + gv.Columns[^2].Width;

            double borders = listView.BorderThickness.Left + listView.BorderThickness.Right;
            double padding = listView.Padding.Left + listView.Padding.Right;

            double vScroll = SystemParameters.VerticalScrollBarWidth;

            const double fudge = 2;

            double available = Math.Max(0, listView.ActualWidth - fixedWidth - borders - padding - vScroll - fudge);

            int firstAuto = 1;
            int lastAuto = gv.Columns.Count - 3;
            int autoCount = lastAuto - firstAuto + 1;
            if (autoCount <= 0) return;

            double each = Math.Floor(available / autoCount);

            double used = each * (autoCount - 1);
            double last = Math.Max(0, available - used);

            for (int i = firstAuto; i < lastAuto; i++)
                gv.Columns[i].Width = each;

            gv.Columns[lastAuto].Width = last;
        }
    }
}
