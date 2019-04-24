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

namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class AendernDokTyp : UserControl
    {
       
        public string Tabelle { get; set; }
        public int _anzahlFelder { get; set; }
        public int _anzahlFelderDisabled { get; set; }
        public List<string> FelderStart = new List<string>();
        public List<string> FelderLoeschen = new List<string>();
        public List<EingabeTabellenfelder> FelderHinzufuegen = new List<EingabeTabellenfelder>();
        

        public void Clear() {
            FelderHinzufuegen = new List<EingabeTabellenfelder>();
            FelderLoeschen = new List<string>();
        }

        public AendernDokTyp() {
            InitializeComponent();
            Tabelle = "OkoDokumentenTyp";
        }

        public void zeichneGrid() {
            FelderStart = new List<string>();
            _anzahlFelderDisabled = 0;
            grdMain.RowDefinitions.Clear();
            grdMain.Children.Clear();
            int rowCounter = 0;

            string[] feldnamen = { };
            string[] feldtypen = { };
            lblTabName.Content = "Ablagedaten bearbeiten";
            //Tabellendaten ermitteln
            Tuple<string, string> tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadDokTypTypesAndFields();
            if (!tuple.Item1.Equals("")) {
                feldnamen = tuple.Item1.Split(';');
                feldtypen = tuple.Item2.Split(';');
                _anzahlFelder = feldtypen.Count();
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
                if (Checkloeschen(feldnamen[i]))
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

       

        
    }
}

