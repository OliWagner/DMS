﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

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


        #region filter datagrid
        private void SuchfelderTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            dataTableForDataGrid.Rows.Clear();
            DgFilter(((TextBox)sender).Name, ((TextBox)sender).Text);

        }

        private DataTable dataTableForDataGrid = new DataTable();
        private void DgFilter(string Feldname, string wert)
        {

            List<DataGridRow> rowsList = new List<DataGridRow>();
            var rows = DataGridHelper.GetDataGridRows(dgDokumente);

            foreach (DataGridRow r in rows)
            {
                bool merkeRow = true;
                foreach (DataGridColumn column in dgDokumente.Columns)
                {
                    if (!dataTableForDataGrid.Columns.Contains(column.Header.ToString())) {
                        dataTableForDataGrid.Columns.Add(new DataColumn() { ColumnName = column.Header.ToString() });
                    }

                    if (column.Header.Equals(Feldname) && column.GetCellContent(r) is TextBlock)
                    {
                        TextBlock cellContent = column.GetCellContent(r) as TextBlock;
                        //Stimmt der Eintrag nicht mit dem Feld überein, merker auf false setzen, damit dir Row nicht in das Ergebnis einfließt
                        if (!cellContent.Text.Contains(wert)) {
                            //An dieser Stelle muss die Zeile aus dem DataGrid der DataTable hinzugefügt werden
                            merkeRow = false;   
                        }
                    }
                }
                //Row ist zu Ende geschrieben und vor allen DIngen auch die COlumns der Tabelle
                if (merkeRow) {
                    //Aus der DataGridRow eine DataROw machen:
                    dataTableForDataGrid.Rows.Add(CopyDataGridRowToDataRow(dataTableForDataGrid, r));
                }
                
            }
            dgDokumente.ItemsSource = null;
            dgDokumente.ItemsSource = dataTableForDataGrid.DefaultView;
        }

        private DataRow CopyDataGridRowToDataRow(DataTable table, DataGridRow row) {
            DataRow drReturner = table.NewRow();

            DataRowView drv = (DataRowView)(row.Item);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var value = drv.Row.ItemArray[i].ToString();

                drReturner[i] = value;
            }


            return drReturner;
        }
        #endregion

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
            dgDokumente2.ItemsSource = dt.DefaultView;
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
                if (dg.Columns.Count > 0) {
                    string header = dg.Columns[0].Header.ToString();
                    if (header.Substring(header.Length - 2, 2).Equals("Id"))
                    {
                        dg.Columns[0].Visibility = Visibility.Hidden;
                    }
                }               
            }
        }
    }
}
