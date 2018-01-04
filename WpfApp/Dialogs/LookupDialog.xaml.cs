using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class LookupDialog : Window
    {
        private List<Tuple<string, string, string>> tupleList;

        public string Tabelle
        {
            get { return cboTabelle.Text; }
        }

        public string Feld
        {
            get { return cboFeld.Text; }
        }

        public LookupDialog()
        {
            InitializeComponent();
            List<string> strList = new List<string>();
            tupleList = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields();
            foreach (var item in tupleList)
            {
                strList.Add(item.Item1);
            }
            cboTabelle.ItemsSource = strList;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void cboTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _tabName = (cboTabelle.SelectedItem);
            if (!_tabName.Equals("")) {
                cboFeld.IsEnabled = true;
                foreach (var item in tupleList)
                {
                    if (item.Item1.Equals(_tabName)) {
                        cboFeld.ItemsSource = item.Item3.Split(';');
                        return;
                    }
                }
            }
            
        }
    }
}
