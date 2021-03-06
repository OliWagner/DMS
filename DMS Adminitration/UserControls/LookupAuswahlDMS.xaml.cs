﻿using System;
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

namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für AuswahlLookupWert.xaml
    /// </summary>
    public partial class LookupAuswahlDMS : UserControl
    {
        public string TabelleDesDokTyps { get; set; }
        public string[] CsvTabFeldnamenDokTyp { get; set; }
        public string[] CsvTabFeldtypenDokTyp { get; set; }
        public SelectionChangedEventArgs selectionChangedEventArgs { get; set; }

        public List<string> StammdatenTabelle = new List<string>();
        public List<string> StammdatenCsvFeldnamen = new List<string>();
        public List<string> StammdatenCsvFeldTypen = new List<string>();

        public LookupAuswahlDMS(string tabelleDesDokumententyps, string _csvFeldnamen, string _csvFeldtypen)
        {
            TabelleDesDokTyps = tabelleDesDokumententyps;
            CsvTabFeldnamenDokTyp = _csvFeldnamen.Split(';');
            CsvTabFeldtypenDokTyp = _csvFeldtypen.Split(';');
            List<Tuple<string, string,string>> tupleList = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields();
            foreach (var item in tupleList)
            {
                StammdatenTabelle.Add(item.Item1);
                StammdatenCsvFeldnamen.Add(item.Item3);
                StammdatenCsvFeldTypen.Add(item.Item2);
            }


            InitializeComponent();           
        }

        Tuple<List<int>, List<object>> tuple;
        public void Fill(string _feld, int Id = 0, string WhereClauseFeld = "", ComboBoxItem comboBoxItem = null)
        {
            cboAuswahl.Items.Clear();
            if (Id > 0)
            {
                tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadComboboxItems(_feld.Split('_')[3], _feld.Split('_')[4], Id, WhereClauseFeld);
            }
            else
            {
                tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadComboboxItems(_feld.Split('_')[3], _feld.Split('_')[4]);
            }
            for (int i = 0; i < tuple.Item1.Count(); i++)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Tag = tuple.Item1.ElementAt(i);
                cbi.Content = tuple.Item2.ElementAt(i);
                cboAuswahl.Items.Add(cbi);
            }
            if (comboBoxItem != null) {
                int index = 0;
                foreach (ComboBoxItem item in cboAuswahl.Items)
                {
                    if (item.ToString().Equals(comboBoxItem.ToString())) {
                        cboAuswahl.SelectedIndex = index;
                    }
                    index++;
                }   
            }


        }

        private void cboAuswahl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Hier nichts tun
        }
    }
}
