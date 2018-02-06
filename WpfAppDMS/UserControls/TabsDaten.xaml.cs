using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfAppDMS
{
    /// <summary>
    /// Interaktionslogik für TabsDaten.xaml
    /// </summary>
    public partial class TabsDaten : UserControl
    {
        public List<string> Items = new List<string>();

        /// <summary>
        /// Event dient MainWIndow dazu, selbst noch einen Eventhandler an das neue Element anzupappen
        /// Sonst kriegt MainWIndow den Klick auf den Speichern Button nicht mit
        /// MainWIndow muss dann noch die eigenen Daten in die DB schreiben
        /// </summary>
        public event EventHandler<EingabeDokumentDatenEventArgs> ItemAdded;

        public TabsDaten()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dokumentenTyp"></param>
        /// <param name="dokumentenTypId"></param>
        /// <param name="csvFeldwerte">optional. Wenn gefüllt, wird auch das Formular gefüllt</param>
        public void Add(string dokumentenTyp, int dokumentenTypId, string csvFeldwerte = "") {
            TabItem item = new TabItem();
            EingabeDokumentDaten edd = new EingabeDokumentDaten();
            edd.zeichneGrid(dokumentenTyp, dokumentenTypId, csvFeldwerte);
            edd._idAktuellerDatensatz = dokumentenTypId;
            edd.btnAbbruch.Click += EingabeDokumentDaten_BtnAbbruch_Click;
            edd.btnSpeichern.Click += EingabeDokumentDaten_BtnSpeichern_Click;
            item.Content = edd;
            item.Header = dokumentenTyp;
            Items.Add(dokumentenTyp);
            tabsMain.Items.Add(item);
            //Hier müsste nun ein Event gefeuert werden, das als Argument das edd haben müsste, damit diesem 
            EingabeDokumentDatenEventArgs args = new EingabeDokumentDatenEventArgs();
            ItemAdded?.Invoke(this, new EingabeDokumentDatenEventArgs() { eingabeDokumentDaten = edd , DokumentenTypId = dokumentenTypId });
        }

        private void EingabeDokumentDaten_BtnSpeichern_Click(object sender, RoutedEventArgs e)
        {
            //Datensatz ist gespeichert (sonst wäre der e.Handled vorher auf true gesetzt worden), Tag bei Abbrechen und Speichern ist der gleiche, also einfach Tab entfernen wie bei Abbruch
            TabItem toRemove = new TabItem();
            string table = ((Button)sender).Tag.ToString().Split('_')[0];

            foreach (TabItem item in tabsMain.Items)
            {
                if (item.Header.Equals(table))
                {
                    toRemove = item;
                    Items.Remove(item.Header.ToString());
                }
            }
            tabsMain.Items.Remove(toRemove);
        }

        private void EingabeDokumentDaten_BtnAbbruch_Click(object sender, RoutedEventArgs e)
        {
            TabItem toRemove = new TabItem();
            string table = ((Button)sender).Tag.ToString();
            foreach (TabItem item in tabsMain.Items)
            {
                if (item.Header.Equals(table)) {
                    toRemove = item;
                    Items.Remove(item.Header.ToString());
                }
            }
            tabsMain.Items.Remove(toRemove);
        }
    }



    public class EingabeDokumentDatenEventArgs : EventArgs
    {
        public EingabeDokumentDaten eingabeDokumentDaten { get; set; }
        public int DokumentenTypId { get; set; }
    }
}
