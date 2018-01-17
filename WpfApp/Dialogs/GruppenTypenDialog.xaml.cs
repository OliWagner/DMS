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
        private List<int> alleDokTypenIds;
        private List<string> alleDokTypen;
        private List<string> alleTabellenInDb;

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

        private void LiesListen() {
            Tuple<Tuple<List<string>, List<int>, List<string>>, Tuple<List<string>, List<int>, List<string>, List<int>, List<string>>> gruppenundTypenTuple = ((DbConnector)App.Current.Properties["Connector"]).ReadDoksAndTypesData();
            alleDokGruppen = gruppenundTypenTuple.Item1.Item1;
            alleDokGruppenIds = gruppenundTypenTuple.Item1.Item2;
            alleDokGruppenBeschreibungen = gruppenundTypenTuple.Item1.Item3;
            alleDokTypen = gruppenundTypenTuple.Item2.Item1;
            alleDokTypenBeschreibungen = gruppenundTypenTuple.Item2.Item3;
            alleDokTypenIds = gruppenundTypenTuple.Item2.Item4;

            cboGruppe.ItemsSource = alleDokGruppen;
            cboTabelle.ItemsSource = alleTabellenInDb;
            // Den Treeview bauen
            //Erst Dictionary bauen. Key ist DOkumententyp (Tabelle) / Value ist die GruppenId
            Dictionary<string, int> dicTypen = new Dictionary<string, int>();
            for (int i = 0; i < alleDokTypen.Count; i++)
            {
                dicTypen.Add(alleDokTypen.ElementAt(i) + " [" + gruppenundTypenTuple.Item2.Item5.ElementAt(i) + "]", gruppenundTypenTuple.Item2.Item4.ElementAt(i));
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
                //TODO Die UnterItems auflisten
                List<string> lstItems = (from KeyValuePair<string, int> typ in dicTypen where typ.Value == kvp.Key orderby typ.Key select typ.Key).ToList();
                
                foreach (string tvUnterItem in lstItems)
                {
                    TreeViewItem unteritem = new TreeViewItem();
                    unteritem.Header = tvUnterItem;
                    string[] txtArray = tvUnterItem.Split('[');
                    string wert = txtArray[0].Trim();
                    int index = alleDokTypen.IndexOf(wert);
                    unteritem.ToolTip = alleDokTypenBeschreibungen.ElementAt(index);
                    item.Items.Add(unteritem);
                }
                tvMain.Items.Add(item);
                //TODO Die UnterItems auflisten
            }

        }


        private void SichernDokGruppen_Executed(object sender, ExecutedRoutedEventArgs e)
        {}

        private void SichernDokGruppen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            string AllowedChars = @"^[a-zA-Z0-9 .,ÄäÖöÜü()]+$";
            if (txtGruppeBezeichnung.Text.Equals("") || txtGruppeBeschreibung.Text.Equals("") || alleDokGruppen.Contains(txtGruppeBezeichnung.Text.Trim()) || !Regex.IsMatch(txtGruppeBezeichnung.Text, AllowedChars) || !Regex.IsMatch(txtGruppeBeschreibung.Text, AllowedChars)) {
                e.CanExecute = false;
            }
        }

        private void SichernDokTypen_Executed(object sender, ExecutedRoutedEventArgs e)
        {}

        private void SichernDokTypen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            string AllowedChars = @"^[a-zA-Z0-9 .,ÄäÖöÜü()]+$";
            if (txtTypBezeichnung.Text.Equals("") || alleDokTypen.Contains(txtTypBezeichnung.Text.Trim()) || !Regex.IsMatch(txtTypBezeichnung.Text, AllowedChars) || !Regex.IsMatch(txtTypBeschreibung.Text, AllowedChars))
            {
                e.CanExecute = false;
            }
            if (cboGruppe.SelectedItem == null) {
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
        }

        private void btnTypVerwerfen_Click(object sender, RoutedEventArgs e)
        {
            txtTypBeschreibung.Text = "";
            txtTypBezeichnung.Text = "";
            cboGruppe.SelectedItem = null;
            cboTabelle.SelectedItem = null;
        }

        private void btnGruppeSpeichern_Click(object sender, RoutedEventArgs e)
        {
            ((DbConnector)App.Current.Properties["Connector"]).AddDokGruppe(txtGruppeBezeichnung.Text, txtGruppeBeschreibung.Text);
            txtGruppeBeschreibung.Text = "";
            txtGruppeBezeichnung.Text = "";
            LiesListen();
        }

        private void btnTypSpeichern_Click(object sender, RoutedEventArgs e)
        {
            int idGruppe = alleDokGruppenIds.ElementAt(alleDokGruppen.IndexOf(cboGruppe.SelectedValue.ToString()));


            ((DbConnector)App.Current.Properties["Connector"]).AddDokTyp(txtTypBezeichnung.Text, txtTypBeschreibung.Text, idGruppe, cboTabelle.SelectedValue.ToString());
            txtTypBeschreibung.Text = "";
            txtTypBezeichnung.Text = "";
            cboGruppe.SelectedItem = null;
            cboTabelle.SelectedItem = null;
            LiesListen();
        }
    }
}
