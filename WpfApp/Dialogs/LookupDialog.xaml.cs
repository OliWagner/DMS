using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public LookupDialog(string AufrufendeTabelle = "")
        {
            InitializeComponent();
           
            tupleList = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields();
            foreach (var item in tupleList)
            {
                if (!item.Item1.Equals(AufrufendeTabelle)) {
                    cboTabelle.Items.Add(new ComboBoxItem() { Content = item.Item1 });
                }                
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void cboTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _tabName = (((ComboBoxItem)cboTabelle.SelectedItem).Content);
            if (!_tabName.Equals("")) {
                cboFeld.IsEnabled = true;
                cboFeld.Items.Clear();
                foreach (var item in tupleList)
                {
                    if (item.Item1.Equals(_tabName)) {
                        foreach (var elem in item.Item3.Split(';')) {
                            ComboBoxItem cboItem = new ComboBoxItem();
                            if (elem.Substring(0,3).Equals("_x_")) {
                                //Nachschlagefelder können nicht als Nachschlagewert referenziert werden
                                cboItem.Content = elem.Split('_')[2];
                                cboItem.IsEnabled = false;
                            } else {
                                cboItem.Content = elem;
                            }                           
                            
                            cboFeld.Items.Add(cboItem);
                        }
                        return;
                    }
                }
            }           
        }
        
    }
}
