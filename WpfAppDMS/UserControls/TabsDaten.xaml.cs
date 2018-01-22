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
    /// Interaktionslogik für TabsDaten.xaml
    /// </summary>
    public partial class TabsDaten : UserControl
    {
        public List<string> Items = new List<string>();

        public TabsDaten()
        {
            InitializeComponent();
        }

        public void Add(string dokumentenTyp, int dokumentenTypId) {
            TabItem item = new TabItem();
            EingabeDokumentDaten edd = new EingabeDokumentDaten();
            edd.zeichneGrid(dokumentenTyp, dokumentenTypId);
            edd.btnAbbruch.Click += EingabeDokumentDaten_BtnAbbruch_Click;
            item.Content = edd;
            item.Header = dokumentenTyp;
            Items.Add(dokumentenTyp);
            tabsMain.Items.Add(item);
        }

        private void EingabeDokumentDaten_BtnAbbruch_Click(object sender, RoutedEventArgs e)
        {
            TabItem toRemove = new TabItem();
            string table = ((Button)sender).Tag.ToString();
            foreach (TabItem item in tabsMain.Items)
            {
                if (item.Header.Equals(table)) {
                    toRemove = item;
                    Items.Remove(item.Header.ToString());
                }
            }
            tabsMain.Items.Remove(toRemove);
        }
    }
}
