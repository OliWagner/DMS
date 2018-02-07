using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfAppDMS
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SqlConnection _con { get; set; }

        //Felder für die Datenbank
        private string Dateiname { get; set; }
        private string Dateipfad { get; set; }
        private string DateiTitel { get; set; }
        private string DateiBeschreibung { get; set; }

        //Todo Noch zu belegen
        //private string Dokumententyp { get; set; }
        //private string Tabelle { get; set; }
        //private int IdInTabelle { get; set; }
        //private string Dateigroesse { get; set; }
        //private string ErfasstAm { get; set; }

        public int _idDesGeradeBearbeitetenDokuments = 0;
       
        public MainWindow()
        {
            Connect();
            InitializeComponent();

            dokTree.MouseRightButtonDown += dokTree_MouseRightButtonDown;

            darstellungDokumente.dgDokumente.MouseRightButtonDown += MouseRightButtonDown_Call;


            //Nach hinzufügen des Items noch den lokalen Eventhandler für das spätere Abfangen in EingabeDokumentDaten_BtnSpeichern_Click einbauen
            tabsDaten.ItemAdded += AddHandlerToEingabeDokumentenDatenInstanz;
        }

        private void MouseRightButtonDown_Call(object sender, MouseButtonEventArgs e)
        {
            int myNewIndex = 0;

            if (((DbConnector)App.Current.Properties["Connector"]).IdCHecker == false && darstellungDokumente.dgDokumente.SelectedItem != null)
            {
                DataRowView rowView = (DataRowView)(darstellungDokumente.dgDokumente.SelectedItem);
                int IdDesDatensatzes = Int32.Parse(rowView.Row.ItemArray[0].ToString());
                for (int i = 0; i < darstellungDokumente.dgTabelleOriginal.Items.Count; i++)
                {
                    darstellungDokumente.dgTabelleOriginal.ScrollIntoView((darstellungDokumente.dgTabelleOriginal.Items[i]));
                    DataGridRow aktRow = (DataGridRow)(darstellungDokumente.dgTabelleOriginal.ItemContainerGenerator.ContainerFromIndex(i));
                    if (aktRow != null && (int)((DataRowView)aktRow.Item).Row.ItemArray[0] == IdDesDatensatzes)
                    {
                        myNewIndex = i;
                    }
                }
                if (myNewIndex >= 0)
                {
                    
                    DataRowView row = (DataRowView)darstellungDokumente.dgTabelleOriginal.Items[myNewIndex];
                    
                    if (row != null)
                    {
                        string DokTyp = darstellungDokumente.dgTabelleOriginal.Columns[0].Header.ToString().Replace("xyx", "").Replace("Id", "");
                        int DokTypId = Int32.Parse(row.Row.ItemArray[0].ToString());
                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < row.Row.ItemArray.Count() - 4; i++)
                        {
                            if (i > 0) {
                                sb.Append(row.Row.ItemArray[i] + ";");
                            }                            
                        }
                        string csvFeldtypen = sb.ToString().Substring(0, sb.Length - 1);

                        tabsDaten.IsEnabled = true;
                        if (!txtTitel.Text.Equals("")) {
                            tabsDaten.tabsMain.Items.Clear();
                            tabsDaten.Items.Clear();
                        }
                        txtBeschreibung.IsEnabled = false;
                        txtTitel.IsEnabled = false;
                        dokTree.IsEnabled = false;
                        txtDropzone.Text = "";
                        if (tabsDaten.Items.Count() == 0) {
                            tabsDaten.Add(DokTyp, DokTypId, csvFeldtypen);
                            tabsDaten.tabsMain.SelectedIndex = 0;
                            //TODO Events für Dropdowns auslösen

                            foreach (var item in ((EingabeDokumentDaten)((TabItem)tabsDaten.tabsMain.Items[0]).Content).grdMain.Children)
                            {
                                if (item.GetType() == typeof(LookupAuswahl)) {
                                    //((LookupAuswahl)item).cboAuswahl.SelectionChanged?.Invoke(item, new SelectionChangedEventArgs());
                                    SelectionChangedEventArgs args = (SelectionChangedEventArgs)((LookupAuswahl)item).selectionChangedEventArgs;
                                    ((LookupAuswahl)item).cboAuswahl.RaiseEvent(args);
                                }
                            } 

                            
                        }
                        
                    }
                }
            }
        }

        private void AddHandlerToEingabeDokumentenDatenInstanz(object sender, EingabeDokumentDatenEventArgs e)
        {
            ((EingabeDokumentDaten)e.eingabeDokumentDaten).btnSpeichern.Click += EingabeDokumentDaten_BtnSpeichern_Click;
        }

        private void EingabeDokumentDaten_BtnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Content.Equals("Speichern"))
            {
                //Der Datensatz ist schon in die Stammdatentabelle geschrieben
                //Datensatz eintragen und Id des eingetragenen Datensatzes ermitteln, ist dem Button als Tag hinterlegt
                int IdEingetragenerDatensatz = Int32.Parse((((Button)sender).Tag.ToString().Split('_')[1]));

                //Als nächstes Checken, ob das Dokument schon eingetragen wurde
                if (_idDesGeradeBearbeitetenDokuments == 0)
                {
                    //Der Datensatz wurde noch nicht eingetragen
                    _idDesGeradeBearbeitetenDokuments = databaseFilePut(Dateipfad);
                }

                //Wenn wir hier angekommen sind, dann klappt auch das Eintragen der Datei in die DB
                //Wir haben jetzt eigentlich alles für die Tabelle Dokumentendaten
                string _Tabelle = ((Button)sender).Tag.ToString().Split('_')[3];
                string _IdDokumentenTyp = ((Button)sender).Tag.ToString().Split('_')[2];

                string dataTxt = _idDesGeradeBearbeitetenDokuments + ";" + _IdDokumentenTyp + ";" + _Tabelle + ";" + IdEingetragenerDatensatz
                    + ";" + txtTitel.Text + ";" + txtBeschreibung.Text + ";" + Dateiname + ";" + DateTime.Today.ToString();
                ((DbConnector)App.Current.Properties["Connector"]).InsertDocumentData(dataTxt);
            }
            else {
                tabsDaten.tabsMain.Items.Clear();
                tabsDaten.Items.Clear();
            }        

            if (darstellungDokumente.cboTypen.SelectedItem != null)
            {
                KeyValuePair<int, string> kvp = (KeyValuePair<int, string>)darstellungDokumente.cboTypen.SelectedItem;
                int id = darstellungDokumente.AlleDokumententypenIds.ElementAt(darstellungDokumente.AlleDokumententypenBezeichnungen.IndexOf(kvp.Value));
                darstellungDokumente.ZeichneDatagrid(id);
            }
            else {
                darstellungDokumente.ZeichneDatagrid();
            }
        }

        private void btnNeueDb_Click(object sender, RoutedEventArgs e)
        {
            ((DbConnector)App.Current.Properties["Connector"]).Close();
            Connect();
            InitializeComponent();
            //dokTree.MouseRightButtonDown += dokTree_MouseRightButtonDown;
            ////Nach hinzufügen des Items noch den lokalen Eventhandler für das spätere Abfangen in EingabeDokumentDaten_BtnSpeichern_Click einbauen
            //tabsDaten.ItemAdded += AddHandlerToEingabeDokumentenDatenInstanz;
            dokTree.Start();
            dokTree.MouseRightButtonDown += dokTree_MouseRightButtonDown;
            tabsDaten.Items.Clear();
            tabsDaten.tabsMain.Items.Clear();
            tabsDaten.ItemAdded += AddHandlerToEingabeDokumentenDatenInstanz;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            _idDesGeradeBearbeitetenDokuments = 0;
            string[] data = { };
            if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                data = e.Data.GetData(DataFormats.FileDrop) as string[];
                // handle the files here!
            }
            //databaseFilePut(data[0]);
            txtTitel.Text = "";
            txtTitel.IsEnabled = true;
            txtBeschreibung.Text = "";
            txtBeschreibung.IsEnabled = true;
            Dateipfad = data[0];
            string[] txtArray = data[0].Split('\\');
            Dateiname = txtDropzone.Text = txtArray[txtArray.Length - 1];
            DateiTitel = "";
            DateiBeschreibung = "";
            tabsDaten.tabsMain.Items.Clear();
            tabsDaten.Items.Clear();
        }

        private void Connect(string vorherigeEingabe = "")
        {
            ConnectionDialog connectionDialog = new ConnectionDialog();
            connectionDialog.btnAbbrechen.Click += conDialogBtnAbbrechen_Click;
            if (!vorherigeEingabe.Equals(""))
            {
                connectionDialog.txtDataSource.Text = vorherigeEingabe.Split(';')[0];
                connectionDialog.txtInitialCatalog.Text = vorherigeEingabe.Split(';')[1];
                connectionDialog.txtUserName.Text = vorherigeEingabe.Split(';')[2];
                connectionDialog.txtPassword.Text = vorherigeEingabe.Split(';')[3];
            }
            if (connectionDialog.ShowDialog() == true)
            {
                //Datenbankverbindung initialisieren und in Objekt schreiben
                App.Current.Properties["Connector"] = new DbConnector("Data Source=" + connectionDialog.txtDataSource.Text + ";Initial Catalog=" + connectionDialog.txtInitialCatalog.Text + ";User ID=" + connectionDialog.txtUserName.Text + ";Password=" + connectionDialog.txtPassword.Text + ";");
                if (!((DbConnector)App.Current.Properties["Connector"]).Connect())
                {
                    Connect(connectionDialog.txtDataSource.Text + ";" + connectionDialog.txtInitialCatalog.Text + ";" + connectionDialog.txtUserName.Text + ";" + connectionDialog.txtPassword.Text);
                }
                Title = "Bearbeiten der Datenbank: " + connectionDialog.txtInitialCatalog.Text;
            }
            else
            {
                Connect();
            }

        }

        #region Events other Components


        private void dokTree_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeView tv = ((DokTree)sender).tvMain;
            if ((TreeViewItem)tv.SelectedItem != null)
            {
                string header = ((TreeViewItem)tv.SelectedItem).Header.ToString();
                if (((TreeViewItem)tv.SelectedItem).Tag != null)
                {
                    int id = Int32.Parse(((TreeViewItem)tv.SelectedItem).Tag.ToString());
                    if (!tabsDaten.Items.Contains(header))
                    {
                        tabsDaten.Add(header, id);
                    }
                }

            }
        }
        private void conDialogBtnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        #endregion


        #region Filestreaming Database
        public void databaseFileReadToDisk(string varID, string dateiname)
        {
            FileInfo fi = new FileInfo(Assembly.GetEntryAssembly().Location);
            string _path = fi.DirectoryName + "\\" + dateiname;

            using (var sqlQuery = new SqlCommand(@"SELECT [Dokument] FROM [dbo].[Dokumente] WHERE [DokumenteId] = @varID", _con))
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

        //public MemoryStream databaseFileReadToMemoryStream(string varID)
        //{
        //    MemoryStream memoryStream = new MemoryStream();
        //    using (var sqlQuery = new SqlCommand(@"SELECT [Dokument] FROM [dbo].[Dokumente] WHERE [DokumenteId] = @varID", _con))
        //    {
        //        sqlQuery.Parameters.AddWithValue("@varID", varID);
        //        using (var sqlQueryResult = sqlQuery.ExecuteReader())
        //            if (sqlQueryResult != null)
        //            {
        //                sqlQueryResult.Read();
        //                var blob = new Byte[(sqlQueryResult.GetBytes(0, 0, null, 0, int.MaxValue))];
        //                sqlQueryResult.GetBytes(0, 0, blob, 0, blob.Length);
        //                //using (var fs = new MemoryStream(memoryStream, FileMode.Create, FileAccess.Write)) {
        //                memoryStream.Write(blob, 0, blob.Length);
        //                //}
        //            }
        //    }
        //    return memoryStream;
        //}

        private int databaseFilePut(string varFilePath)
        {
            byte[] file;
            using (var stream = new FileStream(varFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(stream))
                {
                    file = reader.ReadBytes((int)stream.Length);
                }
            }

            using (var sqlWrite = new SqlCommand("INSERT INTO OkoDokumente (Dokument) Values(@File)", ((DbConnector)App.Current.Properties["Connector"])._con))
            {
                sqlWrite.Parameters.Add("@File", SqlDbType.VarBinary, file.Length).Value = file;
                sqlWrite.ExecuteNonQuery();             
            }

            int neueId = 0;
            using (var command = new SqlCommand("SELECT ISNULL(MAX(OkoDokumenteId), 0) FROM OkoDokumente", ((DbConnector)App.Current.Properties["Connector"])._con))
            {
               
                Int32.TryParse(command.ExecuteScalar().ToString(), out neueId);
            }
            return neueId;  
        }
        #endregion

        #region Textbox Events, Enable/Disable der Usercontrols
        private void txtTitel_TextChanged(object sender, RoutedEventArgs e)
        {
            DateiTitel = txtTitel.Text;
            DateiBeschreibung = txtBeschreibung.Text;
            CheckEnableUserControls();
        }

        private void txtBeschreibung_TextChanged(object sender, RoutedEventArgs e)
        {
            DateiTitel = txtTitel.Text;
            DateiBeschreibung = txtBeschreibung.Text;
            CheckEnableUserControls();
        }

        private void CheckEnableUserControls()
        {
            if (DateiTitel.Trim().Equals(""))
            {
                tabsDaten.IsEnabled = false;
                dokTree.IsEnabled = false;
            }
            else
            {
                tabsDaten.IsEnabled = true;
                dokTree.IsEnabled = true;
            }
        }
        #endregion
    }
}
