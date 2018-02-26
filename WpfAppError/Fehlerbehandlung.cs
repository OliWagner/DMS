using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppError
{
    public static class Fehlerbehandlung
    {
        private static string _path
        {
            get; set;
        }

        public static void Error(string stackTrace, string message, string ownCode) {

            FileInfo fi = new FileInfo(Assembly.GetEntryAssembly().Location);
            _path = fi.DirectoryName + "\\Errors.txt";
            if (File.Exists(_path))
            {
                //An FIle anhängen
                using (StreamWriter streamWriter = new StreamWriter(_path, true))
                {
                    streamWriter.Write(DateTime.Now + "\r\n" + stackTrace + "\r\n" + message + Environment.NewLine + ownCode + "\r\n\r\n");
                }
            }
            else
            {
                try { 
                //File erstellen
                using (var x = File.Create(_path)) { }
                using (StreamWriter sw = new StreamWriter(_path))
                {
                    sw.Write(DateTime.Now + "\n" + stackTrace + "\n" + message + "\n" + ownCode + "\n\n");
                }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Es konnte keine Datei auf dem Laufwerk angelegt werden. --> " + ex.Message);
                }
            }
        }
    }
}
