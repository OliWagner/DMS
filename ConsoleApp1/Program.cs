using System;
using DatabaseConnector;
using System.Data.SqlClient;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var myString = "provider=System.Data.SqlClient;provider connection string=&quot;Data Source=(local)\\sqlexpress;Initial Catalog=OKOrganizer;User ID=sa;Password=95hjh11!;";
            Connector con = new Connector(myString);
            
        }
    }
}
