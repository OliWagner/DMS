using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Vigenere;

namespace DMS_Adminitration
{
    public class ConnectionDetails
    {
        public string DataSource { get; set; }
        public string InitialCatalog { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class ConnectionDirectory
    {
        [XmlElement("ConnectionDetails")]
        public List<ConnectionDetails> ConnectionsList = new List<ConnectionDetails>();

        public void Add(ConnectionDetails elem)
        {
            ConnectionsList.Add(elem);
        }

        public void Remove(ConnectionDetails elem)
        {
            ConnectionsList.Remove(elem);
        }
    }
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        Einstellungen Einstellungen = new Einstellungen();

        public MainWindow()
        {
            Connect();
            InitializeComponent();
            InitializeVendorComponents();
            ReadEinstellungen(); 
        }

        private void Ausgangsstellung()
        {
            grdMain.Children.Clear();
            eingabeTabelle.Visibility = Visibility.Hidden;
            uploadCsv.Visibility = Visibility.Hidden;
            aendernTabellen.Visibility = Visibility.Hidden;
            tabellendaten.Visibility = Visibility.Hidden;
            pflegeTabellendaten.Visibility = Visibility.Hidden;
            uebersichtTabellen.IsEnabled = true;
            uebersichtTabellen.zeichneGrid();
            grdMain.Children.Add(uebersichtTabellen);
        }

        private void ReadEinstellungen() {
            Tuple<string, string> tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadEinstellungen();
            Einstellungen.Ordnerpfad = tuple.Item1;
            ordnerAnzeigen.Pfad = tuple.Item1;
            Einstellungen.DatenbearbeitungEinAus = tuple.Item2;
            if (Einstellungen.DatenbearbeitungEinAus.Equals("aus")) {
                ribbon.Items.Remove(RibTabAblage);
                ribbon.Items.Remove(RibTabStamm);
                ramDbEinAus.ImageSource = new BitmapImage(new Uri("/img/aus.png", UriKind.Relative));
            }
        }

        private ConnectionDirectory DeSerialize(string file)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(ConnectionDirectory));
            TextReader reader = new StreamReader(@file);
            object obj = deserializer.Deserialize(reader);
            ConnectionDirectory XmlData = (ConnectionDirectory)obj;
            reader.Close();
            return XmlData;
        }
        public static ConnectionDirectory connectionDirectory = new ConnectionDirectory();
        VigenereQuadrath q = new VigenereQuadrath("PicKerL1465JHdg");

        private void Connect(string vorherigeEingabe = "")
        {
            //Verzeichnis der Anwendung ermitteln
            FileInfo fi = new FileInfo(Assembly.GetEntryAssembly().Location);
            string _path = fi.DirectoryName + "\\Connections.xml";
            if (File.Exists(_path))
            {
                ConnectionDirectory cd = DeSerialize(_path);
                connectionDirectory.ConnectionsList = new List<ConnectionDetails>();
                foreach (var item in cd.ConnectionsList)
                {
                    connectionDirectory.Add(new ConnectionDetails { InitialCatalog = q.EntschlüssleText(item.InitialCatalog).Replace(" ", "\\"), DataSource = q.EntschlüssleText(item.DataSource), Username = q.EntschlüssleText(item.Username), Password = q.EntschlüssleText(item.Password) });
                }
            }

            string datasource = connectionDirectory.ConnectionsList.ElementAt(0).DataSource;
            string initialcatalog = connectionDirectory.ConnectionsList.ElementAt(0).InitialCatalog;
            string dbuser = connectionDirectory.ConnectionsList.ElementAt(0).Username;
            string dbpasswort = connectionDirectory.ConnectionsList.ElementAt(0).Password;

            //string datasource = "LAPTOP-CTMG3F1D\\SQLEXPRESS";
            //string initialcatalog = "Dokumentenmanagement";
            //string dbuser = "sa";
            //string dbpasswort = "95hjh11!";

            Application.Current.Properties["Connector"] = new DbConnector("Data Source='"+datasource+"';Initial Catalog='"+initialcatalog+"';User ID='"+dbuser+"';Password='"+dbpasswort+"';");
            if (!((DbConnector)App.Current.Properties["Connector"]).Connect())
            {
                Connect(datasource+";"+initialcatalog+";"+dbuser+";"+dbpasswort);
            }
            Title = "Bearbeiten der Datenbank";

        }

        //UserControls initialisieren
        private void InitializeVendorComponents()
        {
            uebersichtTabellen.zeichneGrid();
            eingabeTabelle.Start();
            //Eventhandler der Usercontrols initialisieren
            uebersichtTabellen.tvMain.SelectedItemChanged += TvMain_SelectedItemChanged;
            tabellendaten.dgTabelle.SelectionChanged += tabellendaten_dgTabelle_SelectionChanged;
            dropfeld.grdDropzone.Drop += GrdDropzone_Drop;
            scanOrdner.SomethingChanged += ScanOrdner_SomethingChanged;
            scanOrdner.MouseLeftButtonDown += ScanOrdner_MouseLeftButtonDown;
        }

        #region Events UserControls
        private void tabellendaten_dgTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StringBuilder csvFeldwerte = new StringBuilder();
            //Mit Werten aus Pflegetabellendaten und den Werten aus SelectedItem das Grid in PflegeTabellendaten neu zeichnen
            DataRowView myrow = (DataRowView)(tabellendaten.dgTabelle.SelectedItem);
            if (myrow != null)
            {
                int myNewIndex = 0;
                int IdDesDatensatzes = Int32.Parse((myrow.Row.ItemArray[0].ToString()));
                for (int i = 0; i < tabellendaten.dgTabelleOriginal.Items.Count; i++)
                {
                    tabellendaten.dgTabelleOriginal.ScrollIntoView((tabellendaten.dgTabelleOriginal.Items[i]));
                    DataGridRow aktRow = (DataGridRow)(tabellendaten.dgTabelleOriginal.ItemContainerGenerator.ContainerFromIndex(i));
                    if (aktRow != null && (int)((DataRowView)aktRow.Item).Row.ItemArray[0] == IdDesDatensatzes)
                    {
                        myNewIndex = i;
                    }
                }
                int index = myNewIndex;
                if (index >= 0)
                {
                    DataRowView row = (DataRowView)tabellendaten.dgTabelleOriginal.Items[index];
                    //DataRowView row = (DataRowView)tabDaten.dgTabelle.SelectedItem;
                    if (row != null)
                    {
                        int counter = 0;
                        foreach (var item in row.Row.ItemArray)
                        {
                            if (counter != 0)
                            {
                                csvFeldwerte.Append(item.ToString() + ";");
                            }
                            else
                            {
                                //item ist die Id des Datensatzes
                                pflegeTabellendaten._idAktuellerDatensatz = Int32.Parse(item.ToString());
                            }
                            counter++;
                        }
                        string txtUebergabe = csvFeldwerte.ToString();
                        txtUebergabe = txtUebergabe.Substring(0, txtUebergabe.Length - 1);
                        // Grid mit den Werten neu zeichnen
                        pflegeTabellendaten.zeichenGrid(pflegeTabellendaten._tabName, pflegeTabellendaten._csvTabFeldnamen, pflegeTabellendaten._csvTabFeldtypen, txtUebergabe);
                    }
                }
            }
        }

        private void TvMain_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if((TreeViewItem)uebersichtTabellen.tvMain.SelectedItem != null)
            {
                TreeViewItem tv_header = (TreeViewItem)uebersichtTabellen.tvMain.SelectedItem;
                pflegeTabellendaten.TabNameUebergabe = tv_header.Header.ToString();
             }
        }

        private void ScanOrdner_SomethingChanged(object sender, MyEventArgs e)
        {
            //TODO Falls das gerade in Arbeit befindliche Dokument gelöscht wird
        }
        
        private void GrdDropzone_Drop(object sender, DragEventArgs e)
        {
            eingabeDokumentDaten.Visibility = Visibility.Visible;
            //grdMain.Children.Add(eingabeDokumentDaten);
            eingabeDokumentDaten.Dropped = true;
            eingabeDokumentDaten.Dateiname = dropfeld.Data[0];
            eingabeDokumentDaten.zeichneGrid();
        }

        private void ScanOrdner_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ScanOrdner _sender = (ScanOrdner)sender;

            eingabeDokumentDaten.Dropped = false;
            eingabeDokumentDaten.Dateiname = _sender.FileName;
            eingabeDokumentDaten.Visibility = Visibility.Visible;
            if (eingabeDokumentDaten.grdMain.Children.Contains(eingabeDokumentDaten)) { 
            eingabeDokumentDaten.grdMain.Children.Add(eingabeDokumentDaten);
                }
            eingabeDokumentDaten.zeichneGrid();
        }
        #endregion

        #region Events/Commands UI

        #region Mainmenu
        private void BtnBeenden_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Blendet die Usercontrols zu den RibbonTabs ein und aus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            grdMain.Children.Clear();
            TxtAblageRecherche = "A";
            Ribbon _sender = (Ribbon)sender;
            RibbonTab sel = (RibbonTab)_sender.SelectedItem;
            if (sel != null)
            {
                if (sel.Name.Equals("RibTabAblage"))
                {
                    if (!grdMain.Children.Contains(aendernDokTyp))
                    {
                        rdOben.MinHeight = 550;
                        rdUnten.MinHeight = 50;
                        cdLinks.MinWidth = 300;
                        grdMain.Children.Add(aendernDokTyp);
                        aendernDokTyp.zeichneGrid();
                        aendernDokTyp.Visibility = Visibility.Visible;

                        grdMain.Children.Add(ordnerAnzeigen);
                        ordnerAnzeigen.Start(Einstellungen.Ordnerpfad);
                        ordnerAnzeigen.Visibility = Visibility.Visible;
                        grdMain.Background = new SolidColorBrush(Colors.White);
                    }
                }
                else if (sel.Name.Equals("RibTabStamm"))
                {
                    if (!grdMain.Children.Contains(uebersichtTabellen))
                    {
                        cdLinks.MinWidth = 200;
                        rdOben.MinHeight = 300;
                        rdUnten.MinHeight = 0;
                        grdMain.Children.Add(uebersichtTabellen);
                        grdMain.Children.Add(eingabeTabelle);
                        grdMain.Children.Add(uploadCsv);
                        grdMain.Children.Add(aendernTabellen);
                        grdMain.Background = new SolidColorBrush(Colors.White);
                    }
                }
                else if (sel.Name.Equals("RibTabDms"))
                {
                    //TODO Unterscheiden zwischen Ablage und Recherche
                    grdMain.Background = new SolidColorBrush(Colors.AliceBlue);
                    cdLinks.MinWidth = 300;
                    rdOben.MaxHeight = 50;
                    rdOben.MinHeight = 50;
                    rdUnten.MinHeight = 400;
                    eingabeDokumentDaten.grdMain.Children.Clear();
                    grdMain.Children.Add(dropfeld);
                    dropfeld.Visibility = Visibility.Visible;
                    grdMain.Children.Add(scanOrdner);
                    scanOrdner.Visibility = Visibility.Visible;
                    scanOrdner.zeichneGrid(Einstellungen.Ordnerpfad);
                    grdMain.Children.Add(eingabeDokumentDaten);
                    eingabeDokumentDaten.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Blendet die RibbonTabs zur Datenbearbeitung ein und aus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDatenbearbeitung_Click(object sender, RoutedEventArgs e)
        {
            if (ribbon.Items.Contains(RibTabStamm) && ribbon.Items.Contains(RibTabAblage))
            {
                ribbon.Items.Remove(RibTabAblage);
                ribbon.Items.Remove(RibTabStamm);
                ramDbEinAus.ImageSource = new BitmapImage(new Uri("/img/aus.png", UriKind.Relative));
                ((DbConnector)App.Current.Properties["Connector"]).SpeichereDatenbearbeitungEinAus("aus");
            }
            else
            {
                ribbon.Items.Remove(RibTabDms);
                ribbon.Items.Add(RibTabStamm);
                ribbon.Items.Add(RibTabAblage);
                ribbon.Items.Add(RibTabDms);
                ramDbEinAus.ImageSource = new BitmapImage(new Uri("/img/ein.png", UriKind.Relative));
                ((DbConnector)App.Current.Properties["Connector"]).SpeichereDatenbearbeitungEinAus("ein");
            }
        }
        #endregion

        #region rgStammLinks

        private bool CheckUcsHidden() {
            if (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Hidden
                    && uploadCsv != null && uploadCsv.Visibility == Visibility.Hidden
                    && aendernTabellen != null && aendernTabellen.Visibility == Visibility.Hidden
                    && tabellendaten != null && tabellendaten.Visibility == Visibility.Hidden
                    && pflegeTabellendaten != null && pflegeTabellendaten.Visibility == Visibility.Hidden
                    ) {
                return true;
            }
            return false;       
        }

        #region Neue Tabelle
        private void BtnNeueTabelle_Click(object sender, RoutedEventArgs e)
        {
            Ausgangsstellung();
            grdMain.Children.Add(eingabeTabelle);
            eingabeTabelle.Visibility = Visibility.Visible;
            eingabeTabelle.Start();
            uebersichtTabellen.IsEnabled = false;
        }

        private void BtnNeueTabelle_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnNeueTabelle_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CheckUcsHidden();
        }
        #endregion

        #region Neue Tabelle aus CSV
        private void BtnNeueTabelleCSV_Click(object sender, RoutedEventArgs e)
        {
            Ausgangsstellung();
            grdMain.Children.Add(uploadCsv);
            uploadCsv.Visibility = Visibility.Visible;

            char csvTrenner = new char();

            //Werte löschen falls vorhanden
            uploadCsv.stringArrayListe = new List<string[]>();
            uploadCsv.alleTabellenfelder = new List<EingabeTabellenfelder>();
            uploadCsv.alleTabellenFeldNamen = new List<string>();

            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV (.csv)|*.csv|XLS (.xls)|*.xls";
            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                //Datei ist ausgewählt, nun Parameter festlegen:
                CsvDialog inputDialog = new CsvDialog();
                if (inputDialog.ShowDialog() == true)
                {
                    csvTrenner = Convert.ToChar(inputDialog.Trenner);
                    uploadCsv.CsvBoolTrueFalse = inputDialog.boolTrueFalse;
                }


                var lines = File.ReadAllLines(dlg.FileName);
                if (lines != null && lines.Length > 0)
                {
                    var firstLine = lines[0];

                    uploadCsv.dgDaten.ItemsSource = uploadCsv.stringArrayListe;
                    uploadCsv.dgDaten.Columns.Clear();

                    int counter = 0;
                    foreach (var item in firstLine.Split(csvTrenner))
                    {
                        EingabeTabellenfelder tabFeld = new EingabeTabellenfelder("x");
                        tabFeld.txtBezeichnung.Text = item.ToString();
                        tabFeld.Margin = new Thickness(5, 5, 0, 0);
                        uploadCsv.alleTabellenfelder.Add(tabFeld);
                        uploadCsv.alleTabellenFeldNamen.Add(item);
                        uploadCsv.dgDaten.Columns.Add(new DataGridTextColumn
                        {
                            Header = item.ToString(),
                            Binding = new System.Windows.Data.Binding("[" + counter + "]")
                        });
                        counter++;
                    }

                    counter = 0;
                    foreach (var item in lines)
                    {
                        if (counter > 0)
                        {
                            var test = item.Split(csvTrenner);
                            uploadCsv.stringArrayListe.Add(test);
                        }
                        counter++;
                    }

                    uploadCsv.zeichneGrid();
                    uploadCsv.dgDaten.ItemsSource = uploadCsv.stringArrayListe;
                }
            }
            uebersichtTabellen.IsEnabled = false;
        }

        private void BtnNeueTabelleCSV_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CheckUcsHidden();
        }

        private void BtnNeueTabelleCSV_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Tabelle Bearbeiten
        private void BtnTabelleBearbeiten_Click(object sender, RoutedEventArgs e)
        {            
            TreeViewItem tvi = (TreeViewItem)uebersichtTabellen.tvMain.SelectedItem;
            string tabName = tvi.Header.ToString();      
            
            aendernTabellen.Restart(tabName, true);
            aendernTabellen.zeichneGrid();
            Ausgangsstellung();
            grdMain.Children.Add(aendernTabellen);
            aendernTabellen.Visibility = Visibility.Visible;
            uebersichtTabellen.IsEnabled = false;
        }

        private void BtnTabelleBearbeiten_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (uebersichtTabellen != null)
            {
                e.CanExecute = uebersichtTabellen.tvMain.SelectedItem != null
                    && CheckUcsHidden();
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void BtnTabelleBearbeiten_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Tabellendaten bearbeiten
        private void BtnTabellendatenBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            
            TreeView tv = uebersichtTabellen.tvMain;
            if (tv.SelectedItem != null)
            {
                Tuple<string, string, string> tuple = uebersichtTabellen.WerteDerAuswahl;
                pflegeTabellendaten.zeichenGrid(tuple.Item1, tuple.Item2, tuple.Item3);
                pflegeTabellendaten.TabNameUebergabe = tuple.Item1;
                tabellendaten.zeichneTabelle(tuple.Item1);

                Ausgangsstellung();
                rdUnten.MinHeight = 300;
                grdMain.Children.Add(tabellendaten);
                tabellendaten.Visibility = Visibility.Visible;
                grdMain.Children.Add(pflegeTabellendaten);
                pflegeTabellendaten.Visibility = Visibility.Visible;
                uebersichtTabellen.IsEnabled = false;
            }
        }

        private void BtnTabellendatenBearbeiten_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (uebersichtTabellen != null)
            {
                e.CanExecute = uebersichtTabellen.tvMain.SelectedItem != null 
                    && CheckUcsHidden();
            }
            else
            {
                e.CanExecute = false;
                
            }
            e.Handled = true;
        }

        private void BtnTabellendatenBearbeiten_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Tabelle leeren
        private void BtnTabelleLeeren_Click(object sender, RoutedEventArgs e)
        {
            //TreeViewItem tvi = (TreeViewItem)uebersichtTabellen.tvMain.SelectedItem;
            //string tabName = tvi.Header.ToString();
            string tabName = pflegeTabellendaten.TabNameUebergabe;
            ((DbConnector)App.Current.Properties["Connector"]).DeleteAllTableData(tabName);
            tabellendaten.Clear();
            tabellendaten.zeichneTabelle(tabName);
        }

        private void BtnTabelleLeeren_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (pflegeTabellendaten != null && pflegeTabellendaten.TabNameUebergabe != null)
            {
                e.CanExecute = !((DbConnector)App.Current.Properties["Connector"]).CheckReferenzen(pflegeTabellendaten.TabNameUebergabe)
                    && ((DbConnector)App.Current.Properties["Connector"]).CheckTableDeleteDataDokTyp(pflegeTabellendaten.TabNameUebergabe);
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void BtnTabelleLeeren_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Tabelle löschen
        private void BtnTabelleLoeschen_Click(object sender, RoutedEventArgs e)
        {
            string tabName = pflegeTabellendaten.TabNameUebergabe;
            ((DbConnector)App.Current.Properties["Connector"]).DeleteTable(tabName);
            Ausgangsstellung();
            uebersichtTabellen.Reset();
            uebersichtTabellen.IsEnabled = true;
        }

        private void BtnTabelleLoeschen_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (pflegeTabellendaten != null && pflegeTabellendaten.TabNameUebergabe != null)
            {
                e.CanExecute = !((DbConnector)App.Current.Properties["Connector"]).CheckReferenzen(pflegeTabellendaten.TabNameUebergabe)
                    && ((DbConnector)App.Current.Properties["Connector"]).CheckTableDeleteDataDokTyp(pflegeTabellendaten.TabNameUebergabe);
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void BtnTabelleLoeschen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion
        
        #endregion

        #region rgStammRechts
        #region Neue Zeile
            private void BtnNeueZeile_Click(object sender, RoutedEventArgs e)
            {
                if (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Visible) {
                    EingabeTabellenfelder ds = new EingabeTabellenfelder();
                    eingabeTabelle.alleDatensaetze.Add(ds);
                    eingabeTabelle.zeichneGrid();
                }

                if (tabellendaten != null && tabellendaten.Visibility == Visibility.Visible) {
                tabellendaten.dgTabelle.SelectedItem = null;
                pflegeTabellendaten.Clear();
                pflegeTabellendaten.zeichenGrid(pflegeTabellendaten.TabNameUebergabe, pflegeTabellendaten._csvTabFeldnamen, pflegeTabellendaten._csvTabFeldtypen);
                }

            if (aendernTabellen != null && aendernTabellen.Visibility == Visibility.Visible)
            {
                EingabeTabellenfelder eingabeTabellenfeld = new EingabeTabellenfelder("", aendernTabellen.Tabelle);
                eingabeTabellenfeld.chkLoeschen.ToolTip = "Hier klicken, um das Feld wieder zu entfernen!";
                aendernTabellen.FelderHinzufuegen.Add(eingabeTabellenfeld);
                aendernTabellen.zeichneGrid();
            }

        }

        private void BtnNeueZeile_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Visible
                    || (tabellendaten != null && tabellendaten.Visibility == Visibility.Visible)
                    || (aendernTabellen != null && aendernTabellen.Visibility == Visibility.Visible)
                    );
            }

            private void BtnNeueZeile_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Abbrechen
        private void BtnAbort_Click(object sender, RoutedEventArgs e)
        {
            if (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Visible) {
                eingabeTabelle.ClearGrid();
                eingabeTabelle.zeichneGrid();  
            }

            if (uploadCsv != null && uploadCsv.Visibility == Visibility.Visible) {
                uploadCsv.Clear();
            }

            if (aendernTabellen != null && aendernTabellen.Visibility == Visibility.Visible) {
                aendernTabellen.Clear();
            }

            if (tabellendaten != null && tabellendaten.Visibility == Visibility.Visible) {
                rdUnten.MinHeight = 0;
            }

            Ausgangsstellung();
            uebersichtTabellen.IsEnabled = true;
        }

        private void BtnAbbrechen_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (
                eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Visible 
                || uploadCsv != null && uploadCsv.Visibility == Visibility.Visible
                || aendernTabellen != null && aendernTabellen.Visibility == Visibility.Visible
                || tabellendaten != null && tabellendaten.Visibility == Visibility.Visible
                );
        }

        private void BtnAbbrechen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Zeile entfernen
            private void BtnZeileEntfernen_Click(object sender, RoutedEventArgs e)
            {
            if (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Visible) {
                List<EingabeTabellenfelder> liste = new List<EingabeTabellenfelder>();
                    foreach (var item in eingabeTabelle.alleDatensaetze)
                    {
                        if ((bool)item.chkLoeschen.IsChecked)
                        {
                            liste.Add(item);
                        }
                    }
                    foreach (var item in liste)
                    {
                        eingabeTabelle.alleDatensaetze.Remove(item);
                    }
                eingabeTabelle.zeichneGrid();
            }

            if (tabellendaten != null && tabellendaten.Visibility == Visibility.Visible) {
                ((DbConnector)App.Current.Properties["Connector"]).DeleteTableData(pflegeTabellendaten._tabName, pflegeTabellendaten._idAktuellerDatensatz);
                tabellendaten.zeichneTabelle(pflegeTabellendaten.TabNameUebergabe);
                pflegeTabellendaten.zeichenGrid(pflegeTabellendaten._tabName, pflegeTabellendaten._csvTabFeldnamen, pflegeTabellendaten._csvTabFeldtypen);
            }

            if (aendernTabellen != null && aendernTabellen.Visibility == Visibility.Visible)
            {
                List<EingabeTabellenfelder> ZuEntfernen = new List<EingabeTabellenfelder>();
                foreach (EingabeTabellenfelder elem in aendernTabellen.FelderHinzufuegen) {
                    if (elem.chkLoeschen.IsChecked == true) {
                        ZuEntfernen.Add(elem);
                    }
                }

                foreach (EingabeTabellenfelder elem in ZuEntfernen) {
                    aendernTabellen.FelderHinzufuegen.Remove(elem);
                }
                aendernTabellen.zeichneGrid();
            }

        }

            private void BtnZeileEntfernen_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
            {
            if (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Visible)
            {
                foreach (var item in eingabeTabelle.alleDatensaetze)
                {
                    if ((bool)item.chkLoeschen.IsChecked)
                    {
                        e.CanExecute = true;
                        return;
                    }
                }
            }
            else if (tabellendaten != null && tabellendaten.Visibility == Visibility.Visible)
            {
                if (tabellendaten.dgTabelle.SelectedItem != null)
                {
                    if (((DbConnector)App.Current.Properties["Connector"]).CheckIfIdLoeschbarStammdaten(pflegeTabellendaten._tabName, pflegeTabellendaten._idAktuellerDatensatz)) {
                        e.CanExecute = true;
                        return;
                    }
                    
                }
            }
            else if (aendernTabellen != null && aendernTabellen.Visibility == Visibility.Visible)
            {
                foreach (EingabeTabellenfelder elem in aendernTabellen.FelderHinzufuegen) {
                    if (elem.chkLoeschen.IsChecked == true) {
                        e.CanExecute = true;
                    } 
                }
            }
            else
            {
                e.CanExecute = false;
            }
                
            }

            private void BtnZeileEntfernen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            {

            }
        #endregion

        #region Save
        private void BtnTabSave_Click(object sender, RoutedEventArgs e)
        {
            #region Neue Tabelle speichern
            if (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Visible)
            {

                Dictionary<string, string> werte = new Dictionary<string, string>();
                //TODO AB HIER DIALOG FÜR TABELLENNAMEN, DAHER txtTebellenname nehmen und weiter, ansonsten abbrechen
                string tabName = "";

                TabNameDialog dialog = new TabNameDialog(eingabeTabelle);
                if (dialog.ShowDialog() == true)
                {
                    tabName = dialog.txtTabName.Text;
                }
                else
                {
                    return;
                }

                //Tabelle merken für hoch bubblemdes event
                string ersatztext = "";
                foreach (EingabeTabellenfelder item in eingabeTabelle.alleDatensaetze)
                {
                    var strIn = item.comBoxFeldtyp.Text;
                    if (strIn.Equals("Nachschlagefeld"))
                    {
                        string tag = item.txtBezeichnung.Tag.ToString();
                        ersatztext = "|x|" + item.txtBezeichnung.Text + "|" + (tag.Replace("_", "|"));
                    }
                    var strOut = ((ComboBoxItem)item.comBoxFeldtyp.SelectedItem).Tag.ToString();
                    if (ersatztext.Equals(""))
                    {
                        werte.Add(item.txtBezeichnung.Text, strOut);
                    }
                    else
                    {
                        werte.Add(ersatztext, strOut);
                        ersatztext = "";
                    }
                }
            //In die Datenbank schreiben
            ((DbConnector)App.Current.Properties["Connector"]).CreateNewTable(tabName, werte);
                eingabeTabelle.Start();
                Ausgangsstellung();
            }
            #endregion

            #region Neue CSV speichern
            if (uploadCsv != null && uploadCsv.Visibility == Visibility.Visible)
            {

                //Array für eventuelle Stringmarkierer
                string[] arrStringMarkierer = { "\'", "\"" };
                //Benötigte Werte um InsertTableData aufrufen zu können
                StringBuilder _csv = new StringBuilder();

                //Erst neue Tabelle anlegen
                Dictionary<string, string> werte = new Dictionary<string, string>();
                //TODO AB HIER DIALOG FÜR TABELLENNAMEN, DAHER txtTebellenname nehmen und weiter, ansonsten abbrechen
                string tabName = "";

                TabNameDialog dialog = new TabNameDialog(eingabeTabelle);
                if (dialog.ShowDialog() == true)
                {
                    tabName = dialog.txtTabName.Text;
                }
                else
                {
                    return;
                }
                //Tabelle merken für hoch bubblemdes event
                //foreach (DataGridTextColumn item in dgDaten.Columns)
                uploadCsv.alleTabellenFeldNamen.Clear();
                foreach (var item in uploadCsv.alleTabellenfelder)
                {
                    var strIn = item.comBoxFeldtyp.Text;
                    var strOut = ((ComboBoxItem)item.comBoxFeldtyp.SelectedItem).Tag.ToString();
                    werte.Add(item.txtBezeichnung.Text, strOut);
                    _csv.Append(((ComboBoxItem)item.comBoxFeldtyp.SelectedItem).Tag.ToString().Substring(0, 3) + ";");
                    uploadCsv.alleTabellenFeldNamen.Add(item.txtBezeichnung.Text);
                }

            //In die Datenbank schreiben
            ((DbConnector)App.Current.Properties["Connector"]).CreateNewTable(tabName, werte);

                //Daten aus dgDaten in die neue Tabelle schreiben
                string txtWerte = _csv.ToString().Substring(0, _csv.Length - 1);
                string[] txtWerteArray = txtWerte.Split(';');
                //Für Aktualisierung in MainWindow merken
                uploadCsv.TabNameUebergabe = tabName;

                //Die Items aus dem DataGrid in die DB schreiben
                List<Dictionary<string, object>> _dicList = new List<Dictionary<string, object>>();
                foreach (string[] item in uploadCsv.dgDaten.Items)
                {
                    //Benötigte Werte um InsertTableData aufrufen zu können
                    Dictionary<string, object> _dic = new Dictionary<string, object>();
                    int counter = 0;
                    foreach (var elem in item)
                    {
                        //An dieser STelle checken, ob es sich um einen bool-Wert handelt und dementsprechend übersetzen
                        bool usedChecker = false;
                        string entry = "false";
                        if (txtWerteArray[counter].Equals("bol"))
                        {
                            usedChecker = true;

                            //Die einzelnen Eingabearten unterscheiden

                            if (uploadCsv.CsvBoolTrueFalse.Equals("1/0"))
                            {
                                if (elem.Equals("1")) { entry = "true"; }
                            }
                            if (uploadCsv.CsvBoolTrueFalse.Equals("true/false"))
                            {
                                if (elem.ToUpper().Trim().Equals("TRUE")) { entry = "true"; }
                            }
                            if (uploadCsv.CsvBoolTrueFalse.Equals("ja/nein"))
                            {
                                if (elem.ToUpper().Trim().Equals("JA")) { entry = "true"; }
                            }
                            if (uploadCsv.CsvBoolTrueFalse.Equals("yes/no"))
                            {
                                if (elem.ToUpper().Trim().Equals("YES")) { entry = "true"; }
                            }
                            if (uploadCsv.CsvBoolTrueFalse.Equals("Wert/kein Wert"))
                            {
                                if (elem.Length > 0) { entry = "true"; }
                            }
                            _dic.Add(uploadCsv.alleTabellenFeldNamen.ElementAt(counter), entry);
                        }

                        //Auch die Markierungen für Strings entfernen
                        if (txtWerteArray[counter].Equals("txt"))
                        {
                            string alles = elem.ToString();
                            //if (arrStringMarkierer.Contains(elem.First().ToString())) { alles = alles.Substring(1, alles.Length ); }
                            //if (arrStringMarkierer.Contains(elem.Last().ToString())) { alles = alles.Substring(0, alles.Length - 1); }
                            _dic.Add(uploadCsv.alleTabellenFeldNamen.ElementAt(counter), alles);
                            usedChecker = true;
                        }

                        if (!usedChecker)
                        {
                            _dic.Add(uploadCsv.alleTabellenFeldNamen.ElementAt(counter), elem);
                        }

                        counter++;
                    }
                    _dicList.Add(_dic);
                }
            ((DbConnector)App.Current.Properties["Connector"]).InsertCsvData(tabName, _dicList, txtWerte);
                uploadCsv.Clear();
                Ausgangsstellung();
            }
            #endregion

            #region Tabellendaten
            if (tabellendaten != null && tabellendaten.Visibility == Visibility.Visible)
            {

                //Benötigte Werte um InsertTableData aufrufen zu können
                string _tabellenname = "";
                Dictionary<string, object> _dic = new Dictionary<string, object>();
                StringBuilder _csv = new StringBuilder();

                string keepValueForDic = "";
                //Datensatz in DB eintragen
                int counter = 0; //wird benötigt, um erstes Element zu kennzeichnen
                foreach (var item in pflegeTabellendaten.grdMain.Children)
                {
                    //item kann Textbox oder Textblock sein

                    //Textblock kann 'Tabellenname' oder 'Feldname (Typ)' sein
                    if (item.GetType() == typeof(TextBlock))
                    {
                        if (counter == 0)
                        {
                            _tabellenname = ((TextBlock)item).Text;
                        }
                        else
                        {
                            //Tabellenfeldname --> Neues Element für Dictionary, aber erst den Wert merken
                            //Wert muss gesplittet werden, davon [0] nehmen
                            keepValueForDic = ((TextBlock)item).Text.Split(' ')[0];
                            var str = ((TextBlock)item).Text.Split(' ')[1].Replace("(", "").Replace(")", "") + ";";
                            if (str.Substring(0, 3).Equals("loo")) { keepValueForDic = ((TextBlock)item).Tag.ToString(); }
                            _csv.Append(str);
                        }
                    }

                    //Textbox kann nur zu einem Feldnamen gehören, der bereits als Key in die Dictionary einger
                    if (item.GetType() == typeof(TextBox))
                    {
                        //Nun neues Element für Dictionary erzeugen und Werte intragen
                        var kvp = new KeyValuePair<string, object>(keepValueForDic, ((TextBox)item).Text);
                        _dic.Add(kvp.Key, kvp.Value);
                    } //Das Gleiche gilt für eine Checkbox bei Boolean-Werten
                    else if (item.GetType() == typeof(CheckBox))
                    {
                        var kvp = new KeyValuePair<string, object>(keepValueForDic, ((CheckBox)item).IsChecked);
                        _dic.Add(kvp.Key, kvp.Value);
                    }
                    else if (item.GetType() == typeof(DatePicker))
                    {
                        var kvp = new KeyValuePair<string, object>(keepValueForDic, ((DatePicker)item).SelectedDate);
                        _dic.Add(kvp.Key, kvp.Value);
                    }
                    else if (item.GetType() == typeof(LookupAuswahl))
                    {
                        if ((ComboBoxItem)((LookupAuswahl)item).cboAuswahl.SelectedItem != null)
                        {
                            var kvp = new KeyValuePair<string, object>(keepValueForDic, ((ComboBoxItem)((LookupAuswahl)item).cboAuswahl.SelectedItem).Tag);
                            _dic.Add(kvp.Key, kvp.Value);
                        }
                        else
                        {
                            var kvp = new KeyValuePair<string, object>(keepValueForDic, null);
                            _dic.Add(kvp.Key, kvp.Value);
                        }

                    }
                    counter++;
                }
                //DBConnector aufrufen
                string txt = _csv.ToString().Substring(0, _csv.Length - 1);
                //Für Aktualisierung in MainWindow merken
                pflegeTabellendaten.TabNameUebergabe = _tabellenname;

                if (pflegeTabellendaten.IstneuerDatensatz)
                {
                    ((DbConnector)App.Current.Properties["Connector"]).InsertTableData(_tabellenname, _dic, txt);
                    pflegeTabellendaten.zeichenGrid(pflegeTabellendaten._tabName, pflegeTabellendaten._csvTabFeldnamen, pflegeTabellendaten._csvTabFeldtypen);
                }
                else if (!pflegeTabellendaten.IstneuerDatensatz)
                {
                    ((DbConnector)App.Current.Properties["Connector"]).UpdateTableData(_tabellenname, pflegeTabellendaten._idAktuellerDatensatz, _dic, txt);
                    StringBuilder sb = new StringBuilder();
                    foreach (var value in _dic.Values)
                    {
                        sb.Append(value + ";");
                    }
                    string txtCsv = sb.ToString();
                    pflegeTabellendaten._csvTabFeldwerte = txtCsv.Substring(0, txtCsv.Length - 1);
                    pflegeTabellendaten.zeichenGrid(pflegeTabellendaten._tabName, pflegeTabellendaten._csvTabFeldnamen, pflegeTabellendaten._csvTabFeldtypen, pflegeTabellendaten._csvTabFeldwerte);
                }
                tabellendaten.zeichneTabelle(pflegeTabellendaten.TabNameUebergabe);
                pflegeTabellendaten.Clear();
                pflegeTabellendaten.zeichenGrid(pflegeTabellendaten.TabNameUebergabe, pflegeTabellendaten._csvTabFeldnamen, pflegeTabellendaten._csvTabFeldtypen);
            }
            #endregion

            #region AendernTabellen
            if (aendernTabellen != null && aendernTabellen.Visibility == Visibility.Visible)
            {
                ((DbConnector)App.Current.Properties["Connector"]).ChangeTableStructure(aendernTabellen.Tabelle, aendernTabellen.FelderLoeschen, aendernTabellen.FelderHinzufuegen);
                aendernTabellen.FelderHinzufuegen = new List<EingabeTabellenfelder>();
                aendernTabellen.Restart(aendernTabellen.Tabelle, true);
                Ausgangsstellung();
            }
            #endregion
        }

        private void BtnTabSave_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            // Neue Tabelle
            if (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Visible)
            {
                //if (eingabeTabelle == null) {
                //    e.CanExecute = false; return;
                //}
                //Es dürfen nur Buchstaben und Nummern als Bezeichner verwendet werden
                string AllowedChars = @"^[a-zA-Z0-9]+$";

                //Variable dient nur dem Test, ob es schon Tabellenfelder gibt, ansonsten darf der Button ja auch nicht aktiv sein
                bool felderChecker = false;
                //Variable, um zu testen, ob Feldnamen doppelt vorhanden sind
                List<string> _feldnamen = new List<string>();

                foreach (var item in eingabeTabelle.grdMain.Children)
                {
                    if (item.GetType() == typeof(EingabeTabellenfelder))
                    {
                        felderChecker = true;
                        var test = (EingabeTabellenfelder)item;
                        if (test.txtBezeichnung.Text.Equals("") || test.comBoxFeldtyp.SelectedItem == null)
                        {
                            e.CanExecute = false;
                            return;
                        }

                        //CHecken ob EIntrag schon mal vorhanden
                        if (_feldnamen.Contains(test.txtBezeichnung.Text))
                        {
                            e.CanExecute = false;
                            return;
                        }
                        else
                        {
                            //In den Feldnamen dürfen auch keine SOnderzeichen enthalten sein
                            if (!Regex.IsMatch(test.txtBezeichnung.Text, AllowedChars))
                            {
                                e.CanExecute = false;
                                return;
                            }
                            _feldnamen.Add(test.txtBezeichnung.Text);
                        }
                    }
                }

                ////Noch testen, ob ein Tabellenname eingegeben wurde + Es dürfen nur Nummer und Ziffern verwendet werden + Es dürfen keine Tabellennamen doppelt sein
                //if (eingabeTabelle.txtTabellenname.Text.Equals("") || !Regex.IsMatch(eingabeTabelle.txtTabellenname.Text, AllowedChars) || eingabeTabelle.alleTabellenNamen.Contains(eingabeTabelle.txtTabellenname.Text)) { felderChecker = false; }
                //Wenn Tabellenfelder da, nur dann kann der Button überhaupt aktiv sein
                if (felderChecker) { e.CanExecute = true; } else { e.CanExecute = false; }
            }
            else
            // Neue Tabelle aus CSV
            if (uploadCsv != null && uploadCsv.Visibility == Visibility.Visible)
            {
                //Zuerst schauen, ob alle Spaltendaten korrekt sind
                if (!uploadCsv.CheckSpaltenDaten())
                {
                    e.CanExecute = false;
                    return;
                }

                //Es dürfen nur Buchstaben und Nummern als Bezeichner verwendet werden
                string AllowedChars = @"^[a-zA-Z0-9]+$";


                //Variable dient nur dem Test, ob es schon Tabellenfelder gibt, ansonsten darf der Button ja auch nicht aktiv sein
                bool felderChecker = false;
                //Variable, um zu testen, ob Feldnamen doppelt vorhanden sind
                List<string> _feldnamen = new List<string>();

                foreach (var item in uploadCsv.grdTabFelder.Children)
                {
                    if (item.GetType() == typeof(EingabeTabellenfelder))
                    {
                        felderChecker = true;
                        var test = (EingabeTabellenfelder)item;
                        if (test.txtBezeichnung.Text.Equals("") || test.comBoxFeldtyp.SelectedItem == null)
                        {
                            e.CanExecute = false;
                            return;
                        }
                        //CHecken ob EIntrag schon mal vorhanden
                        if (_feldnamen.Contains(test.txtBezeichnung.Text))
                        {
                            e.CanExecute = false;
                            return;
                        }
                        else
                        {
                            //In den Feldnamen dürfen auch keine SOnderzeichen enthalten sein
                            if (!Regex.IsMatch(test.txtBezeichnung.Text, AllowedChars))
                            {
                                e.CanExecute = false;
                                return;
                            }
                            _feldnamen.Add(test.txtBezeichnung.Text);
                        }
                    }
                }

                //Wenn Tabellenfelder da, nur dann kann der Button überhaupt aktiv sein
                if (felderChecker) { e.CanExecute = true; }
                else
                {
                    e.CanExecute = false;
                }
            }
            else
            //Tabellendaten Speichern
            if (tabellendaten != null && tabellendaten.Visibility == Visibility.Visible) {
                e.CanExecute = true;
            }
            else
            //Tabellenänderungen Speichern
            if (aendernTabellen != null && aendernTabellen.Visibility == Visibility.Visible)
            {

                List<string> Checkliste = aendernTabellen.FelderStart;
                e.CanExecute = true;
                //sollte noch garnichts passiert sein, ist es BLödsinn zu speichern
                if (aendernTabellen.FelderLoeschen.Count() == 0 && grdMain.Children.Count == aendernTabellen._anzahlFelder)
                {
                    e.CanExecute = false;
                }
                //Es dürfen nur Buchstaben und Nummern als Bezeichner verwendet werden
                string AllowedChars = @"^[a-zA-Z0-9]+$";

                foreach (var item in aendernTabellen.grdMain.Children)
                {
                    //Ist die ANzahl der zu löschenden Felder gleich der Anzahl der existierenden Felder, gibt es keine Felder mehr --> false
                    if (aendernTabellen.FelderLoeschen.Count() == (aendernTabellen._anzahlFelder - aendernTabellen._anzahlFelderDisabled))
                    {
                        e.CanExecute = false;
                    }

                    if (item.GetType() == typeof(EingabeTabellenfelder))
                    {
                        //Es gibt EIngabefelder, also per se erst mal wieder true
                        e.CanExecute = true;
                        //Erst mal schauen, ob überhaupt ausgefüllt
                        EingabeTabellenfelder eingabeTabellenfeld = (EingabeTabellenfelder)item;
                        if (eingabeTabellenfeld.txtBezeichnung.Text.Length < 3 || eingabeTabellenfeld.comBoxFeldtyp.SelectedItem == null)
                        {
                            e.CanExecute = false;
                        }
                        //ob korrekt ausgefüllt
                        if (!Regex.IsMatch(eingabeTabellenfeld.txtBezeichnung.Text, AllowedChars))
                        {
                            e.CanExecute = false;
                        }
                        //Dann schauen, ob die Feldnamen schon existieren
                        if (Checkliste.Contains(eingabeTabellenfeld.txtBezeichnung.Text))
                        {
                            e.CanExecute = false;
                        }
                        //else {
                        //    //Checkliste.Add(eingabeTabellenfeld.txtBezeichnung.Text);
                        //TODO --> Es ist noch möglich, zwei mal denselben Namen als Tabellenfeld anzugeben
                        //}
                    }
                }
            }
            else
            {
                e.CanExecute = false;
            }
            
        }

        private void BtnTabSave_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            eingabeTabelle.ClearGrid();
            //Ausgangsstellung();
        }
        #endregion

        #endregion

        #region rgAblagedaten
        #region Feld Hinzu
        private void BtnFeldHinzu_Click(object sender, RoutedEventArgs e)
        {
            EingabeTabellenfelder eingabeTabellenfeld = new EingabeTabellenfelder("", "OkoDokumentenTyp");
            eingabeTabellenfeld.chkLoeschen.ToolTip = "Hier klicken, um das Feld wieder zu entfernen!";
            aendernDokTyp.FelderHinzufuegen.Add(eingabeTabellenfeld);
            aendernDokTyp.zeichneGrid();
        }

        private void BtnFeldHinzu_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void BtnFeldHinzu_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Feld entfernen
        private void BtnFeldEntfernen_Click(object sender, RoutedEventArgs e)
        {
            List<EingabeTabellenfelder> ZuEntfernen = new List<EingabeTabellenfelder>();
            foreach (EingabeTabellenfelder elem in aendernDokTyp.FelderHinzufuegen)
            {
                if (elem.chkLoeschen.IsChecked == true)
                {
                    ZuEntfernen.Add(elem);
                }
            }

            foreach (EingabeTabellenfelder elem in ZuEntfernen)
            {
                aendernDokTyp.FelderHinzufuegen.Remove(elem);
            }
            aendernDokTyp.zeichneGrid();
        }

        private void BtnFeldEntfernen_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (aendernDokTyp != null && aendernDokTyp.Visibility == Visibility.Visible)
            {
                foreach (EingabeTabellenfelder elem in aendernDokTyp.FelderHinzufuegen)
                {
                    if (elem.chkLoeschen.IsChecked == true)
                    {
                        e.CanExecute = true;
                    }
                }
            }
        }

        private void BtnFeldEntfernen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Speichern
        private void BtnTypSpeichern_Click(object sender, RoutedEventArgs e)
        {
            ((DbConnector)App.Current.Properties["Connector"]).ChangeDokTypStructure(aendernDokTyp.FelderLoeschen, aendernDokTyp.FelderHinzufuegen);
            aendernDokTyp.Clear();
            aendernDokTyp.zeichneGrid();
            darstellungDokumente.ZeichneDatagridForm();
            darstellungDokumente.ZeichneDatagridTab("OkoDokumentenTyp");
            TxtAblageRecherche = "A";
            eingabeDokumentDaten.zeichneGrid();
        }

        private void BtnTypSpeichern_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (aendernDokTyp == null) { e.CanExecute = false; return; }

            List<string> Checkliste = aendernDokTyp.FelderStart;

            e.CanExecute = true;
            //sollte noch garnichts passiert sein, ist es BLödsinn zu speichern
            if (aendernDokTyp.FelderHinzufuegen.Count() > 0 || aendernDokTyp.FelderLoeschen.Count() > 0)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }

            //Es dürfen nur Buchstaben und Nummern als Bezeichner verwendet werden
            string AllowedChars = @"^[a-zA-Z0-9]+$";

            foreach (var item in aendernDokTyp.grdMain.Children)
            {

                //Ist die ANzahl der zu löschenden Felder gleich der Anzahl der existierenden Felder, gibt es keine Felder mehr --> false
                //if (aendernDokTyp.FelderLoeschen.Count() == (aendernDokTyp._anzahlFelder - aendernDokTyp._anzahlFelderDisabled))
                //{
                //    e.CanExecute = false;
                //}

                if (item.GetType() == typeof(EingabeTabellenfelder))
                {
                    //Es gibt EIngabefelder, also per se erst mal wieder true
                    e.CanExecute = true;
                    //Erst mal schauen, ob überhaupt ausgefüllt
                    EingabeTabellenfelder eingabeTabellenfeld = (EingabeTabellenfelder)item;
                    if (eingabeTabellenfeld.txtBezeichnung.Text.Equals("") || eingabeTabellenfeld.comBoxFeldtyp.SelectedItem == null)
                    {
                        e.CanExecute = false;
                    }
                    //ob korrekt ausgefüllt
                    if (!Regex.IsMatch(eingabeTabellenfeld.txtBezeichnung.Text, AllowedChars))
                    {
                        e.CanExecute = false;
                    }
                    //Dann schauen, ob die Feldnamen schon existieren
                    if (Checkliste.Contains(eingabeTabellenfeld.txtBezeichnung.Text))
                    {

                        e.CanExecute = false;
                    }
                    //else {
                    //    //Checkliste.Add(eingabeTabellenfeld.txtBezeichnung.Text);
                    //TODO --> Es ist noch möglich, zwei mal denselben Namen als Tabellenfeld anzugeben
                    //}
                }
            }
        }

        private void BtnTypSpeichern_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Abbrechen
        private void BtnBearbeitungAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            aendernDokTyp.FelderLoeschen = new List<string>();
            aendernDokTyp.FelderHinzufuegen = new List<EingabeTabellenfelder>();
            aendernDokTyp.zeichneGrid();
        }

        private void BtnBearbeitungAbbrechen_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = aendernDokTyp != null && (aendernDokTyp.FelderHinzufuegen.Count() > 0 || aendernDokTyp.FelderLoeschen.Count() > 0);
        }

        private void BtnBearbeitungAbbrechen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Ordner festlegen
        private void BtnOrdnerFestlegen_Click(object sender, RoutedEventArgs e)
        {
            ordnerAnzeigen.Visibility = Visibility.Visible;
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) {
                    string pfad = dialog.SelectedPath;
                    Einstellungen.Ordnerpfad = pfad;
                    ordnerAnzeigen.Start(pfad);
                    eingabeDokumentDaten.zeichneGrid();
                }
            }
        }

        private void BtnOrdnerSpeichern_Click(object sender, RoutedEventArgs e)
        {
            ((DbConnector)App.Current.Properties["Connector"]).OrdnerSpeichern(ordnerAnzeigen.Pfad);
            ordnerAnzeigen.HasChanged = false;
            ordnerAnzeigen.Start(ordnerAnzeigen.Pfad);
        }

        private void BtnOrdnerSpeichern_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ordnerAnzeigen != null && ordnerAnzeigen.HasChanged;
        }

        private void BtnOrdnerSpeichern_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }


        #endregion

        private void BtnAnwendungen_Click(object sender, RoutedEventArgs e)
        {
            AnwendungsauswahlDialog dialog = new AnwendungsauswahlDialog();
            if (dialog.ShowDialog() == true)
            {
                //darstellungDokumente.Anwendungen = ((DbConnector)App.Current.Properties["Connector"]).ReadAnwendungen();
            }
        }

        #endregion

        #region  rgDMS
        #region Steuerung Ablage/Recherche
        private string TxtAblageRecherche = "A";
        private void BtnRecherche_Click(object sender, RoutedEventArgs e)
            {
                TxtAblageRecherche = "R";
                grdMainDmsGrundstellung();
                cdLinks.MinWidth = 500;
                rdOben.MaxHeight = 1000;
                rdOben.MinHeight = 500;
                rdUnten.MinHeight = 0;
                grdMain.Children.Add(darstellungDokumente);
                darstellungDokumente.Visibility = Visibility.Visible;
                darstellungDokumente.ZeichneGrid();
            }

            private void BtnAblage_Click(object sender, RoutedEventArgs e)
            {
                TxtAblageRecherche = "A";
                grdMainDmsGrundstellung();
                grdMain.Children.Add(dropfeld);
                dropfeld.Visibility = Visibility.Visible;
                grdMain.Children.Add(scanOrdner);
                scanOrdner.Visibility = Visibility.Visible;
                grdMain.Children.Add(eingabeDokumentDaten);
                //eingabeDokumentDaten.Visibility = Visibility.Visible;
            }

            private void grdMainDmsGrundstellung()
            {
                cdLinks.MinWidth = 300;
                rdOben.MaxHeight = 50;
                rdOben.MinHeight = 50;
                rdUnten.MinHeight = 400;
                grdMain.Children.Remove(dropfeld);
                dropfeld.Visibility = Visibility.Hidden;
                grdMain.Children.Remove(scanOrdner);
                scanOrdner.Visibility = Visibility.Hidden;
                grdMain.Children.Remove(eingabeDokumentDaten);
                eingabeDokumentDaten.Visibility = Visibility.Hidden;
                grdMain.Children.Remove(darstellungDokumente);
                darstellungDokumente.Visibility = Visibility.Hidden;
             }
        #endregion

            #region DOkument anzeigen
        private void BtnDokShow_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = (DataRowView)darstellungDokumente.dgTabelleOriginal.SelectedItem;
            int counter = 0;
            string IdInTabelle = "";
            string Dateiname = "";
            foreach (DataGridColumn item in darstellungDokumente.dgTabelleOriginal.Columns)
            {
                if (item.Header.Equals("OkoDokumentenTypId")) {
                    IdInTabelle = drv.Row.ItemArray[counter].ToString(); }
                if (item.Header.Equals("Dateiname")) {
                    Dateiname = drv.Row.ItemArray[counter].ToString(); }
                counter++;
            }
            int Id = ((DbConnector)App.Current.Properties["Connector"]).ReadIdDokument(IdInTabelle);
            databaseFileReadToMemoryStream(Id.ToString(), Dateiname);
        }

        private void BtnDokShow_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (TxtAblageRecherche.Equals("A"))
            {
                e.CanExecute = false;
            }
            else
            {
                //Buttons für die Recherche
                if (darstellungDokumente.dgTabelleOriginal.SelectedItem != null)
                {
                    e.CanExecute = true;
                    return;
                }
                e.CanExecute = false;
            }
        }

        private void BtnDokShow_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

            #region Dokument bearbeiten
            private void BtnDokEdit_Click(object sender, RoutedEventArgs e)
            {
            //Formular für die Eingabe muss mit ausgewähltem Datensatz befüllt werden
            DataRowView row = (DataRowView)darstellungDokumente.dgTabelleOriginal.SelectedItem;
            int idaktuell = Int32.Parse(row.Row.ItemArray[0].ToString());
            string dateiname = row.Row.ItemArray[row.Row.ItemArray.Length - 1].ToString();
            //CsvFeldwerte für Formular des Datensatzes erzeugen
            StringBuilder csv = new StringBuilder();
            for (int i = 1; i < row.Row.ItemArray.Count()-1; i++)
            {
                csv.Append(row.Row.ItemArray[i] + ";");
            }
            string csvwerte = csv.ToString().Substring(0, csv.Length - 1);

            TxtAblageRecherche = "A";
                grdMainDmsGrundstellung();
                grdMain.Children.Add(dropfeld);
                dropfeld.Visibility = Visibility.Visible;
                grdMain.Children.Add(scanOrdner);
                scanOrdner.Visibility = Visibility.Visible;
                grdMain.Children.Add(eingabeDokumentDaten);
                eingabeDokumentDaten.Visibility = Visibility.Visible;
                eingabeDokumentDaten.Dateiname = dateiname;
                eingabeDokumentDaten.TabNameUebergabe = dateiname;
                eingabeDokumentDaten._idAktuellerDatensatz = idaktuell;
                eingabeDokumentDaten.zeichneGrid(csvwerte);
            }
        private void BtnDokEdit_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
            {
                if (TxtAblageRecherche.Equals("A"))
                {
                    e.CanExecute = false;
                }
                else
                {
                    //Buttons für die Recherche
                    if (darstellungDokumente.dgTabelleOriginal.SelectedItem != null)
                    {
                        e.CanExecute = true;
                        return;
                    }
                    e.CanExecute = false;
                }
            }
            private void BtnDokEdit_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            {

            }
            #endregion

            #region Dokument löschen
            private void BtnDokDelete_Click(object sender, RoutedEventArgs e)
            {
                DataRowView row = (DataRowView)darstellungDokumente.dgTabelleOriginal.SelectedItem;
                int idaktuell = Int32.Parse(row.Row.ItemArray[0].ToString());
                ((DbConnector)App.Current.Properties["Connector"]).DeleteDokumentendatensatz(idaktuell);
                darstellungDokumente.ZeichneDatagridTab("OkoDokumentenTyp");
            }
            private void BtnDokDelete_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
            {
                if (TxtAblageRecherche.Equals("A"))
                {
                    e.CanExecute = false;
                }
                else
                {
                    //Buttons für die Recherche
                    if (darstellungDokumente.dgTabelleOriginal.SelectedItem != null)
                    {
                        e.CanExecute = true;
                        return;
                    }
                    e.CanExecute = false;
                }
            }

            private void BtnDokDelete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            {

            }
        #endregion

            #region Dok zu Export
            private List<int> DoksFuerExport = new List<int>();
            private void BtnDokExport_Click(object sender, RoutedEventArgs e)
            {
                var row = ((DataRowView)darstellungDokumente.dgTabelleOriginal.SelectedItem).Row.ItemArray;
                int counter = 0;
                foreach (DataGridColumn col in darstellungDokumente.dgTabelleOriginal.Columns)
                {
                    if (col.Header.Equals("OkoDokumentenTypId"))
                    {
                        if (!DoksFuerExport.Contains(Int32.Parse(row[counter].ToString())))
                        {
                            DoksFuerExport.Add(Int32.Parse(row[counter].ToString()));
                        }

                    }
                    counter++;
                }
            }
            private void BtnDokExport_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
            {
                if (TxtAblageRecherche.Equals("A"))
                {
                    e.CanExecute = false;
                }
                else
                {
                //Buttons für die Recherche
                if (darstellungDokumente.dgTabelleOriginal.SelectedItem != null) {
                        e.CanExecute = true;
                        return;
                    }
                    e.CanExecute = false;
                }
            }
            private void BtnBtnDokExport_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            {

            }
            #endregion

            #region Exportdialog
            private void BtnExportDialog_Click(object sender, RoutedEventArgs e)
            {
                ExportDialog dialog = new ExportDialog(DoksFuerExport);
                dialog.btnExportieren.Click += ExportDialog_BtnExportieren_Click;
                if (dialog.ShowDialog() == true)
                {
                    DoksFuerExport.Clear();
                    foreach (Exportdaten item in dialog.lstExport)
                    {
                        DoksFuerExport.Add(item.OkoDokumenteDatenId);
                    }
                }
            }
            private void BtnExportDialog_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
            {
                if (TxtAblageRecherche.Equals("A"))
                {
                    //Buttons für die ABlage
                    e.CanExecute = false;
                }
                else
                {
                    //Buttons für die Recherche
                    e.CanExecute = DoksFuerExport.Count() > 0;
                }
            }
            private void BtnExportDialog_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            {

            }
            #endregion

        #region Dok Speichern
        private void BtnDokSpeichern_Click(object sender, RoutedEventArgs e)
    {
            #region Datensatz der bezogenen Tabelle speichern
            //Benötigte Werte um InsertTableData aufrufen zu können
            string _tabellenname = "OkoDokumentenTyp";
            Dictionary<string, object> _dic = new Dictionary<string, object>();
            StringBuilder _csv = new StringBuilder();

            string keepValueForDic = "";
            int counter = 0; //wird benötigt, um erstes Element zu kennzeichnen
            foreach (var item in eingabeDokumentDaten.grdMain.Children)
            {
                //item kann Textbox oder Textblock sein

                //Textblock kann 'Tabellenname' oder 'Feldname (Typ)' sein
                if (item.GetType() == typeof(TextBlock))
                {
                    if (counter == 0)
                    {
                        //_tabellenname = ((TextBlock)item).Text;
                    }
                    else
                    {
                        //Tabellenfeldname --> Neues Element für Dictionary, aber erst den Wert merken
                        //Wert muss gesplittet werden, davon [0] nehmen
                        keepValueForDic = ((TextBlock)item).Text.Split(' ')[0];
                        var str = ((TextBlock)item).Text.Split(' ')[1].Replace("(", "").Replace(")", "") + ";";
                        if (str.Substring(0, 3).Equals("loo")) {
                            keepValueForDic = ((TextBlock)item).Tag.ToString();
                        }
                        _csv.Append(str);
                    }
                }

                //Textbox kann nur zu einem Feldnamen gehören, der bereits als Key in die Dictionary einger
                if (item.GetType() == typeof(TextBox))
                {
                    //Nun neues Element für Dictionary erzeugen und Werte intragen
                    var kvp = new KeyValuePair<string, object>(keepValueForDic, ((TextBox)item).Text);
                    _dic.Add(kvp.Key, kvp.Value);
                } //Das Gleiche gilt für eine Checkbox bei Boolean-Werten
                else if (item.GetType() == typeof(CheckBox))
                {
                    var kvp = new KeyValuePair<string, object>(keepValueForDic, ((CheckBox)item).IsChecked);
                    _dic.Add(kvp.Key, kvp.Value);
                }
                else if (item.GetType() == typeof(DatePicker))
                {
                    var kvp = new KeyValuePair<string, object>(keepValueForDic, ((DatePicker)item).SelectedDate);
                    _dic.Add(kvp.Key, kvp.Value);
                }
                else if (item.GetType() == typeof(LookupAuswahlDMS))
                {
                    if ((ComboBoxItem)((LookupAuswahlDMS)item).cboAuswahl.SelectedItem != null)
                    {
                        var kvp = new KeyValuePair<string, object>(keepValueForDic, ((ComboBoxItem)((LookupAuswahlDMS)item).cboAuswahl.SelectedItem).Tag);
                        _dic.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        var kvp = new KeyValuePair<string, object>(keepValueForDic, null);
                        _dic.Add(kvp.Key, kvp.Value);
                    }

                }
                counter++;
            }
            //DBConnector aufrufen
            string txt = _csv.ToString().Substring(0, _csv.Length - 1);
            //Für Aktualisierung in MainWindow merken
            eingabeDokumentDaten.TabNameUebergabe = _tabellenname;

            if (eingabeDokumentDaten._idAktuellerDatensatz == 0)//Speichern
            {
                eingabeDokumentDaten._idAktuellerDatensatz = ((DbConnector)App.Current.Properties["Connector"]).InsertTableData(_tabellenname, _dic, txt, true);
                //Dokument i die Datenbank schreiben
                if (eingabeDokumentDaten.Dropped) {
                    eingabeDokumentDaten._idDesGeradeBearbeitetenDokuments = databaseFilePut(eingabeDokumentDaten.Dateiname);
                } else {
                    string path = Einstellungen.Ordnerpfad + "\\" + eingabeDokumentDaten.Dateiname;
                    eingabeDokumentDaten._idDesGeradeBearbeitetenDokuments = databaseFilePut(path);
                }

                if (eingabeDokumentDaten.Dropped) {
                    var tester = eingabeDokumentDaten.Dateiname.Split('\\');
                    int laenge = tester.Count();
                    eingabeDokumentDaten.Dateiname = tester[laenge - 1];
                }
                //Dokumentedaten wegschreiben
                string dataTxt = eingabeDokumentDaten._idDesGeradeBearbeitetenDokuments + ";" + eingabeDokumentDaten._idAktuellerDatensatz
                    + ";" + eingabeDokumentDaten.Dateiname + ";" + DateTime.Today.ToString();
                ((DbConnector)App.Current.Properties["Connector"]).InsertDocumentData(dataTxt);
                //Dokument verschieben falls aus Scanordner
                if (eingabeDokumentDaten.Dropped == false)
                {
                    VerschiebeDatei();
                }
            }
            else //("Sichern"))
            {
                ((DbConnector)App.Current.Properties["Connector"]).UpdateTableData(_tabellenname, eingabeDokumentDaten._idAktuellerDatensatz, _dic, txt);
                StringBuilder sb = new StringBuilder();
                foreach (var value in _dic.Values)
                {
                    sb.Append(value + ";");
                }
                string txtCsv = sb.ToString();
                eingabeDokumentDaten._csvTabFeldwerte = txtCsv.Substring(0, txtCsv.Length - 1);
            }
            #endregion
            
            //Zurück setzen
            eingabeDokumentDaten.Dropped = false;
            eingabeDokumentDaten.Dateiname = "";
            eingabeDokumentDaten._idAktuellerDatensatz = 0;
            dropfeld.Data = new string[]{ };
            eingabeDokumentDaten.zeichneGrid();
            eingabeDokumentDaten.Visibility = Visibility.Hidden;
            eingabeDokumentDaten.grdMain.Children.Remove(eingabeDokumentDaten);
            //Neu zeichnen
            scanOrdner.zeichneGrid(Einstellungen.Ordnerpfad);
        }

        private void VerschiebeDatei() {
            List<string> dirs = new List<string>(Directory.EnumerateDirectories(Einstellungen.Ordnerpfad));
            //Erstmal ermitteln ob der Ordner existiert
            bool Checker = false;
            foreach (var dir in dirs)
            {
                if ((dir.Substring(dir.LastIndexOf(Path.DirectorySeparatorChar) + 1).Equals("Archiviert"))){
                    Checker = true;
                }
            }
            if (!Checker) {
                System.IO.Directory.CreateDirectory(Einstellungen.Ordnerpfad+"\\Archiviert");
            }
            //Dokument verschieben
            //File.Move(Einstellungen.Ordnerpfad + "\\" + eingabeDokumentDaten.Dateiname, Einstellungen.Ordnerpfad + "\\Archiviert\\" + eingabeDokumentDaten.Dateiname);
            File.Delete(Einstellungen.Ordnerpfad + "\\" + eingabeDokumentDaten.Dateiname);
        }

        private void BtnDokSpeichern_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
            {
                if (TxtAblageRecherche.Equals("A"))
                {
                    //Buttons für die ABlage
                    if (eingabeDokumentDaten != null && eingabeDokumentDaten.Visibility == Visibility.Visible && eingabeDokumentDaten._tabName != null && eingabeDokumentDaten._tabName != null && !eingabeDokumentDaten._tabName.Equals("") && eingabeDokumentDaten.grdMain.Children.Count > 0)
                    {
                        e.CanExecute = true;
                        return;
                    }
                    e.CanExecute = false;
                }
                else
                {
                    //Buttons für die Recherche
                    e.CanExecute = false;
                }
            }
            private void BtnDokSpeichern_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            {

            }

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

            #region DokAbbrechen
            private void BtnDokAbort_Click(object sender, RoutedEventArgs e)
            {
                if (TxtAblageRecherche.Equals("A")) {
                    //Ablage
                    eingabeDokumentDaten.Visibility = Visibility.Hidden;
                } else {
                    //Recherche
                }
            }
            private void BtnDokAbort_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
            {
                if (TxtAblageRecherche.Equals("A"))
                {
                    //Buttons für die ABlage
                    if (eingabeDokumentDaten != null && eingabeDokumentDaten.Visibility == Visibility.Visible && eingabeDokumentDaten._tabName != null && !eingabeDokumentDaten._tabName.Equals("") && eingabeDokumentDaten.grdMain.Children.Count > 0)
                    {
                        e.CanExecute = true;
                        return;
                    }
                    e.CanExecute = false;
                }
                else
                {
                    //Buttons für die Recherche
                    e.CanExecute = false;
                }
            }
            private void BtnDokAbort_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            {

            }
        #endregion

        #endregion

        #endregion

        public void databaseFileReadToMemoryStream(string varID, string dateiname)
        {
            string _anwendungspath = "";
            if (dateiname.Contains("."))
            {
                string _endung = "." + dateiname.Split('.')[1];
                foreach (Tuple<int, string, string> item in darstellungDokumente.Anwendungen)
                {
                    if (item.Item2.Equals(_endung))
                    {
                        _anwendungspath = item.Item3;
                    }
                }
            }

            MemoryStream memoryStream = new MemoryStream();
            using (var sqlQuery = new SqlCommand(@"SELECT [Dokument] FROM [OkoDokumente] WHERE [OkoDokumenteId] = @varID", ((DbConnector)App.Current.Properties["Connector"])._con))
            {
                sqlQuery.Parameters.AddWithValue("@varID", varID);
                using (var sqlQueryResult = sqlQuery.ExecuteReader())
                    if (sqlQueryResult != null)
                    {
                        sqlQueryResult.Read();
                        var blob = new Byte[(sqlQueryResult.GetBytes(0, 0, null, 0, int.MaxValue))];
                        sqlQueryResult.GetBytes(0, 0, blob, 0, blob.Length);
                        //using (var fs = new MemoryStream(memoryStream, FileMode.Create, FileAccess.Write)) {
                        memoryStream.Write(blob, 0, blob.Length);
                        //}
                    }
            }
            //return memoryStream;
            using (var fileStream = File.OpenWrite(dateiname))
            {
                memoryStream.WriteTo(fileStream);
            }
            if (_anwendungspath.Equals(""))
            {
                Process.Start(dateiname);
            }
            else
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = _anwendungspath;
                info.Arguments = dateiname;
                Process.Start(info);
            }

        }

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
                    if (_gemerkteAlteDateinamen.Contains(ed.Dateiname))
                    {
                        //Beim Umbenennen feststellen, ob der neue Name schon existiert
                        bool prozessBeendet = false;
                        int counter = 0;
                        while (!prozessBeendet)
                        {
                            counter++;
                            string[] arrDateiname = ed.Dateiname.Split('.');
                            neuerName = arrDateiname[0] + "(" + counter + ")." + arrDateiname[1];
                            if (!_gemerkteNeueDateinamen.Contains(neuerName))
                            {
                                prozessBeendet = true;

                            }
                        }
                        _gemerkteAlteDateinamen.Add(alterName);
                        _gemerkteNeueDateinamen.Add(neuerName);
                        _gemerkteDokIds.Add(idDesDoks);
                    }
                    else
                    {
                        //Nein --> Originalnamen in beide Listen schreiben, Id in IdListe
                        _gemerkteAlteDateinamen.Add(alterName);
                        _gemerkteNeueDateinamen.Add(alterName);
                        _gemerkteDokIds.Add(idDesDoks);
                    }
                    //--> Datei Export
                    string exportDateiNameFile = neuerName.Equals("") ? alterName : neuerName;
                    ExportFileToDisk(ed.DokumenteId.ToString(), pathString, exportDateiNameFile);
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
    }

    public class Einstellungen {
        public string Ordnerpfad { get; set; }
        public string DatenbearbeitungEinAus { get; set; }
    }

    public static class Fehlerbehandlung
    {
        private static string _path
        {
            get; set;
        }

        public static void Error(string stackTrace, string message, string ownCode)
        {

            FileInfo fi = new FileInfo(Assembly.GetEntryAssembly().Location);
            _path = fi.DirectoryName + "\\Errors.txt";
            if (File.Exists(_path))
            {
                //An FIle anhängen
                using (StreamWriter streamWriter = new StreamWriter(_path, true))
                {
                    streamWriter.Write(DateTime.Now + "\r\n" + stackTrace + "\r\n" + message + Environment.NewLine + ownCode + "\r\n\r\n");
                }
            }
            else
            {
                try
                {
                    //File erstellen
                    using (var x = File.Create(_path)) { }
                    using (StreamWriter sw = new StreamWriter(_path))
                    {
                        sw.Write(DateTime.Now + "\n" + stackTrace + "\n" + message + "\n" + ownCode + "\n\n");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Es konnte keine Datei auf dem Laufwerk angelegt werden. --> " + ex.Message);
                }
            }
        }
    }
}
