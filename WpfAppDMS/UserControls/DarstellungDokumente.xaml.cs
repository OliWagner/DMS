﻿using System;
using System.Collections;
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
        List<OkoDokumententyp> AlleDokumententypen;
        public List<string> AlleDokumententypenBezeichnungen;
        public List<int> AlleDokumententypenIds;
        Dictionary<int, OkoDokTypTabellenfeldtypen> okoDokTypTabellenfeldtypen;

        public DarstellungDokumente()
        {
            InitializeComponent();
            suchfelder.ItemAdded += AddHandlerToTextBoxSuchfeld;
            ZeichneGrid();
        }

        private void AddHandlerToTextBoxSuchfeld(object sender, SuchfeldAddedEventArgs e)
        {
            e.textbox.TextChanged += SuchfelderTextBoxTextChanged;
        }

        private void SuchfelderTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            DgFilter(((TextBox)sender).Name, ((TextBox)sender).Text);
        }

        private void DgFilter(string Feldname, string wert)
        {
            var rows = DataGridHelper.GetDataGridRows(dgDokumente);
            foreach (DataGridRow r in rows)
            {
                DataRowView rv = (DataRowView)r.Item;
                foreach (DataGridColumn column in dgDokumente.Columns)
                {
                    if (column.Header.Equals(Feldname) && column.GetCellContent(r) is TextBlock)
                    {
                        TextBlock cellContent = column.GetCellContent(r) as TextBlock;
                        MessageBox.Show(cellContent.Text);
                    }
                }

            }
        }
        

        public void ZeichneGrid() {
            //Alle aktuellen Daten sammeln
            Tuple<Dictionary<int, string>, List<OkoDokumententyp>, List<string>, List<int>, Dictionary<int, OkoDokTypTabellenfeldtypen>> data = ((DbConnector)App.Current.Properties["Connector"]).ReadAllDataDarstellungDokumente();
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
            ((DbConnector)App.Current.Properties["Connector"]).IdCHecker = true;
            KeyValuePair<int, string> kvp = (KeyValuePair<int, string>)cbo.SelectedItem;
            //TOdo Hier muss ich mir etwas anderes überlegen, wie ich an die DOkumententypen zu der Gruppe komme
            cboTypen.ItemsSource = AlleDokumententypen.Where(p => p.OkoDokumentengruppenId == kvp.Key).ToDictionary(p => p.OkoDokumententypId, p => p.Bezeichnung);

            int dokGruppenId = kvp.Key;

            //TODO Die Dokumente zur DOkGruppe darstellen
            ZeichneDataGridDokGruppe(dokGruppenId);

        }

        private void cboTypen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;
            if (cbo.SelectedItem != null) {
                ((DbConnector)App.Current.Properties["Connector"]).IdCHecker = false;
                KeyValuePair <int, string> kvp = (KeyValuePair<int, string>)cbo.SelectedItem;
                int id = AlleDokumententypenIds.ElementAt(AlleDokumententypenBezeichnungen.IndexOf(kvp.Value));
                ZeichneDatagrid(id);               
            }
            
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
            if (e.PropertyType == typeof(System.Decimal))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "F2";
        }

        public void ZeichneDatagrid(int idInTabelle)
        {            
                OkoDokTypTabellenfeldtypen typ = (from KeyValuePair<int, OkoDokTypTabellenfeldtypen> kvp in okoDokTypTabellenfeldtypen where kvp.Key == idInTabelle select kvp.Value).FirstOrDefault();
                dgDokumente.Columns.Clear();
                ZeichneDatagrid(typ.Tabellenname);
                suchfelder.grdMain.Children.Clear();
                suchfelder.Fill(typ.Tabellenname);
        }


        public void ZeichneDataGridDokGruppe(int idDokgruppe)
        {
            dgDokumente.ItemsSource = null;
            dgDokumente.Columns.Clear();

            DataTable dtOriginal = new DataTable();
            DataTable dt = new DataTable();
           
            DataTable table = ((DbConnector)App.Current.Properties["Connector"]).ReadDoksFuerDokgruppe(idDokgruppe);
            dgDokumente.ItemsSource = table.DefaultView;
            suchfelder.grdMain.Children.Clear();
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

        /// <summary>
        /// Iniziiert den Aufruf des Formulars für den ausgewählten Datensatz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgDokumente_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Behandlung erfolgt in MainWIndow
                    
        }

        private void dgDokumente_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgDokumente_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            if (dg.Items.Count > 0) {
                string header = dg.Columns[0].Header.ToString();
                if (header.Substring(header.Length - 2, 2).Equals("Id"))
                {
                    dg.Columns[0].Visibility = Visibility.Hidden;
                }
            }
        }
    }
}
