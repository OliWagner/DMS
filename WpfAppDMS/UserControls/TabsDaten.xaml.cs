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
            item.Content = edd;
            item.Header = dokumentenTyp;
            Items.Add(dokumentenTyp);
            tabsMain.Items.Add(item);
        }
    }
}
