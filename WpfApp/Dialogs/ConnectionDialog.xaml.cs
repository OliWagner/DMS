using System.Windows;

namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        public string DataSource
        {
            get { return txtDataSource.Text; }
        }

        public string InitialCatalog
        {
            get { return txtInitialCatalog.Text; }
        }

        public string UserName
        {
            get { return txtUserName.Text; }
        }

        public string Password
        {
            get { return txtPassword.Text; }
        }



        public ConnectionDialog()
        {
            InitializeComponent();
            txtDataSource.Text = "LAPTOP-CTMG3F1D\\SQLEXPRESS";
            txtInitialCatalog.Text = "OKOrganizer";
            txtUserName.Text = "sa";
            txtPassword.Text = "95hjh11!";
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //Wird in Mainindow behandelt, schließt die Applikation
        }
    }
}
