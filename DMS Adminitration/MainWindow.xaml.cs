using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;

namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            Connect();
            InitializeComponent();
            InitializeVendorComponents();
        }

        private void Ausgangsstellung()
        {
            grdMain.Children.Clear();
            eingabeTabelle.Visibility = Visibility.Hidden;
            uploadCsv.Visibility = Visibility.Hidden;
            aendernTabellen.Visibility = Visibility.Hidden;
            tabellendaten.Visibility = Visibility.Hidden;
            pflegeTabellendaten.Visibility = Visibility.Hidden;
            uebersichtTabellen.zeichneGrid();
            grdMain.Children.Add(uebersichtTabellen);
        }

        private void Connect(string vorherigeEingabe = "")
        {

            Application.Current.Properties["Connector"] = new DbConnector("Data Source='LAPTOP-CTMG3F1D\\SQLEXPRESS';Initial Catalog='Dokumentenmanagement';User ID='sa';Password='95hjh11!';");
            if (!((DbConnector)App.Current.Properties["Connector"]).Connect())
            {
                Connect("LAPTOP-CTMG3F1D\\SQLEXPRESS;Dokumentenmanagement;sa;95hjh11!");
            }
            Title = "Bearbeiten der Datenbank: ";
            
        }

        //UserControls initialisieren
        private void InitializeVendorComponents()
        {
            uebersichtTabellen.zeichneGrid();
            //Eventhandler der Usercontrols initialisieren
            uebersichtTabellen.tvMain.SelectedItemChanged += TvMain_SelectedItemChanged;
        }


        #region Gruppen Command Events

        private void BtnGruppeNeu_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void BtnGruppeNeu_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnGruppeBearbeiten_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void BtnGruppeBearbeiten_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnTypNeu_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void BtnTypNeu_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnTypBearbeiten_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void BtnTypBearbeiten_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        #endregion

        #region Übersicht Tabellen (Treeview)

        private void TvMain_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var org = (TreeView)sender;
            if (org.SelectedItem != null)
            {
                Tuple<string, string, string> tuple = uebersichtTabellen.WerteDerAuswahl;
            }

        }
        #endregion

        #region Events/CanExecute Buttonclicks

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

            Ribbon _sender = (Ribbon)sender;
            RibbonTab sel = (RibbonTab)_sender.SelectedItem;
            if (sel != null)
            {
                if (sel.Name.Equals("RibTabAblage"))
                {

                }
                else if (sel.Name.Equals("RibTabStamm"))
                {
                    if (!grdMain.Children.Contains(uebersichtTabellen))
                    {
                        grdMain.Children.Add(uebersichtTabellen);
                        grdMain.Children.Add(eingabeTabelle);
                        grdMain.Children.Add(uploadCsv);
                        grdMain.Children.Add(aendernTabellen);
                    }
                }
                else if (sel.Name.Equals("RibTabDms"))
                {

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
            }
            else
            {
                ribbon.Items.Remove(RibTabDms);
                ribbon.Items.Add(RibTabStamm);
                ribbon.Items.Add(RibTabAblage);
                ribbon.Items.Add(RibTabDms);
            }
        }
        #endregion

        #region rgStammLinks

        private bool CheckUcsHidden() {
            if (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Hidden
                    && uploadCsv != null && uploadCsv.Visibility == Visibility.Hidden
                    && aendernTabellen != null && aendernTabellen.Visibility == Visibility.Hidden
                    && tabellendaten != null && tabellendaten.Visibility == Visibility.Hidden
                    && pflegeTabellendaten != null && pflegeTabellendaten.Visibility == Visibility.Hidden) {
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
            Ausgangsstellung();
            aendernTabellen = new AendernTabelle(tabName, true);
            aendernTabellen.zeichneGrid();
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
            if (uebersichtTabellen != null)
            {
                e.CanExecute = tabellendaten.Visibility == Visibility.Visible;
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
            //TreeViewItem tvi = (TreeViewItem)uebersichtTabellen.tvMain.SelectedItem;
            //string tabName = tvi.Header.ToString();

            string tabName = pflegeTabellendaten.TabNameUebergabe;
            ((DbConnector)App.Current.Properties["Connector"]).DeleteTable(tabName);
            Ausgangsstellung();
        }

        private void BtnTabelleLoeschen_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (uebersichtTabellen != null)
            {
                e.CanExecute = tabellendaten.Visibility == Visibility.Visible;
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

        #region nach implementierung entfernen
        private void BtnGruppeNeue_Click(object sender, RoutedEventArgs e)
        {
            //ZU ENTFERNEN
        }

        private void BtnGruppeBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            //ZU ENTFERNEN
        }

        private void BtnTypNeu_Click(object sender, RoutedEventArgs e)
        {
            //ZU ENTFERNEN
        }

        private void BtnTypBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            //ZU ENTFERNEN
        }
        #endregion

        #region rgStammRechts
        #region Neue Zeile
        private void BtnNeueZeile_Click(object sender, RoutedEventArgs e)
        {
            EingabeTabellenfelder ds = new EingabeTabellenfelder();
            eingabeTabelle.alleDatensaetze.Add(ds);
            eingabeTabelle.zeichneGrid();
        }

            private void BtnNeueZeile_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Visible);
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
                e.CanExecute = false;
                e.Handled = true;
            }

            private void BtnZeileEntfernen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            {

            }
        #endregion

        #region Save
        private void BtnTabSave_Click(object sender, RoutedEventArgs e)
        {
            #region Neue Tabelle speichern
            if (eingabeTabelle != null && eingabeTabelle.Visibility == Visibility.Visible) {
                
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
            if (uploadCsv != null && uploadCsv.Visibility==Visibility.Visible) {
                
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
        }

        private void BtnTabSave_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            //Achtung Neue Tabelle ist ein if-Statement, der zweite Block csv ist das else if dazu und enthält das letzte else
            //Bei weiteren Ergänzungen beachten
            #region Neue Tabelle
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
            #endregion

            #region Neue Tabelle aus CSV
            //if gehört zu einem else if aus der region drüber
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
            {
                e.CanExecute = false;
            }
            #endregion
        }

        private void BtnTabSave_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            eingabeTabelle.ClearGrid();
            Ausgangsstellung();
        }
        #endregion
        #endregion

        #endregion
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
