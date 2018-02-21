using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfAppDMS
{
    class DbConnector
    {
        public SqlConnection _con;
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
                
        
        public void Close() {
            _con.Close();
            _con.Dispose();
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
                    sbWerte.Append("'" + entry.Value.ToString() + "',");
                }
                counter++;
            }
            string sqlSpalten = sbSpalten.ToString().Substring(0, sbSpalten.Length - 1);
            string sqlWerte = sbWerte.ToString().Substring(0, sbWerte.Length - 1);
            if (IsDokType) {
                sb.Append("Insert into xyx" + tabellenname + " (" + sqlSpalten + ") VALUES (" + sqlWerte + ")");
            } else {
                sb.Append("Insert into " + tabellenname + " (" + sqlSpalten + ") VALUES (" + sqlWerte + ")");
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

                try
                {
                    if (IsDokType)
                    { command.CommandText = "SELECT ISNULL(MAX(xyx" + tabellenname + "Id), 0) FROM xyx" + tabellenname; }
                    else { command.CommandText = "SELECT ISNULL(MAX(" + tabellenname + "Id), 0) FROM " + tabellenname; }
                        
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
                command.CommandText = "Insert into OkoDokumenteDaten (OkoDokumenteId, OkoDokumentenTypId, Tabelle, IdInTabelle, Titel, Beschreibung, Dateiname, ErfasstAm) VALUES (" + _daten[0] + ", " + _daten[1] + ", '" + _daten[2] + "', " + _daten[3] + ", '" + _daten[4] + "', '" + _daten[5] + "', '" + _daten[6] + "', '" + _daten[7] + "')";
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
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        public string DeleteDokumentendatensatz(int dokumenteDatenId)
        {
            //Aus DokumenteDaten die DOkumenteId ermitteln
            DataTable dt = new DataTable();
            string query = "Select OkoDokumenteId, Tabelle, IdInTabelle from OkoDokumenteDaten where OkoDokumenteDatenId = " + dokumenteDatenId;
            SqlCommand cmd = new SqlCommand(query, _con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);
            da.Dispose();
            int aktuelleDokumenteId =  dt.Rows[0].Field<int>(0);
            string aktuelleTabelle = dt.Rows[0].Field<string>(1);
            int aktuelleIdInTabelle = dt.Rows[0].Field<int>(2);
            //Nachschauen, ob noch weitere Referenzen auf das Dokument vorhanden sind
            DataTable dt2 = new DataTable();
            string query2 = "Select * from OkoDokumenteDaten where OkoDokumenteId = " + aktuelleDokumenteId;
            SqlCommand cmd2 = new SqlCommand(query2, _con);
            SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
            da2.Fill(dt2);
            da2.Dispose();

            StringBuilder sb = new StringBuilder();
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
            command.ExecuteNonQuery();
            transaction.Commit();
            return aktuelleTabelle;
        }

        public List<Tuple<int, string, string>> ReadAnwendungen()
        {
            List<Tuple<int, string, string>> list = new List<Tuple<int, string, string>>();
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
           
            return list;
        }

        public int ReadIdDokument(string idInTabelle, string tabelle, string dateiname)
        {
            
            DataTable dt = new DataTable();

            string query = "Select OkoDokumenteId from OkoDokumenteDaten where IdInTabelle = " + idInTabelle + " and Tabelle = '" + tabelle + "' and Dateiname = '" + dateiname + "'";

            SqlCommand cmd = new SqlCommand(query, _con);
            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);
            da.Dispose();
            return dt.Rows[0].Field<int>(0);
        }

        public List<Exportdaten> ReadExportDaten(List<int> OkoDokumenteDatenIds)
        {
            List<Exportdaten> daten = new List<Exportdaten>();
            DataTable dt = new DataTable();
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            foreach (var id in OkoDokumenteDatenIds)
            {
                if (counter == 0) {
                    sb.Append("tabDaten.OkoDokumenteDatenId = " + id);
                } else {
                    sb.Append(" OR tabDaten.OkoDokumenteDatenId = " + id);
                }
                
                counter++;
            }
            string query = "select tabDaten.OkoDokumenteId, tabDaten.Dateiname, tabDaten.Titel, tabDaten.ErfasstAm, tabDaten.IdInTabelle, tabDaten.Tabelle, tabTypen.Bezeichnung, tabDaten.OkoDokumenteDatenId" +
                    " from OkoDokumenteDaten tabDaten INNER JOIN OkoDokumententyp tabTypen ON tabDaten.OkoDokumententypId = " +
                    "tabTypen.OkoDokumententypId AND ("+sb.ToString()+");";
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
            
            return daten;
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
                        Console.WriteLine("An exception of type " + ex.GetType() +
                            " was encountered while attempting to roll back the transaction.");
                    }
                }
                Console.WriteLine("An exception of type " + e.GetType() +
                    " was encountered while inserting the data.");
                Console.WriteLine("Neither record was written to database.");
            }
        }

        /// <summary>
        /// GIbt die Daten nur anders aus....
        /// </summary>
        /// <returns></returns>
        public Tuple<Dictionary<int,string>, List<OkoDokumententyp>, List<string>, List<int>, Dictionary<int, OkoDokTypTabellenfeldtypen>>  ReadAllDataDarstellungDokumente()
        {
            Dictionary<int, string> dicGruppen = new Dictionary<int, string>();
            List<OkoDokumententyp> dicTypen = new List<OkoDokumententyp>();
            List<string> AlleDokumententypenBezeichnungen = new List<string>();
            List<int> AlleDokumententypenIds = new List<int>();
            Dictionary<int, OkoDokTypTabellenfeldtypen> okoDOkTypTabellenfeldtypen = new Dictionary<int, OkoDokTypTabellenfeldtypen>();

            Tuple<Tuple<List<string>, List<int>, List<string>>, Tuple<List<string>, List<int>, List<string>, List<int>, List<string>>> data = ReadDoksAndTypesData();

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
                    dicGruppen.Add(row.Field<int>(0), row.Field<string>(1));
                }
            }
            foreach (KeyValuePair<int, string> kvp in dicGruppen.OrderBy(kvp => kvp.Value));

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
                    dicTypen.Add(new OkoDokumententyp { OkoDokumententypId = row.Field<int>(0), Bezeichnung = row.Field<string>(1), Beschreibung = row.Field<string>(2), OkoDokumentengruppenId = row.Field<int>(3) });
                    AlleDokumententypenBezeichnungen.Add(row.Field<string>(1));
                    AlleDokumententypenIds.Add(row.Field<int>(0));
                }
            }
            foreach (OkoDokumententyp kvp in dicTypen.OrderBy(kvp => kvp.Bezeichnung)) ;

            //Nun noch die Daten zu den einzelnen DOkumententypen einsammeln
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = _con;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM OkoDokTypTabellenfeldtypen";

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    //int IdDesDOkumententyps = AlleDokumententypenIds.ElementAt(
                    string test = row.Field<string>(1);
                    test = test.Substring(3, test.Length - 3);
                    int elementAt =  AlleDokumententypenBezeichnungen.IndexOf(test);
                    int IdDesDOkumententyps = AlleDokumententypenIds.ElementAt(elementAt);
                    //Wir nehmen hier als Id die GruppenId, um das Ding in der Anzeige filtern zu können
                    okoDOkTypTabellenfeldtypen.Add(IdDesDOkumententyps, new OkoDokTypTabellenfeldtypen { Tabellenname = row.Field<string>(1), CsvFeldnamen = row.Field<string>(3), CsvWertetypen = row.Field<string>(2) });
                    
                }
            }
            return Tuple.Create(dicGruppen, dicTypen, AlleDokumententypenBezeichnungen, AlleDokumententypenIds, okoDOkTypTabellenfeldtypen);
        }


        public Tuple<Tuple<List<string>, List<int>, List<string>>, Tuple<List<string>, List<int>, List<string>, List<int>, List<string>>> ReadDoksAndTypesData()
        {
            Tuple<List<string>, List<int>, List<string>> gruppenDaten;
            Tuple<List<string>, List<int>, List<string>, List<int>, List<string>> typenDaten;
            //Item 1-3 Tuple1
            List<string> gruppen = new List<string>();
            List<int> gruppenIds = new List<int>();
            List<string> gruppenBeschreibungen = new List<string>();
            //Item 1-5 Tuple2
            List<string> typen = new List<string>();
            List<int> typenIds = new List<int>();
            List<string> typenBeschreibungen = new List<string>();
            List<int> typenGruppenIds = new List<int>();
            List<string> typenTabellen = new List<string>();

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

            gruppenDaten = Tuple.Create(gruppen, gruppenIds, gruppenBeschreibungen);
            typenDaten = Tuple.Create(typen, typenIds, typenBeschreibungen, typenGruppenIds, typenTabellen);
            return Tuple.Create(gruppenDaten, typenDaten);
        }

        public void UpdateTableData(string tabellenname, int datensatzId, Dictionary<string, object> werte, string csvWerteTypen)
        {
            //SQL bauen
            StringBuilder sb = new StringBuilder();
            StringBuilder sbInner = new StringBuilder();

            //array für ABgleich der Wertetypen
            string[] arrayAbgleich = csvWerteTypen.Split(';');


            sb.Append("UPDATE xyx" + tabellenname + " SET ");
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
                    else
                    {
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
                    if (item.Value != null && !item.Value.ToString().Equals(""))
                    {
                        sbInner.Append(item.Key + "='" + DateTime.Parse(item.Value.ToString()) + "',");
                    }
                    else
                    {
                        sbInner.Append(item.Key + "=NULL,");
                    }
                }
                else if (arrayAbgleich[counter].Substring(0, 3).Equals("bol"))
                {
                    sbInner.Append(item.Key + "='" + Boolean.Parse(item.Value.ToString()) + "',");
                }
                else
                {
                    //Ansonsten muss es ein String sein
                    sbInner.Append(item.Key + "='" + item.Value.ToString().Replace("'", "''") + "',");
                }


                counter++;
            }
            string txt = sbInner.ToString();
            txt = txt.Substring(0, txt.Length - 1);
            sb.Append(txt);
            sb.Append(" WHERE xyx" + tabellenname + "Id = " + datensatzId);

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

        public Tuple<List<int>, List<object>> ReadComboboxItems(string _tabelle, string _feld, int idInRefTabelle = 0, string feldIn_tabelle = "")
        {
            List<int> lstInt = new List<int>();
            List<object> lstObject = new List<object>();

            string query = "";
            DataTable dt = new DataTable();
            if (idInRefTabelle == 0) {
                query = "select " + _tabelle + "Id, " + _feld + " from " + _tabelle;
            } else {
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
            return Tuple.Create(lstInt, lstObject);
        }

        /// <summary>
        /// Liest die Tabelle mit den Metainfomationen für die Datenbank ab
        /// </summary>
        /// <returns>Tuple of Strings. Tuple aus Tabellenname, csvFeldtyp, csvFeldnamen</returns>
        public List<Tuple<string, string, string>> ReadTableNamesTypesAndFields(bool IsDokumentType = false)
        {
            List<Tuple<string, string, string>> liste = new List<Tuple<string, string, string>>();
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = _con;
                cmd.CommandType = CommandType.Text;
                if (IsDokumentType) {
                    //if (IsDataForSuchfelder) {
                    //    cmd.CommandText = "select * from " + tabellenname + " tab1 INNER JOIN OkoDokumenteDaten tab2 ON tab1." + tabellenname + "Id = tab2.IdInTabelle";
                    //} else {
                        cmd.CommandText = "SELECT * FROM OkoDokTypTabellenfeldtypen";
                    //}                   
                } else {
                    cmd.CommandText = "SELECT * FROM OkoTabellenfeldtypen";
                }
                

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

        public string ReadTableNameByDokId(int id)
        {
            List<Tuple<string, string, string>> liste = new List<Tuple<string, string, string>>();
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = _con;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT Tabelle FROM OkoDokumententyp where OkoDokumententypId = " + id;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    return row.Field<string>(0);
                }
            }
            return "";
        }


        public DataTable ReadExportDataTable(string tabellenname, int id)
        {
            string[] _csvWerteFeldnamen = { };
            string[] _csvWerteFeldnamenOriginal = { };
            string[] _csvWerteFeldTypen = { };
            List<Tuple<List<int>, List<object>>> _nachschlageFelderWerte = new List<Tuple<List<int>, List<object>>>();

            //Zuerst brauche ich die Feldtypen der Tabellenfelder und die Namen der Felder

            string _namen = "";
            string _typen = "";

           
                List<Tuple<string, string, string>> Werte = ReadTableNamesTypesAndFields(true);
                foreach (var item in Werte)
                {
                    if (item.Item1.Equals(tabellenname))
                    {
                        _namen = tabellenname + "Id;" + item.Item3;
                        _typen = "int;" + item.Item2;
                    }
                }
            
            _csvWerteFeldnamen = _namen.Split(';');
            _csvWerteFeldnamenOriginal = _namen.Split(';');
            _csvWerteFeldTypen = _typen.Split(';');

            //Originaldaten einlesen
            DataTable dt = new DataTable();
            DataTable dtCopy = new DataTable();
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

            string query = "select * from " + tabellenname + " Where " + tabellenname + "Id =" + id;

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
                                string wert = tuple.Item2.ElementAt(index).ToString();
                                rowCopy[_csvWerteFeldnamen[i]] = wert;
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
            //Nicht benötigte Columns entfernen
            List<DataColumn> zuLoeschen = new List<DataColumn>();
            foreach (DataColumn column in dtCopy.Columns)
            {
                if (column.ColumnName.Equals(tabellenname + "Id"))
                {
                    zuLoeschen.Add(column);
                }

            }
            foreach (DataColumn item in zuLoeschen)
            {
                dtCopy.Columns.Remove(item);
            }
            return dtCopy;
        }


        #region ALter Kram --> Erst löschen wenn alle FUnktionalitäten fertig
        private string GetFieldTypes(string inputString)
        {
            string returner = "";
            switch (inputString)
            {
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

        public bool IdCHecker = true;
        public DataTable ReadDoksFuerDokgruppe(int dokGruppenId) {
            IdCHecker = true;
            DataTable table = new DataTable();
            

            //ich muss zuerst eine Liste der DokumententypIds der Gruppe erstellen
            DataTable dt0 = new DataTable();
            string query0 = "Select OkoDokumententypId from OkoDokumententyp Where OkoDokumentengruppenId = " + dokGruppenId;
            SqlCommand cmd0 = new SqlCommand(query0, _con);
            SqlDataAdapter da0 = new SqlDataAdapter(cmd0);
            da0.Fill(dt0);
            da0.Dispose();
            List<int> lstOkoDokumentenTypIds = new List<int>();
            foreach (DataRow row in dt0.Rows)
            {
                lstOkoDokumentenTypIds.Add(row.Field<int>(0));
            }


            DataTable dt = new DataTable();
            dt = ReadTableDataWerteErsetztFuerDarstellung("OkoDokumenteDaten");
            
            table.Rows.Clear();
            int counter = 0;
            DataTable dtb = dt.Copy();
            foreach (DataRow row in dt.Rows) {
                if (lstOkoDokumentenTypIds.Contains(row.Field<int>(0))) {
                   
                    dtb.Rows.Clear();
                    var a = dtb.NewRow();

                    a.ItemArray = row.ItemArray;
                    dtb.Rows.Add(a);
                    table.Merge(dtb);

                }
                counter++;
            }
            return table;
        }

        public Tuple<List<string>, List<string>> ReadDataSuchfelder(string tabellenname)
        {
            DataTable dt = new DataTable();
            string query = "Select * from OkoDokTypTabellenfeldtypen Where Tabellenname = '" + tabellenname + "'";
            SqlCommand cmd = new SqlCommand(query, _con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            da.Dispose();

            string feldnamen = dt.Rows[0].Field<string>(3) + ";Dateiname;Beschreibung;Titel";
            string feldtypen = dt.Rows[0].Field<string>(2) + ";txt;txt;txt";

            string[] _feldnamen = feldnamen.Split(';');
            string[] _feldtypen = feldtypen.Split(';');
            return Tuple.Create(_feldnamen.ToList(), _feldtypen.ToList());
        }

        /// <summary>
        /// Liest die Originaldaten der Tabelle ein
        /// </summary>
        /// <param name="tabellenname">Einzulesende Tabelle</param>
        /// <returns></returns>
        public DataTable ReadTableData(string tabellenname = "")
        {
            DataTable dt = new DataTable();
            string query = "";
            if (tabellenname.Equals(""))
            {
                query = "Select Dateiname, Beschreibung, Titel, ErfasstAm from OkoDokumenteDaten";
                //query = "Select Tabelle, IdInTabelle, Dateiname, Beschreibung, Titel, ErfasstAm from OkoDokumenteDaten";
            }
            else
            {
                query = "select * from " + tabellenname + " tab1 LEFT JOIN (Select Tabelle, IdInTabelle, Dateiname, Beschreibung, Titel, ErfasstAm from OkoDokumenteDaten) as tab2 ON tab1." + tabellenname + "Id = tab2.IdInTabelle AND tab2.Tabelle = '" + tabellenname + "'";
            }

            SqlCommand cmd = new SqlCommand(query, _con);
            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            // this will query your database and return the result to your datatable
            da.Fill(dt);
            da.Dispose();
            //Nicht benötigte Columns entfernen
            List<DataColumn> zuLoeschen = new List<DataColumn>();
            foreach (DataColumn column in dt.Columns)
            {
                if (column.ColumnName.Equals("IdInTabelle") || column.ColumnName.Equals("Tabelle"))
                {
                    zuLoeschen.Add(column);
                }
            }
            foreach (DataColumn item in zuLoeschen)
            {
                dt.Columns.Remove(item);
            }

            return dt;
        }

        /// <summary>
        /// Liest die Daten der Tabelle ein und ersetzt Referenzen zu anderen Tabellen mit den entsprechenden Werten für die Darstellung in Datagrid. Die Spaltenheader werden auch korrigiert
        /// </summary>
        /// <param name="tabellenname">Die einzulesende Tabelle</param>
        /// <returns>Tabellendaten mit referenzierten Nachschlagewerten</returns>
        public DataTable ReadTableDataWerteErsetztFuerDarstellung(string tabellenname = "")
        {
            

            string[] _csvWerteFeldnamen = { };
            string[] _csvWerteFeldnamenOriginal = { };
            string[] _csvWerteFeldTypen = { };
            List<Tuple<List<int>, List<object>>> _nachschlageFelderWerte = new List<Tuple<List<int>, List<object>>>();

            //Zuerst brauche ich die Feldtypen der Tabellenfelder und die Namen der Felder

            string _namen = "";
            string _typen = "";

            if (tabellenname.Equals(""))
            {
                _namen = "Tabelle;IdInTabelle;Dateiname;Beschreibung;Titel;ErfasstAm";
                _typen = "txt;int;txt;txt;txt;dat";
            }
            else if (tabellenname.Equals("OkoDokumenteDaten"))
            {
                _namen = "OkoDokumentenTypId;Tabelle;IdInTabelle;Dateiname;Beschreibung;Titel;ErfasstAm";
                _typen = "int;txt;int;txt;txt;txt;dat";
            }
            else
            {
                List<Tuple<string, string, string>> Werte = ReadTableNamesTypesAndFields(true);
                foreach (var item in Werte)
                {
                    if (item.Item1.Equals(tabellenname))
                    {
                        _namen = tabellenname + "Id;" + item.Item3 + ";OkoDokumenteDatenId;Tabelle;IdInTabelle;Dateiname;Beschreibung;Titel;ErfasstAm";
                        _typen = "int;" + item.Item2 + ";int;txt;int;txt;txt;txt;dat";
                    }
                }
            }
            _csvWerteFeldnamen = _namen.Split(';');
            _csvWerteFeldnamenOriginal = _namen.Split(';');
            _csvWerteFeldTypen = _typen.Split(';');

            //Originaldaten einlesen
            DataTable dt = new DataTable();
            DataTable dtCopy = new DataTable();
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

            string query = "";
            if (tabellenname.Equals(""))
            {
                query = "Select Tabelle, IdInTabelle, Dateiname, Beschreibung, Titel, ErfasstAm from OkoDokumenteDaten";
            }
            else if (tabellenname.Equals("OkoDokumenteDaten"))
            {
                query = "Select OkoDokumentenTypId, Tabelle, IdInTabelle, Dateiname, Beschreibung, Titel, ErfasstAm from OkoDokumenteDaten";
            }
            else
            {
                query = "select * from " + tabellenname + " tab1 LEFT JOIN (Select OkoDokumenteDatenId, Tabelle, IdInTabelle, Dateiname, Beschreibung, Titel, ErfasstAm from OkoDokumenteDaten) as tab2 ON tab1." + tabellenname + "Id = tab2.IdInTabelle AND tab2.Tabelle ='" + tabellenname + "'";
            }

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
                                string wert = tuple.Item2.ElementAt(index).ToString();
                                rowCopy[_csvWerteFeldnamen[i]] = wert;
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

            //Nicht benötigte Columns entfernen
            List<DataColumn> zuLoeschen = new List<DataColumn>();
            foreach (DataColumn column in dtCopy.Columns)
            {
                //if (column.ColumnName.Equals("IdInTabelle") || column.ColumnName.Equals("Tabelle") || (column.ColumnName.Equals(tabellenname + "Id") && IdCHecker))
                //{
                //    zuLoeschen.Add(column);
                //}
                if (column.ColumnName.Contains("xyx"))
                {
                    column.ColumnName = column.ColumnName.Replace("xyx", "");
                }
            }
            //foreach (DataColumn item in zuLoeschen)
            //{
            //    dtCopy.Columns.Remove(item);
            //}

            return dtCopy;
        }

        //public void UpdateTableData(string tabellenname, int datensatzId, Dictionary<string, object> werte, string csvWerteTypen)
        //{
        //    //SQL bauen
        //    StringBuilder sb = new StringBuilder();
        //    StringBuilder sbInner = new StringBuilder();

        //    //array für ABgleich der Wertetypen
        //    string[] arrayAbgleich = csvWerteTypen.Split(';');


        //    sb.Append("UPDATE " + tabellenname + " SET ");
        //    int counter = 0;
        //    foreach (var item in werte)
        //    {
        //        //Unterscheidung nach Feldtypen
        //        if (arrayAbgleich[counter].Substring(0, 3).Equals("dec"))
        //        {
        //            if (!item.Value.ToString().Equals(""))
        //            {
        //                sbInner.Append(item.Key + "=" + item.Value.ToString().Replace(",", ".") + ",");
        //            }
        //            else
        //            {
        //                sbInner.Append(item.Key + "=NULL,");
        //            }
        //        }
        //        else if (arrayAbgleich[counter].Substring(0, 3).Equals("int"))
        //        {
        //            if (!item.Value.ToString().Equals(""))
        //            {
        //                sbInner.Append(item.Key + "=" + Int32.Parse(item.Value.ToString()) + ",");
        //            }
        //            else
        //            {
        //                sbInner.Append(item.Key + "=NULL,");
        //            }
        //        }
        //        else if (arrayAbgleich[counter].Substring(0, 3).Equals("dat"))
        //        {
        //            if (item.Value != null && !item.Value.ToString().Equals(""))
        //            {
        //                sbInner.Append(item.Key + "='" + DateTime.Parse(item.Value.ToString()) + "',");
        //            }
        //            else
        //            {
        //                sbInner.Append(item.Key + "=NULL,");
        //            }
        //        }
        //        else if (arrayAbgleich[counter].Substring(0, 3).Equals("bol"))
        //        {
        //            sbInner.Append(item.Key + "='" + Boolean.Parse(item.Value.ToString()) + "',");
        //        }
        //        else
        //        {
        //            //Ansonsten muss es ein String sein
        //            sbInner.Append(item.Key + "='" + item.Value.ToString().Replace("'", "''") + "',");
        //        }


        //        counter++;
        //    }
        //    string txt = sbInner.ToString();
        //    txt = txt.Substring(0, txt.Length - 1);
        //    sb.Append(txt);
        //    sb.Append(" WHERE " + tabellenname + "Id = " + datensatzId);

        //    string sql = sb.ToString();
        //    SqlCommand command = _con.CreateCommand();
        //    SqlTransaction transaction;
        //    // Start a local transaction.
        //    transaction = _con.BeginTransaction(IsolationLevel.ReadCommitted);
        //    // Must assign both transaction object and connection
        //    // to Command object for a pending local transaction
        //    command.Connection = _con;
        //    command.Transaction = transaction;
        //    command.CommandText = sql;
        //    command.ExecuteNonQuery();
        //    transaction.Commit();
        //}

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


        #endregion



    }
}
