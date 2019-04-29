using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DMS_Adminitration
{
    class DbConnector
    {
        public SqlConnection _con;
        string _conString;
 
        public DbConnector(String conString) {
            _conString = conString;
        }

        #region Anwendungen
        public List<Tuple<int, string, string>> ReadAnwendungen()
        {
            List<Tuple<int, string, string>> list = new List<Tuple<int, string, string>>();
            try
            {
                DataTable dt = new DataTable();

                string query = "Select * from OkoAnwendungen";

                SqlCommand cmd = new SqlCommand(query, _con);
                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dt);
                da.Dispose();
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(Tuple.Create(row.Field<int>(0), row.Field<string>(1), row.Field<string>(2)));
                }
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0033xx");
            }
            return list;
        }
        public void DeleteAnwendung(int datensatzId)
        {
            string sql = "Delete from OkoAnwendungen Where OkoAnwendungenId =" + datensatzId;

            SqlCommand command = _con.CreateCommand();
            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            command.Connection = _con;
            command.Transaction = transaction;
            command.CommandText = sql;
            try
            {
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0025ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0025xx");
            }
        }
        public void AnwendungEintragen(string _endung, string _dateiname)
        {

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
                command.CommandText = "Insert into OkoAnwendungen (Dateiendung, Anwendung) VALUES ('" + _endung + "', '" + _dateiname + "')";
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0027ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0027xx");
            }
        }
        #endregion


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
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0001xx");
                return false;
            }
        }
                
        public bool CreateNewTable(string Tabellenname, Dictionary<string, string> Werte, bool IsDokTypTabelle = false)
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
                    sbFeldnamen.Append(item.Key.ToString().Replace("|", "_") + ";");
                    sb.Append("[" + item.Key.ToString().Replace("|", "_") + "] " + GetFieldTypes(item.Value.ToString()) + ",");
                }
                sb.Append("CONSTRAINT [PK_" + Tabellenname + "] PRIMARY KEY CLUSTERED");
                sb.Append("(");
                sb.Append("[" + Tabellenname + "Id] ASC");
                sb.Append(")WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
                sb.Append(") ON [PRIMARY]");

                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();
                //Die Tabellenstruktur speichern
                if (IsDokTypTabelle) {
                    command.CommandText = ("Insert into OkoDokTypTabellenfeldtypen (Tabellenname, CsvWertetypen, CsvFeldnamen) VALUES ('" + Tabellenname + "','" + sbFeldtypen.ToString().Substring(0, sbFeldtypen.Length - 1) + "','" + sbFeldnamen.ToString().Substring(0, sbFeldnamen.Length - 1) + "')");
                }
                else {
                    command.CommandText = ("Insert into OkoTabellenfeldtypen (Tabellenname, CsvWertetypen, CsvFeldnamen) VALUES ('" + Tabellenname + "','" + sbFeldtypen.ToString().Substring(0, sbFeldtypen.Length - 1) + "','" + sbFeldnamen.ToString().Substring(0, sbFeldnamen.Length - 1) + "')");
                }

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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0003xx");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0002xx");
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
                case "look":
                    returner = "[int] NULL";
                    break;
            }
            return returner;
        }

        /// <summary>
        /// Schreibt den Datensatz in die Tabelle
        /// </summary>
        /// <param name="daten">Ein csv mit allen Werten in der Reihenfolge der Felder</param>
        /// <returns></returns>
        public void InsertDocumentData(string daten)
        {
            string[] _daten = daten.Split(';');
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
                command.CommandText = "Insert into OkoDokumenteDaten (OkoDokumenteId, IdInTabelle, Dateiname, ErfasstAm) VALUES (" + _daten[0] + ", " + _daten[1] + ", '" + _daten[2] + "', '" + _daten[3] + "')";
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0024ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0024xx");
            }
        }

        /// <summary>
        /// Testet, ob die Id des Datensatzes in der Stammdatentabelle als Nachschlagewert in OkoDokumentenTyp referenziert ist
        /// </summary>
        /// <param name="tabelle">Stammdatentabelle</param>
        /// <param name="id">Id der Stammdatentabelle</param>
        /// <returns>false wenn Datensatz nicht gelöscht werden darf</returns>
        public bool CheckIfIdLoeschbarStammdaten(string tabelle, int id) {
            try
            {
                DataTable dt0 = new DataTable();
                string query0 = "Select * from OkoDokumentenTyp;";
                SqlCommand cmd0 = new SqlCommand(query0, _con);
                SqlDataAdapter da0 = new SqlDataAdapter(cmd0);
                da0.Fill(dt0);
                da0.Dispose();

                DataTable dt = new DataTable();
                string query = "Select CsvFeldnamen from OkoDokTypTabellenfeldtypen Where OkoDokTypTabellenfeldtypenId = '1';";
                SqlCommand cmd = new SqlCommand(query, _con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
                foreach (DataRow row in dt.Rows)
                {
                    string text = row.Field<string>(0);
                    foreach (var item in text.Split(';'))
                    {
                        if (item.Contains("_"))
                        {
                            if (item.Split('_')[3].Equals(tabelle))
                            {
                                if (dt0.AsEnumerable().Where(x => x.Field<int>(item) == id).Count() > 0) {
                                    return false;
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0042xx");
            }
            return true;
        }


        /// <summary>
        /// Prüft ob Tabelle als Nachschlagefeld im Dokumententyp referenziert ist
        /// </summary>
        /// <param name="tabelle"></param>
        /// <returns>true wenn Daten gelöscht werden dürfen</returns>
        public bool CheckTableDeleteDataDokTyp(string tabelle)
        {
            DataTable dt = new DataTable();
            try
            {
                string query = "Select CsvFeldnamen from OkoDokTypTabellenfeldtypen Where OkoDokTypTabellenfeldtypenId = '1';";
                SqlCommand cmd = new SqlCommand(query, _con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0042xx");
            }
            string feldname = dt.Rows[0].Field<string>(0);
            foreach (var item in feldname.Split(';'))
            {
                if (item.Length > 2 && item.Substring(0, 3).Equals("_x_")) {
                    if (item.Split('_')[3].Equals(tabelle)) {
                        return false;
                    }
                }
            }
            return true;
        }

        public string DeleteDokumentendatensatz(int dokumenteDatenId)
        {
            //Aus DokumenteDaten die DOkumenteId ermitteln
            StringBuilder sb = new StringBuilder();


            DataTable dt = new DataTable();
            string query = "Select OkoDokumenteId, Tabelle, IdInTabelle from OkoDokumenteDaten where OkoDokumenteDatenId = " + dokumenteDatenId;
            SqlCommand cmd = new SqlCommand(query, _con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);
            da.Dispose();

            int aktuelleDokumenteId = dt.Rows[0].Field<int>(0);
            string aktuelleTabelle = dt.Rows[0].Field<string>(1);
            int aktuelleIdInTabelle = dt.Rows[0].Field<int>(2);
            //Nachschauen, ob noch weitere Referenzen auf das Dokument vorhanden sind

            DataTable dt2 = new DataTable();
            string query2 = "Select * from OkoDokumenteDaten where OkoDokumenteId = " + aktuelleDokumenteId;
            SqlCommand cmd2 = new SqlCommand(query2, _con);
            SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
            da2.Fill(dt2);
            da2.Dispose();

            sb.Append("Delete from OkoDokumenteDaten Where OkoDokumenteDatenId =" + dokumenteDatenId + ";");
            sb.Append("Delete from " + aktuelleTabelle + " Where " + aktuelleTabelle + "Id =" + aktuelleIdInTabelle + ";");

            if (dt2.Rows.Count == 1)
            {
                //Es ist kein weiterer Eintrag vorhanden, Dokument kann gelöscht werden
                sb.Append("Delete from OkoDokumente Where OkoDokumenteId =" + aktuelleDokumenteId + ";");
            }

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
            try
            {
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0026ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0026xx");
            }
            return aktuelleTabelle;
        }

        /// <summary>
        /// Liest die Tabelle mit den Metainfomationen für die Datenbank ab
        /// </summary>
        /// <returns>Tuple of Strings. Tuple aus Tabellenname, csvFeldtyp, csvFeldnamen</returns>
        public List<Tuple<string, string, string>> ReadTableNamesTypesAndFields() {
            
            List<Tuple<string, string, string>> liste = new List<Tuple<string, string, string>>();
            try { 
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = _con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM OkoTabellenfeldtypen";
             
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        var myT = Tuple.Create<string, string, string>(row.Field<string>(1), row.Field<string>(2), row.Field<string>(3));

                        liste.Add(myT);
                    }
                }
            }
            catch (Exception ex) {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0004xx");
                
            }
            return liste;
        }

        public void SpeichereDatenbearbeitungEinAus(string einAus) {
            SqlCommand command = _con.CreateCommand();
            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            command.Connection = _con;
            command.Transaction = transaction;
            try
            {
                command.CommandText = "UPDATE OkoEinstellungen SET DatenbearbeitungEinAus = '" + einAus + "' Where id = 1;";
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "aa0219yx");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0019xx");
            }
        }

        /// <summary>
        /// Prüft, ob zu löschende STammdatentabelle als Nachschlagefeld in anderer SDT referenziert ist
        /// </summary>
        /// <returns>true wenn die Tabelle NICHT!!! gelöscht werden darf</returns>
        public bool CheckReferenzen(string Tabelle)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = _con;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT CsvFeldnamen FROM OkoTabellenfeldtypen Where Tabellenname IN(SELECT name FROM Dokumentenmanagement.dbo.sysobjects WHERE xtype = 'U' AND NAME NOT LIKE 'Oko%' AND NAME NOT LIKE '"+Tabelle+"' UNION ALL Select 'OkoDokumententyp'); ";

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    string text = row.Field<string>(0);
                    foreach (var item in text.Split(';'))
                    {
                        if (item.Contains("_"))
                        {
                            if (item.Split('_')[3].Equals(Tabelle))
                            {
                                return true;
                            }
                        }
                    }
                                
                }
            }
            return false;
        }

        public List<Tuple<string, string, string>> ReadDokTyp()
        {
            List<Tuple<string, string, string>> liste = new List<Tuple<string, string, string>>();
            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = _con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM OkoDokumentenTyp";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        var myT = Tuple.Create<string, string, string>(row.Field<string>(1), row.Field<string>(2), row.Field<string>(3));

                        liste.Add(myT);
                    }
                }
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0004xx");

            }
            return liste;
        }

        public Tuple<List<string>, List<string>> ReadDataSuchfelder(string tabellenname)
        {

            DataTable dt = new DataTable();
            try
            {
                string query = "Select * from OkoDokTypTabellenfeldtypen Where OkoDokTypTabellenfeldtypenId = '1';";
                SqlCommand cmd = new SqlCommand(query, _con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0042xx");
            }
            string feldnamen = dt.Rows[0].Field<string>(3);
            string feldtypen = dt.Rows[0].Field<string>(2);

            feldnamen = feldnamen + ";Dateiname";
            feldtypen = feldtypen + ";txt50n";

            string[] _feldnamen = feldnamen.Split(';');
            string[] _feldtypen = feldtypen.Split(';');
            return Tuple.Create(_feldnamen.ToList(), _feldtypen.ToList());

        }

        /// <summary>
        /// Liest alle Daten zu Dokumentengruppen und Dokumententypen ein
        /// </summary>
        /// <returns>
        /// Tuple aus zwei weiteren Tuplen
        /// Item 1 --> Gruppen string, GruppenIds int, Gruppenbeschreibungen string
        /// Item 2 --> Typen string, TypenIds int, Typenbeschreibungen string, TypenGruppenIds int, TypenTabellen string
        /// </returns>
        public Tuple<Tuple<List<string>, List<int>, List<string>>, Tuple<List<string>, List<int>, List<string>, List<int>>> ReadDoksAndTypesData()
        {
            Tuple<List<string>, List<int>, List<string>> gruppenDaten;
            Tuple<List<string>, List<int>, List<string>, List<int>> typenDaten;
            //Item 1-3 Tuple1
            List<string> gruppen = new List<string>();
            List<int> gruppenIds = new List<int>();
            List<string> gruppenBeschreibungen = new List<string>();
            //Item 1-5 Tuple2
            List<string> typen = new List<string>();
            List<int> typenIds = new List<int>();
            List<string> typenBeschreibungen = new List<string>();
            List<int> typenGruppenIds = new List<int>();
            //List<string> typenTabellen = new List<string>();
            try { 
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = _con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM OkoDokumentengruppen";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        gruppen.Add(row.Field<string>(1));
                        gruppenIds.Add(row.Field<int>(0));
                        gruppenBeschreibungen.Add(row.Field<string>(2));
                    }
                }

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = _con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM OkoDokumententyp";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        typen.Add(row.Field<string>(1));
                        typenIds.Add(row.Field<int>(0));
                        typenBeschreibungen.Add(row.Field<string>(2));
                        typenGruppenIds.Add(row.Field<int>(3));
                    }
                }

                
            }
            catch (Exception ex) {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0005xx");
            }
            gruppenDaten = Tuple.Create(gruppen, gruppenIds, gruppenBeschreibungen);
            typenDaten = Tuple.Create(typen, typenIds, typenBeschreibungen, typenGruppenIds);
            return Tuple.Create(gruppenDaten, typenDaten);
        }

        public void ChangeTableStructure(string Tabelle, List<string> FelderLoeschen, List<EingabeTabellenfelder> FelderHinzufuegen)
        {
            List<string> csvFeldnamen = new List<string>();
            List<string> csvTypen = new List<string>();

            List<Tuple<string, string, string>> alleTabDaten = ReadTableNamesTypesAndFields();
            foreach (var item in alleTabDaten)
            {
                if (item.Item1.Equals(Tabelle))
                {
                    csvFeldnamen = item.Item3.Split(';').ToList();
                    csvTypen = item.Item2.Split(';').ToList();
                }
            }

            StringBuilder sb = new StringBuilder();

            foreach (var item in FelderLoeschen)
            {
                sb.Append("ALTER TABLE " + Tabelle + " DROP COLUMN " + item + " ;");
                int index = csvFeldnamen.IndexOf(item);
                csvFeldnamen.Remove(item);
                csvTypen.RemoveAt(index);
            }

            foreach (EingabeTabellenfelder item in FelderHinzufuegen)
            {
                string ersatztext = "";
                string txt = "";
                var strIn = item.comBoxFeldtyp.Text;
                if (strIn.Equals("Nachschlagefeld"))
                {
                    string tag = item.txtBezeichnung.Tag.ToString();
                    ersatztext = "|x|" + item.txtBezeichnung.Text + "|" + (tag.Replace("_", "|"));
                }
                var strOut = ((ComboBoxItem)item.comBoxFeldtyp.SelectedItem).Tag.ToString();
                if (ersatztext.Equals(""))
                {
                    txt = item.txtBezeichnung.Text;
                }
                else
                {
                    txt = ersatztext.Replace("|", "_");
                    ersatztext = "";
                }
                sb.Append("ALTER TABLE " + Tabelle + " ADD " + txt + " " + GetFieldTypes(strOut) + ";");
                csvFeldnamen.Add(txt);
                csvTypen.Add(strOut);
                txt = "";
            }

            StringBuilder sbCsvFelder = new StringBuilder();
            StringBuilder sbCsvTypen = new StringBuilder();
            for (int i = 0; i < csvFeldnamen.Count; i++)
            {
                sbCsvFelder.Append(csvFeldnamen[i] + ";");
                sbCsvTypen.Append(csvTypen[i] + ";");
            }

            string csvFelder = sbCsvFelder.ToString().Substring(0, sbCsvFelder.Length - 1);
            string csvTypes = sbCsvTypen.ToString().Substring(0, sbCsvTypen.Length - 1); ;

            sb.Append("UPDATE OkoTabellenfeldtypen SET CsvWertetypen = '" + csvTypes + "', CsvFeldnamen = '" + csvFelder + "' Where Tabellenname = '" + Tabelle + "';");
            //< CsvWertetypen, varchar(max),> ,[CsvFeldnamen] = < CsvFeldnamen, varchar(max),> WHERE < Suchbedingungen,,>);

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
            catch (Exception ex)
            {
                transaction.Rollback();
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0006xx");
            }
        }

        public Tuple<string, string> ReadDokTypTypesAndFields() {
            string feldnamen = "";
            string feldtypen = "";
            try
            {
                DataTable dt = new DataTable();
                string query = "select CsvFeldnamen, CsvWertetypen from OkoDokTypTabellenfeldtypen where OkoDokTypTabellenfeldtypenId = 1";
                SqlCommand cmd = new SqlCommand(query, _con);
                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dt);
                da.Dispose();
                foreach (DataRow row in dt.Rows)
                {
                    feldnamen = row.Field<string>(0);
                    feldtypen = row.Field<string>(1);
                }
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0007xx");
            }
            return Tuple.Create(feldnamen, feldtypen);
        }

        

        public void ChangeDokTypStructure(List<string> FelderLoeschen, List<EingabeTabellenfelder> FelderHinzufuegen)
        {
            List<string> csvFeldnamen = new List<string>();
            List<string> csvTypen = new List<string>();

            Tuple<string, string> alleTabDaten = ReadDokTypTypesAndFields();
            if (!alleTabDaten.Item1.Equals("")) {
                csvFeldnamen = alleTabDaten.Item1.Split(';').ToList();
                csvTypen = alleTabDaten.Item2.Split(';').ToList();
            }
                    
               

            StringBuilder sb = new StringBuilder();

            foreach (var item in FelderLoeschen)
            {
                sb.Append("ALTER TABLE OkoDokumentenTyp DROP COLUMN " + item + " ;");
                int index = csvFeldnamen.IndexOf(item);
                csvFeldnamen.Remove(item);
                csvTypen.RemoveAt(index);
            }

            foreach (EingabeTabellenfelder item in FelderHinzufuegen)
            {
                string ersatztext = "";
                string txt = "";
                var strIn = item.comBoxFeldtyp.Text;
                if (strIn.Equals("Nachschlagefeld"))
                {
                    string tag = item.txtBezeichnung.Tag.ToString();
                    ersatztext = "|x|" + item.txtBezeichnung.Text + "|" + (tag.Replace("_", "|"));
                }
                var strOut = ((ComboBoxItem)item.comBoxFeldtyp.SelectedItem).Tag.ToString();
                if (ersatztext.Equals(""))
                {
                    txt = item.txtBezeichnung.Text;
                }
                else
                {
                    txt = ersatztext.Replace("|", "_");
                    ersatztext = "";
                }
                sb.Append("ALTER TABLE OkoDokumentenTyp ADD " + txt + " " + GetFieldTypes(strOut) + ";");
                csvFeldnamen.Add(txt);
                csvTypen.Add(strOut);
                txt = "";
            }

            StringBuilder sbCsvFelder = new StringBuilder();
            StringBuilder sbCsvTypen = new StringBuilder();
            for (int i = 0; i < csvFeldnamen.Count; i++)
            {
                    sbCsvFelder.Append(csvFeldnamen[i] + ";");
                    sbCsvTypen.Append(csvTypen[i] + ";");
            }

            string csvFelder = "";
            string csvTypes = "";
            if (sbCsvFelder.Length > 0) {
                csvFelder = sbCsvFelder.ToString().Substring(0, sbCsvFelder.Length - 1);
                csvTypes = sbCsvTypen.ToString().Substring(0, sbCsvTypen.Length - 1); 
                }
            sb.Append("UPDATE OkoDokTypTabellenfeldtypen SET CsvWertetypen = '" + csvTypes + "', CsvFeldnamen = '" + csvFelder + "' Where Tabellenname = 'OkoDokumentenTyp';");
            
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
            catch (Exception ex)
            {
                transaction.Rollback();
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0006xx");
            }
        }

        public Tuple<List<int>, List<object>> ReadComboboxItems(string _tabelle, string _feld)
        {
            List<int> lstInt = new List<int>();
            List<object> lstObject = new List<object>();
            try { 
            DataTable dt = new DataTable();
            string query = "select "+ _tabelle +"Id, "+ _feld + " from " + _tabelle;
            SqlCommand cmd = new SqlCommand(query, _con);
            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);
            da.Dispose();
            foreach (DataRow row in dt.Rows)
            {
                lstInt.Add(row.Field<int>(0));
                lstObject.Add(row.Field<object>(1) != null ? row.Field<object>(1) : "");
            }
            }
            catch (Exception ex) {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0007xx");
            }
            return Tuple.Create(lstInt, lstObject);
        }

        public Tuple<List<int>, List<object>> ReadComboboxItems(string _tabelle, string _feld, int idInRefTabelle = 0, string feldIn_tabelle = "")
        {
            List<int> lstInt = new List<int>();
            List<object> lstObject = new List<object>();
            try
            {
                string query = "";
                DataTable dt = new DataTable();
                if (idInRefTabelle == 0)
                {
                    query = "select " + _tabelle + "Id, " + _feld + " from " + _tabelle;
                }
                else
                {
                    query = "select " + _tabelle + "Id, " + _feld + " from " + _tabelle + " WHERE " + feldIn_tabelle + "=" + idInRefTabelle;
                }


                SqlCommand cmd = new SqlCommand(query, _con);
                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dt);
                da.Dispose();
                foreach (DataRow row in dt.Rows)
                {
                    lstInt.Add(row.Field<int>(0));
                    lstObject.Add(row.Field<object>(1));
                }
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xxTT37xx");
            }
            return Tuple.Create(lstInt, lstObject);
        }

        public List<Exportdaten> ReadExportDaten(List<int> OkoDokumenteDatenIds)
        {
            List<Exportdaten> daten = new List<Exportdaten>();
            try
            {
                DataTable dt = new DataTable();
                StringBuilder sb = new StringBuilder();
                int counter = 0;
                foreach (var id in OkoDokumenteDatenIds)
                {
                    if (counter == 0)
                    {
                        sb.Append("tabDaten.OkoDokumenteDatenId = " + id);
                    }
                    else
                    {
                        sb.Append(" OR tabDaten.OkoDokumenteDatenId = " + id);
                    }

                    counter++;
                }
                string query = "select tabDaten.OkoDokumenteId, tabDaten.Dateiname, tabDaten.Titel, tabDaten.ErfasstAm, tabDaten.IdInTabelle, tabDaten.Tabelle, tabTypen.Bezeichnung, tabDaten.OkoDokumenteDatenId" +
                        " from OkoDokumenteDaten tabDaten INNER JOIN OkoDokumententyp tabTypen ON tabDaten.OkoDokumententypId = " +
                        "tabTypen.OkoDokumententypId AND (" + sb.ToString() + ");";
                SqlCommand cmd = new SqlCommand(query, _con);
                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dt);
                da.Dispose();
                foreach (DataRow item in dt.Rows)
                {
                    daten.Add(new Exportdaten()
                    {
                        DokumenteId = item.Field<int>(0),
                        Dateiname = item.Field<string>(1),
                        Titel = item.Field<string>(2),
                        ErfasstAm = item.Field<DateTime>(3),
                        IdInTabelle = item.Field<int>(4),
                        Tabelle = item.Field<string>(5),
                        DokumentenTyp = item.Field<string>(6),
                        OkoDokumenteDatenId = item.Field<int>(7)
                    });
                }
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0035xx");
            }
            return daten;
        }

        /// <summary>
        /// Liest die Originaldaten der Tabelle ein
        /// </summary>
        /// <param name="tabellenname">Einzulesende Tabelle</param>
        /// <returns></returns>
        public DataTable ReadTableData(string tabellenname) {
            DataTable dt = new DataTable();
            string query = "";
            try {
                if (tabellenname.Equals("OkoDokumentenTyp")) { 
                query = "select * from " + tabellenname + " tab1 INNER JOIN (Select IdInTabelle, Dateiname, ErfasstAm from OkoDokumenteDaten) as tab2 ON tab1." + tabellenname + "Id = tab2.IdInTabelle";

                //query = "select * from OkoDokumentenTyp tab1 INNER JOIN OkoDokumenteDaten as tab2 ON tab1.OkoDokumentenTypId = tab2.IdInTabelle WHERE tab2.IdInTabelle = 37";
                } else {
                    query = "select * from " + tabellenname;
                }
                SqlCommand cmd = new SqlCommand(query, _con);
                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.SelectCommand = cmd;
                da.Fill(dt);
            da.Dispose();
            }
            catch (Exception ex) {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0008xx");
            }
            return dt;
        }


        /// <summary>
        /// Dient dazu, eine Liste der Tabellennamen in den in den Dokumententypen referenzierten Tabellen zu erstellen
        /// </summary>
        /// <param name="tabellenname"></param>
        /// <returns></returns>
        public List<string> ReadAllDokTypenTabellenNamen()
        {
            List<string> returner = new List<string>();
            try { 
            DataTable dt = new DataTable();

            string query = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE  TABLE_TYPE = 'BASE TABLE'";
            SqlCommand cmd = new SqlCommand(query, _con);
            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);
            da.Dispose();
            List<string> _dokTyenTabellenNamen = new List<string>();
            foreach (DataRow drv in dt.Rows) {
                if (drv.ItemArray[2].ToString().Substring(0,3).Equals("xyx")) {
                    _dokTyenTabellenNamen.Add(drv.ItemArray[2].ToString());
                }
            }
            //Jetzt durch die Felder aller Reftabellen
            foreach (string tabName in _dokTyenTabellenNamen)
            {
                DataTable dt2 = new DataTable();
                //Für jede Tabelle alle Felder lesen und die Tabellennamen der _x_ Felder in Returner speichern
                string query2 = "SELECT column_name FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME ='" + tabName + "'";
                SqlCommand cmd2 = new SqlCommand(query2, _con);
                // create data adapter
                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                // this will query your database and return the result to your datatable
                da2.Fill(dt2);
                da2.Dispose();
      
                foreach (DataRow drv in dt2.Rows)
                {
                    if (drv.ItemArray[0].ToString().Substring(0, 3).Equals("_x_"))
                    {
                        returner.Add(drv.ItemArray[0].ToString().Split('_')[3]);
                    }
                }
            }
            returner = returner.Distinct().ToList();
            }
            catch (Exception ex) {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0009xx");
            }
            return returner;
        }

        /// <summary>
        /// Liest die Daten der Tabelle ein und ersetzt Referenzen zu anderen Tabellen mit den entsprechenden Werten für die Darstellung in Datagrid. Die Spaltenheader werden auch korrigiert
        /// </summary>
        /// <param name="tabellenname">Die einzulesende Tabelle</param>
        /// <returns>Tabellendaten mit referenzierten Nachschlagewerten</returns>
        public DataTable ReadTableDataWerteErsetztFuerDarstellung(string tabellenname)
        {
            string[] _csvWerteFeldnamen = { };
            string[] _csvWerteFeldnamenOriginal = { };
            string[] _csvWerteFeldTypen = { };
            List<Tuple<List<int>, List<object>>> _nachschlageFelderWerte = new List<Tuple<List<int>, List<object>>>();

            //Zuerst brauche ich die Feldtypen der Tabellenfelder und die Namen der Felder
            List <Tuple<string, string, string>> Werte = ReadTableNamesTypesAndFields();
            foreach (var item in Werte)
            {
                if (item.Item1.Equals(tabellenname)) {
                    string _namen = tabellenname + "Id;" + item.Item3;
                    string _typen = "int;" + item.Item2;
                    _csvWerteFeldnamen = _namen.Split(';');
                    _csvWerteFeldnamenOriginal = _namen.Split(';');
                    _csvWerteFeldTypen = _typen.Split(';');
                }
            }
            //Originaldaten einlesen
            DataTable dt = new DataTable();
            DataTable dtCopy = new DataTable();
            try { 
            //Die Header und DataTypes für die Kopie festlegen;
            for (int i = 0; i < _csvWerteFeldTypen.Length; i++)
            {
                string txt = _csvWerteFeldnamen[i];
                if (_csvWerteFeldnamen[i].Contains("_x_")){
                    //Nachschlagefelder für die spätere Vearbeitung in dieser Methdode merken                    
                    _nachschlageFelderWerte.Add(ReadComboboxItems(_csvWerteFeldnamen[i].Split('_')[3], _csvWerteFeldnamen[i].Split('_')[4]));
                    txt = txt.Split('_')[2];
                    _csvWerteFeldnamen[i] = txt;
                }
                DataColumn column = new DataColumn(txt);
                switch (_csvWerteFeldTypen[i].Substring(0, 3))
                {
                    case "int": column.DataType = typeof(int); break;
                    case "dat": column.DataType = typeof(DateTime); break;
                    case "dec": column.DataType = typeof(decimal); break;
                    case "bol": column.DataType = typeof(bool); break;
                    case "loo": column.DataType = typeof(string); break;
                    case "txt": column.DataType = typeof(string); break;
                }
                dtCopy.Columns.Add(column);
            }

            string query = "select * from " + tabellenname;
            SqlCommand cmd = new SqlCommand(query, _con);
            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);
            da.Dispose();
            //Reihe für Reihe
            //int counter = 0;
            foreach (DataRow row in dt.Rows)
            {
                DataRow rowCopy = dtCopy.NewRow();
                //Spalte für Spalte
                int _nachschlageZaehler = 0;
                for (int i = 0; i < _csvWerteFeldTypen.Length; i++)
                {
                    switch (_csvWerteFeldTypen[i].Substring(0, 3))
                    {
                        case "int":
                            rowCopy[_csvWerteFeldnamen[i]] = row.Field<int?>(i) != null ? row.Field<int?>(i) : 0; break;
                        case "dat":
                            if (row.Field<DateTime?>(i) != null)
                            {
                                rowCopy[_csvWerteFeldnamen[i]] = row.Field<DateTime?>(i);
                            }
                            else {
                                rowCopy[_csvWerteFeldnamen[i]] = DBNull.Value;
                            }
                            break;
                        case "dec":
                            rowCopy[_csvWerteFeldnamen[i]] = row.Field<decimal?>(i) != null ? row.Field<decimal?>(i) : 0; break;
                        case "bol":
                            rowCopy[_csvWerteFeldnamen[i]] = row.Field<bool?>(i);
                            break;
                        case "loo":
                            //Ich brauche eine Liste der Felder und Ids
                            Tuple<List<int>, List<object>> tuple = _nachschlageFelderWerte.ElementAt(_nachschlageZaehler);
                            //Index ermitteln
                            if (row.Field<int?>(i) != null)
                            {
                                int index = tuple.Item1.IndexOf(row.Field<int>(i));
                                if (index >= 0) {
                                    string wert = tuple.Item2.ElementAt(index).ToString();
                                    rowCopy[_csvWerteFeldnamen[i]] = wert;
                                } else {
                                    rowCopy[_csvWerteFeldnamen[i]] = "";
                                }
                                

                            }
                            else {
                                rowCopy[_csvWerteFeldnamen[i]] = "";
                            }
                            _nachschlageZaehler++;
                            break;
                        case "txt":
                            rowCopy[_csvWerteFeldnamen[i]] = row.Field<string>(i); break;
                    }
                }
                dtCopy.Rows.Add(rowCopy);
            }
            }
            catch (Exception ex) {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0010xx");
            }
            return dtCopy;
        }

        public DataTable ReadTableDataWerteErsetztFuerDarstellungDokTyp()
        {
            string tabellenname = "OkoDokumentenTyp";
            string[] _csvWerteFeldnamen = { };
            string[] _csvWerteFeldnamenOriginal = { };
            string[] _csvWerteFeldTypen = { };
            List<Tuple<List<int>, List<object>>> _nachschlageFelderWerte = new List<Tuple<List<int>, List<object>>>();

            //Zuerst brauche ich die Feldtypen der Tabellenfelder und die Namen der Felder
            Tuple<string, string> Werte = ReadDokTypTypesAndFields();
           
                    string _namen = tabellenname + "Id;" + Werte.Item1 + ";Dateiname";
                    string _typen = "int;" + Werte.Item2 + ";txt50n";
                    _csvWerteFeldnamen = _namen.Split(';');
                    _csvWerteFeldnamenOriginal = _namen.Split(';');
                    _csvWerteFeldTypen = _typen.Split(';');
          
            //Originaldaten einlesen
            DataTable dt = new DataTable();
            DataTable dtCopy = new DataTable();
            try
            {
                //Die Header und DataTypes für die Kopie festlegen;
                for (int i = 0; i < _csvWerteFeldTypen.Length; i++)
                {
                    string txt = _csvWerteFeldnamen[i];
                    if (_csvWerteFeldnamen[i].Contains("_x_"))
                    {
                        //Nachschlagefelder für die spätere Vearbeitung in dieser Methdode merken                    
                        _nachschlageFelderWerte.Add(ReadComboboxItems(_csvWerteFeldnamen[i].Split('_')[3], _csvWerteFeldnamen[i].Split('_')[4]));
                        txt = txt.Split('_')[2];
                        _csvWerteFeldnamen[i] = txt;
                    }
                    DataColumn column = new DataColumn(txt);
                    switch (_csvWerteFeldTypen[i].Substring(0, 3))
                    {
                        case "int": column.DataType = typeof(int); break;
                        case "dat": column.DataType = typeof(DateTime); break;
                        case "dec": column.DataType = typeof(decimal); break;
                        case "bol": column.DataType = typeof(bool); break;
                        case "loo": column.DataType = typeof(string); break;
                        case "txt": column.DataType = typeof(string); break;
                    }
                    dtCopy.Columns.Add(column);
                }

                //string query = "select * from " + tabellenname;
                string query = "select * from OkoDokumentenTyp tab1 INNER JOIN OkoDokumenteDaten as tab2 ON tab1.OkoDokumentenTypId = tab2.IdInTabelle";
                SqlCommand cmd = new SqlCommand(query, _con);
                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dt);
                da.Dispose();
                //Die überflüssigen Columns entfernen
                List<DataColumn> liste = new List<DataColumn>();
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ColumnName.Equals("IdInTabelle")
                        || column.ColumnName.Equals("OkoDokumenteDatenId")
                        || column.ColumnName.Equals("OkoDokumenteId")
                        || column.ColumnName.Equals("ErfasstAm")
                        ) {
                        liste.Add(column);
                    }
                }
                foreach (var item in liste)
                {
                    dt.Columns.Remove(item);
                }

                //Reihe für Reihe
                //int counter = 0;
                foreach (DataRow row in dt.Rows)
                {
                    DataRow rowCopy = dtCopy.NewRow();
                    //Spalte für Spalte
                    int _nachschlageZaehler = 0;
                    for (int i = 0; i < _csvWerteFeldTypen.Length; i++)
                    {
                        switch (_csvWerteFeldTypen[i].Substring(0, 3))
                        {
                            case "int":
                                rowCopy[_csvWerteFeldnamen[i]] = row.Field<int?>(i) != null ? row.Field<int?>(i) : 0; break;
                            case "dat":
                                if (row.Field<DateTime?>(i) != null)
                                {
                                    rowCopy[_csvWerteFeldnamen[i]] = row.Field<DateTime?>(i);
                                }
                                else
                                {
                                    rowCopy[_csvWerteFeldnamen[i]] = DBNull.Value;
                                }
                                break;
                            case "dec":
                                rowCopy[_csvWerteFeldnamen[i]] = row.Field<decimal?>(i) != null ? row.Field<decimal?>(i) : 0; break;
                            case "bol":
                                rowCopy[_csvWerteFeldnamen[i]] = row.Field<bool?>(i);
                                break;
                            case "loo":
                                //Ich brauche eine Liste der Felder und Ids
                                Tuple<List<int>, List<object>> tuple = _nachschlageFelderWerte.ElementAt(_nachschlageZaehler);
                                //Index ermitteln
                                if (row.Field<int?>(i) != null)
                                {
                                    int index = tuple.Item1.IndexOf(row.Field<int>(i));
                                    if (index >= 0)
                                    {
                                        string wert = tuple.Item2.ElementAt(index).ToString();
                                        rowCopy[_csvWerteFeldnamen[i]] = wert;
                                    }
                                    else
                                    {
                                        rowCopy[_csvWerteFeldnamen[i]] = "";
                                    }


                                }
                                else
                                {
                                    rowCopy[_csvWerteFeldnamen[i]] = "";
                                }
                                _nachschlageZaehler++;
                                break;
                            case "txt":
                                rowCopy[_csvWerteFeldnamen[i]] = row.Field<string>(i); break;
                        }
                    }
                    dtCopy.Rows.Add(rowCopy);
                }
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0010xx");
            }
            return dtCopy;
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
                else if (csvArray[counter].Substring(0, 3).Equals("loo"))
                {
                    sbSpalten.Append(entry.Key + ",");
                    if (entry.Value == null || entry.Value.ToString().Equals(""))
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0011ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0011xx");
            }
            return neueId;
        }

        public int InsertTableData(string tabellenname, Dictionary<string, object> werte, string csvWerteTypen, bool IsDokType = false)
        {
            int neueId = 0;
            //Zunächst mal aus den Angaben einen SQL-String bauen
            StringBuilder sb = new StringBuilder();
            StringBuilder sbSpalten = new StringBuilder();
            StringBuilder sbWerte = new StringBuilder();
            int counter = 0;
            var csvArray = csvWerteTypen.Split(';');
            try
            {
                foreach (KeyValuePair<string, object> entry in werte)
                {
                    string txt = csvArray[counter].Substring(0, 3);
                    if (txt.Equals("txt"))
                    {
                        sbSpalten.Append(entry.Key + ",");
                        sbWerte.Append("'" + entry.Value.ToString().Replace("'", "''") + "',");
                    }
                    else if (txt.Equals("dec"))
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
                    else if (txt.Equals("int"))
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
                    else if (txt.Equals("loo"))
                    {
                        sbSpalten.Append(entry.Key + ",");
                        if (entry.Value == null || entry.Value.ToString().Equals(""))
                        {
                            sbWerte.Append("NULL,");
                        }
                        else
                        {
                            sbWerte.Append(Int32.Parse(entry.Value.ToString()) + ",");
                        }
                    }
                    else if (txt.Equals("dat"))
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
                    else if (txt.Equals("bol"))
                    {
                        sbSpalten.Append(entry.Key + ",");
                        sbWerte.Append("'" + entry.Value.ToString() + "',");
                    }
                    counter++;
                }
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0031xx");
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
                    Fehlerbehandlung.Error(innerEx.StackTrace.ToString(), innerEx.Message, "xx0022xx");
                }


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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0023ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0023xx");
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0012ax");
                        
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0012xx");
                
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
                    if (item.Value != null && !item.Value.ToString().Equals("")) {
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
                    if (item.Value != null) {
                        sbInner.Append(item.Key + "='" + item.Value.ToString().Replace("'", "''") + "',");
                    } else {
                        sbInner.Append(item.Key + "='',");
                    }
                    
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
            try { 
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0013ax");
                        
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0013xx");
                
            }
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
            try { 
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0014ax");
                        
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0014xx");
                
            }
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
            try { 
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
                            Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0015ax");
                        }
                    }
                    Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0015xx");
                }
         }

        public void DeleteTable(string tabellenname)
        {
          
            string sql = "Drop Table " + tabellenname;
            string sql2 = "Delete from OkoTabellenfeldtypen where Tabellenname = '" + tabellenname + "'";

            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            // Must assign both transaction object and connection
            // to Command object for a pending local transaction

            SqlCommand command = _con.CreateCommand();
            SqlCommand command2 = _con.CreateCommand();


            try { 
            command.Connection = _con;
            command.Transaction = transaction;
            command.CommandText = sql;
            command.ExecuteNonQuery();

                
            command2.Connection = _con;
            command2.Transaction = transaction;
            command2.CommandText = sql2;
            command2.ExecuteNonQuery();
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0016ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0016xx");
            }
        }

        public void AddDokGruppe(string bezeichnung, string beschreibung) {           
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
                command.CommandText = "Insert into OkoDokumentengruppen (Bezeichnung, Beschreibung) VALUES ('" + bezeichnung + "', '" + beschreibung + "')";
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0017ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0017xx");
            }

        }

        public void UpdateDokGruppe(string bezeichnung, string beschreibung, int id) {
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
                command.CommandText = "UPDATE OkoDokumentengruppen SET Bezeichnung = '" + bezeichnung + "', Beschreibung = '" + beschreibung + "' Where OkoDokumentengruppenId = '" + id + "';";
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0018ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0018xx");
            }
        }

        public void UpdateDokTyp(string bezeichnung, string beschreibung, int id, int dokGruppenId)
        {
            SqlCommand command = _con.CreateCommand();
            SqlTransaction transaction;
            // Start a local transaction.
            transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
            command.Connection = _con;
            command.Transaction = transaction;
            try
            {
                command.CommandText = "UPDATE OkoDokumententyp SET Bezeichnung = '" + bezeichnung + "', Beschreibung = '" + beschreibung + "', OkoDokumentengruppenId = '" + dokGruppenId + "' Where OkoDokumententypId = '" + id + "';";
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0019ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0019xx");
            }
        }

        public void AddDokTyp(string bezeichnung, string beschreibung, int gruppenId) {
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
                command.CommandText = "Insert into OkoDokumententyp (Bezeichnung, Beschreibung, OkoDokumentengruppenId) VALUES ('" + bezeichnung + "', '" + beschreibung + "', "+gruppenId+")";
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
                        Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0020ax");
                    }
                }
                Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xx0020xx");
            }
        }

        public void OrdnerSpeichern(string pfad) {
            if (!pfad.Equals("")) {
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
                    command.CommandText = "UPDATE OkoEinstellungen SET Ordner = '" + pfad + "' Where id = 1;";
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
                            Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xy0018bb");
                        }
                    }
                    Fehlerbehandlung.Error(e.StackTrace.ToString(), e.Message, "xy0018bb");
                }
            }
        }

        public Tuple<string, string> ReadEinstellungen() {
            string pfad = "";
            string dokumentenbearbeitungEinAus = "";
            try
            {
                DataTable dt = new DataTable();
                string query = "select Ordner, DatenbearbeitungEinAus from OkoEinstellungen where id = 1";
                SqlCommand cmd = new SqlCommand(query, _con);
                // create data adapter
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                // this will query your database and return the result to your datatable
                da.Fill(dt);
                da.Dispose();
                foreach (DataRow row in dt.Rows)
                {
                    pfad = row.Field<string>(0);
                    dokumentenbearbeitungEinAus = row.Field<string>(1);
                }
            }
            catch (Exception ex)
            {
                Fehlerbehandlung.Error(ex.StackTrace.ToString(), ex.Message, "xx0007xx");
            }
            return Tuple.Create(pfad, dokumentenbearbeitungEinAus);
        }
    }
}
