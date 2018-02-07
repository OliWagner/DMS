using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaktionslogik für EingabeDokumentDaten.xaml
    /// </summary>
    public partial class EingabeDokumentDaten : UserControl
    {
        public string TabNameUebergabe { get; set; }
        public string _tabName { get; set; }
        public string _csvTabFeldnamen { get; set; }
        public string _csvTabFeldtypen { get; set; }
        public string _csvTabFeldwerte { get; set; }
        public int _idAktuellerDatensatz { get; set; }
        private int _aktuelleDokumentenTypId { get; set; }
        private string _aktuellerTabName { get; set; }

        public EingabeDokumentDaten()
        {            
            InitializeComponent();
        }

        #region zeichenGrid
        public void zeichneGrid(string dokumentenTyp, int dokumentenTypId, string csvTabFeldwerte = "") {

            _aktuelleDokumentenTypId = dokumentenTypId;
            btnAbbruch.Tag = dokumentenTyp;
            btnSpeichern.Tag = dokumentenTyp;
            //Tabelle zu DOkumententyp ermitteln und zeichneGrid(3 Params) aufrufen
            //alter Code
            //string tabName = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNameByDokId(dokumentenTypId);
            //neuer Code
            string tabName = "xyx" + dokumentenTyp;


            _aktuellerTabName = tabName;
            List<Tuple<string, string, string>> tableNamesAndTypes = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields(true);
            string csvTypen = "";
            string csvFelder = "";
            foreach (var tuple in tableNamesAndTypes)
            {
                if (tuple.Item1.Equals(tabName)) {
                    csvTypen = tuple.Item2;
                    csvFelder = tuple.Item3;
                }
            }


            if (csvTabFeldwerte.Equals(""))
            {
                zeichneGrid(tabName, csvFelder, csvTypen);
            }
            else {
                zeichneGrid(tabName, csvFelder, csvTypen, csvTabFeldwerte);
            }
            
        }

        public void zeichneGrid(string tabName, string csvTabFeldnamen, string csvTabFeldtypen)
        {
            btnSpeichern.Content = "Speichern";
            _tabName = tabName;
            _csvTabFeldnamen = csvTabFeldnamen;
            _csvTabFeldtypen = csvTabFeldtypen;
            _csvTabFeldwerte = "";

            var csvFeldnamen = csvTabFeldnamen.Split(';');
            var csvFeldtypen = csvTabFeldtypen.Split(';');
            grdMain.Children.Clear();
            grdMain.RowDefinitions.Clear();
            //Überschrift generieren
            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            txtBlock1.Text = tabName.Replace("xyx", ""); ;
            txtBlock1.Margin = new Thickness(5, 0, 0, 0);
            txtBlock1.Foreground = new SolidColorBrush(Colors.Black);
            txtBlock1.VerticalAlignment = VerticalAlignment.Top;

            RowDefinition gridRow = new RowDefinition();
            gridRow.Height = new GridLength(30);
            grdMain.RowDefinitions.Add(gridRow);

            Grid.SetRow(txtBlock1, 0);
            grdMain.Children.Add(txtBlock1);

            for (int i = 0; i < csvFeldnamen.Length; i++)
            {

                ColumnDefinition cL = new ColumnDefinition();
                cL.Width = new GridLength(150);
                grdMain.ColumnDefinitions.Add(cL);
                ColumnDefinition cR = new ColumnDefinition();
                cR.Width = new GridLength(350);
                grdMain.ColumnDefinitions.Add(cR);

                RowDefinition gR = new RowDefinition();
                gR.Height = new GridLength(30);
                grdMain.RowDefinitions.Add(gR);

                TextBlock tbLabel = new TextBlock();
                tbLabel.Text = (csvFeldnamen.ElementAt(i).Length > 2 && csvFeldnamen.ElementAt(i).Substring(0, 3).Equals("_x_") ? csvFeldnamen.ElementAt(i).Split('_')[2] : csvFeldnamen.ElementAt(i)) + " (" + csvFeldtypen.ElementAt(i).Substring(0, 3) + ")";
                tbLabel.Tag = csvFeldnamen.ElementAt(i);
                tbLabel.Width = 150;
                tbLabel.Height = 30;
                tbLabel.Margin = new Thickness(5, 0, 0, 0);
                tbLabel.Foreground = new SolidColorBrush(Colors.Black);
                tbLabel.VerticalAlignment = VerticalAlignment.Top;
                tbLabel.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetColumn(tbLabel, 0);
                Grid.SetRow(tbLabel, (i + 1));
                grdMain.Children.Add(tbLabel);

                if (csvFeldtypen[i].Substring(0, 3).Equals("bol"))
                {
                    CheckBox tb = new CheckBox();
                    tb.Name = "Wert" + i;
                    tb.Width = 20;
                    tb.Height = 20;
                    tb.VerticalAlignment = VerticalAlignment.Top;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, (i + 1));
                    grdMain.Children.Add(tb);
                }
                else if (csvFeldtypen[i].Substring(0, 3).Equals("loo"))
                {
                    LookupAuswahl tb = new LookupAuswahl(_tabName, _csvTabFeldnamen, _csvTabFeldnamen);
                    tb.Tag = csvFeldnamen.ElementAt(i).Split('_')[3];
                    tb.cboAuswahl.Tag = tb;
                    tb.cboAuswahl.Name = csvFeldnamen.ElementAt(i);
                    tb.cboAuswahl.SelectionChanged += CboAuswahl_SelectionChanged;
                    tb.Fill(csvFeldnamen.ElementAt(i));
                    tb.VerticalAlignment = VerticalAlignment.Top;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, (i + 1));
                    grdMain.Children.Add(tb);
                }
                else
                {
                    TextBox tb = new TextBox();
                    tb.Name = "Wert" + i;
                    tb.MinWidth = 250;
                    tb.MaxWidth = 250;
                    tb.Height = 20;
                    //Datepicker falls benötigt
                    DatePicker dp = new DatePicker();
                    dp.Name = "Wert" + i;
                    dp.Width = 200;
                    dp.Height = 25;
                    //Verschiedene Eventhandler für die Feldtypen setzen
                    if (csvFeldtypen[i].Substring(0, 3).Equals("dat"))
                    {
                        //Wenn es sich um ein Datum handelt, muss ein Datepicker hinzugefügt werden
                        dp.VerticalAlignment = VerticalAlignment.Top;
                        dp.HorizontalAlignment = HorizontalAlignment.Left;
                        Grid.SetColumn(dp, 1);
                        Grid.SetRow(dp, (i + 1));
                        grdMain.Children.Add(dp);
                    }
                    else
                    {
                        if (csvFeldtypen[i].Substring(0, 3).Equals("dec"))
                        {
                            tb.TextChanged += Tb_TextChangedDec;
                        }
                        else if (csvFeldtypen[i].Substring(0, 3).Equals("int"))
                        {
                            tb.TextChanged += Tb_TextChangedInt;
                        }
                        else if (csvFeldtypen[i].Substring(0, 3).Equals("txt"))
                        {
                            if (csvFeldtypen[i].Equals("txt50n")) { tb.TextChanged += Tb_TextChangedTxt50; }
                            if (csvFeldtypen[i].Equals("txt255n"))
                            {
                                tb.TextChanged += Tb_TextChangedTxt255;
                                tb.TextWrapping = TextWrapping.Wrap;
                                tb.Height = 75;
                                grdMain.RowDefinitions[i + 1].Height = new GridLength(80);

                            }
                            if (csvFeldtypen[i].Equals("txtmn"))
                            {
                                tb.TextChanged += Tb_TextChangedTxtm;
                                tb.TextWrapping = TextWrapping.Wrap;
                                tb.Height = 115;
                                grdMain.RowDefinitions[i + 1].Height = new GridLength(120);
                            }

                        }

                        tb.VerticalAlignment = VerticalAlignment.Top;
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        Grid.SetColumn(tb, 1);
                        Grid.SetRow(tb, (i + 1));
                        grdMain.Children.Add(tb);
                    }
                }
            }
        }

        
        public void zeichneGrid(string tabName, string csvTabFeldnamen, string csvTabFeldtypen, string csvTabFeldwerte)
        {
            btnSpeichern.Content = "Sichern";
            _tabName = tabName;
            _csvTabFeldnamen = csvTabFeldnamen;
            _csvTabFeldtypen = csvTabFeldtypen;
            _csvTabFeldwerte = csvTabFeldwerte;

            var csvFeldnamen = csvTabFeldnamen.Split(';');
            var csvFeldtypen = csvTabFeldtypen.Split(';');
            var csvFeldwerte = csvTabFeldwerte.Split(';');

            grdMain.Children.Clear();
            grdMain.RowDefinitions.Clear();
            //Überschrift generieren
            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            txtBlock1.Text = tabName.Contains("xyx") ? tabName.Replace("xyx","") : tabName;
            txtBlock1.Margin = new Thickness(5, 0, 0, 0);
            txtBlock1.Foreground = new SolidColorBrush(Colors.Black);
            txtBlock1.VerticalAlignment = VerticalAlignment.Top;

            RowDefinition gridRow = new RowDefinition();
            gridRow.Height = new GridLength(30);
            grdMain.RowDefinitions.Add(gridRow);

            Grid.SetRow(txtBlock1, 0);
            grdMain.Children.Add(txtBlock1);

            for (int i = 0; i < csvFeldnamen.Length; i++)
            {

                ColumnDefinition cL = new ColumnDefinition();
                cL.Width = new GridLength(150);
                grdMain.ColumnDefinitions.Add(cL);
                ColumnDefinition cR = new ColumnDefinition();
                cR.Width = new GridLength(350);
                grdMain.ColumnDefinitions.Add(cR);

                RowDefinition gR = new RowDefinition();
                gR.Height = new GridLength(30);
                grdMain.RowDefinitions.Add(gR);

                TextBlock tbLabel = new TextBlock();
                tbLabel.Text = (csvFeldnamen.ElementAt(i).Length > 2 && csvFeldnamen.ElementAt(i).Substring(0, 3).Equals("_x_") ? csvFeldnamen.ElementAt(i).Split('_')[2] : csvFeldnamen.ElementAt(i)) + " (" + csvFeldtypen.ElementAt(i).Substring(0, 3) + ")";
                tbLabel.Tag = csvFeldnamen.ElementAt(i);
                tbLabel.Width = 150;
                tbLabel.Height = 30;
                tbLabel.Margin = new Thickness(5, 0, 0, 0);
                tbLabel.Foreground = new SolidColorBrush(Colors.Black);
                tbLabel.VerticalAlignment = VerticalAlignment.Top;
                tbLabel.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetColumn(tbLabel, 0);
                Grid.SetRow(tbLabel, (i + 1));
                grdMain.Children.Add(tbLabel);
                if (csvFeldtypen[i].Substring(0, 3).Equals("bol"))
                {
                    CheckBox tb = new CheckBox();
                    tb.Name = "Wert" + i;
                    tb.Width = 20;
                    tb.Height = 20;
                    var test = csvFeldwerte.ElementAt(i).ToString();
                    tb.IsChecked = csvFeldwerte.ElementAt(i).ToString().Equals("True");
                    tb.VerticalAlignment = VerticalAlignment.Top;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, (i + 1));
                    grdMain.Children.Add(tb);
                }
                else if (csvFeldtypen[i].Substring(0, 3).Equals("loo"))
                {
                    LookupAuswahl tb = new LookupAuswahl(_tabName, _csvTabFeldnamen, _csvTabFeldnamen);
                    tb.Tag = csvFeldnamen.ElementAt(i).Split('_')[3];
                    tb.cboAuswahl.Tag = tb;
                    tb.cboAuswahl.Name = csvFeldnamen.ElementAt(i);
                    tb.cboAuswahl.SelectionChanged += CboAuswahl_SelectionChanged;
                    tb.Fill(csvFeldnamen.ElementAt(i));
                    Tuple<List<int>, List<object>> tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadComboboxItems(csvFeldnamen.ElementAt(i).Split('_')[3], csvFeldnamen.ElementAt(i).Split('_')[4]);
                    if (!csvFeldwerte.ElementAt(i).ToString().Equals(""))
                    {
                        int position = tuple.Item1.IndexOf(Int32.Parse(csvFeldwerte.ElementAt(i).ToString()));
                        tb.cboAuswahl.SelectedIndex = position;
                    }

                    tb.VerticalAlignment = VerticalAlignment.Top;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, (i + 1));
                    grdMain.Children.Add(tb);
                }
                else
                {
                    //Textbox erstellen
                    TextBox tb = new TextBox();
                    tb.Name = "Wert" + i;
                    tb.Width = 250;
                    tb.MaxWidth = 250;
                    tb.Height = 20;

                    //Datepicker falls benötigt
                    DatePicker dp = new DatePicker();
                    dp.Name = "Wert" + i;
                    dp.Width = 200;
                    dp.Height = 25;

                    //Verschiedene Eventhandler für die Feldtypen setzen
                    if (csvFeldtypen[i].Substring(0, 3).Equals("dat"))
                    {
                        //Hat sich erledigt, ist jetzt Datepicker
                        //tb.TextChanged += Tb_TextChangedDat;
                        //tb.MaxLength = 10;
                    }
                    else if (csvFeldtypen[i].Substring(0, 3).Equals("dec"))
                    {
                        tb.TextChanged += Tb_TextChangedDec;
                    }
                    else if (csvFeldtypen[i].Substring(0, 3).Equals("int"))
                    {
                        tb.TextChanged += Tb_TextChangedInt;
                    }
                    else if (csvFeldtypen[i].Substring(0, 3).Equals("txt"))
                    {
                        if (csvFeldtypen[i].Equals("txt50n"))
                        {
                            tb.TextChanged += Tb_TextChangedTxt50;
                        }
                        if (csvFeldtypen[i].Equals("txt255n"))
                        {
                            tb.TextChanged += Tb_TextChangedTxt255;
                            tb.TextWrapping = TextWrapping.Wrap;
                            tb.Height = 75;
                            grdMain.RowDefinitions[i + 1].Height = new GridLength(80);
                        }
                        if (csvFeldtypen[i].Equals("txtmn"))
                        {
                            tb.TextChanged += Tb_TextChangedTxtm;
                            tb.TextWrapping = TextWrapping.Wrap;
                            tb.Height = 115;
                            grdMain.RowDefinitions[i + 1].Height = new GridLength(120);
                        }
                    }
                    if (csvFeldtypen[i].Substring(0, 3).Equals("dat"))
                    {
                        if (csvFeldwerte.ElementAt(i).ToString().Length > 10)
                        {
                            int tag = Int32.Parse(csvFeldwerte.ElementAt(i).ToString().Substring(0, 2));
                            int monat = Int32.Parse(csvFeldwerte.ElementAt(i).ToString().Substring(3, 2));
                            int jahr = Int32.Parse(csvFeldwerte.ElementAt(i).ToString().Substring(6, 4));
                            dp.SelectedDate = new DateTime(jahr, monat, tag);
                        }
                        dp.VerticalAlignment = VerticalAlignment.Top;
                        dp.HorizontalAlignment = HorizontalAlignment.Left;
                        Grid.SetColumn(dp, 1);
                        Grid.SetRow(dp, (i + 1));
                        grdMain.Children.Add(dp);
                    }
                    else
                    {
                        tb.Text = csvFeldwerte.ElementAt(i).ToString();
                        tb.VerticalAlignment = VerticalAlignment.Top;
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        Grid.SetColumn(tb, 1);
                        Grid.SetRow(tb, (i + 1));
                        grdMain.Children.Add(tb);
                    }
                }
            }
        }

        private void CboAuswahl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;
            LookupAuswahl auswahl = (LookupAuswahl)cbo.Tag;
            auswahl.selectionChangedEventArgs = e;
            string _TabellenFeldBezogeneTabelle = "";
            string _TabellenFeldBezogeneTabelleString = "";
            string xxx = "";


            string FeldnameDesLookupFeldes = cbo.Name;
            //_feld hat immer die Form _x_Feldname_Reftabelle_FeldInRefTabelle
            string refTabelle = FeldnameDesLookupFeldes.Split('_')[3];
            string WhereClauseFeld = "";

            if ((ComboBoxItem)cbo.SelectedItem != null) {
                int IdFuerWhereClause = Int32.Parse(((ComboBoxItem)cbo.SelectedItem).Tag.ToString());

                int counter = 0;
                foreach (string item in auswahl.StammdatenCsvFeldnamen)
                {
                    foreach (var fn in item.Split(';'))
                    {
                        if (fn.Length > 2 && fn.Substring(0, 3).Equals("_x_"))
                        {
                            if (fn.Split('_')[3].Equals(refTabelle))
                            {

                                foreach (var csv in auswahl.StammdatenCsvFeldnamen.ElementAt(counter).Split(';'))
                                {
                                    if (csv.Contains("_x_") && csv.Contains(refTabelle))
                                    {
                                        xxx = csv;
                                        _TabellenFeldBezogeneTabelle = auswahl.StammdatenTabelle.ElementAt(counter);

                                       //Leider muss man das Feld Bezeichncung nennen, keine Ahnung, wie es anders gehen könnte. Woher weiss ich, welches Feld im Lookup eigentlich in der COmbobox steht???
                                       //Wie man es dreht, bei dem Datenodell MUSS ich wohl dabei bleiben, dass Lookupfelder nur auf das Feld Bezeichnung gelegt werden können.
                                       //Dieselben Probleme hatten wohl mal die Sharepoint-Entwickler, deswegen gibts den Title...
                                        _TabellenFeldBezogeneTabelleString = "___" + auswahl.StammdatenTabelle.ElementAt(counter) + "_Bezeichnung";
                                        WhereClauseFeld = fn;
                                    }
                                }
                            }
                        }
                    }

                    counter++;
                }

                //Wir haben jetzt das Feld, die WhereCLause und die Id, nun noch rausfinden, welche COmbobox neu zu zeichnen ist
                foreach (var child in grdMain.Children)
                {
                    if (child.GetType() == typeof(LookupAuswahl) && !WhereClauseFeld.Equals(""))
                    {
                        if (((LookupAuswahl)child).Tag.ToString().Equals(_TabellenFeldBezogeneTabelle))
                        {
                            //TODO --> cbi ist falsch, ich brauche das Item aus der COmboBOx, die dann geändert wird!!!
                            ComboBoxItem cbi = (ComboBoxItem)((LookupAuswahl)child).cboAuswahl.SelectedItem;
                            if (cbi == null)
                            {
                                ((LookupAuswahl)child).Fill(_TabellenFeldBezogeneTabelleString, IdFuerWhereClause, WhereClauseFeld);
                            }
                            else {
                                ((LookupAuswahl)child).Fill(_TabellenFeldBezogeneTabelleString, IdFuerWhereClause, WhereClauseFeld, cbi);
                            }
                            
                        }
                    }
                }
            }

            
        }
        #endregion

        #region TextChanged Events für die Eingabefelder
        private void Tb_TextChangedDec(object sender, TextChangedEventArgs e)
        {
            Regex r = new Regex("^[0-9]+(?:[\\.\\,]\\d{0,5})?$");
            TextBox _sender = (TextBox)sender;
            if (_sender.Text.ToString().Length > 0)
            {
                var test = _sender.Name;
                if (!r.IsMatch(_sender.Text.ToString()))
                {
                    TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                    ctrl.Text = _sender.Text.Substring(0, _sender.Text.Length - 1);
                    ctrl.CaretIndex = _sender.Text.Length;
                }
            }
        }
        private void Tb_TextChangedInt(object sender, TextChangedEventArgs e)
        {
            Regex r = new Regex("[0-9]");
            TextBox _sender = (TextBox)sender;
            if (_sender.Text.ToString().Length > 0)
            {
                if (_sender.Text.ToString().Length > 9)
                {
                    TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                    ctrl.Text = _sender.Text.Substring(0, 9);
                    ctrl.CaretIndex = _sender.Text.Length;
                }

                foreach (char item in _sender.Text.ToArray())
                {
                    if (!r.IsMatch(item.ToString()))
                    {
                        TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                        ctrl.Text = _sender.Text.Substring(0, _sender.Text.Length - 1);
                        ctrl.CaretIndex = _sender.Text.Length;
                    }
                }
            }
        }
        private void Tb_TextChangedTxt50(object sender, TextChangedEventArgs e)
        {
            TextBox _sender = (TextBox)sender;
            if (_sender.Text.ToString().Length > 0)
            {
                if (_sender.Text.ToString().Length > 50)
                {
                    TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                    ctrl.Text = _sender.Text.Substring(0, 50);
                    ctrl.CaretIndex = _sender.Text.Length;
                }
            }
        }
        private void Tb_TextChangedTxt255(object sender, TextChangedEventArgs e)
        {
            TextBox _sender = (TextBox)sender;
            if (_sender.Text.ToString().Length > 0)
            {
                if (_sender.Text.ToString().Length > 255)
                {
                    TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                    ctrl.Text = _sender.Text.Substring(0, 255);
                    ctrl.CaretIndex = _sender.Text.Length;
                }
            }
        }
        private void Tb_TextChangedTxtm(object sender, TextChangedEventArgs e)
        {
            TextBox _sender = (TextBox)sender;
            if (_sender.Text.ToString().Length > 8000)
            {
                if (_sender.Text.ToString().Length > 8000)
                {
                    TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                    ctrl.Text = _sender.Text.Substring(0, 8000);
                    ctrl.CaretIndex = _sender.Text.Length;
                }
            }
        }
        #endregion

        public void btnAbbruch_Click(object sender, RoutedEventArgs e)
        {
            //Entfernt das Tab aus dem TabControl, wird in TabsDaten behandelt
        }

        public void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {
           
            #region Datensatz der bezogenen Tabelle speichern
            //Benötigte Werte um InsertTableData aufrufen zu können
            string _tabellenname = "";
            Dictionary<string, object> _dic = new Dictionary<string, object>();
            StringBuilder _csv = new StringBuilder();

            string keepValueForDic = "";
            int counter = 0; //wird benötigt, um erstes Element zu kennzeichnen
            foreach (var item in grdMain.Children)
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
            TabNameUebergabe = _tabellenname;

            if (btnSpeichern.Content.Equals("Speichern"))
            {
                int tagWert = ((DbConnector)App.Current.Properties["Connector"]).InsertTableData(_tabellenname, _dic, txt, true);

                btnSpeichern.Tag = btnSpeichern.Tag.ToString() + "_" + tagWert + "_" + _aktuelleDokumentenTypId + "_" + _aktuellerTabName;
            }
            else if (btnSpeichern.Content.Equals("Sichern"))
            {
                btnSpeichern.Tag = _idAktuellerDatensatz;
                ((DbConnector)App.Current.Properties["Connector"]).UpdateTableData(_tabellenname, _idAktuellerDatensatz, _dic, txt);
                StringBuilder sb = new StringBuilder();
                foreach (var value in _dic.Values)
                {
                    sb.Append(value + ";");
                }
                string txtCsv = sb.ToString();
                _csvTabFeldwerte = txtCsv.Substring(0, txtCsv.Length - 1);
                //zeichneGrid(_tabName, _csvTabFeldnamen, _csvTabFeldtypen, _csvTabFeldwerte);
            }
            #endregion
        }
    }
}
