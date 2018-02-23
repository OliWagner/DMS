using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Linq;
using Vigenere;
using System;
using System.Windows.Input;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;

namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class AendernTabelleDialog : Window
    {
       
        public string Tabelle { get; set; }
        private int _anzahlFelder { get; set; }
        private int _anzahlFelderDisabled { get; set; }
        private List<string> FelderStart = new List<string>();
        public List<string> FelderLoeschen = new List<string>();
        public List<EingabeTabellenfelder> FelderHinzufuegen = new List<EingabeTabellenfelder>();
        private bool IstBereitsReferenziert { get; set; }

        public AendernTabelleDialog(string tabelle, bool istBereitsReferenziert)
        {
            IstBereitsReferenziert = istBereitsReferenziert;
            InitializeComponent();
            Tabelle = tabelle;
            zeichneGrid();
        }


        private void zeichneGrid() {
            FelderStart = new List<string>();
            _anzahlFelderDisabled = 0;
            grdMain.RowDefinitions.Clear();
            grdMain.Children.Clear();
            int rowCounter = 0;

            string[] feldnamen = { };
            string[] feldtypen = { };
            lblTabName.Content = Tabelle;
            //Tabellendaten ermitteln
            List<Tuple<string, string, string>> lstTuple = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields();
            foreach (Tuple<string, string, string> tuple in lstTuple)
            {
                if (tuple.Item1.Equals(Tabelle)) {
                    feldnamen = tuple.Item3.Split(';');
                    feldtypen = tuple.Item2.Split(';');
                    _anzahlFelder = feldtypen.Count();
                }
            }
            //Nun die Tabelendaten darstellen
            for (int i = 0; i < feldtypen.Length; i++)
            {
                FelderStart.Add(feldnamen[i].Length > 2 && feldnamen[i].Substring(0, 3).Equals("_x_") ? feldnamen[i].Split('_')[2] : feldnamen[i]);
                RowDefinition gridRow1 = new RowDefinition();
                gridRow1.Height = new GridLength(30);
                grdMain.RowDefinitions.Add(gridRow1);
                TextBlock lbl = new TextBlock() { Text = feldnamen[i].Length > 2 && feldnamen[i].Substring(0,3).Equals("_x_") ? feldnamen[i].Split('_')[2] : feldnamen[i] };
                lbl.HorizontalAlignment = HorizontalAlignment.Left;
                lbl.Margin = new Thickness(5, 0, 0, 0);
                lbl.FontSize = 14;
                TextBlock lbl2 = new TextBlock() { Text = "(" + feldtypen[i].Substring(0, 3) + ")" };
                lbl2.Margin = new Thickness(100, 0, 0, 0);
                lbl2.HorizontalAlignment = HorizontalAlignment.Left;
                lbl2.FontSize = 14;
                TextBlock lbl3 = new TextBlock();
                if (Checkloeschen(feldnamen[i]) && !IstBereitsReferenziert)
                {
                    //Schon zum Löschen markiert?
                    if (!FelderLoeschen.Contains(feldnamen[i])) {
                        lbl3.Text = "löschen";
                        lbl3.Tag = feldnamen[i];
                        lbl3.TextDecorations = TextDecorations.Underline;
                        lbl3.Foreground = System.Windows.Media.Brushes.Blue;
                    } else {
                        lbl3.Text = "wird gelöscht";
                        lbl3.Tag = feldnamen[i];
                        lbl3.TextDecorations = null;
                        lbl3.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    lbl3.PreviewMouseDown += FeldLoeschen;
                }
                else {
                    lbl3.Text = "---";
                    lbl3.ToolTip = "Feld kann nicht gelöscht werden.";
                    //Feld merken für Save_CanExecute
                    _anzahlFelderDisabled++;
                }

                lbl3.Margin = new Thickness(200, 0, 0, 0);
                lbl3.HorizontalAlignment = HorizontalAlignment.Left;
                lbl3.FontSize = 14;

                Grid panel = new Grid();
                panel.Width = 500;
                panel.Children.Add(lbl);
                panel.Children.Add(lbl2);
                panel.Children.Add(lbl3);

                Grid.SetRow(panel, i);
                grdMain.Children.Add(panel);

                rowCounter++;
            }

            //Nun noch die EIngaben für neue Tabellenfelder
            foreach (EingabeTabellenfelder eingabeTabellenfeld in FelderHinzufuegen)
            {                
                //Man gebe dem Ganzen ein wenig EIngabeTabellenfeld hinzu
                RowDefinition gridRow1 = new RowDefinition();
                gridRow1.Height = new GridLength(30);
                grdMain.RowDefinitions.Add(gridRow1);
                Grid.SetRow(eingabeTabellenfeld, rowCounter);
                grdMain.Children.Add(eingabeTabellenfeld);
                rowCounter++;
            }
        }

       

        /// <summary>
        /// Check, ob ein Feld gelöscht werden darf. Wenn eine andere Tabelle dieses Feld als Nachschlagefeld referenziert, darf es nicht gelöscht werden.
        /// </summary>
        /// <param name="feldname">Der Name des zu löschenden Feldes</param>
        /// <returns>true wenn keine Abhängigkeiten bestehen</returns>
        private bool Checkloeschen(string feldname) {
            List<Tuple<string, string, string>> alleTabellen = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields();
            foreach (Tuple<string, string, string> tuple in alleTabellen)
            {
                //Nicht die eigenen Felder prüfen
                if (!tuple.Item1.Equals(Tabelle)) {
                    foreach (string _feldname in tuple.Item3.Split(';'))
                    {
                        if (_feldname.Substring(0,3).Equals("_x_")) {
                            string[] arr = _feldname.Split('_');
                            if (arr[3].Equals(Tabelle) && arr[4].Equals(feldname)) { return false; }
                        }
                    }
                }
            }
            return true;
        }

        #region Eventhandler
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnHinzu_Click(object sender, RoutedEventArgs e)
        {
            EingabeTabellenfelder eingabeTabellenfeld = new EingabeTabellenfelder("", Tabelle);
            eingabeTabellenfeld.chkLoeschen.ToolTip = "Hier klicken, um das Feld wieder zu entfernen!";
            eingabeTabellenfeld.chkLoeschen.Click += eingabeTabellenfeld_ChkLoeschen_Click;
            FelderHinzufuegen.Add(eingabeTabellenfeld);
            zeichneGrid();
        }

        private void eingabeTabellenfeld_ChkLoeschen_Click(object sender, RoutedEventArgs e)
        {
            CheckBox _sender = (CheckBox)sender;
            EingabeTabellenfelder eingabeTabellenfeld = Helpers.FindParent<EingabeTabellenfelder>(_sender);
            FelderHinzufuegen.Remove(eingabeTabellenfeld);
            zeichneGrid();
        }

        private void FeldLoeschen(object sender, MouseButtonEventArgs e)
        {
            TextBlock txtObject = (TextBlock)sender;
            if (txtObject.Text.Equals("löschen"))
            {
                FelderLoeschen.Add(txtObject.Tag.ToString());
                txtObject.Text = "wird gelöscht";
                txtObject.TextDecorations = null;
                txtObject.Foreground = System.Windows.Media.Brushes.Black;
            }
            else if (txtObject.Text.Equals("wird gelöscht"))
            {
                FelderLoeschen.Remove(txtObject.Tag.ToString());
                txtObject.Text = "löschen";
                txtObject.TextDecorations = TextDecorations.Underline;
                txtObject.Foreground = System.Windows.Media.Brushes.Blue;
            }
        }
        #endregion

        #region COmmands
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            List<string> Checkliste = FelderStart;

            e.CanExecute = true;
            //sollte noch garnichts passiert sein, ist es BLödsinn zu speichern
            if (FelderLoeschen.Count() == 0 && grdMain.Children.Count == _anzahlFelder) {
                e.CanExecute = false;
            }

            //Es dürfen nur Buchstaben und Nummern als Bezeichner verwendet werden
            string AllowedChars = @"^[a-zA-Z0-9]+$";

            foreach (var item in grdMain.Children)
            {
                //Ist die ANzahl der zu löschenden Felder gleich der Anzahl der existierenden Felder, gibt es keine Felder mehr --> false
                if (FelderLoeschen.Count() == (_anzahlFelder - _anzahlFelderDisabled)) {
                    e.CanExecute = false;
                }

                if (item.GetType() == typeof(EingabeTabellenfelder))
                {
                    //Es gibt EIngabefelder, also per se erst mal wieder true
                    e.CanExecute = true;
                    //Erst mal schauen, ob überhaupt ausgefüllt
                    EingabeTabellenfelder eingabeTabellenfeld = (EingabeTabellenfelder)item;
                    if (eingabeTabellenfeld.txtBezeichnung.Text.Equals("") || eingabeTabellenfeld.comBoxFeldtyp.SelectedItem == null) {
                        e.CanExecute = false;
                    }
                    //ob korrekt ausgefüllt
                    if (!Regex.IsMatch(eingabeTabellenfeld.txtBezeichnung.Text, AllowedChars)) {
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
        #endregion

        
    }
}

