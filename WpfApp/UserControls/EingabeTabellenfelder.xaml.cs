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

namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für EingabeTabellenfelder.xaml
    /// </summary>
    public partial class EingabeTabellenfelder : UserControl
    {
        public EingabeTabellenfelder()
        {
            InitializeComponent();
            List<Tuple<string, string, string>> alleTabellenAusserSystem = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields();
            if (alleTabellenAusserSystem.Count() == 0) {
                cbiLookup.IsEnabled = false;
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
