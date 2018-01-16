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
            InitializeComponent();
            Connect();
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

        

        public void Connect()
        {
                _con = new SqlConnection();
                _con.ConnectionString = "Data Source='LAPTOP-CTMG3F1D\\SQLEXPRESS';Initial Catalog='OKOrganizer';User ID='sa';Password='95hjh11!';";
                _con.Open();
        }

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

        //obsolete
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //databaseFileReadToDisk("1", "Output.txt");
            //MemoryStream ms = databaseFileReadToMemoryStream("1");
            var test = 0;
        }

        private void txtTitel_LostFocus(object sender, RoutedEventArgs e)
        {
            DateiTitel = txtTitel.Text;
        }

        private void txtBeschreibung_LostFocus(object sender, RoutedEventArgs e)
        {
            DateiBeschreibung = txtBeschreibung.Text;
        }
    }
}
