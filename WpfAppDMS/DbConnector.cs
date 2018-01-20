﻿using System;
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
                
        
        public void Close() {
            _con.Close();
            _con.Dispose();
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

        /// <summary>
        /// Liest die Tabelle mit den Metainfomationen für die Datenbank ab
        /// </summary>
        /// <returns>Tuple of Strings. Tuple aus Tabellenname, csvFeldtyp, csvFeldnamen</returns>
        public List<Tuple<string, string, string>> ReadTableNamesTypesAndFields()
        {
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

        public Tuple<List<int>, List<object>> ReadComboboxItems(string _tabelle, string _feld)
        {
            List<int> lstInt = new List<int>();
            List<object> lstObject = new List<object>();

            DataTable dt = new DataTable();
            string query = "select " + _tabelle + "Id, " + _feld + " from " + _tabelle;
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
        /// Liest die Originaldaten der Tabelle ein
        /// </summary>
        /// <param name="tabellenname">Einzulesende Tabelle</param>
        /// <returns></returns>
        public DataTable ReadTableData(string tabellenname)
        {
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
            List<Tuple<string, string, string>> Werte = ReadTableNamesTypesAndFields();
            foreach (var item in Werte)
            {
                if (item.Item1.Equals(tabellenname))
                {
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
                            //TODO Referentwert ersetzen
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
            return dtCopy;
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
            sb.Append(" WHERE " + tabellenname + "Id = " + datensatzId);

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


        #endregion



    }
}