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
    public class OkoDokTypTabellenfeldtypen {
        public string Tabellenname { get; set; }
        public string CsvWertetypen { get; set; }
        public string CsvFeldnamen { get; set; }

    }
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
        public List<string> AlleDokumententypenBezeichnungen;
        public List<int> AlleDokumententypenIds;
        Dictionary<int, OkoDokTypTabellenfeldtypen> okoDokTypTabellenfeldtypen;

        public DarstellungDokumente()
        {
            InitializeComponent();
            ZeichneGrid();
        }

        public void ZeichneGrid() {
            //Alle aktuellen Daten sammeln
            Tuple<Dictionary<int, string>, Dictionary<int, string>, List<string>, List<int>, Dictionary<int, OkoDokTypTabellenfeldtypen>> data = ((DbConnector)App.Current.Properties["Connector"]).ReadAllDataDarstellungDokumente();
            AlleDokumentengruppen = data.Item1;
            AlleDokumententypen = data.Item2;
            AlleDokumententypenBezeichnungen = data.Item3;
            AlleDokumententypenIds = data.Item4;
            okoDokTypTabellenfeldtypen = data.Item5;

            //Checkboxen
            cboGruppen.ItemsSource = AlleDokumentengruppen;
            cboTypen.ItemsSource = AlleDokumententypen;
            ZeichneDatagrid();
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
                ZeichneDatagrid(id);               
            }
            
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var test = e.PropertyType;

            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
            if (e.PropertyType == typeof(System.Decimal))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "F2";
        }

        public void ZeichneDatagrid(int idInTabelle)
        {
            OkoDokTypTabellenfeldtypen typ = (from KeyValuePair<int, OkoDokTypTabellenfeldtypen> kvp in okoDokTypTabellenfeldtypen where kvp.Key == idInTabelle select kvp.Value).FirstOrDefault();
            dgDokumente.ItemsSource = null;
            dgDokumente.Columns.Clear();
            ZeichneDatagrid(typ.Tabellenname);
            suchfelder.grdMain.Children.Clear();
            suchfelder.Fill(typ.Tabellenname);
        }


        public void ZeichneDatagrid(string tabelle = "") {

            DataTable dtOriginal = new DataTable();
            DataTable dt = new DataTable();
            if (tabelle.Equals("")) {
                dtOriginal = ((DbConnector)App.Current.Properties["Connector"]).ReadTableData();
                dt = ((DbConnector)App.Current.Properties["Connector"]).ReadTableDataWerteErsetztFuerDarstellung();
            } else {
                dtOriginal = ((DbConnector)App.Current.Properties["Connector"]).ReadTableData(tabelle);
                dt = ((DbConnector)App.Current.Properties["Connector"]).ReadTableDataWerteErsetztFuerDarstellung(tabelle);
            }

            //DataGrid füllen
            //dgDokumente.AutoGenerateColumns = true;
            dgDokumente.ItemsSource = dt.DefaultView;
            dgTabelleOriginal.ItemsSource = dtOriginal.DefaultView;
        }
    }
}
