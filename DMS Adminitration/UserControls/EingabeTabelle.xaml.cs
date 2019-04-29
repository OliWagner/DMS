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

namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für EingabeTabelle.xaml
    /// </summary>
    public partial class EingabeTabelle : UserControl
    {
        public List<EingabeTabellenfelder> alleDatensaetze;
        public List<string> alleTabellenNamen;
        
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
        }

        public void ClearGrid() {
            alleDatensaetze = new List<EingabeTabellenfelder>();
            zeichneGrid();
        }

        public void zeichneGrid() {          
            this.grdMain.Children.Clear();
            grdMain.RowDefinitions.Clear();

            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = "Bitte machen Sie Ihre Angaben";
            txtBlock1.FontSize = 14;
            //txtBlock1.FontWeight = FontWeights.Bold;
            //txtBlock1.Foreground = new SolidColorBrush(Colors.Green);
            txtBlock1.VerticalAlignment = VerticalAlignment.Center;
            
            RowDefinition gridRow = new RowDefinition();
            gridRow.Height = new GridLength(25);
            grdMain.RowDefinitions.Add(gridRow);
            Grid.SetRow(txtBlock1, 0);
            
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
    }

}
