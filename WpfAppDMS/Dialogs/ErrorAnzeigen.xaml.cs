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
using System.Windows.Shapes;

namespace WpfAppDMS
{
    /// <summary>
    /// Interaktionslogik für ErrorAnzeigen.xaml
    /// </summary>
    public partial class ErrorAnzeigen : Window
    {
        public ErrorAnzeigen(string stackTrace, string message, string ownCode)
        {
            InitializeComponent();
            lblMessage.Content = "Fehlermeldung: " +message;
            lblStack.Content = "Stack: " + stackTrace;
            lblOwnCode.Content = "Bitte übersenden Sie uns auch den folgenden Code: " + ownCode;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
