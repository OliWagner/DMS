﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;


namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für EingabeTabellenfelder.xaml
    /// </summary>
    public partial class EingabeTabellenfelder : UserControl
    {
        public EingabeTabellenfelder(string check = "")
        {
            InitializeComponent();
            List<Tuple<string, string, string>> alleTabellenAusserSystem = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields();

            //Nachschlagefelder dürfen bei CSV einlesen nicht aktiv sein
            if (check.Equals(""))
            {
                //wenn keine weiteren Tabellen vorhanden, das Feld deaktivieren
                if (alleTabellenAusserSystem.Count() == 0)
                {
                    cbiLookup.IsEnabled = false;
                }
            }
            else {
                comBoxFeldtyp.Items.Remove(cbiLookup);
            }         
        }


        private void txtBezeichnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).Text = ((TextBox)sender).Text.Replace("_", "");
            //Hier nichts tun Event wird in Upload verarbeitet
            //Dient zur Änderung der Headerspalten im DataGrid
        }

        private void comBoxFeldtyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            if (item.Content.Equals("Nachschlagefeld")) {
                LookupDialog dialog = new LookupDialog();
                if (dialog.ShowDialog() == true) {
                    txtBezeichnung.Tag = dialog.Tabelle + "_" + dialog.Feld;
                    comBoxFeldtyp.IsEnabled = false;
                }
            }
        }
    }
}
