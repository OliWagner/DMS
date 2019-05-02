using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : Window
    {
        public string PathString = "";
        public List<Exportdaten> lstExport = new List<Exportdaten>();
        public int test = 123;
        public ExportDialog(List<int> OkoDokumenteDatenIds)
        {
            InitializeComponent();
            lstExport = ((DbConnector)App.Current.Properties["Connector"]).ReadExportDaten(OkoDokumenteDatenIds);
            ZeichneGrid(lstExport);
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Nix
        }
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (grdMain.Children.Count == 0)
            {
                e.CanExecute = false;
            }
            else {
                e.CanExecute = true;
            }
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

                
                Label lblErfasstAm = new Label();
                lblErfasstAm.Content = ed.ErfasstAm.ToShortDateString();
                lblErfasstAm.Margin = new Thickness(200, 0, 0, 0);

                Label lblLoeschen = new Label();
                lblLoeschen.Width = 20;
                lblLoeschen.Tag = ed;
                lblLoeschen.Content = "x";
                lblLoeschen.Margin = new Thickness(280, 0, 0, 0);
                lblLoeschen.MouseDoubleClick += LblLoeschen_MouseDoubleClick;
                lblLoeschen.ToolTip = "Doppelklicken, um Eintrag zu entfernen";
               

                Grid.SetRow(lblDateiname, (counter));
                Grid.SetRow(lblErfasstAm, (counter));
                Grid.SetRow(lblLoeschen, (counter));

                grdMain.Children.Add(lblDateiname);
                grdMain.Children.Add(lblErfasstAm);
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
            //Eigentlicher Export
            //DarstellungDOkumente.ExportDialog_BtnExportieren_Click
            //Exportverzeichnis auswählen
            System.Windows.Forms.FolderBrowserDialog FolderChooser = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = FolderChooser.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.Cancel)
            {
                string path = FolderChooser.SelectedPath;
                string datetime = "OkoExport_" + DateTime.Now;
                datetime = datetime.Replace(" ", "").Replace(":", "");
                PathString = System.IO.Path.Combine(path, datetime);
                DialogResult = true;
            }
            else {
                e.Handled = true;
            }
            

        }
    }

    public class Exportdaten {
        public int DokumenteId { get; set; }
        public string Dateiname { get; set; }
        public DateTime ErfasstAm { get; set; }
        public int IdInTabelle { get; set; }
        public int OkoDokumenteDatenId { get; set; }
    }
}
