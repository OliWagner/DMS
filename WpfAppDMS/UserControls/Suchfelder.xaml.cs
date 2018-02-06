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
            Tuple<List<string>, List<string>> tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadDataSuchfelder(_tabelle);
            int counter = 0;
            for (int i = 0; i < tuple.Item1.Count; i++)
            {
                //Titelzeile schreiben
                if (!tuple.Item2.ElementAt(i).Contains("date"))
                {
                    Label label = new Label();
                    label.Content = tuple.Item1.ElementAt(i).Contains("_x_") ? tuple.Item1.ElementAt(i).Split('_')[2] : tuple.Item1.ElementAt(i);
                    label.VerticalAlignment = VerticalAlignment.Top;
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.Width = 100;
                
                    grdMain.ColumnDefinitions.Add(new ColumnDefinition());
                    Grid.SetColumn(label, counter);
                    Grid.SetRow(label, 0);
                
                    grdMain.Children.Add(label);
                    //TODO --> Formularfeld schreiben










                    counter++;
                }
                
                
                
            }



        }


    }
}
