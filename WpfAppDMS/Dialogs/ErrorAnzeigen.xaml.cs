using System.Windows;

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
