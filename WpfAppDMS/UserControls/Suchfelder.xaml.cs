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
    /// Interaktionslogik für Suchfelder.xaml
    /// </summary>
    public partial class Suchfelder : UserControl
    {
        public Suchfelder()
        {
            InitializeComponent();
        }

        public void Fill(string _tabelle) {
            //Neue DBConnector-Funktion, die alle Felder und deren Feldtypen zurück gibt
            //Daten werden wie beim Join vom Grid sortiert, da COde übernehmen
            //Danach vielleicht direkt auf der DataTable filtern? Dann wäre man fertig
        }


    }
}
