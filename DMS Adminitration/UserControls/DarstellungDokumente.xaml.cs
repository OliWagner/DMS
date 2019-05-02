using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace DMS_Adminitration
{
    public class MyDynamicExport : DynamicObject
    {
        Dictionary<string, object> properties = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return properties.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            properties[binder.Name] = value;
            return true;
        }

        public Dictionary<string, object> GetProperties()
        {
            return properties;
        }
    }

    public class OkoDokTypTabellenfeldtypen {
        public string CsvWertetypen { get; set; }
        public string CsvFeldnamen { get; set; }
    }

    /// <summary>
    /// Interaktionslogik für DarstellungDokumente.xaml
    /// </summary>
    public partial class DarstellungDokumente : UserControl
    {
        public DarstellungDokumente()
        {
            InitializeComponent();
            okoDokTypTabellenfeldtypen = new OkoDokTypTabellenfeldtypen();
            Anwendungen = ((DbConnector)App.Current.Properties["Connector"]).ReadAnwendungen();
            suchfelder.ItemAdded += AddHandlerToTextBoxSuchfeld;
            ZeichneGrid();
        }

        //Ordnet Dateiendungen der richtigen ANwendung zu beim ANzeigen
        public List<Tuple<int, string, string>> Anwendungen { get; set; }

        OkoDokTypTabellenfeldtypen okoDokTypTabellenfeldtypen { get; set; }

        

        private void AddHandlerToTextBoxSuchfeld(object sender, SuchfeldAddedEventArgs e)
        {
            e.textbox.TextChanged += SuchfelderTextBoxTextChanged;
        }


        #region filter datagrid
        private void SuchfelderTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            dataTableForDataGrid.Rows.Clear();
            //DgFilter(((TextBox)sender).Name, ((TextBox)sender).Text);
            DgFilter();
        }

        private DataTable dataTableForDataGrid = new DataTable();
        private void DgFilter(string Feldname, string wert)
        {

            List<DataGridRow> rowsList = new List<DataGridRow>();
            var rows = DataGridHelper.GetDataGridRows(dgDokumente2);

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


        private void DgFilter()
        {

            List<DataGridRow> rowsList = new List<DataGridRow>();
            var rows = DataGridHelper.GetDataGridRows(dgDokumente2);

            foreach (DataGridRow r in rows)
            {
                bool merkeRow = true;
                foreach (DataGridColumn column in dgDokumente.Columns)
                {
                    if (!dataTableForDataGrid.Columns.Contains(column.Header.ToString()))
                    {
                        dataTableForDataGrid.Columns.Add(new DataColumn() { ColumnName = column.Header.ToString() });
                    }

                    if (column.GetCellContent(r) is TextBlock)
                    {
                        string header = column.Header.ToString();
                        KeyValuePair<string, TextBox> kvpAktuelleBox = dicBezeichnungFeldUndTextBox.Where(x => x.Key.Equals(header)).FirstOrDefault();
                        if (kvpAktuelleBox.Value != null) { 
                            TextBox txtBox = kvpAktuelleBox.Value;
                            TextBlock cellContent = column.GetCellContent(r) as TextBlock;
                            //Stimmt der Eintrag nicht mit dem Feld überein, merker auf false setzen, damit dir Row nicht in das Ergebnis einfließt
                            if (!cellContent.Text.Contains(txtBox.Text))
                            {
                                //An dieser Stelle muss die Zeile aus dem DataGrid der DataTable hinzugefügt werden
                                merkeRow = false;
                            }
                        }
                    }
                }
                //Row ist zu Ende geschrieben und vor allen DIngen auch die COlumns der Tabelle
                if (merkeRow)
                {
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
            Tuple<string, string> tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadDokTypTypesAndFields();
            okoDokTypTabellenfeldtypen.CsvFeldnamen = tuple.Item1;
            okoDokTypTabellenfeldtypen.CsvWertetypen = tuple.Item2;
            suchfelder.Fill();
            ZeichneDatagridForm();
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
            if (e.PropertyType == typeof(System.Decimal))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "F2";
        }

        /// <summary>
        /// Das Feld beinhaltet bei Auswahl eines Datentyps die Feldbezeichnungen und Textfelder der Filterfelder, damit später über alle Felder gefiltert werden kann
        /// </summary>
        Dictionary<string, TextBox> dicBezeichnungFeldUndTextBox = new Dictionary<string, TextBox>();
        public void ZeichneDatagridForm()
        {            
                dgDokumente.Columns.Clear();
                ZeichneDatagridTab("OkoDokumentenTyp");
                suchfelder.grdMain.Children.Clear();
                suchfelder.Fill("OkoDokumentenTyp", out dicBezeichnungFeldUndTextBox);
        }

        public void ZeichneDatagridTab(string tabelle) {
            
            DataTable dtOriginal = new DataTable();
            DataTable dt = new DataTable();
            
            dtOriginal = ((DbConnector)App.Current.Properties["Connector"]).ReadTableData(tabelle);
            dt = ((DbConnector)App.Current.Properties["Connector"]).ReadTableDataWerteErsetztFuerDarstellungDokTyp();
            

            //DataGrid füllen
            //dgDokumente.AutoGenerateColumns = true;
            dgDokumente.ItemsSource = dt.DefaultView;
            dgDokumente2.ItemsSource = dt.DefaultView;
            foreach (DataGridColumn column in dgDokumente.Columns)
            {
                if (column.Header.Equals(tabelle + "Id"))
                {
                    column.Visibility = Visibility.Hidden;
                }
            }
            dgTabelleOriginal.ItemsSource = dt.DefaultView;
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
            foreach (DataGridColumn column in dg.Columns)
            {
                if (column.Header.Equals("IdInTabelle") || column.Header.Equals("Tabelle") || column.Header.Equals("OkoDokumenteDatenId"))
                {
                    column.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Nix
        }
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (dgDokumente != null) {
                if (suchfelder.grdMain.Children.Count > 0 && dgDokumente.SelectedItem != null) { e.CanExecute = true; } else { e.CanExecute = false; }
            }
        }
        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Nix
        }
        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (dgDokumente != null)
            {
                if (suchfelder.grdMain.Children.Count > 0 && dgDokumente.SelectedItem != null && DoksFuerExport.Count == 0) { e.CanExecute = true; } else { e.CanExecute = false; }
            }
        }
        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Nix
        }
        private void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
                if (DoksFuerExport.Count > 0) { e.CanExecute = true; } else { e.CanExecute = false; }
        }
        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Nix
        }
        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DoksFuerExport.Count == 0) { e.CanExecute = false; } else { e.CanExecute = true; }
        }

        private void btnDokAnzeigen_Click(object sender, RoutedEventArgs e)
        {
            //Wird in MainWIndow behandelt
        }

        private List<int> DoksFuerExport = new List<int>();
        private void btnZumExportHinzu_Click(object sender, RoutedEventArgs e)
        {
            var row = ((DataRowView)dgDokumente.SelectedItem).Row.ItemArray;
            int counter = 0;
            foreach (DataGridColumn col in dgDokumente.Columns)
            {
                if (col.Header.Equals("OkoDokumenteDatenId")) {
                    if (!DoksFuerExport.Contains(Int32.Parse(row[counter].ToString()))) {
                        DoksFuerExport.Add(Int32.Parse(row[counter].ToString()));
                    }
                    
                }
                counter++;
            }
        }

        

        /// <summary>
        /// Dokumentenexport
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportDialog_BtnExportieren_Click(object sender, RoutedEventArgs e)
        {
            //Liste aus Exportdialog aufrufen
            Grid grid = (Grid)((Button)sender).Parent;
            ExportDialog dialog = (ExportDialog)grid.Parent;

            string pathString = dialog.PathString;
            DirectoryInfo info = Directory.CreateDirectory(pathString);
            //In das neue Verzeichnis exportieren
            //TODO
            //Es müsste reichen, sich den originalen Namen des DOkuments mit DokId und neuem Namen zu merken...
            List<int> _gemerkteDokIds = new List<int>();
            List<string> _gemerkteAlteDateinamen = new List<string>();
            List<string> _gemerkteNeueDateinamen = new List<string>();
            
            //Wenn Eintrag schon vorhanden --> einfach XML raus schreiben
            //Wenn nicht, EIntrag hinzufügen + XML schreiben
            foreach (Exportdaten ed in dialog.lstExport)
            {
                string alterName = ed.Dateiname;
                string neuerName = "";
                int idDesDoks = ed.DokumenteId;

                if (!_gemerkteDokIds.Contains(ed.DokumenteId))
                {
                    //Achtung: Listeneinträge immer für ALLE Listen setzen, um den Zugriff per Index gewährleisten zu können
                    //Die DOkId ist noch nicht exportiert
                    //Gibt es den Dateinamen schon in den Originalnamen?  
                    if (_gemerkteAlteDateinamen.Contains(ed.Dateiname)) {
                        //Beim Umbenennen feststellen, ob der neue Name schon existiert
                        bool prozessBeendet = false;
                        int counter = 0;
                        while (!prozessBeendet) {
                            counter++;
                            string[] arrDateiname = ed.Dateiname.Split('.');
                            neuerName = arrDateiname[0] + "(" + counter + ")."+ arrDateiname[1];
                            if (!_gemerkteNeueDateinamen.Contains(neuerName)) {
                                prozessBeendet = true;

                            } 
                        }
                        _gemerkteAlteDateinamen.Add(alterName);
                        _gemerkteNeueDateinamen.Add(neuerName);
                        _gemerkteDokIds.Add(idDesDoks);
                    } else {
                        //Nein --> Originalnamen in beide Listen schreiben, Id in IdListe
                        _gemerkteAlteDateinamen.Add(alterName);
                        _gemerkteNeueDateinamen.Add(alterName);
                        _gemerkteDokIds.Add(idDesDoks);
                    }
                    //--> Datei Export
                    string exportDateiNameFile = neuerName.Equals("") ? alterName : neuerName;
                    ExportFileToDisk(ed.DokumenteId.ToString(), pathString, exportDateiNameFile);
                }
                else {
                    //Die DOkId ist schon exportiert 
                    //--> Nur CSV wegschreiben mit neuem Dateinamen --> Nach Index aus den Listen wählen (Alter.IndexOf ..> Neue.ElementAt
                    int index = _gemerkteDokIds.IndexOf(ed.DokumenteId);
                    neuerName = _gemerkteNeueDateinamen.ElementAt(index);
                }
            }


            //Nach Export Listen leeren
            dialog.lstExport.Clear();
            DoksFuerExport.Clear();
        }

        public void ExportFileToDisk(string varID, string path, string dateiname)
        {
            string _path = System.IO.Path.Combine(path, dateiname); 
            using (var sqlQuery = new SqlCommand(@"SELECT [Dokument] FROM [dbo].[OkoDokumente] WHERE [OkoDokumenteId] = @varID", ((DbConnector)App.Current.Properties["Connector"])._con))
            {
                sqlQuery.Parameters.AddWithValue("@varID", varID);
                using (var sqlQueryResult = sqlQuery.ExecuteReader())
                    if (sqlQueryResult != null)
                    {
                        sqlQueryResult.Read();
                        var blob = new Byte[(sqlQueryResult.GetBytes(0, 0, null, 0, int.MaxValue))];
                        sqlQueryResult.GetBytes(0, 0, blob, 0, blob.Length);
                        using (var fs = new FileStream(_path, FileMode.Create, FileAccess.Write))
                            fs.Write(blob, 0, blob.Length);
                    }
            }
        }

        /// <summary>
        /// Notlösung, da mein COmmand nicht funktioniert wie es soll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgDokumente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int OkoDokumentendatenId = 0;

            if (dgDokumente != null && dgDokumente.SelectedItem != null)
            {
                var row = ((DataRowView)dgDokumente.SelectedItem).Row.ItemArray;
                int counter = 0;
                foreach (DataGridColumn col in dgDokumente.Columns)
                {
                    if (col.Header.Equals("OkoDokumenteDatenId"))
                    {
                        if (!DoksFuerExport.Contains(Int32.Parse(row[counter].ToString())))
                        {
                            OkoDokumentendatenId = Int32.Parse(row[counter].ToString());
                        }

                    }
                    counter++;
                }
            }
    }

        //private void btnDokLoeschen_Click(object sender, RoutedEventArgs e)
        //{
        //    int index = 0;
        //    object[] itemArray = ((DataRowView)dgDokumente.SelectedItem).Row.ItemArray;
        //    int counter = 0;
        //    foreach (var col in dgDokumente.Columns)
        //    {
        //        if (col.Header.Equals("OkoDokumenteDatenId")) {
        //            index = Int32.Parse(itemArray[counter].ToString());
        //            }
        //        counter++;
        //    }
        //    string tab = ((DbConnector)App.Current.Properties["Connector"]).DeleteDokumentendatensatz(index);
        //    ZeichneDatagridTab(tab);
        //}
    }
}
