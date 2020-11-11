using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace DMS_Adminitration
{
    public class MyDynamicExport : DynamicObject
    {
        Dictionary<string, object> properties = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return properties.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            properties[binder.Name] = value;
            return true;
        }

        public Dictionary<string, object> GetProperties()
        {
            return properties;
        }
    }

    public class OkoDokTypTabellenfeldtypen {
        public string CsvWertetypen { get; set; }
        public string CsvFeldnamen { get; set; }
    }

    /// <summary>
    /// Interaktionslogik für DarstellungDokumente.xaml
    /// </summary>
    public partial class DarstellungDokumente : UserControl
    {
        public DarstellungDokumente()
        {
            InitializeComponent();
            okoDokTypTabellenfeldtypen = new OkoDokTypTabellenfeldtypen();
            Anwendungen = ((DbConnector)App.Current.Properties["Connector"]).ReadAnwendungen();
            suchfelder.ItemAdded += AddHandlerToTextBoxSuchfeld;
            ZeichneGrid();
            btnSyncStarten.IsEnabled = CanRequestNotifications();
        }

        //Ordnet Dateiendungen der richtigen ANwendung zu beim ANzeigen
        public List<Tuple<int, string, string>> Anwendungen { get; set; }

        OkoDokTypTabellenfeldtypen okoDokTypTabellenfeldtypen { get; set; }

        

        private void AddHandlerToTextBoxSuchfeld(object sender, SuchfeldAddedEventArgs e)
        {
            e.textbox.TextChanged += SuchfelderTextBoxTextChanged;
        }


        #region filter datagrid
        private void SuchfelderTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            dataTableForDataGrid.Rows.Clear();
            //DgFilter(((TextBox)sender).Name, ((TextBox)sender).Text);
            DgFilter();
        }

        private DataTable dataTableForDataGrid = new DataTable();
        private void DgFilter(string Feldname, string wert)
        {

            List<DataGridRow> rowsList = new List<DataGridRow>();
            var rows = DataGridHelper.GetDataGridRows(dgDokumente2);

            foreach (DataGridRow r in rows)
            {
                bool merkeRow = true;
                foreach (DataGridColumn column in dgDokumente.Columns)
                {
                    if (!dataTableForDataGrid.Columns.Contains(column.Header.ToString())) {
                        dataTableForDataGrid.Columns.Add(new DataColumn() { ColumnName = column.Header.ToString() });
                    }

                    if (column.Header.Equals(Feldname) && column.GetCellContent(r) is TextBlock)
                    {
                        TextBlock cellContent = column.GetCellContent(r) as TextBlock;
                        //Stimmt der Eintrag nicht mit dem Feld überein, merker auf false setzen, damit dir Row nicht in das Ergebnis einfließt
                        if (!cellContent.Text.Contains(wert)) {
                            //An dieser Stelle muss die Zeile aus dem DataGrid der DataTable hinzugefügt werden
                            merkeRow = false;   
                        }
                    }
                }
                //Row ist zu Ende geschrieben und vor allen DIngen auch die COlumns der Tabelle
                if (merkeRow) {
                    //Aus der DataGridRow eine DataROw machen:
                    dataTableForDataGrid.Rows.Add(CopyDataGridRowToDataRow(dataTableForDataGrid, r));
                }
                
            }
            dgTabelleOriginal.ItemsSource = null;
            dgTabelleOriginal.ItemsSource = dataTableForDataGrid.DefaultView;
        }


        private void DgFilter()
        {

            List<DataGridRow> rowsList = new List<DataGridRow>();
            var rows = DataGridHelper.GetDataGridRows(dgDokumente2);

            foreach (DataGridRow r in rows)
            {
                bool merkeRow = true;
                foreach (DataGridColumn column in dgDokumente.Columns)
                {
                    if (!dataTableForDataGrid.Columns.Contains(column.Header.ToString()))
                    {
                        dataTableForDataGrid.Columns.Add(new DataColumn() { ColumnName = column.Header.ToString() });
                    }

                    if (column.GetCellContent(r) is TextBlock)
                    {
                        string header = column.Header.ToString();
                        KeyValuePair<string, TextBox> kvpAktuelleBox = dicBezeichnungFeldUndTextBox.Where(x => x.Key.Equals(header)).FirstOrDefault();
                        if (kvpAktuelleBox.Value != null) { 
                            TextBox txtBox = kvpAktuelleBox.Value;
                            TextBlock cellContent = column.GetCellContent(r) as TextBlock;
                            //Stimmt der Eintrag nicht mit dem Feld überein, merker auf false setzen, damit dir Row nicht in das Ergebnis einfließt
                            if (!cellContent.Text.Contains(txtBox.Text))
                            {
                                //An dieser Stelle muss die Zeile aus dem DataGrid der DataTable hinzugefügt werden
                                merkeRow = false;
                            }
                        }
                    }
                }
                //Row ist zu Ende geschrieben und vor allen DIngen auch die COlumns der Tabelle
                if (merkeRow)
                {
                    //Aus der DataGridRow eine DataROw machen:
                    dataTableForDataGrid.Rows.Add(CopyDataGridRowToDataRow(dataTableForDataGrid, r));
                }

            }
            //dgDokumente.ItemsSource = null;
            //dgDokumente.ItemsSource = dataTableForDataGrid.DefaultView;
            dgTabelleOriginal.ItemsSource = null;
            dgTabelleOriginal.ItemsSource = dataTableForDataGrid.DefaultView;
        }




        private DataRow CopyDataGridRowToDataRow(DataTable table, DataGridRow row) {
            DataRow drReturner = table.NewRow();

            DataRowView drv = (DataRowView)(row.Item);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var value = drv.Row.ItemArray[i].ToString();

                drReturner[i] = value;
            }


            return drReturner;
        }
        #endregion

        public void ZeichneGrid() {
            //Alle aktuellen Daten sammeln
            Tuple<string, string> tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadDokTypTypesAndFields();
            okoDokTypTabellenfeldtypen.CsvFeldnamen = tuple.Item1;
            okoDokTypTabellenfeldtypen.CsvWertetypen = tuple.Item2;
            suchfelder.Fill();
            ZeichneDatagridForm();
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";
            if (e.PropertyType == typeof(System.Decimal))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "F2";
        }

        /// <summary>
        /// Das Feld beinhaltet bei Auswahl eines Datentyps die Feldbezeichnungen und Textfelder der Filterfelder, damit später über alle Felder gefiltert werden kann
        /// </summary>
        Dictionary<string, TextBox> dicBezeichnungFeldUndTextBox = new Dictionary<string, TextBox>();
        public void ZeichneDatagridForm()
        {            
                dgDokumente.Columns.Clear();
                ZeichneDatagridTab("OkoDokumentenTyp");
                suchfelder.grdMain.Children.Clear();
                suchfelder.Fill("OkoDokumentenTyp", out dicBezeichnungFeldUndTextBox);
        }

        public void ZeichneDatagridTab(string tabelle) {
            
            DataTable dtOriginal = new DataTable();
            DataTable dt = new DataTable();
            
            dtOriginal = ((DbConnector)App.Current.Properties["Connector"]).ReadTableData(tabelle);
            dt = ((DbConnector)App.Current.Properties["Connector"]).ReadTableDataWerteErsetztFuerDarstellungDokTyp();
            

            //DataGrid füllen
            //dgDokumente.AutoGenerateColumns = true;
            dgDokumente.ItemsSource = dt.DefaultView;
            dgDokumente2.ItemsSource = dt.DefaultView;
            foreach (DataGridColumn column in dgDokumente.Columns)
            {
                if (column.Header.Equals(tabelle + "Id"))
                {
                    column.Visibility = Visibility.Hidden;
                }
            }
            dgTabelleOriginal.ItemsSource = dt.DefaultView;
        }

        private void dgDokumente_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            if (dg.Items.Count > 0) {
                if (dg.Columns.Count > 0) {
                    string header = dg.Columns[0].Header.ToString();
                    if (header.Substring(header.Length - 2, 2).Equals("Id"))
                    {
                        dg.Columns[0].Visibility = Visibility.Hidden;
                    }
                }               
            }
            foreach (DataGridColumn column in dg.Columns)
            {
                if (column.Header.Equals("IdInTabelle") || column.Header.Equals("Tabelle") || column.Header.Equals("OkoDokumenteDatenId"))
                {
                    column.Visibility = Visibility.Hidden;
                }
            }
        }

        private void btnDokAnzeigen_Click(object sender, RoutedEventArgs e)
        {
            //Wird in MainWIndow behandelt
        }

        private List<int> DoksFuerExport = new List<int>();
        private void btnZumExportHinzu_Click(object sender, RoutedEventArgs e)
        {
            var row = ((DataRowView)dgDokumente.SelectedItem).Row.ItemArray;
            int counter = 0;
            foreach (DataGridColumn col in dgDokumente.Columns)
            {
                if (col.Header.Equals("OkoDokumenteDatenId")) {
                    if (!DoksFuerExport.Contains(Int32.Parse(row[counter].ToString()))) {
                        DoksFuerExport.Add(Int32.Parse(row[counter].ToString()));
                    }
                    
                }
                counter++;
            }
        }
       
        public void ExportFileToDisk(string varID, string path, string dateiname)
        {
            string _path = System.IO.Path.Combine(path, dateiname); 
            using (var sqlQuery = new SqlCommand(@"SELECT [Dokument] FROM [dbo].[OkoDokumente] WHERE [OkoDokumenteId] = @varID", ((DbConnector)App.Current.Properties["Connector"])._con))
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

        /// <summary>
        /// Notlösung, da mein COmmand nicht funktioniert wie es soll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgDokumente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int OkoDokumentendatenId = 0;

            if (dgDokumente != null && dgDokumente.SelectedItem != null)
            {
                var row = ((DataRowView)dgDokumente.SelectedItem).Row.ItemArray;
                int counter = 0;
                foreach (DataGridColumn col in dgDokumente.Columns)
                {
                    if (col.Header.Equals("OkoDokumenteDatenId"))
                    {
                        if (!DoksFuerExport.Contains(Int32.Parse(row[counter].ToString())))
                        {
                            OkoDokumentendatenId = Int32.Parse(row[counter].ToString());
                        }

                    }
                    counter++;
                }
            }
    }

        #region SqlDependency
        private const string tableName = "OkoDokumentenTyp";
        private const string dataSource = @"LAPTOP-CTMG3F1D\SQLEXPRESS";
        private const string database = "Dokumentenmanagement";
        // The following objects are reused
        // for the lifetime of the application.
        private DataSet dataToWatch = null;
        private SqlConnection connection = null;
        private SqlCommand command = null;

        private string GetConnectionString()
        {
            // To avoid storing the connection string in your code,
            // you can retrive it from a configuration file.
            return string.Format(@"Data Source={0};Integrated Security=true;Initial Catalog={1};Pooling=False;User Id=sa;Password=95hjh11!;", dataSource, database);
        }

        private string GetSQL()
        {
            // NOTE: IT IS VERY IMPORTANT TO USE THE 2 PART TABLE NAME 
            // WITH DBO (databaseowner) AS PREFIX
            // NOTE2: You must specify column names. The * wildcard does not work
            string sql = ((DbConnector)App.Current.Properties["Connector"]).GetSqlForDependency();
            //return "select tab1.Titel as Titel, tab1.Text as Text, tab1.Geburtstag as Geburtstag, tab2.IdInTabelle as Id, tab2.ErfasstAm as Erfasst, tab2.Dateiname as Dateiname from OkoDokumentenTyp tab1 INNER JOIN OkoDokumenteDaten as tab2 ON tab1.OkoDokumentenTypId = tab2.IdInTabelle";
            return "select "+sql+" from OkoDokumentenTyp tab1 INNER JOIN OkoDokumenteDaten as tab2 ON tab1.OkoDokumentenTypId = tab2.IdInTabelle";
        }

        private bool CanRequestNotifications()
        {
            // In order to use the callback feature of the
            // SqlDependency, the application must have
            // the SqlClientPermission permission.
            try
            {
                SqlClientPermission perm = new SqlClientPermission(PermissionState.Unrestricted);
                perm.Demand();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DispatcherTimer timer = new DispatcherTimer();
        private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            // This event will occur on a thread pool thread.
            // Updating the UI from a worker thread is not permitted.
            // The following code checks to see if it is safe to
            // update the UI.
            ISynchronizeInvoke i = new DispatcherSynchronizeInvoke(this.Dispatcher);

            // If InvokeRequired returns True, the code
            // is executing on a worker thread.
            if (i.InvokeRequired)
            {
                // Create a delegate to perform the thread switch.
                OnChangeEventHandler tempDelegate =
                    new OnChangeEventHandler(dependency_OnChange);

                object[] args = { sender, e };

                // Marshal the data from the worker thread
                // to the UI thread.
                i.BeginInvoke(tempDelegate, args);

                return;
            }

            // Remove the handler, since it is only good
            // for a single notification.
            SqlDependency dependency = (SqlDependency)sender;
            dependency.OnChange -= dependency_OnChange;

            // At this point, the code is executing on the
            // UI thread, so it is safe to update the UI.
            // Reload the dataset that is bound to the grid.

            if (!timer.IsEnabled) {               
                timer.Interval = new TimeSpan(0, 0, 5);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            GetData();
        }

        private void timerCallback(object state)
        {
            throw new NotImplementedException();
        }

        private void GetData()
        {
            // Empty the dataset so that there is only
            // one batch of data displayed.
            dataToWatch.Clear();

            // Make sure the command object does not already have
            // a notification object associated with it.
            command.Notification = null;

            // Create and bind the SqlDependency object
            // to the command object.
            SqlDependency dependency = new SqlDependency(command);
            dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);

            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(dataToWatch, tableName);

                //dataGridView1.DataSource = dataToWatch;
                //dataGridView1.DataMember = tableName;
                dgTabelleOriginal.ItemsSource = dataToWatch.Tables["OkoDokumentenTyp"].AsDataView();
                dgTabelleOriginal.DataContext = tableName;
            }
        }

        //private void button1_Click(object sender, EventArgs e)
        private void SqlDependencyStart()
        {
            // Remove any existing dependency connection, then create a new one.
            SqlDependency.Stop(GetConnectionString());
            SqlDependency.Start(GetConnectionString());

            if (connection == null)
            {
                connection = new SqlConnection(GetConnectionString());
            }

            if (command == null)
            {
                // GetSQL is a local procedure that returns
                // a paramaterized SQL string. You might want
                // to use a stored procedure in your application.
                command = new SqlCommand(GetSQL(), connection);
            }
            if (dataToWatch == null)
            {
                dataToWatch = new DataSet();
            }
            GetData();
        }

        //private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        private void SqlDependencyStop()
        {
            SqlDependency.Stop(GetConnectionString());
        }

        private void btnSyncStarten_Click(object sender, RoutedEventArgs e)
        {
            SqlDependencyStart();
        }

        private void btnSyncStoppen_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            SqlDependencyStop();
            ZeichneDatagridForm();
        }
        #endregion


    }
}
