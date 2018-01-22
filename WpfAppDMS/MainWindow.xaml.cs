using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SqlConnection _con { get; set; }

        //Felder für die Datenbank
        private string Dateiname { get; set; }
        private string Dateipfad { get; set; }
        private string DateiTitel { get; set; }
        private string DateiBeschreibung { get; set; }

        //Noch zu belegen
        private string Dokumententyp { get; set; }
        private string Tabelle { get; set; }
        private int IdInTabelle { get; set; }
        private string Dateigroesse { get; set; }
        private string ErfasstAm { get; set; }
       
        public MainWindow()
        {
            Connect();
            InitializeComponent();

            dokTree.MouseRightButtonDown += dokTree_MouseRightButtonDown;
        }

        private void dokTree_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeView tv = ((DokTree)sender).tvMain;
            if ((TreeViewItem)tv.SelectedItem != null) { 
                string header = ((TreeViewItem)tv.SelectedItem).Header.ToString();
                if (((TreeViewItem)tv.SelectedItem).Tag != null) {
                    int id = Int32.Parse(((TreeViewItem)tv.SelectedItem).Tag.ToString());
                    if (header.Contains('[') && !tabsDaten.Items.Contains(header))
                    {
                        tabsDaten.Add(header, id);
                    }
                }
                
            }
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string[] data = { };
            if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                data = e.Data.GetData(DataFormats.FileDrop) as string[];
                // handle the files here!
            }
            //databaseFilePut(data[0]);
            txtTitel.Text = "";
            txtTitel.IsEnabled = true;
            txtBeschreibung.Text = "";
            txtBeschreibung.IsEnabled = true;
            Dateipfad = data[0];
            string[] txtArray = data[0].Split('\\');
            Dateiname = txtDropzone.Text = txtArray[txtArray.Length - 1];
            DateiTitel = "";
            DateiBeschreibung = "";
        }

        private void conDialogBtnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
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

        #region Filestreaming Database
        public void databaseFileReadToDisk(string varID, string dateiname)
        {
            FileInfo fi = new FileInfo(Assembly.GetEntryAssembly().Location);
            string _path = fi.DirectoryName + "\\" + dateiname;

            using (var sqlQuery = new SqlCommand(@"SELECT [Dokument] FROM [dbo].[Dokumente] WHERE [DokumenteId] = @varID", _con))
            {
                sqlQuery.Parameters.AddWithValue("@varID", varID);
                using (var sqlQueryResult = sqlQuery.ExecuteReader())
                    if (sqlQueryResult != null)
                    {
                        sqlQueryResult.Read();
                        var blob = new Byte[(sqlQueryResult.GetBytes(0, 0, null, 0, int.MaxValue))];
                        sqlQueryResult.GetBytes(0, 0, blob, 0, blob.Length);
                        using (var fs = new FileStream(_path, FileMode.Create, FileAccess.Write))
                            fs.Write(blob, 0, blob.Length);
                    }
            }
        }

        //public MemoryStream databaseFileReadToMemoryStream(string varID)
        //{
        //    MemoryStream memoryStream = new MemoryStream();
        //    using (var sqlQuery = new SqlCommand(@"SELECT [Dokument] FROM [dbo].[Dokumente] WHERE [DokumenteId] = @varID", _con))
        //    {
        //        sqlQuery.Parameters.AddWithValue("@varID", varID);
        //        using (var sqlQueryResult = sqlQuery.ExecuteReader())
        //            if (sqlQueryResult != null)
        //            {
        //                sqlQueryResult.Read();
        //                var blob = new Byte[(sqlQueryResult.GetBytes(0, 0, null, 0, int.MaxValue))];
        //                sqlQueryResult.GetBytes(0, 0, blob, 0, blob.Length);
        //                //using (var fs = new MemoryStream(memoryStream, FileMode.Create, FileAccess.Write)) {
        //                memoryStream.Write(blob, 0, blob.Length);
        //                //}
        //            }
        //    }
        //    return memoryStream;
        //}

        private void databaseFilePut(string varFilePath)
        {
            byte[] file;
            using (var stream = new FileStream(varFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(stream))
                {
                    file = reader.ReadBytes((int)stream.Length);
                }
            }

            using (var sqlWrite = new SqlCommand("INSERT INTO Dokumente (Dokument) Values(@File)", _con))
            {
                sqlWrite.Parameters.Add("@File", SqlDbType.VarBinary, file.Length).Value = file;
                sqlWrite.ExecuteNonQuery();
            }
        }
        #endregion

        #region Textbox Events, Enable/Disable der Usercontrols
        private void txtTitel_TextChanged(object sender, RoutedEventArgs e)
        {
            DateiTitel = txtTitel.Text;
            DateiBeschreibung = txtBeschreibung.Text;
            CheckEnableUserControls();
        }

        private void txtBeschreibung_TextChanged(object sender, RoutedEventArgs e)
        {
            DateiTitel = txtTitel.Text;
            DateiBeschreibung = txtBeschreibung.Text;
            CheckEnableUserControls();
        }

        private void CheckEnableUserControls()
        {
            if (DateiTitel.Trim().Equals(""))
            {
                tabsDaten.IsEnabled = false;
                dokTree.IsEnabled = false;
            }
            else
            {
                tabsDaten.IsEnabled = true;
                dokTree.IsEnabled = true;
            }
        }
        #endregion
    }
}
