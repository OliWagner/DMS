﻿using System;
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

namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für Upload.xaml
    /// </summary>
    public partial class Upload : System.Windows.Controls.UserControl
    {
        public List<string[]> stringArrayListe { get; set; }
        public List<EingabeTabellenfelder> alleTabellenfelder { get; set; }
        public List<string> alleTabellenNamen;
        public List<string> alleTabellenFeldNamen;
        public string TabNameUebergabe { get; set; }
        //Werte, die benötigt werden, um beim EIntragen in die Datenbank die STrings und Booleans nachbearbeiten zu können
        public string CsvBoolTrueFalse { get; set; }

        public Upload()
        {
            InitializeComponent();
            stringArrayListe = new List<string[]>();
            alleTabellenfelder = new List<EingabeTabellenfelder>();
            alleTabellenNamen = new List<string>();
            alleTabellenFeldNamen = new List<string>();
            DbConnector con = (DbConnector)App.Current.Properties["Connector"];
            List<Tuple<string, string, string>> allTabs = con.ReadTableNamesTypesAndFields();
            foreach (var item in allTabs)
            {
                alleTabellenNamen.Add(item.Item1.ToString());
            }
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

        public bool CheckSpaltenDaten() {
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

        public void Clear()
        {
            alleTabellenfelder = new List<EingabeTabellenfelder>();
            stringArrayListe = new List<string[]>();
            dgDaten.ItemsSource = null;
            while (dgDaten.Columns.Count > 0)
            {
                dgDaten.Columns.RemoveAt(0);
            }
            
            zeichneGrid();
        }
 
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

        private void ComBoxFeldtyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Nichts machen
        }

        private void TxtBezeichnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox box = (System.Windows.Controls.TextBox)sender;
            dgDaten.Columns[(int)box.Tag].Header = box.Text;
        }
 
        #endregion

        public void zeichneGrid()
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
