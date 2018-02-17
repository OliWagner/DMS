using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace WpfAppDMS.Dialogs
{
    /// <summary>
    /// Interaktionslogik für AnwendungsauswahlDialog.xaml
    /// </summary>
    public partial class AnwendungsauswahlDialog : Window
    {
        string TxtAnwendung { get; set; }
        string TxtDateiEndung { get; set; }
        List<Tuple<int, string, string>> Anwendungen { get; set; }

        public AnwendungsauswahlDialog()
        {
            InitializeComponent();
            Anwendungen = ((DbConnector)App.Current.Properties["Connector"]).ReadAnwendungen();
            ZeichneGrid();
        }

        private void ZeichneGrid() {
            int counter = 0;
            foreach (Tuple<int, string, string> tuple in Anwendungen)
            {
                RowDefinition rowdef = new RowDefinition();
                rowdef.Height = new GridLength(30);
                grdMain.RowDefinitions.Add(rowdef);
                //ColumnDefinition coldefLinks = new ColumnDefinition();
                //coldefLinks.Width = new GridLength(50);
                //ColumnDefinition coldefMitte = new ColumnDefinition();
                //coldefMitte.Width = new GridLength(380);
                //ColumnDefinition coldefRechts = new ColumnDefinition();
                //coldefRechts.Width = new GridLength(20);

                Label lblEndung = new Label();
                lblEndung.Content = tuple.Item2;
                lblEndung.Margin = new Thickness(0, 0, 0, 0);

                Label lblAnwendung = new Label();
                lblAnwendung.Content = tuple.Item3;
                lblAnwendung.Margin = new Thickness(50, 0, 0, 0);

                Label lblLoeschen = new Label();
                lblLoeschen.Width = 20;
                lblLoeschen.Content = "x";
                lblLoeschen.Margin = new Thickness(420, 0, 0, 0);
                lblLoeschen.MouseDoubleClick += LblLoeschen_MouseDoubleClick;
                lblLoeschen.ToolTip = "Doppelklicken, um Eintrag zu entfernen";
                lblLoeschen.Tag = tuple.Item1;

                //Grid.SetColumn(lblEndung, 0);
                //Grid.SetColumn(lblAnwendung, 1);
                //Grid.SetColumn(lblLoeschen, 2);

                Grid.SetRow(lblEndung, (counter));
                Grid.SetRow(lblAnwendung, (counter));
                Grid.SetRow(lblLoeschen, (counter));

                grdMain.Children.Add(lblEndung);
                grdMain.Children.Add(lblAnwendung);
                grdMain.Children.Add(lblLoeschen);

                counter++;
            }
        }

        private void LblLoeschen_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int lblSenderId = Int32.Parse(((Label)sender).Tag.ToString());
            ((DbConnector)App.Current.Properties["Connector"]).DeleteAnwendung(lblSenderId);
            grdMain.Children.Clear();
            Anwendungen = ((DbConnector)App.Current.Properties["Connector"]).ReadAnwendungen();
            ZeichneGrid();
        }

        private void btnAuswahl_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                TxtAnwendung = openFileDialog.FileName;
                TxtDateiEndung = txtEndung.Text;
            }
        }

        private void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            ((DbConnector)App.Current.Properties["Connector"]).AnwendungEintragen(TxtDateiEndung, TxtAnwendung);

            TxtAnwendung = "";
            TxtDateiEndung = "";
            txtEndung.Text = ".";
            grdMain.Children.Clear();
            Anwendungen = ((DbConnector)App.Current.Properties["Connector"]).ReadAnwendungen();
            ZeichneGrid();
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Nix
        }
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (TxtAnwendung == null || TxtDateiEndung == null || TxtAnwendung.Equals("") || TxtDateiEndung.Equals(""))
            {
                 e.CanExecute = false; 
            }
            else { e.CanExecute = true; }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
