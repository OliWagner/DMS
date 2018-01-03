using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WpfApp
{
    public static class Helpers
    {
        public static void BindWidth(this FrameworkElement bindMe, FrameworkElement toMe)
        {
            Binding b = new Binding();
            b.Mode = BindingMode.OneWay;
            b.Source = toMe.ActualWidth - 50;
            bindMe.SetBinding(FrameworkElement.WidthProperty, b);
        }
    }
}
