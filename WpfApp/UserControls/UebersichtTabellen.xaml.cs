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

namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für UebersichtTabellen.xaml
    /// </summary>
    public partial class UebersichtTabellen : UserControl
    {
        List<Tuple<string, string, string>> alleTabellennamen = new List<Tuple<string, string, string>>();
        public Tuple<string, string, string> WerteDerAuswahl { get; set; }


        public UebersichtTabellen()
        {
            InitializeComponent();
        }

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //DO nothing
        }

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (tvMain.Items != null) { 
                foreach (TreeViewItem item in tvMain.Items)
                {
                    if ((bool)item.IsSelected)
                    {
                        e.CanExecute = true;
                        return;
                    }
                }
            }
            e.CanExecute = false;
        }

        public void zeichneGrid() {
            tvMain.Items.Clear();
            tvMain.SelectedItemChanged += TvMain_SelectedItemChanged;
            alleTabellennamen = ((DbConnector)App.Current.Properties["Connector"]).ReadTableNamesTypesAndFields();
            foreach (var item in alleTabellennamen)
            {
                //Neues Mainitem für Treeview erzeugen
                TreeViewItem treeItem = new TreeViewItem();
                treeItem.Header = item.Item1;

                //csv trennen und in Items schreiben Feldname(Feldtyp)
                var csvNamen = item.Item3.Split(';');
                var csvTypen = item.Item2.Split(';');
                int counter = 0;
                foreach (var csvItem in csvNamen)
                {
                    var newItem = new TreeViewItem() { Header = csvItem + " (" + csvTypen.ElementAt(counter) + ")" };
                    newItem.IsEnabled = false;
                    treeItem.Items.Add(newItem);
                    counter++;
                }
                treeItem.Tag = item.Item3 + ":" + item.Item2;
                tvMain.Items.Add(treeItem);
            }
        }

        private void TvMain_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selItem = (TreeViewItem)tvMain.SelectedItem;
            if (selItem != null)
            {
                var tabHeader = selItem.Header.ToString();
                var csvs = selItem.Tag.ToString().Split(':');
                WerteDerAuswahl = Tuple.Create<string, string, string>(tabHeader, csvs[0], csvs[1]);
            }
            
        }

        private void btnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)tvMain.SelectedItem;
            string tabName = tvi.Header.ToString();
            ((DbConnector)App.Current.Properties["Connector"]).DeleteTable(tabName);
            zeichneGrid();
        }

        private void btnLeeren_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)tvMain.SelectedItem;
            string tabName = tvi.Header.ToString();
            ((DbConnector)App.Current.Properties["Connector"]).DeleteAllTableData(tabName);
            zeichneGrid();
        }
    }
}
