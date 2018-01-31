using System;
using System.Collections.Generic;
using System.Data;
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

namespace WpfAppDMS
{
    /// <summary>
    /// Interaktionslogik für DarstellungDokumente.xaml
    /// </summary>
    public partial class DarstellungDokumente : UserControl
    {
        //Was brauche ich alles?

        //Dictionary aller Dokumentengruppen
        Dictionary<int, string> AlleDokumentengruppen;
        //Dictionary aller Dokumententypen
        Dictionary<int, string> AlleDokumententypen;
        List<string> AlleDokumententypenBezeichnungen;
        List<int> AlleDokumententypenIds;

        public DarstellungDokumente()
        {
            InitializeComponent();
            ZeichneGrid();
        }

        public void ZeichneGrid() {
            Tuple<Dictionary<int, string>, Dictionary<int, string>, List<string>, List<int>> data = ((DbConnector)App.Current.Properties["Connector"]).ReadAllDataDarstellungDokumente();
            AlleDokumentengruppen = data.Item1;
            AlleDokumententypen = data.Item2;
            AlleDokumententypenBezeichnungen = data.Item3;
            AlleDokumententypenIds = data.Item4;
            cboGruppen.ItemsSource = AlleDokumentengruppen;
            cboTypen.ItemsSource = AlleDokumententypen;
        }

        private void cboGruppen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;
            KeyValuePair<int, string> kvp = (KeyValuePair<int, string>)cbo.SelectedItem;
            cboTypen.ItemsSource = AlleDokumententypen.Where(p => p.Key == kvp.Key).ToDictionary(p => p.Key, p => p.Value);
        }

        private void cboTypen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;
            if (cbo.SelectedItem != null) {
                KeyValuePair<int, string> kvp = (KeyValuePair<int, string>)cbo.SelectedItem;
                int id = AlleDokumententypenIds.ElementAt(AlleDokumententypenBezeichnungen.IndexOf(kvp.Value));
                ZeichneDatagrid("xyx" + kvp.Value);
            }
            
        }

        private void ZeichneDatagrid(string tabelle) {
            DataTable dtOriginal = new DataTable();
            DataTable dt = new DataTable();
            dtOriginal = ((DbConnector)App.Current.Properties["Connector"]).ReadTableData(tabelle);
            dt = ((DbConnector)App.Current.Properties["Connector"]).ReadTableDataWerteErsetztFuerDarstellung(tabelle);
            //DataGrid füllen
            dgMain.ItemsSource = dt.DefaultView;
            //dgTabelleOriginal.ItemsSource = dtOriginal.DefaultView;





        }
    }
}
