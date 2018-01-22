using System;
using System.Collections.Generic;
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

namespace WpfAppDMS
{
    /// <summary>
    /// Interaktionslogik für DokTree.xaml
    /// </summary>
    public partial class DokTree : UserControl
    {
        private List<string> alleDokGruppen;
        private List<int> alleDokGruppenIds;
        private List<string> alleDokGruppenBeschreibungen;
        private List<string> alleDokTypenBeschreibungen;
        private List<int> alleDokTypenGruppenIds;
        private List<string> alleDokTypen;
        private List<int> alleDokTypenIds;
        private List<string> alleDokTypTabellen;
        private List<string> alleTabellenInDb;

        public DokTree()
        {
            InitializeComponent();
            Start();
        }

        private void Start() {
            alleTabellenInDb = new List<string>();
            List<Tuple<string, string, string>> lst = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields();
            foreach (Tuple<string, string, string> tuple in lst)
            {
                alleTabellenInDb.Add(tuple.Item1);
            }
            LiesListen();
        }

        private void LiesListen()
        {
            Tuple<Tuple<List<string>, List<int>, List<string>>, Tuple<List<string>, List<int>, List<string>, List<int>, List<string>>> gruppenundTypenTuple = ((DbConnector)App.Current.Properties["Connector"]).ReadDoksAndTypesData();
            alleDokGruppen = gruppenundTypenTuple.Item1.Item1;
            alleDokGruppenIds = gruppenundTypenTuple.Item1.Item2;
            alleDokGruppenBeschreibungen = gruppenundTypenTuple.Item1.Item3;
            alleDokTypen = gruppenundTypenTuple.Item2.Item1;
            alleDokTypenIds = gruppenundTypenTuple.Item2.Item2;
            alleDokTypenBeschreibungen = gruppenundTypenTuple.Item2.Item3;
            alleDokTypenGruppenIds = gruppenundTypenTuple.Item2.Item4;
            alleDokTypTabellen = gruppenundTypenTuple.Item2.Item5;

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
                    unteritem.MouseRightButtonDown += Unteritem_MouseRightButtonDown;
                    string[] txtArray = tvUnterItem.Split('[');
                    string wert = txtArray[0].Trim();
                    int index = alleDokTypen.IndexOf(wert);
                    unteritem.ToolTip = alleDokTypenBeschreibungen.ElementAt(index);
                    unteritem.Tag = alleDokTypenIds.ElementAt(index);
                    item.Items.Add(unteritem);
                }
                tvMain.Items.Add(item);
            }

        }

        private void Unteritem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Hier passiert nichts, in Mainwindow.dokTree_MouseRightButtonDown wird mit dem Rechtsklick auf ein Dokumententyp ein Tab zur Zuordnung geöffnet.
        }
    }
}
