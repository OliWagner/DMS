using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für Upload.xaml
    /// </summary>
    public partial class Upload : Window
    {
        public List<string[]> stringArrayListe { get; set; }
        private List<EingabeTabellenfelder> alleTabellenfelder { get; set; }
        private List<string> alleTabellenNamen;
        private List<string> alleTabellenFeldNamen;
        public string TabNameUebergabe { get; set; }
        //Werte, die benötigt werden, um beim EIntragen in die Datenbank die STrings und Booleans nachbearbeiten zu können
        private string CsvBoolTrueFalse { get; set; }

        public Upload()
        {
            InitializeComponent();
            stringArrayListe = new List<string[]>();
            alleTabellenfelder = new List<EingabeTabellenfelder>();
            alleTabellenNamen = new List<string>();
            alleTabellenFeldNamen = new List<string>();
            var con = (DbConnector)App.Current.Properties["Connector"];
            List<Tuple<string, string, string>> allTabs = con.ReadTableNamesTypesAndFields();
            foreach (var item in allTabs)
            {
                alleTabellenNamen.Add(item.Item1.ToString());
            }
        }

        #region COmmands

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Nix
        }
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //Zuerst schauen, ob alle Spaltendaten korrekt sind
            if (!CheckSpaltenDaten()) {
                e.CanExecute = false;
                return;
            }

            //Es dürfen nur Buchstaben und Nummern als Bezeichner verwendet werden
            string AllowedChars = @"^[a-zA-Z0-9]+$";


            //Variable dient nur dem Test, ob es schon Tabellenfelder gibt, ansonsten darf der Button ja auch nicht aktiv sein
            bool felderChecker = false;
            //Variable, um zu testen, ob Feldnamen doppelt vorhanden sind
            List<string> _feldnamen = new List<string>();

            foreach (var item in grdTabFelder.Children)
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

            //Noch testen, ob ein Tabellenname eingegeben wurde + Es dürfen nur Nummer und Ziffern verwendet werden + Es dürfen keine Tabellennamen doppelt sein
            if (txtTabname.Text.Equals("") || !Regex.IsMatch(txtTabname.Text, AllowedChars) || alleTabellenNamen.Contains(txtTabname.Text)) { felderChecker = false; }
            //Wenn Tabellenfelder da, nur dann kann der Button überhaupt aktiv sein
            if (felderChecker) { e.CanExecute = true; } else {
                e.CanExecute = false;
            }
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //DO nothing
        }
        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (grdTabFelder.Children.Count > 0)
            {
                e.CanExecute = true;
                return;
            }
            e.CanExecute = false;
        }
        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //DO nothing
        }
        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (alleTabellenfelder != null)
            {
                foreach (var item in alleTabellenfelder)
                {
                    if ((bool)item.chkLoeschen.IsChecked)
                    {
                        e.CanExecute = true;
                        return;
                    }
                }
            }
            e.CanExecute = false;
        }

        private bool CheckSingleValueForType(string type, string value) {
            switch (type) {
                case "boln":
                    if (!value.Equals("0") && !value.Equals("1"))
                        return false;
                    break;
                case "daten":
                    DateTime dateresult;
                    if (!DateTime.TryParse(value, out dateresult))
                        return false;
                    break;
                case "decn":
                    double result;
                    if (!Double.TryParse(value, out result))
                        return false;
                    break;
                case "intn":
                    int result2;
                    if (!Int32.TryParse(value, out result2))
                        return false;                   
                    break;
                case "txt50n":
                    if (value.Length > 50)
                        return false;
                    break;
                case "txt255n":
                    if (value.Length > 255)
                        return false;
                    break;
                case "txtmn":
                    break;
            }
            return true;
        }
        private bool CheckSpaltenDaten() {
            if (alleTabellenfelder != null && alleTabellenfelder.Count() > 0)
            {
                foreach (var item in alleTabellenfelder)
                {
                    //Zuerst schauen, ob überhaupt eine Auswahl getätigt wurde
                    if (item.comBoxFeldtyp.SelectedItem == null)
                    {
                        return false;
                    }
                }
                //Hier angekommen wissen wir, dass alle Dropdowns eine Auswahl haben --> Nun checken, ob die Spaltendaten mit dem jeweiligen Feldtypen harmoniert
                foreach (var row in dgDaten.Items)
                {
                    var werte = (string[])row;
                   //Nun durch die Werte der einzelnen Row iterieren
                   //Dabei kann man i auch als Index für die Instanzen von EingabeTabellenfelder nehmen, um den Wert des entsprechenden Dropdowns zu lesen
                    for (int i = 0; i < werte.Count() - 1; i++)
                    {
                        ComboBoxItem cboItem = (ComboBoxItem)alleTabellenfelder[i].comBoxFeldtyp.SelectedItem;
                        var tag = cboItem.Tag.ToString();
                        //Wert und Typ nun gegeneinander checken
                        if (!CheckSingleValueForType(tag, werte[i])) {
                            return false;
                        }
                    }

                }
            }
            else {
                return false;
            }
            //kein Killkriterium getroffen --> Alle Daten sind in Ordnung, um unter dem angegebenen Feldtypen abgespeichert zu werden
            return true;
        }
        #endregion

        #region Handlers
        private void btnHinzu_Click(object sender, RoutedEventArgs e)
        {
            EingabeTabellenfelder ds = new EingabeTabellenfelder("x");
            ds.Margin = new Thickness(5, 5, 0, 0);
            ds.txtBezeichnung.TextChanged += TxtBezeichnung_TextChanged;
            //ds.comBoxFeldtyp.SelectionChanged += ComBoxFeldtyp_SelectionChanged;
            alleTabellenfelder.Add(ds);
            alleTabellenFeldNamen.Add("neu");
            zeichneGrid();
            dgDaten.Columns.Add(new DataGridTextColumn
            {
                Header = "neu",
                Binding = new System.Windows.Data.Binding("[" + grdTabFelder.Children.Count + "]"),
                IsReadOnly = false
            });
        }

        private void Clear() {
            alleTabellenfelder = new List<EingabeTabellenfelder>();
            stringArrayListe = new List<string[]>();
            dgDaten.ItemsSource = null;
            while (dgDaten.Columns.Count > 0)
            {
                dgDaten.Columns.RemoveAt(0);
            }
            txtTabname.Text = "";
            zeichneGrid();
        }

        private void btnVerwerfen_Click(object sender, RoutedEventArgs e)
        {
            Clear();           
        }

        private void ComBoxFeldtyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Nichts machen
        }

        private void TxtBezeichnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)sender;
            dgDaten.Columns[(int)box.Tag].Header = box.Text;
        }

        private void btnWeg_Click(object sender, RoutedEventArgs e)
        {
            List<int> indexZumEntfernenListe = new List<int>();
            List<EingabeTabellenfelder> liste = new List<EingabeTabellenfelder>();

            int indexCounter = 0;
            foreach (var item in alleTabellenfelder)
                {
                    if ((bool)item.chkLoeschen.IsChecked)
                    {
                        liste.Add(item);
                        indexZumEntfernenListe.Add(indexCounter);
                       
                    }
                indexCounter++;
                }
            liste.Reverse();
            
                foreach (var item in liste)
                {
                //Column aus DataGrid entfernen 
                    dgDaten.Columns.RemoveAt((int)item.txtBezeichnung.Tag);
                    alleTabellenfelder.Remove(item);
                    alleTabellenFeldNamen.RemoveAt((int)item.txtBezeichnung.Tag);
            }

            //TEST UM INDEX AUS EINEM ARRAY ZU ENTFERNEN
            indexZumEntfernenListe.Reverse();
            List<string[]> newList = new List<string[]>();
            foreach (string[] item in stringArrayListe)
                {
                    List<string> checker = new List<string>();
                    for (int i = 0; i < item.Length; i++)
                    {
                    
                            if(!indexZumEntfernenListe.Contains(i))
                            checker.Add(item[i]);
                    
                    }
                newList.Add(checker.ToArray());
                }
            indexZumEntfernenListe.Clear();
            stringArrayListe = newList;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx
            dgDaten.ItemsSource = stringArrayListe;
            dgDaten.Columns.Clear();

            int counter = 0;
            foreach (var item in alleTabellenFeldNamen)
            {
                
                dgDaten.Columns.Add(new DataGridTextColumn
                {
                    Header = item.ToString(),
                    Binding = new System.Windows.Data.Binding("[" + counter + "]")
                });
                counter++;
            }
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            zeichneGrid();
            dgDaten.ItemsSource = stringArrayListe;
        }

        private void btnCsvEinlesen_Click(object sender, RoutedEventArgs e)
        {
            char csvTrenner = new char();
   
            //Werte löschen falls vorhanden
            stringArrayListe = new List<string[]>();
            alleTabellenfelder = new List<EingabeTabellenfelder>();
            alleTabellenFeldNamen = new List<string>();

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
                    CsvBoolTrueFalse = inputDialog.boolTrueFalse;
                }


                var lines = File.ReadAllLines(dlg.FileName);
                if (lines != null && lines.Count() > 0)
                {
                    var firstLine = lines[0];

                    dgDaten.ItemsSource = stringArrayListe;
                    dgDaten.Columns.Clear();

                    int counter = 0;
                    foreach (var item in firstLine.Split(csvTrenner))
                    {
                        EingabeTabellenfelder tabFeld = new EingabeTabellenfelder("x");
                        tabFeld.txtBezeichnung.Text = item.ToString();
                        tabFeld.Margin = new Thickness(5, 5, 0, 0);
                        alleTabellenfelder.Add(tabFeld);
                        alleTabellenFeldNamen.Add(item);
                        dgDaten.Columns.Add(new DataGridTextColumn
                        {
                            Header = item.ToString(),
                            Binding = new System.Windows.Data.Binding("[" + counter + "]")
                        });
                        counter++;
                    }

                    counter = 0;
                    foreach (var item in lines)
                    {
                        if (counter > 0) {
                            var test = item.Split(csvTrenner);
                            stringArrayListe.Add(test);
                        }
                        counter++;
                    }

                    zeichneGrid();
                    dgDaten.ItemsSource = stringArrayListe;   
                }               
            }
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //Array für eventuelle Stringmarkierer
            string[] arrStringMarkierer = { "\'", "\"" };
            //Benötigte Werte um InsertTableData aufrufen zu können
            StringBuilder _csv = new StringBuilder();

            //Erst neue Tabelle anlegen
            Dictionary<string, string> werte = new Dictionary<string, string>();
            string tabName = txtTabname.Text;
            //Tabelle merken für hoch bubblemdes event
            //foreach (DataGridTextColumn item in dgDaten.Columns)
            alleTabellenFeldNamen.Clear();
            foreach (var item in alleTabellenfelder)
            {
                var strIn = item.comBoxFeldtyp.Text;
                var strOut = ((ComboBoxItem)item.comBoxFeldtyp.SelectedItem).Tag.ToString();
                werte.Add(item.txtBezeichnung.Text, strOut);
                _csv.Append(((ComboBoxItem)item.comBoxFeldtyp.SelectedItem).Tag.ToString().Substring(0,3) + ";");
                alleTabellenFeldNamen.Add(item.txtBezeichnung.Text);
            }
            
            //In die Datenbank schreiben
            ((DbConnector)App.Current.Properties["Connector"]).CreateNewTable(tabName, werte);

            //Daten aus dgDaten in die neue Tabelle schreiben
            string txtWerte = _csv.ToString().Substring(0, _csv.Length - 1);
            string[] txtWerteArray = txtWerte.Split(';');
            //Für Aktualisierung in MainWindow merken
            TabNameUebergabe = tabName;

            //Die Items aus dem DataGrid in die DB schreiben
            List<Dictionary<string, object>> _dicList = new List<Dictionary<string, object>>();
            foreach (string[] item in dgDaten.Items)
            {
                //Benötigte Werte um InsertTableData aufrufen zu können
                Dictionary<string, object> _dic = new Dictionary<string, object>();
                int counter = 0;
                foreach (var elem in item)
                {
                    //An dieser STelle checken, ob es sich um einen bool-Wert handelt und dementsprechend übersetzen
                    bool usedChecker = false;
                    string entry = "false";
                    if (txtWerteArray[counter].Equals("bol")){
                        usedChecker = true;
                        
                        //Die einzelnen Eingabearten unterscheiden
                        
                        if (CsvBoolTrueFalse.Equals("1/0")) {
                            if (elem.Equals("1")) { entry = "true";}
                        }
                        if (CsvBoolTrueFalse.Equals("true/false"))
                        {
                            if (elem.ToUpper().Trim().Equals("TRUE")) { entry = "true"; }
                        }
                        if (CsvBoolTrueFalse.Equals("ja/nein"))
                        {
                            if (elem.ToUpper().Trim().Equals("JA")) { entry = "true"; }
                        }
                        if (CsvBoolTrueFalse.Equals("yes/no"))
                        {
                            if (elem.ToUpper().Trim().Equals("YES")) { entry = "true"; }
                        }
                        if (CsvBoolTrueFalse.Equals("Wert/kein Wert"))
                        {
                            if (elem.Length > 0) { entry = "true"; }
                        }
                        _dic.Add(alleTabellenFeldNamen.ElementAt(counter), entry);
                    }

                    //Auch die Markierungen für Strings entfernen
                    if (txtWerteArray[counter].Equals("txt"))
                    {
                        string alles = elem.ToString();
                        //if (arrStringMarkierer.Contains(elem.First().ToString())) { alles = alles.Substring(1, alles.Length ); }
                        //if (arrStringMarkierer.Contains(elem.Last().ToString())) { alles = alles.Substring(0, alles.Length - 1); }
                        _dic.Add(alleTabellenFeldNamen.ElementAt(counter), alles);
                        usedChecker = true;
                    }

                    if (!usedChecker)
                    {
                        _dic.Add(alleTabellenFeldNamen.ElementAt(counter), elem);
                    }
                    
                    counter++;
                }
                _dicList.Add(_dic);
            }
            ((DbConnector)App.Current.Properties["Connector"]).InsertCsvData(tabName, _dicList, txtWerte);
            Clear();
        }
        #endregion

        private void zeichneGrid()
        {
            grdTabFelder.Children.Clear();
            int y = 0;
            foreach (EingabeTabellenfelder item in alleTabellenfelder)
            {
                item.comBoxFeldtyp.ToolTip = "Wählen Sie den Datentypen des Feldes aus";
                item.txtBezeichnung.TextChanged += TxtBezeichnung_TextChanged;
                //item.comBoxFeldtyp.SelectionChanged += ComBoxFeldtyp_SelectionChanged;
                item.txtBezeichnung.Tag = y;
                RowDefinition gridRow1 = new RowDefinition();
                gridRow1.Height = new GridLength(30);
                grdTabFelder.RowDefinitions.Add(gridRow1);
                Grid.SetRow(item, y);
                y = y + 1;
                grdTabFelder.Children.Add(item);
            }
        }

        
    }
}
