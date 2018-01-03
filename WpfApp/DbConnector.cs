using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    class DbConnector
    {
        SqlConnection _con;
        string _conString;
 
        public DbConnector(String conString) {
            _conString = conString;
        }

        public bool Connect() {
            try
            {
                _con = new SqlConnection();
                _con.ConnectionString = _conString;
                _con.Open();
                return true;
            }
            catch (Exception ex)
            {
                //FehlerMessage = ex.InnerException.Message;
                return false;
            }
        }
        
        
        public bool CreateNewTable(string Tabellenname, Dictionary<string, string> Werte)
        {
            SqlCommand command = _con.CreateCommand();
            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            command.Connection = _con;
            command.Transaction = transaction;
            try {
                StringBuilder sbFeldtypen = new StringBuilder();
                StringBuilder sbFeldnamen = new StringBuilder();
                StringBuilder sb = new StringBuilder();
                sb.Append("CREATE TABLE [dbo].["+Tabellenname+"](");
                sb.Append("[" + Tabellenname + "Id] [int] IDENTITY(1,1) NOT NULL,");
                foreach (var item in Werte)
                {
                    sbFeldtypen.Append(item.Value.ToString() + ";");
                    sbFeldnamen.Append(item.Key.ToString() + ";");
                    sb.Append("[" + item.Key.ToString() + "] " + GetFieldTypes(item.Value.ToString()) + ",");
                }
                sb.Append("CONSTRAINT [PK_" + Tabellenname + "] PRIMARY KEY CLUSTERED");
                sb.Append("(");
                sb.Append("[" + Tabellenname + "Id] ASC");
                sb.Append(")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
                sb.Append(") ON [PRIMARY]");

                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();
                //Die Tabellenstruktur speichern
                command.CommandText = ("Insert into Tabellenfeldtypen (Tabellenname, CsvWertetypen, CsvFeldnamen) VALUES ('" + Tabellenname+ "','" + sbFeldtypen.ToString().Substring(0, sbFeldtypen.Length - 1) + "','" + sbFeldnamen.ToString().Substring(0, sbFeldnamen.Length - 1) + "')");
                command.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception e)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (SqlException ex)
                {
                    if (transaction.Connection != null)
                    {
                        Console.WriteLine("An exception of type " + ex.GetType() +
                            " was encountered while attempting to roll back the transaction.");
                    }
                }
                Console.WriteLine("An exception of type " + e.GetType() +
                    " was encountered while inserting the data.");
                Console.WriteLine("Neither record was written to database.");
            }
            return false;
        }

        public void Close() {
            _con.Close();
            _con.Dispose();
        }

        private string GetFieldTypes(string inputString) {
            string returner = "";
            switch (inputString) {
                case "intn":
                    returner = "[int] NULL";
                    break;
                case "decn":
                    returner = "[decimal](18, 5) NULL";
                    break;
                case "daten":
                    returner = "[datetime] NULL";
                    break;
                case "txt50n":
                    returner = "[varchar](50) NULL";
                    break;
                case "txt255n":
                    returner = "[varchar](255) NULL";
                    break;
                case "txtmn":
                    returner = "[varchar](MAX) NULL";
                    break;
                case "boln":
                    returner = "[bit] NULL";
                    break;
            }
            return returner;
        }

        /// <summary>
        /// Liest die Tabelle mit den Metainfomationen für die Datenbank ab
        /// </summary>
        /// <returns>Tuple of Strings. Tuple<Tabellenname, csvFeldtyp, csvFeldnamen></returns>
        public List<Tuple<string, string, string>> ReadTableNamesTypesAndFields() {
            List<Tuple<string, string, string>> liste = new List<Tuple<string, string, string>>();
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = _con;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM Tabellenfeldtypen";
             
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    var myT = Tuple.Create<string, string, string>(row.Field<string>(1), row.Field<string>(2), row.Field<string>(3));

                    liste.Add(myT);
                }
            }
            return liste;
        }

        public DataTable ReadTableData(string tabellenname) {
            DataTable dt = new DataTable();

            string query = "select * from " + tabellenname;
            SqlCommand cmd = new SqlCommand(query, _con);
            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);
            da.Dispose();

            return dt;
        }

        /// <summary>
        /// Fügt einer Tabelle einen neuen Eintrag hinzu
        /// </summary>
        /// <param name="tabellenname">Name der Tabelle</param>
        /// <param name="werte">Der String Key hält den Spaltennamen, value hält den Wert als object --> muss noch gecastet werden, siehe dazu csvWerteTypen</param>
        /// <param name="csvWerteTypen">Enthält die Typen der Werte. Zu den Angaben in diesem String werden die Werte gecastet.\n
        /// Bislang werden die Werte txt für varchar, dec für decimal(2), int für int und date für Datetime unterstützt.
        /// </param>
        /// <returns></returns>
        public int InsertTableData(string tabellenname, Dictionary<string, object> werte, string csvWerteTypen)
        {
            int neueId = 0;
            //Zunächst mal aus den Angaben einen SQL-String bauen
            StringBuilder sb = new StringBuilder();
            StringBuilder sbSpalten = new StringBuilder();
            StringBuilder sbWerte = new StringBuilder();
            int counter = 0;
            var csvArray = csvWerteTypen.Split(';');
            foreach (KeyValuePair<string, object> entry in werte)
            {
                if (csvArray[counter].Substring(0, 3).Equals("txt"))
                {
                    sbSpalten.Append(entry.Key + ",");
                    sbWerte.Append("'" + entry.Value.ToString().Replace("'", "''") + "',");
                }
                else if (csvArray[counter].Substring(0, 3).Equals("dec"))
                {
                    sbSpalten.Append(entry.Key + ",");
                    if (entry.Value.ToString().Equals(""))
                    {
                        sbWerte.Append("NULL,");
                    }
                    else
                    {
                        decimal val = Decimal.Parse(entry.Value.ToString());
                        sbWerte.Append(val.ToString().Replace(",", ".") + ",");
                    }
                }
                else if (csvArray[counter].Substring(0, 3).Equals("int"))
                {
                    sbSpalten.Append(entry.Key + ",");
                    if (entry.Value.ToString().Equals(""))
                    {
                        sbWerte.Append("NULL,");
                    }
                    else
                    {
                        sbWerte.Append(Int32.Parse(entry.Value.ToString()) + ",");
                    }
                }
                else if (csvArray[counter].Substring(0, 3).Equals("dat"))
                {
                    sbSpalten.Append(entry.Key + ",");
                    if (entry.Value == null || entry.Value.ToString().Equals(""))
                    {
                        sbWerte.Append("NULL,");
                    }
                    else
                    {
                        sbWerte.Append("'" + entry.Value.ToString() + "',");
                    }
                }
                else if (csvArray[counter].Substring(0, 3).Equals("bol"))
                {
                    sbSpalten.Append(entry.Key + ",");                   
                    sbWerte.Append("'" +entry.Value.ToString() + "',");
                }
                counter++;
            }
            string sqlSpalten = sbSpalten.ToString().Substring(0, sbSpalten.Length - 1);
            string sqlWerte = sbWerte.ToString().Substring(0, sbWerte.Length - 1);

            sb.Append("Insert into " + tabellenname + " (" + sqlSpalten + ") VALUES (" + sqlWerte + ")");

            SqlCommand command = _con.CreateCommand();
            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            command.Connection = _con;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();

                try
                {
                    command.CommandText = "SELECT ISNULL(MAX(" + tabellenname + "Id), 0) FROM " + tabellenname;
                    Int32.TryParse(command.ExecuteScalar().ToString(), out neueId);
                }
                catch (Exception innerEx)
                {
                    var test = innerEx.InnerException.Message;
                }


                transaction.Commit();
                return neueId;
            }
            catch (Exception e)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (SqlException ex)
                {
                    if (transaction.Connection != null)
                    {
                        Console.WriteLine("An exception of type " + ex.GetType() +
                            " was encountered while attempting to roll back the transaction.");
                    }
                }
                Console.WriteLine("An exception of type " + e.GetType() +
                    " was encountered while inserting the data.");
                Console.WriteLine("Neither record was written to database.");
            }
            return neueId;
        }

        public void InsertCsvData(string tabellenname, List<Dictionary<string, object>> lstWerte, string csvWerteTypen)
        {
            
            //Zunächst mal aus den Angaben einen SQL-String bauen
            StringBuilder sb = new StringBuilder();
            
            
            var csvArray = csvWerteTypen.Split(';');
            foreach (Dictionary<string, object> werte in lstWerte)
            {
                int counter = 0;
                StringBuilder sbSpalten = new StringBuilder();
                StringBuilder sbWerte = new StringBuilder();
                foreach (KeyValuePair<string, object> entry in werte)
                {
                    if (csvArray[counter].Substring(0, 3).Equals("txt"))
                    {
                        sbSpalten.Append(entry.Key + ",");
                        sbWerte.Append("'" + entry.Value.ToString().Replace("'", "''") + "',");
                    }
                    else if (csvArray[counter].Substring(0, 3).Equals("dec"))
                    {
                        sbSpalten.Append(entry.Key + ",");
                        if (entry.Value.ToString().Equals(""))
                        {
                            sbWerte.Append("NULL,");
                        }
                        else
                        {
                            decimal val = Decimal.Parse(entry.Value.ToString());
                            sbWerte.Append(val.ToString().Replace(",", ".") + ",");
                        }
                    }
                    else if (csvArray[counter].Substring(0, 3).Equals("int"))
                    {
                        sbSpalten.Append(entry.Key + ",");
                        if (entry.Value.ToString().Equals(""))
                        {
                            sbWerte.Append("NULL,");
                        }
                        else
                        {
                            sbWerte.Append(Int32.Parse(entry.Value.ToString()) + ",");
                        }
                    }
                    else if (csvArray[counter].Substring(0, 3).Equals("dat"))
                    {
                        sbSpalten.Append(entry.Key + ",");
                        if (entry.Value.ToString().Equals(""))
                        {
                            sbWerte.Append("NULL,");
                        }
                        else
                        {
                            sbWerte.Append("'" + entry.Value.ToString() + "',");
                        }
                    }
                    else if (csvArray[counter].Substring(0, 3).Equals("bol"))
                    {
                        sbSpalten.Append(entry.Key + ",");
                        sbWerte.Append("'" + (entry.Value.ToString().Equals("true") ? 1 : 0) + "',");
                    }
                    counter++;
                }
                string sqlSpalten = sbSpalten.ToString().Substring(0, sbSpalten.Length - 1);
                string sqlWerte = sbWerte.ToString().Substring(0, sbWerte.Length - 1);

                sb.Append("Insert into " + tabellenname + " (" + sqlSpalten + ") VALUES (" + sqlWerte + ");");

            }

            SqlCommand command = _con.CreateCommand();
            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            command.Connection = _con;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();
                transaction.Commit();                
            }
            catch (Exception e)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (SqlException ex)
                {
                    if (transaction.Connection != null)
                    {
                        Console.WriteLine("An exception of type " + ex.GetType() +
                            " was encountered while attempting to roll back the transaction.");
                    }
                }
                Console.WriteLine("An exception of type " + e.GetType() +
                    " was encountered while inserting the data.");
                Console.WriteLine("Neither record was written to database.");
            }
        }
        public void UpdateTableData(string tabellenname, int datensatzId, Dictionary<string, object> werte, string csvWerteTypen)
        {
            //SQL bauen
            StringBuilder sb = new StringBuilder();
            StringBuilder sbInner = new StringBuilder();

            //array für ABgleich der Wertetypen
            string[] arrayAbgleich = csvWerteTypen.Split(';');


            sb.Append("UPDATE " + tabellenname + " SET ");
            int counter = 0;
            foreach (var item in werte)
            {
                //Unterscheidung nach Feldtypen
                if (arrayAbgleich[counter].Substring(0, 3).Equals("dec"))
                {
                    if (!item.Value.ToString().Equals(""))
                    {
                        sbInner.Append(item.Key + "=" + item.Value.ToString().Replace(",", ".") + ",");
                    }
                    else {
                        sbInner.Append(item.Key + "=NULL,");
                    }
                }
                else if (arrayAbgleich[counter].Substring(0, 3).Equals("int"))
                {
                    if (!item.Value.ToString().Equals(""))
                    {
                        sbInner.Append(item.Key + "=" + Int32.Parse(item.Value.ToString()) + ",");
                    }
                    else
                    {
                        sbInner.Append(item.Key + "=NULL,");
                    }
                }
                else if (arrayAbgleich[counter].Substring(0, 3).Equals("dat"))
                {
                    if (!item.Value.ToString().Equals("")) {
                        sbInner.Append(item.Key + "='" + DateTime.Parse(item.Value.ToString()) + "',");
                    } else {
                        sbInner.Append(item.Key + "=NULL,");
                    }
                }
                else if (arrayAbgleich[counter].Substring(0, 3).Equals("bol"))
                {                   
                        sbInner.Append(item.Key + "='" + Boolean.Parse(item.Value.ToString()) + "',");
                }
                else {
                    //Ansonsten muss es ein String sein
                    sbInner.Append(item.Key +"='"+ item.Value.ToString().Replace("'","''") + "',");
                }
                
                
                counter++;
            }
            string txt = sbInner.ToString();
            txt = txt.Substring(0, txt.Length - 1);
            sb.Append(txt);
            sb.Append(" WHERE " + tabellenname +"Id = " +datensatzId);

            string sql = sb.ToString();
            SqlCommand command = _con.CreateCommand();
            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            command.Connection = _con;
            command.Transaction = transaction;
            command.CommandText = sql;
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        public void DeleteTableData(string tabellenname, int datensatzId)
        {
            string sql = "Delete from " + tabellenname + " Where " + tabellenname + "Id =" + datensatzId;

            SqlCommand command = _con.CreateCommand();
            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            command.Connection = _con;
            command.Transaction = transaction;
            command.CommandText = sql;
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        public void DeleteAllTableData(string tabellenname)
        {
            string sql = "Delete from " + tabellenname;

            SqlCommand command = _con.CreateCommand();
            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            command.Connection = _con;
            command.Transaction = transaction;
            command.CommandText = sql;
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        public void DeleteTable(string tabellenname)
        {
          
            string sql = "Drop Table " + tabellenname;
            string sql2 = "Delete from Tabellenfeldtypen where Tabellenname = '" + tabellenname + "'";

            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction

            SqlCommand command = _con.CreateCommand();
            
            command.Connection = _con;
            command.Transaction = transaction;
            command.CommandText = sql;
            command.ExecuteNonQuery();

            SqlCommand command2 = _con.CreateCommand();          
            command2.Connection = _con;
            command2.Transaction = transaction;
            command2.CommandText = sql2;
            command2.ExecuteNonQuery();
            transaction.Commit();
        }

    }
}
