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
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für EingabeTabelle.xaml
    /// </summary>
    public partial class EingabeTabelle : Window
    {
        private List<EingabeTabellenfelder> alleDatensaetze;
        private List<string> alleTabellenNamen;
        
        public EingabeTabelle()
        {
            InitializeComponent();
        }

        public void Start() {
            alleDatensaetze = new List<EingabeTabellenfelder>();
            alleTabellenNamen = new List<string>();
            var con = (DbConnector)App.Current.Properties["Connector"];
            List<Tuple<string, string, string>> allTabs = con.ReadTableNamesTypesAndFields();
            foreach (var item in allTabs)
            {
                alleTabellenNamen.Add(item.Item1.ToString());
            }

            zeichneGrid();
            Show();
        }

        public void ClearGrid() {
            alleDatensaetze = new List<EingabeTabellenfelder>();
            txtTabellenname.Text = "";
            zeichneGrid();
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            //TODO Es dürfen nur Buchstaben und Nummern als Bezeichner verwendet werden
            string AllowedChars = @"^[a-zA-Z0-9]+$";
            

            //Variable dient nur dem Test, ob es schon Tabellenfelder gibt, ansonsten darf der Button ja auch nicht aktiv sein
            bool felderChecker = false;
            //Variable, um zu testen, ob Feldnamen doppelt vorhanden sind
            List<string> _feldnamen = new List<string>();

            foreach (var item in grdMain.Children)
            {
                if (item.GetType() == typeof(EingabeTabellenfelder)) {
                    felderChecker = true;
                    var test = (EingabeTabellenfelder)item;
                    if (test.txtBezeichnung.Text.Equals("") || test.comBoxFeldtyp.SelectedItem == null) {
                        e.CanExecute = false;
                        return;
                    }
                    //CHecken ob EIntrag schon mal vorhanden
                    if (_feldnamen.Contains(test.txtBezeichnung.Text)) {
                        e.CanExecute = false;
                        return;
                    } else {
                        //In den Feldnamen dürfen auch keine SOnderzeichen enthalten sein
                        if (!Regex.IsMatch(test.txtBezeichnung.Text, AllowedChars)) {
                            e.CanExecute = false;
                            return;
                        }
                        _feldnamen.Add(test.txtBezeichnung.Text);
                    }
                }              
            }
            
            //Noch testen, ob ein Tabellenname eingegeben wurde + Es dürfen nur Nummer und Ziffern verwendet werden + Es dürfen keine Tabellennamen doppelt sein
            if (txtTabellenname.Text.Equals("") || !Regex.IsMatch(txtTabellenname.Text, AllowedChars) || alleTabellenNamen.Contains(txtTabellenname.Text)) { felderChecker = false; }
            //Wenn Tabellenfelder da, nur dann kann der Button überhaupt aktiv sein
            if (felderChecker) { e.CanExecute = true; } else { e.CanExecute = false; }            
        }
        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //DO nothing
        }
        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (alleDatensaetze != null)
            {
                foreach (var item in alleDatensaetze)
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


        private void zeichneGrid() {          
            grdMain.Children.Clear();

            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = "Bitte machen Sie Ihre Angaben";
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            txtBlock1.Foreground = new SolidColorBrush(Colors.Green);
            txtBlock1.VerticalAlignment = VerticalAlignment.Center;
            
            RowDefinition gridRow = new RowDefinition();
            gridRow.Height = new GridLength(25);
            grdMain.RowDefinitions.Add(gridRow);
            Grid.SetRow(txtBlock1, 0);
            Grid.SetRow(txtTabellenname, 0);
            grdMain.Children.Add(txtBlock1);

            int y = 1;
            foreach (EingabeTabellenfelder item in alleDatensaetze)
            {               
                item.txtBezeichnung.ToolTip = "Geben Sie hier den Namen des Feldes ein";
                item.comBoxFeldtyp.ToolTip = "Wählen Sie den Datentypen des Feldes aus";
                RowDefinition gridRow1 = new RowDefinition();
                gridRow1.Height = new GridLength(30);
                grdMain.RowDefinitions.Add(gridRow1);
                Grid.SetRow(item, y);
                y = y + 1;
                grdMain.Children.Add(item);
            }
        }

        private void btnNeu_Click(object sender, RoutedEventArgs e)
        {
            EingabeTabellenfelder ds = new EingabeTabellenfelder();
            alleDatensaetze.Add(ds);
            zeichneGrid();
        }

        private void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> werte = new Dictionary<string, string>();
            string tabName = txtTabellenname.Text;

            //Tabelle merken für hoch bubblemdes event
            string ersatztext = "";
            foreach (EingabeTabellenfelder item in alleDatensaetze)
            {
                var strIn = item.comBoxFeldtyp.Text;
                if (strIn.Equals("Nachschlagefeld")) {
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
        }

        private void btnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid();
        }

        private void btnEntfernen_Click(object sender, RoutedEventArgs e)
        {
            List<EingabeTabellenfelder> liste = new List<EingabeTabellenfelder>();
            foreach (var item in alleDatensaetze)
            {
                if ((bool)item.chkLoeschen.IsChecked) {
                    liste.Add(item);
                }
            }
            foreach (var item in liste)
            {
                alleDatensaetze.Remove(item);
            }
            zeichneGrid();
        }
    }

}
