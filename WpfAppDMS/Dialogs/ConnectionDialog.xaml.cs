using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Linq;
using Vigenere;
using System;
using System.Windows.Input;
using WpfAppError;

namespace WpfAppDMS
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        #region Fields
        public string MyGuid
        {
            get { return txtGuid.Text; }
        }

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
        VigenereQuadrath q = new VigenereQuadrath("PicKerL1465JHdg");

        private string _path
        {
            get; set;
        }
        #endregion

        public ConnectionDialog()
        {
            InitializeComponent();

            //Verzeichnis der Anwendung ermitteln
            FileInfo fi = new FileInfo(Assembly.GetEntryAssembly().Location);
            _path = fi.DirectoryName +"\\Connections.xml";
            if (File.Exists(_path))
            {
                ConnectionDirectory cd = DeSerialize(_path);
                connectionDirectory.ConnectionsList = new List<ConnectionDetails>();
                foreach (var item in cd.ConnectionsList)
                {
                    connectionDirectory.Add(new ConnectionDetails { MyGuid = q.EntschlüssleText(item.MyGuid), InitialCatalog = q.EntschlüssleText(item.InitialCatalog).Replace(" ","\\"), DataSource = q.EntschlüssleText(item.DataSource), Username = q.EntschlüssleText(item.Username), Password = q.EntschlüssleText(item.Password)  });
                    //Der Combobox die Elemente hinzufügen
                    ComboBoxItem cboItem = new ComboBoxItem();
                    cboItem.Content = q.EntschlüssleText(item.InitialCatalog);
                    cboConnections.Items.Add(cboItem);
                }
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

        #region Commands
        private void VerbindenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!txtDataSource.Text.Equals("") && !txtInitialCatalog.Text.Equals("") && !txtUserName.Text.Equals("")&& !txtPassword.Text.Equals("")) {
                e.CanExecute = true;
                return;
            }
            e.CanExecute = false;
        }

        private void VerbindenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Do nothing
        }

        private void NeuCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!txtDataSource.Text.Equals("") || !txtInitialCatalog.Text.Equals("") || !txtUserName.Text.Equals("") || !txtPassword.Text.Equals(""))
            {
                e.CanExecute = true;
                return;
            }
            e.CanExecute = false;
        }

        private void NeuCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Do nothing
        }

        private void LoeschenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (cboConnections.SelectedItem != null)
            {
                e.CanExecute = true;
                return;
            }
            e.CanExecute = false;
        }

        private void LoeschenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Do nothing
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
            //Wenn es sich um einen neuen Datensatz handelt, einfach addieren
            if (txtGuid.Text.Equals(""))
            {
                connectionDirectory.Add(new ConnectionDetails {MyGuid = "new", InitialCatalog = txtInitialCatalog.Text, DataSource = txtDataSource.Text, Username = txtUserName.Text, Password = txtPassword.Text });
            }
            //Die Daten verschlüsseln
            foreach (ConnectionDetails item in connectionDirectory.ConnectionsList)
            {
                //Unterscheiden ob es sich um einen neuen Datensatz oder Änderungen handelt oder ob einfach aus alter Datei übernommen
                //GUids stimmen überein --> Änderung
                if (item.MyGuid.Equals(txtGuid.Text))
                {
                    item.MyGuid = q.VerschlüssleText(item.MyGuid);
                    item.InitialCatalog = q.VerschlüssleText(txtInitialCatalog.Text);
                    item.DataSource = q.VerschlüssleText(txtDataSource.Text);
                    item.Username = q.VerschlüssleText(txtUserName.Text);
                    item.Password = q.VerschlüssleText(txtPassword.Text);
                }
                else if (item.MyGuid.Equals("new"))
                {
                    item.MyGuid = q.VerschlüssleText(Guid.NewGuid().ToString());
                    item.InitialCatalog = q.VerschlüssleText(txtInitialCatalog.Text);
                    item.DataSource = q.VerschlüssleText(txtDataSource.Text);
                    item.Username = q.VerschlüssleText(txtUserName.Text);
                    item.Password = q.VerschlüssleText(txtPassword.Text);
                }
                else
                {
                    item.MyGuid = q.VerschlüssleText(item.MyGuid);
                    item.InitialCatalog = q.VerschlüssleText(item.InitialCatalog);
                    item.DataSource = q.VerschlüssleText(item.DataSource);
                    item.Username = q.VerschlüssleText(item.Username);
                    item.Password = q.VerschlüssleText(item.Password);
                } 
            }
            //XML wegschreiben
            Serialize(connectionDirectory, _path);
            this.Close();
        }

        private void cboConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboConnections.SelectedItem != null) {
                string conName = ((ComboBoxItem)cboConnections.SelectedItem).Content.ToString();
                ConnectionDetails conDetails = (from ConnectionDetails cd in connectionDirectory.ConnectionsList where cd.InitialCatalog.Equals(conName) select cd).FirstOrDefault();
                txtGuid.Text = conDetails.MyGuid;
                txtInitialCatalog.Text = conDetails.InitialCatalog;
                txtDataSource.Text = conDetails.DataSource;
                txtUserName.Text = conDetails.Username;
                txtPassword.Text = conDetails.Password;
            }           
        }

        private void btnNeu_Click(object sender, RoutedEventArgs e)
        {
            txtGuid.Text = "";
            txtInitialCatalog.Text = "";
            txtDataSource.Text = "";
            txtUserName.Text = "";
            txtPassword.Text = "";
            cboConnections.SelectedItem = null;
        }

        private void btnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            ConnectionDetails ToRemove = null;
            foreach (ConnectionDetails item in connectionDirectory.ConnectionsList)
            {
                if (txtGuid.Text.Equals(item.MyGuid)) {
                    ToRemove = item;
                }
            }
            if (ToRemove != null) {
                connectionDirectory.Remove(ToRemove);
                //Die Daten verschlüsseln
                foreach (ConnectionDetails item in connectionDirectory.ConnectionsList)
                {
                        item.MyGuid = q.VerschlüssleText(item.MyGuid);
                        item.InitialCatalog = q.VerschlüssleText(item.InitialCatalog);
                        item.DataSource = q.VerschlüssleText(item.DataSource);
                        item.Username = q.VerschlüssleText(item.Username);
                        item.Password = q.VerschlüssleText(item.Password);
                }
                //XML wegschreiben
                Serialize(connectionDirectory, _path);
                this.Close();
            }
            
        }
        #endregion
    }


    public class ConnectionDetails
    {
        public string MyGuid { get; set; }
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

        public void Remove(ConnectionDetails elem)
        {
            ConnectionsList.Remove(elem);
        }
    }
}

