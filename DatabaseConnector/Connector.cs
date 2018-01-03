using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace DatabaseConnector
{


    public class Connector
    {
        /// <summary>
        /// Aufruf mit Connectionstring, liefert die Datenbakverbindung zurück
        /// </summary>
        /// <param name="conString">Der ConnectionString zur gewünschten Datenbank</param>
        public Connector(String conString) {
            Connection = connect(conString);
        }

        private SqlConnection connect(String conString) {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = conString;
            con.Open();
            return con;
        }

        public SqlConnection Connection { get => Connection; set => Connection = value; }
    }
}
