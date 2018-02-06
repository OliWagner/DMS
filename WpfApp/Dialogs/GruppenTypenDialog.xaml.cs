using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class GruppenTypenDialog : Window
    {
        private List<string> alleDokGruppen;
        private List<int> alleDokGruppenIds;
        private List<string> alleDokGruppenBeschreibungen;
        private List<string> alleDokTypenBeschreibungen;
        private List<int> alleDokTypenGruppenIds;
        private List<string> alleDokTypen;
        private List<int> alleDokTypenIds;
        private List<string> alleTabellenInDb;

        private bool DokumentTypInBearbeitung = false;
        private bool DokumentGruppeInBearbeitung = false;

        //Variable, die entscheidet, ob es sich bei dem Datensatz im Formular um einen neuen EIntrag handelt, oder ob der EIntrag geändert werden soll
        int AktDokGruppenId = 0;
        int AktDokTypenId = 0;

        //DIe Tabellenfelder für die Tabelle zum Dokumententypen
        private Dictionary<string, string> DokTypFeldwerte = new Dictionary<string, string>();

        public GruppenTypenDialog()
        {            
            InitializeComponent();
            alleTabellenInDb = new List<string>();
            List<Tuple<string, string, string>> lst = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields();
            foreach (Tuple<string, string, string> tuple in lst)
            {
                alleTabellenInDb.Add(tuple.Item1);
            }
            LiesListen();
        }

        private void Item_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            int index = alleDokGruppen.IndexOf(item.Header.ToString());
            AktDokGruppenId = alleDokGruppenIds.ElementAt(index);
            txtGruppeBezeichnung.Text = item.Header.ToString();
            txtGruppeBeschreibung.Text = alleDokGruppenBeschreibungen.ElementAt(index);
            DokumentGruppeInBearbeitung = true;
        }

        private void Unteritem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            int index = alleDokTypen.IndexOf(item.Header.ToString());
            AktDokTypenId = alleDokTypenIds.ElementAt(index);
            txtTypBezeichnung.Text = item.Header.ToString();
            txtTypBezeichnung.IsEnabled = false;
            txtTypBeschreibung.Text = alleDokTypenBeschreibungen.ElementAt(index);
            

            int AktGruppenid = alleDokTypenGruppenIds.ElementAt(index);
            int GruppenIndex = alleDokGruppenIds.IndexOf(AktGruppenid);
            cboGruppe.SelectedValue = alleDokGruppen.ElementAt(GruppenIndex);
            btnWerteAnlegen.IsEnabled = false;
            DokumentTypInBearbeitung = true;
            e.Handled = true;
        }


        private void LiesListen() {
            Tuple<Tuple<List<string>, List<int>, List<string>>, Tuple<List<string>, List<int>, List<string>, List<int>>> gruppenundTypenTuple = ((DbConnector)App.Current.Properties["Connector"]).ReadDoksAndTypesData();
            alleDokGruppen = gruppenundTypenTuple.Item1.Item1;
            alleDokGruppenIds = gruppenundTypenTuple.Item1.Item2;
            alleDokGruppenBeschreibungen = gruppenundTypenTuple.Item1.Item3;
            alleDokTypen = gruppenundTypenTuple.Item2.Item1;
            alleDokTypenIds = gruppenundTypenTuple.Item2.Item2;
            alleDokTypenBeschreibungen = gruppenundTypenTuple.Item2.Item3;
            alleDokTypenGruppenIds = gruppenundTypenTuple.Item2.Item4;
    
            cboGruppe.ItemsSource = alleDokGruppen;
            // Den Treeview bauen
            //Erst Dictionary bauen. Key ist DOkumententyp (Tabelle) / Value ist die GruppenId
            Dictionary<string, int> dicTypen = new Dictionary<string, int>();
            for (int i = 0; i < alleDokTypen.Count; i++)
            {
                dicTypen.Add(alleDokTypen.ElementAt(i), gruppenundTypenTuple.Item2.Item4.ElementAt(i));
            }
            Dictionary<int, string> dicGruppen = new Dictionary<int, string>();
            for (int i = 0; i < alleDokGruppen.Count; i++)
            {
                dicGruppen.Add(alleDokGruppenIds.ElementAt(i), alleDokGruppen.ElementAt(i));
            }
            tvMain.Items.Clear();
            foreach (KeyValuePair<int, string> kvp in dicGruppen.OrderBy(x => x.Value))
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = kvp.Value;
                item.ToolTip = alleDokGruppenBeschreibungen.ElementAt(alleDokGruppenIds.IndexOf(kvp.Key));
                item.MouseRightButtonDown += Item_MouseRightButtonDown;
                
                List<string> lstItems = (from KeyValuePair<string, int> typ in dicTypen where typ.Value == kvp.Key orderby typ.Key select typ.Key).ToList();
                
                foreach (string tvUnterItem in lstItems)
                {
                    TreeViewItem unteritem = new TreeViewItem();
                    unteritem.Header = tvUnterItem;
                    unteritem.MouseRightButtonDown += Unteritem_MouseRightButtonDown;
                    int index = alleDokTypen.IndexOf(tvUnterItem);
                    unteritem.ToolTip = alleDokTypenBeschreibungen.ElementAt(index);                    
                    item.Items.Add(unteritem);
                }
                tvMain.Items.Add(item);
            }

        }

       
        private void SichernDokGruppen_Executed(object sender, ExecutedRoutedEventArgs e)
        {}

        private void SichernDokGruppen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            string AllowedChars = @"^[a-zA-Z0-9 .,ÄäÖöÜü()]+$";
            if (txtGruppeBezeichnung.Text.Equals("") || txtGruppeBeschreibung.Text.Equals("") || !Regex.IsMatch(txtGruppeBezeichnung.Text, AllowedChars) || !Regex.IsMatch(txtGruppeBeschreibung.Text, AllowedChars)) {
                e.CanExecute = false;
            }

            if (alleDokGruppen != null && alleDokGruppen.Contains(txtGruppeBezeichnung.Text.Trim())) {
                e.CanExecute = false;
                if (AktDokGruppenId != 0) {
                    e.CanExecute = true;
                }
            }
        }

        private void SichernDokTypen_Executed(object sender, ExecutedRoutedEventArgs e)
        {}

        private void SichernDokTypen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            string AllowedChars = @"^[a-zA-Z0-9 .,ÄäÖöÜü()]+$";
            
            if (alleDokTypen != null && (alleDokTypen.Contains(txtTypBezeichnung.Text.Trim()) || DokTypFeldwerte.Count == 0))
            {
                e.CanExecute = false;
                if (AktDokTypenId != 0)
                {
                    e.CanExecute = true;
                }
            }
            if (txtTypBezeichnung.Text.Equals("") || (alleDokTypen.Contains(txtTypBezeichnung.Text.Trim()) && DokumentTypInBearbeitung == false ) || !Regex.IsMatch(txtTypBezeichnung.Text, AllowedChars) || !Regex.IsMatch(txtTypBeschreibung.Text, AllowedChars))
            {
                e.CanExecute = false;

            }
            if (cboGruppe.SelectedItem == null)
            {
                e.CanExecute = false;
            }
        }

        private void btnFertig_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnGruppeAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            txtGruppeBeschreibung.Text = "";
            txtGruppeBezeichnung.Text = "";
            AktDokGruppenId = 0;
            DokumentGruppeInBearbeitung = false;
        }

        private void btnTypVerwerfen_Click(object sender, RoutedEventArgs e)
        {
            txtTypBeschreibung.Text = "";
            txtTypBezeichnung.Text = "";
            cboGruppe.SelectedItem = null;
            AktDokTypenId = 0;
            btnWerteAnlegen.IsEnabled = true;
            DokumentTypInBearbeitung = false;
            txtTypBezeichnung.IsEnabled = true;
        }

        private void btnGruppeSpeichern_Click(object sender, RoutedEventArgs e)
        {
            if (AktDokGruppenId == 0) {
                ((DbConnector)App.Current.Properties["Connector"]).AddDokGruppe(txtGruppeBezeichnung.Text, txtGruppeBeschreibung.Text);
                         
            } else {
                ((DbConnector)App.Current.Properties["Connector"]).UpdateDokGruppe(txtGruppeBezeichnung.Text, txtGruppeBeschreibung.Text, AktDokGruppenId);
                AktDokGruppenId = 0;
            }
            txtGruppeBeschreibung.Text = "";
            txtGruppeBezeichnung.Text = "";
            DokumentGruppeInBearbeitung = false;
            LiesListen();
        }

        private void btnTypSpeichern_Click(object sender, RoutedEventArgs e)
        {
            int idGruppe = alleDokGruppenIds.ElementAt(alleDokGruppen.IndexOf(cboGruppe.SelectedValue.ToString()));

            if (AktDokTypenId == 0)
            {
                ((DbConnector)App.Current.Properties["Connector"]).AddDokTyp(txtTypBezeichnung.Text, txtTypBeschreibung.Text, idGruppe);
                ((DbConnector)App.Current.Properties["Connector"]).CreateNewTable("xyx"+txtTypBezeichnung.Text, DokTypFeldwerte, true);
            }
            else
            {
                ((DbConnector)App.Current.Properties["Connector"]).UpdateDokTyp(txtTypBezeichnung.Text, txtTypBeschreibung.Text, AktDokTypenId, idGruppe);
            }
            txtTypBeschreibung.Text = "";
            txtTypBezeichnung.Text = "";
            cboGruppe.SelectedItem = null;
            AktDokTypenId = 0;
            DokumentTypInBearbeitung = false;
            LiesListen();
            txtTypBezeichnung.IsEnabled = true;
        }

        private void btnWerteAnlegen_Click(object sender, RoutedEventArgs e)
        {
            EingabeDokTypFelder eingabeDokTypFelder = new EingabeDokTypFelder();
            eingabeDokTypFelder.Start();
            eingabeDokTypFelder.Width = 550;
            eingabeDokTypFelder.Height = 530;
            eingabeDokTypFelder.Title = "Zum Dokumententyp zu erfassende Werte festlegen";
            if (eingabeDokTypFelder.ShowDialog() == true) {
                DokTypFeldwerte = eingabeDokTypFelder.Werte;
            }
        }
    }
}
