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

namespace WpfApp.UserControls
{
    /// <summary>
    /// Interaktionslogik für AuswahlLookupWert.xaml
    /// </summary>
    public partial class LookupAuswahl : UserControl
    {
        public LookupAuswahl()
        {
            InitializeComponent();
            
        }

        public void Fill(string _feld)
        {
            Tuple<List<int>, List<object>> tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadComboboxItems(_feld.Split('_')[3], _feld.Split('_')[4]);

            for(int i = 0; i < tuple.Item1.Count(); i++)
            {
                
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Tag = tuple.Item1.ElementAt(i);
                    cbi.Content = tuple.Item2.ElementAt(i);
                    cboAuswahl.Items.Add(cbi);
                         
            }
        }
    }
}
