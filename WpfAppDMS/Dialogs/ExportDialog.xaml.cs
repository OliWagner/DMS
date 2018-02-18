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
using System.Windows.Shapes;

namespace WpfAppDMS
{
    /// <summary>
    /// Interaktionslogik für ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : Window
    {
        public List<Exportdaten> lstExport = new List<Exportdaten>();
        public int test = 123;
        public ExportDialog(List<int> OkoDokumenteDatenIds)
        {
            InitializeComponent();
            lstExport = ((DbConnector)App.Current.Properties["Connector"]).ReadExportDaten(OkoDokumenteDatenIds);
            ZeichneGrid(lstExport);
        }

        private void ZeichneGrid(List<Exportdaten> lstExport) {
            int counter = 0;
            grdMain.Children.Clear();
            grdMain.RowDefinitions.Clear();
            foreach (Exportdaten ed in lstExport)
            {
                RowDefinition rowdef = new RowDefinition();
                rowdef.Height = new GridLength(30);
                grdMain.RowDefinitions.Add(rowdef);

                Label lblDateiname = new Label();
                lblDateiname.Content = ed.Dateiname;
                lblDateiname.Margin = new Thickness(0, 0, 0, 0);

                Label lblTitel = new Label();
                lblTitel.Content = ed.Titel;
                lblTitel.Margin = new Thickness(200, 0, 0, 0);

                Label lblErfasstAm = new Label();
                lblErfasstAm.Content = ed.ErfasstAm.ToShortDateString();
                lblErfasstAm.Margin = new Thickness(400, 0, 0, 0);

                Label lblDokumententyp = new Label();
                lblDokumententyp.Content = ed.DokumentenTyp;
                lblDokumententyp.Margin = new Thickness(500, 0, 0, 0);

                Label lblLoeschen = new Label();
                lblLoeschen.Width = 20;
                lblLoeschen.Tag = ed;
                lblLoeschen.Content = "x";
                lblLoeschen.Margin = new Thickness(750, 0, 0, 0);
                lblLoeschen.MouseDoubleClick += LblLoeschen_MouseDoubleClick;
                lblLoeschen.ToolTip = "Doppelklicken, um Eintrag zu entfernen";
               

                Grid.SetRow(lblDateiname, (counter));
                Grid.SetRow(lblTitel, (counter));
                Grid.SetRow(lblErfasstAm, (counter));
                Grid.SetRow(lblDokumententyp, (counter));
                Grid.SetRow(lblLoeschen, (counter));

                grdMain.Children.Add(lblDateiname);
                grdMain.Children.Add(lblTitel);
                grdMain.Children.Add(lblErfasstAm);
                grdMain.Children.Add(lblDokumententyp);
                grdMain.Children.Add(lblLoeschen);

                counter++;
            } 
        }

        private void LblLoeschen_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Exportdaten ed = (Exportdaten)((Label)sender).Tag;
            lstExport.Remove(ed);
            ZeichneGrid(lstExport);
        }

        private void btnAlleLoeschen_Click(object sender, RoutedEventArgs e)
        {
            lstExport.Clear();
            ZeichneGrid(lstExport);
        }

        private void btnSchliessen_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            //Wird in DarstellungDokumente weiter behandelt --> EInträge aus der List dort entfernen
        }

        private void btnExportieren_Click(object sender, RoutedEventArgs e)
        {
            //TODO --> Eigentlicher Export
            //Nach Export Liste leeren und true zurück
            lstExport.Clear();
            DialogResult = true;
        }
    }

    public class Exportdaten {
        public int DokumenteId { get; set; }
        public string Dateiname { get; set; }
        public string Titel { get; set; }
        public DateTime ErfasstAm { get; set; }
        public string DokumentenTyp { get; set; }
        public int IdInTabelle { get; set; }
        public string Tabelle { get; set; }
        public int OkoDokumenteDatenId { get; set; }
    }
}
