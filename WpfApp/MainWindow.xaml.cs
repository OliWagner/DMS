using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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


namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool eingabeTabelleAktiv = false;
        private bool uploadAktiv = false;

        public MainWindow()
        {
            //Datenbankverbindung initialisieren und in Objekt schreiben
            App.Current.Properties["Connector"] = new DbConnector("Data Source=LAPTOP-CTMG3F1D\\SQLEXPRESS;Initial Catalog=OKOrganizer;User ID=sa;Password=95hjh11!;");

            InitializeComponent();
            uebersichtTabellen.zeichneGrid();

            //Eventhandler der Usercontrols initialisieren
            uebersichtTabellen.tvMain.SelectedItemChanged += TvMain_SelectedItemChanged;
            pflegeTabellendaten.btnSpeichern.Click += BtnTabDatenSpeichern_Click;
            pflegeTabellendaten.btnAbbruch.Click += BtnTabDatenAbbruch_Click;
            tabDaten.dgTabelle.SelectionChanged += dgTabelle_SelectionChanged;
            tabDaten.btnLoeschen.Click += tabDatenBtnLoeschen_Click;
            uebersichtTabellen.btnLoeschen.Click += uebersichtTabellenbtnLoeschen_Click;
            uebersichtTabellen.btnLeeren.Click += uebersichtTabellenbtnLeeren_Click;
        }



        private void BtnNeueTabelle_Click(object sender, RoutedEventArgs e)
        {
            if (!eingabeTabelleAktiv) {
                EingabeTabelle eingabeTabelle = new EingabeTabelle();
                eingabeTabelle.btnSpeichern.Click += BtnSpeichern_Click;
                eingabeTabelle.Closed += EingabeTabelleClosed;
                eingabeTabelle.Name = "eingabeTabelle";
                eingabeTabelle.Title = "Neue Tabelle anlegen";
                eingabeTabelle.Width = 600;
                eingabeTabelle.Height = 500;
                eingabeTabelle.Start();               
                eingabeTabelleAktiv = true;
            } else
            {               
                foreach (var item in Application.Current.Windows)
                {
                    var myItem = (Window)item;
                    if (myItem.Name.Equals("eingabeTabelle")) { myItem.Focus(); }
                }
            }
        }

        private void EingabeTabelleClosed(object sender, EventArgs e)
        {
            eingabeTabelleAktiv = false;
        }

        private void UploadClosed(object sender, EventArgs e)
        {
            uploadAktiv = false;
        }

        private void BtnCsvEinlesenMain_Click(object sender, RoutedEventArgs e)
        {
            if (!uploadAktiv)
            {
                Upload upload = new Upload();
                upload.btnSave.Click += upload_BtnSave_Click;
                upload.Closed += UploadClosed;
                upload.Name = "upload";
                upload.Width = 600;
                upload.Height = 850;
                upload.Title = "Tabelle aus CSV-Datei einlesen";
                upload.Show();
                uploadAktiv = true;
            }
            else
            {
                foreach (var item in Application.Current.Windows)
                {
                    var myItem = (Window)item;
                    if (myItem.Name.Equals("upload")) { myItem.Focus(); }
                }
            }
        }


        private void upload_BtnSave_Click(object sender, RoutedEventArgs e)
        {
           uebersichtTabellen.zeichneGrid();
            foreach (var item in Application.Current.Windows)
            {
                var myItem = (Window)item;
                if (myItem.Name.Equals("upload")) { myItem.Close(); }
            }
        }

        private void uebersichtTabellenbtnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            pflegeTabellendaten.Clear();
            tabDaten.Clear();
        }

        private void uebersichtTabellenbtnLeeren_Click(object sender, RoutedEventArgs e)
        {
            tabDaten.Clear();
        }

        private void tabDatenBtnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            ((DbConnector)App.Current.Properties["Connector"]).DeleteTableData(pflegeTabellendaten._tabName, pflegeTabellendaten._idAktuellerDatensatz);
            tabDaten.zeichneTabelle(pflegeTabellendaten.TabNameUebergabe);
            pflegeTabellendaten.zeichenGrid(pflegeTabellendaten._tabName, pflegeTabellendaten._csvTabFeldnamen, pflegeTabellendaten._csvTabFeldtypen);
        }

        private void dgTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StringBuilder csvFeldwerte = new StringBuilder();
            //Mit Werten aus Pflegetabellendaten und den Werten aus SelectedItem das Grid in PflegeTabellendaten neu zeichnen
            DataRowView row = (DataRowView)tabDaten.dgTabelle.SelectedItem;
            if (row != null) {
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

        private void BtnTabDatenAbbruch_Click(object sender, RoutedEventArgs e)
        {
            uebersichtTabellen.zeichneGrid();
            tabDaten.Clear();
        }

        private void BtnTabDatenSpeichern_Click(object sender, RoutedEventArgs e)
        {
            tabDaten.zeichneTabelle(pflegeTabellendaten.TabNameUebergabe);
        }

        private void TvMain_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var org = (TreeView)sender;            
            if (org.SelectedItem != null) {
                Tuple<string, string, string> tuple = uebersichtTabellen.WerteDerAuswahl;
                pflegeTabellendaten.zeichenGrid(tuple.Item1, tuple.Item2, tuple.Item3);
                pflegeTabellendaten.TabNameUebergabe = tuple.Item1;
                tabDaten.zeichneTabelle(tuple.Item1);
            }
            
        }

        private void BtnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            //TODO eingabeTabelle.ClearGrid();
            uebersichtTabellen.zeichneGrid();
            eingabeTabelleAktiv = false;
        }

        
    }
}
