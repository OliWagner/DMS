using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Linq;


namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        public string DataSource
        {
            get { return txtDataSource.Text; }
        }

        public string InitialCatalog
        {
            get { return txtInitialCatalog.Text; }
        }

        public string UserName
        {
            get { return txtUserName.Text; }
        }

        public string Password
        {
            get { return txtPassword.Text; }
        }

        public static ConnectionDirectory connectionDirectory = new ConnectionDirectory();

        private string _path
        {
            get; set;
        }

        public ConnectionDialog()
        {
            InitializeComponent();

            //Verzeichnis der ANwendung ermitteln
            FileInfo fi = new FileInfo(Assembly.GetEntryAssembly().Location);
            _path = fi.DirectoryName +"\\Connections.xml";
            if (File.Exists(_path))
            {
                ConnectionDirectory cd = DeSerialize(_path);
                foreach (var item in cd.ConnectionsList)
                {
                    connectionDirectory.Add(new ConnectionDetails { InitialCatalog = item.InitialCatalog, DataSource = item.DataSource, Username = item.Username, Password = item.Password  });
                    //Der Combobox die Elemente hinzufügen
                    ComboBoxItem cboItem = new ComboBoxItem();
                    cboItem.Content = item.InitialCatalog;
                    cboConnections.Items.Add(cboItem);
                }
            }
            else
            {
                txtDataSource.Text = "LAPTOP-CTMG3F1D\\SQLEXPRESS";
                txtInitialCatalog.Text = "OKOrganizer";
                txtUserName.Text = "sa";
                txtPassword.Text = "95hjh11!";
            }

            
        }

        #region Serialization
        private ConnectionDirectory DeSerialize(string file)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(ConnectionDirectory));
            TextReader reader = new StreamReader(@file);
            object obj = deserializer.Deserialize(reader);
            ConnectionDirectory XmlData = (ConnectionDirectory)obj;
            reader.Close();
            return XmlData;
        }

        private void Serialize(ConnectionDirectory details, string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ConnectionDirectory));
            using (TextWriter writer = new StreamWriter(@file))
            {
                serializer.Serialize(writer, details);
            }
        }
        #endregion


        #region Eventhandler
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //Wird in Mainindow behandelt, schließt die Applikation
        }

        private void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            connectionDirectory.Add(new ConnectionDetails { InitialCatalog = txtInitialCatalog.Text, DataSource = txtDataSource.Text, Username = txtUserName.Text, Password = txtPassword.Text });
            Serialize(connectionDirectory, _path);
        }

        private void cboConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string conName = ((ComboBoxItem)cboConnections.SelectedItem).Content.ToString();
            ConnectionDetails conDetails = (from ConnectionDetails cd in connectionDirectory.ConnectionsList where cd.InitialCatalog.Equals(conName) select cd).FirstOrDefault();
            txtInitialCatalog.Text = conDetails.InitialCatalog;
            txtDataSource.Text = conDetails.DataSource;
            txtUserName.Text = conDetails.Username;
            txtPassword.Text = conDetails.Password;
        }
    }
    #endregion





    public class ConnectionDetails
    {
        public string DataSource { get; set; }
        public string InitialCatalog { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ConnectionDirectory
    {
        [XmlElement("ConnectionDetails")]
        public List<ConnectionDetails> ConnectionsList = new List<ConnectionDetails>();

        public void Add(ConnectionDetails elem)
        {
            ConnectionsList.Add(elem);
        }
    }














}
