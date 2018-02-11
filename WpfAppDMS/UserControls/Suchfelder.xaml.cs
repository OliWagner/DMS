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
                    counter++;
                }
                
                
                
            }



        }


        //FOlgenede FUnktionen müssen in Darstellung DOkumente
        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            DgFilter(((TextBox)sender).Name, ((TextBox)sender).Text);
        }

        private void DgFilter(string Feldname, string wert) {
            //DataGrid dgFiltered = new DataGrid();
            
            //foreach(ColumnDefinition column in dgD)
            //{
            //    dgFiltered.Columns.Add(column.);

            //}


        }

    }
}
