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
    public class SuchfeldAddedEventArgs : EventArgs
    {
        public TextBox textbox { get; set; }
    }
    /// <summary>
    /// Interaktionslogik für Suchfelder.xaml
    /// </summary>
    public partial class Suchfelder : UserControl
    {
        public event EventHandler<SuchfeldAddedEventArgs> ItemAdded;
        public Suchfelder()
        {
            InitializeComponent();
        }

        public void Fill(string _tabelle, out Dictionary<string, TextBox> dicBezeichnungFeldUndTextBox) {
            dicBezeichnungFeldUndTextBox = new Dictionary<string, TextBox>();
            //Dictionary<string, TextBox> DicBezeichnungFeldUndTextBox = dicBezeichnungFeldUndTextBox;

            Tuple<List<string>, List<string>> tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadDataSuchfelder(_tabelle);
            grdMain.Children.Clear();
            grdMain.ColumnDefinitions.Clear();
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
                    //Ich brauche eine Textbox für alle Felder auch Nachschlagefeldern
                    //Die Felder müssen Infos zur bezogenen Column haben --> Name des Feldes ColumnName, Tag des Feldes
                    TextBox tb = new TextBox();
                    tb.TextChanged += Tb_TextChanged;
                    tb.Width = 90;
                    tb.Name = label.Content.ToString();
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetColumn(tb, counter);
                    Grid.SetRow(tb, 1);
                    grdMain.Children.Add(tb);
                    ItemAdded?.Invoke(this, new SuchfeldAddedEventArgs() { textbox = tb });

                    dicBezeichnungFeldUndTextBox.Add(label.Content.ToString(), tb);
                    counter++;
                }
                
                
                
            }



        }


        //FOlgenede FUnktionen müssen in Darstellung DOkumente
        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Wird ausserhalb behandelt --> DarstellungDOkumente
        }

        

    }
}
