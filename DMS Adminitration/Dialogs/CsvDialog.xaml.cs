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

namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class CsvDialog : Window
    {
        public string Trenner
        {
            get { return cboTrenner.Text; }
        }

        public string boolTrueFalse
        {
            get { return cboBoolean.Text; }
        }

        public CsvDialog()
        {
            InitializeComponent();
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Nix
        }
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            if(cboBoolean.SelectedItem == null || cboTrenner.SelectedItem == null)
                 e.CanExecute = false;
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
