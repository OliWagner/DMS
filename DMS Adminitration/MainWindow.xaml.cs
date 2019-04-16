using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;

namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Connect();
            InitializeVendorComponents();
        }

        private void Connect(string vorherigeEingabe = "")
        {
            ConnectionDialog connectionDialog = new ConnectionDialog();
            connectionDialog.btnAbbrechen.Click += conDialogBtnAbbrechen_Click;
            if (!vorherigeEingabe.Equals(""))
            {
                connectionDialog.txtDataSource.Text = vorherigeEingabe.Split(';')[0];
                connectionDialog.txtInitialCatalog.Text = vorherigeEingabe.Split(';')[1];
                connectionDialog.txtUserName.Text = vorherigeEingabe.Split(';')[2];
                connectionDialog.txtPassword.Text = vorherigeEingabe.Split(';')[3];
            }
            if (connectionDialog.ShowDialog() == true)
            {
                //Datenbankverbindung initialisieren und in Objekt schreiben
                App.Current.Properties["Connector"] = new DbConnector("Data Source=" + connectionDialog.txtDataSource.Text + ";Initial Catalog=" + connectionDialog.txtInitialCatalog.Text + ";User ID=" + connectionDialog.txtUserName.Text + ";Password=" + connectionDialog.txtPassword.Text + ";");
                if (!((DbConnector)App.Current.Properties["Connector"]).Connect())
                {
                    Connect(connectionDialog.txtDataSource.Text + ";" + connectionDialog.txtInitialCatalog.Text + ";" + connectionDialog.txtUserName.Text + ";" + connectionDialog.txtPassword.Text);
                }
                Title = "Bearbeiten der Datenbank: " + connectionDialog.txtInitialCatalog.Text;
            }
            else
            {
                Connect();
            }

        }

        private void InitializeVendorComponents()
        {
            uebersichtTabellen.zeichneGrid();

            //Eventhandler der Usercontrols initialisieren
            uebersichtTabellen.tvMain.SelectedItemChanged += TvMain_SelectedItemChanged;
            
        }


        #region Command Events

        private void BtnNeueTabelle_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            
        }

        private void BtnNeueTabelle_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;           
        }

        private void BtnNeueTabelleCSV_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void BtnNeueTabelleCSV_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnTabelleBearbeiten_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (uebersichtTabellen != null)
            {
                e.CanExecute = uebersichtTabellen.tvMain.SelectedItem != null;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void BtnTabelleBearbeiten_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnTabellendatenBearbeiten_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {

            if (uebersichtTabellen != null)
            {
                e.CanExecute = uebersichtTabellen.tvMain.SelectedItem != null;
            }
            else
            {
                e.CanExecute = false;
            }

        }

        private void BtnTabellendatenBearbeiten_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnTabelleLeeren_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (uebersichtTabellen != null)
            {
                e.CanExecute = uebersichtTabellen.tvMain.SelectedItem != null;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void BtnTabelleLeeren_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnTabelleLoeschen_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (uebersichtTabellen != null)
            {
                e.CanExecute = uebersichtTabellen.tvMain.SelectedItem != null;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void BtnTabelleLoeschen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnGruppeNeu_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void BtnGruppeNeu_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnGruppeBearbeiten_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void BtnGruppeBearbeiten_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnTypNeu_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void BtnTypNeu_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }

        private void BtnTypBearbeiten_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void BtnTypBearbeiten_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Übersicht Tabellen (Treeview)

        private void TvMain_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var org = (TreeView)sender;
            if (org.SelectedItem != null)
            {
                Tuple<string, string, string> tuple = uebersichtTabellen.WerteDerAuswahl;
            }

        }
        #endregion

        #region Events Buttonclicks
        private void conDialogBtnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void BtnNeueTabelle_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnNeueTabelleCSV_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnTabelleBearbeiten_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnTabellendatenBearbeiten_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnTabelleLeeren_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnTabelleLoeschen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnGruppeNeue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnGruppeBearbeiten_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnTypNeu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnTypBearbeiten_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnBeenden_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        #endregion

        private void ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            grdMain.Children.Clear();

            Ribbon _sender = (Ribbon)sender;
            RibbonTab sel = (RibbonTab)_sender.SelectedItem;
            if (sel != null) { 
                if (sel.Name.Equals("RibTabAblage"))
                {
                
                }
                else if (sel.Name.Equals("RibTabStamm"))
                {
                    if (!grdMain.Children.Contains(uebersichtTabellen)) { 
                    grdMain.Children.Add(uebersichtTabellen);
                    }
                }
                else if (sel.Name.Equals("RibTabDms"))
                {
                
                }
            }
        }

        private void BtnDatenbearbeitung_Click(object sender, RoutedEventArgs e)
        {
            if (ribbon.Items.Contains(RibTabStamm) && ribbon.Items.Contains(RibTabAblage))
            {
                ribbon.Items.Remove(RibTabAblage);
                ribbon.Items.Remove(RibTabStamm);
            }
            else
            {
                ribbon.Items.Remove(RibTabDms);
                ribbon.Items.Add(RibTabStamm);
                ribbon.Items.Add(RibTabAblage);
                ribbon.Items.Add(RibTabDms);
            }
        }
    }

    public static class Fehlerbehandlung
    {
        private static string _path
        {
            get; set;
        }

        public static void Error(string stackTrace, string message, string ownCode)
        {

            FileInfo fi = new FileInfo(Assembly.GetEntryAssembly().Location);
            _path = fi.DirectoryName + "\\Errors.txt";
            if (File.Exists(_path))
            {
                //An FIle anhängen
                using (StreamWriter streamWriter = new StreamWriter(_path, true))
                {
                    streamWriter.Write(DateTime.Now + "\r\n" + stackTrace + "\r\n" + message + Environment.NewLine + ownCode + "\r\n\r\n");
                }
            }
            else
            {
                try
                {
                    //File erstellen
                    using (var x = File.Create(_path)) { }
                    using (StreamWriter sw = new StreamWriter(_path))
                    {
                        sw.Write(DateTime.Now + "\n" + stackTrace + "\n" + message + "\n" + ownCode + "\n\n");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Es konnte keine Datei auf dem Laufwerk angelegt werden. --> " + ex.Message);
                }
            }
        }
    }
}
