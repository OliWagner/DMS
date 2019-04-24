using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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


namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für PflegeTabellendaten.xaml
    /// </summary>
    public partial class PflegeTabellendaten : UserControl
    {
        public string TabNameUebergabe { get; set; }
        public string _tabName { get; set; }
        public string _csvTabFeldnamen { get; set; }
        public string _csvTabFeldtypen { get; set; }
        public string _csvTabFeldwerte { get; set; }
        public int _idAktuellerDatensatz { get; set; }
        public bool IstneuerDatensatz { get; set; }

        public PflegeTabellendaten()
        {
            InitializeComponent();
        }

        public void zeichenGrid(string tabName, string csvTabFeldnamen, string csvTabFeldtypen) {
            IstneuerDatensatz = true;
            _tabName = tabName;
            _csvTabFeldnamen = csvTabFeldnamen;
            _csvTabFeldtypen = csvTabFeldtypen;
            _csvTabFeldwerte = "";

            var csvFeldnamen = csvTabFeldnamen.Split(';');
            var csvFeldtypen = csvTabFeldtypen.Split(';');
            grdMain.Children.Clear();
            grdMain.RowDefinitions.Clear();
            //Überschrift generieren
            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            txtBlock1.Text = tabName;
            txtBlock1.Margin = new Thickness(5, 0, 0, 0);
            txtBlock1.Foreground = new SolidColorBrush(Colors.Black);
            txtBlock1.VerticalAlignment = VerticalAlignment.Top;

            RowDefinition gridRow = new RowDefinition();
            gridRow.Height = new GridLength(30);
            grdMain.RowDefinitions.Add(gridRow);

            Grid.SetRow(txtBlock1, 0);           
            grdMain.Children.Add(txtBlock1);

            for (int i = 0; i < csvFeldnamen.Length; i++) {

                ColumnDefinition cL = new ColumnDefinition();
                cL.Width = new GridLength(200);
                grdMain.ColumnDefinitions.Add(cL);
                ColumnDefinition cR = new ColumnDefinition();
                cR.Width = new GridLength(400);
                grdMain.ColumnDefinitions.Add(cR);

                RowDefinition gR = new RowDefinition();
                gR.Height = new GridLength(30);
                grdMain.RowDefinitions.Add(gR);

                TextBlock tbLabel = new TextBlock();
                tbLabel.Text = (csvFeldnamen.ElementAt(i).Length > 2 && csvFeldnamen.ElementAt(i).Substring(0, 3).Equals("_x_") ? csvFeldnamen.ElementAt(i).Split('_')[2] : csvFeldnamen.ElementAt(i)) + " (" + csvFeldtypen.ElementAt(i).Substring(0,3) + ")";
                tbLabel.Tag = csvFeldnamen.ElementAt(i);
                tbLabel.Width = 180;
                tbLabel.Height = 30;
                tbLabel.Margin = new Thickness(5, 0, 0, 0);
                tbLabel.Foreground = new SolidColorBrush(Colors.Black);
                tbLabel.VerticalAlignment = VerticalAlignment.Top;
                tbLabel.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetColumn(tbLabel, 0);
                Grid.SetRow(tbLabel, (i + 1));
                grdMain.Children.Add(tbLabel);

                if (csvFeldtypen[i].Substring(0, 3).Equals("bol"))
                {
                    CheckBox tb = new CheckBox();
                    tb.Name = "Wert" + i;
                    tb.Width = 20;
                    tb.Height = 20;
                    tb.VerticalAlignment = VerticalAlignment.Top;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, (i + 1));
                    grdMain.Children.Add(tb);
                }
                else if (csvFeldtypen[i].Substring(0, 3).Equals("loo"))
                {
                    LookupAuswahl tb = new LookupAuswahl();
                    tb.Fill(csvFeldnamen.ElementAt(i));
                    tb.VerticalAlignment = VerticalAlignment.Top;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, (i + 1));
                    grdMain.Children.Add(tb);
                } else
                {
                    TextBox tb = new TextBox();
                    tb.Name = "Wert" + i;
                    tb.MinWidth = 350;
                    tb.MaxWidth = 350;
                    tb.Height = 20;
                    //Datepicker falls benötigt
                    DatePicker dp = new DatePicker();
                    dp.Name = "Wert" + i;
                    dp.Width = 200;
                    dp.Height = 25;
                    //Verschiedene Eventhandler für die Feldtypen setzen
                    if (csvFeldtypen[i].Substring(0, 3).Equals("dat"))
                    {
                        //Wenn es sich um ein Datum handelt, muss ein Datepicker hinzugefügt werden
                        dp.VerticalAlignment = VerticalAlignment.Top;
                        dp.HorizontalAlignment = HorizontalAlignment.Left;
                        Grid.SetColumn(dp, 1);
                        Grid.SetRow(dp, (i + 1));
                        grdMain.Children.Add(dp);
                    }
                    else 
                    {
                        if (csvFeldtypen[i].Substring(0, 3).Equals("dec"))
                        {
                            tb.TextChanged += Tb_TextChangedDec;
                        }
                        else if (csvFeldtypen[i].Substring(0, 3).Equals("int"))
                        {
                            tb.TextChanged += Tb_TextChangedInt;
                        }
                        else if (csvFeldtypen[i].Substring(0, 3).Equals("txt"))
                        {
                            if (csvFeldtypen[i].Equals("txt50n")) { tb.TextChanged += Tb_TextChangedTxt50; }
                            if (csvFeldtypen[i].Equals("txt255n"))
                            {
                                tb.TextChanged += Tb_TextChangedTxt255;
                                tb.TextWrapping = TextWrapping.Wrap;
                                tb.Height = 75;
                                grdMain.RowDefinitions[i + 1].Height = new GridLength(80);

                            }
                            if (csvFeldtypen[i].Equals("txtmn"))
                            {
                                tb.TextChanged += Tb_TextChangedTxtm;
                                tb.TextWrapping = TextWrapping.Wrap;
                                tb.Height = 115;
                                grdMain.RowDefinitions[i + 1].Height = new GridLength(120);
                            }

                        }

                        tb.VerticalAlignment = VerticalAlignment.Top;
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        Grid.SetColumn(tb, 1);
                        Grid.SetRow(tb, (i + 1));
                        grdMain.Children.Add(tb);
                    }
                }
            }
        }

        public void zeichenGrid(string tabName, string csvTabFeldnamen, string csvTabFeldtypen, string csvTabFeldwerte)
        {
            IstneuerDatensatz = false;
            _tabName = tabName;
            _csvTabFeldnamen = csvTabFeldnamen;
            _csvTabFeldtypen = csvTabFeldtypen;
            _csvTabFeldwerte = csvTabFeldwerte;

            var csvFeldnamen = csvTabFeldnamen.Split(';');
            var csvFeldtypen = csvTabFeldtypen.Split(';');
            var csvFeldwerte = csvTabFeldwerte.Split(';');

            grdMain.Children.Clear();
            grdMain.RowDefinitions.Clear();
            //Überschrift generieren
            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.FontSize = 14;
            txtBlock1.FontWeight = FontWeights.Bold;
            txtBlock1.Text = tabName;
            txtBlock1.Margin = new Thickness(5, 0, 0, 0);
            txtBlock1.Foreground = new SolidColorBrush(Colors.Black);
            txtBlock1.VerticalAlignment = VerticalAlignment.Top;

            RowDefinition gridRow = new RowDefinition();
            gridRow.Height = new GridLength(30);
            grdMain.RowDefinitions.Add(gridRow);

            Grid.SetRow(txtBlock1, 0);
            grdMain.Children.Add(txtBlock1);

            for (int i = 0; i < csvFeldnamen.Length; i++)
            {

                ColumnDefinition cL = new ColumnDefinition();
                cL.Width = new GridLength(200);
                grdMain.ColumnDefinitions.Add(cL);
                ColumnDefinition cR = new ColumnDefinition();
                cR.Width = new GridLength(400);
                grdMain.ColumnDefinitions.Add(cR);

                RowDefinition gR = new RowDefinition();
                gR.Height = new GridLength(30);
                grdMain.RowDefinitions.Add(gR);

                TextBlock tbLabel = new TextBlock();
                tbLabel.Text = (csvFeldnamen.ElementAt(i).Length > 2 && csvFeldnamen.ElementAt(i).Substring(0, 3).Equals("_x_") ? csvFeldnamen.ElementAt(i).Split('_')[2] : csvFeldnamen.ElementAt(i)) + " (" + csvFeldtypen.ElementAt(i).Substring(0,3) + ")";
                tbLabel.Tag = csvFeldnamen.ElementAt(i);
                tbLabel.Width = 180;
                tbLabel.Height = 30;
                tbLabel.Margin = new Thickness(5, 0, 0, 0);
                tbLabel.Foreground = new SolidColorBrush(Colors.Black);
                tbLabel.VerticalAlignment = VerticalAlignment.Top;
                tbLabel.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetColumn(tbLabel, 0);
                Grid.SetRow(tbLabel, (i + 1));
                grdMain.Children.Add(tbLabel);
                if (csvFeldtypen[i].Substring(0, 3).Equals("bol")) {
                    CheckBox tb = new CheckBox();
                    tb.Name = "Wert" + i;
                    tb.Width = 20;
                    tb.Height = 20;
                    var test = csvFeldwerte.ElementAt(i).ToString();
                    tb.IsChecked = csvFeldwerte.ElementAt(i).ToString().Equals("True");
                    tb.VerticalAlignment = VerticalAlignment.Top;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, (i + 1));
                    grdMain.Children.Add(tb);
                }
                else if (csvFeldtypen[i].Substring(0, 3).Equals("loo"))
                {
                    LookupAuswahl tb = new LookupAuswahl();
                    tb.Fill(csvFeldnamen.ElementAt(i));
                    //COmboBOx vorbelegen
                    Tuple<List<int>, List<object>> tuple = ((DbConnector)App.Current.Properties["Connector"]).ReadComboboxItems(csvFeldnamen.ElementAt(i).Split('_')[3], csvFeldnamen.ElementAt(i).Split('_')[4]);
                    if (!csvFeldwerte.ElementAt(i).ToString().Equals("")) {
                        int position = tuple.Item1.IndexOf(Int32.Parse(csvFeldwerte.ElementAt(i).ToString()));                  
                        tb.cboAuswahl.SelectedIndex = position;
                    }

                    tb.VerticalAlignment = VerticalAlignment.Top;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, (i + 1));
                    grdMain.Children.Add(tb);
                }
                else { 
                    //Textbox erstellen
                    TextBox tb = new TextBox();
                    tb.Name = "Wert" + i;
                    tb.Width = 350;
                    tb.MaxWidth = 350;
                    tb.Height = 20;

                    //Datepicker falls benötigt
                    DatePicker dp = new DatePicker();
                    dp.Name = "Wert" + i;
                    dp.Width = 200;
                    dp.Height = 25;

                    //Verschiedene Eventhandler für die Feldtypen setzen
                    if (csvFeldtypen[i].Substring(0, 3).Equals("dat"))
                    {
                        //Hat sich erledigt, ist jetzt Datepicker
                        //tb.TextChanged += Tb_TextChangedDat;
                        //tb.MaxLength = 10;
                    }
                    else if (csvFeldtypen[i].Substring(0, 3).Equals("dec"))
                    {
                        tb.TextChanged += Tb_TextChangedDec;
                    }
                    else if (csvFeldtypen[i].Substring(0, 3).Equals("int"))
                    {
                        tb.TextChanged += Tb_TextChangedInt;
                    }
                    else if (csvFeldtypen[i].Substring(0, 3).Equals("txt"))
                    {
                        if (csvFeldtypen[i].Equals("txt50n")) {
                            tb.TextChanged += Tb_TextChangedTxt50;
                        }
                        if (csvFeldtypen[i].Equals("txt255n")) {
                            tb.TextChanged += Tb_TextChangedTxt255;
                            tb.TextWrapping = TextWrapping.Wrap;
                            tb.Height = 75;
                            grdMain.RowDefinitions[i + 1].Height = new GridLength(80);
                        }
                        if (csvFeldtypen[i].Equals("txtmn")) {
                            tb.TextChanged += Tb_TextChangedTxtm;
                            tb.TextWrapping = TextWrapping.Wrap;
                            tb.Height = 115;
                            grdMain.RowDefinitions[i + 1].Height = new GridLength(120);
                        }
                    }
                    if (csvFeldtypen[i].Substring(0, 3).Equals("dat"))
                    {
                        if (csvFeldwerte.ElementAt(i).ToString().Length > 10) {
                            int tag = Int32.Parse(csvFeldwerte.ElementAt(i).ToString().Substring(0, 2));
                            int monat = Int32.Parse(csvFeldwerte.ElementAt(i).ToString().Substring(3, 2));
                            int jahr = Int32.Parse(csvFeldwerte.ElementAt(i).ToString().Substring(6, 4));
                            dp.SelectedDate = new DateTime(jahr,monat,tag);
                            } 
                            dp.VerticalAlignment = VerticalAlignment.Top;
                            dp.HorizontalAlignment = HorizontalAlignment.Left;
                            Grid.SetColumn(dp, 1);
                            Grid.SetRow(dp, (i + 1));
                            grdMain.Children.Add(dp);                  
                    }
                    else
                    {
                        tb.Text = csvFeldwerte.ElementAt(i).ToString();
                        tb.VerticalAlignment = VerticalAlignment.Top;
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        Grid.SetColumn(tb, 1);
                        Grid.SetRow(tb, (i + 1));
                        grdMain.Children.Add(tb);
                    }                   
                }
            }
        }

        private void Tb_TextChangedDec(object sender, TextChangedEventArgs e)
        {          
            Regex r = new Regex("^[0-9]+(?:[\\.\\,]\\d{0,5})?$");
            TextBox _sender = (TextBox)sender;
            if (_sender.Text.ToString().Length > 0) {
                var test = _sender.Name;
                if (!r.IsMatch(_sender.Text.ToString()))
                {
                    TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                    ctrl.Text = _sender.Text.Substring(0, _sender.Text.Length - 1);
                    ctrl.CaretIndex = _sender.Text.Length;
                }
            }
        }
        private void Tb_TextChangedInt(object sender, TextChangedEventArgs e)
        {
            Regex r = new Regex("[0-9]");
            TextBox _sender = (TextBox)sender;
            if (_sender.Text.ToString().Length > 0)
            {
                if (_sender.Text.ToString().Length > 9)
                {
                    TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                    ctrl.Text = _sender.Text.Substring(0, 9);
                    ctrl.CaretIndex = _sender.Text.Length;
                }

                foreach (char item in _sender.Text.ToArray())
                {
                    if (!r.IsMatch(item.ToString()))
                    {
                        TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                        ctrl.Text = _sender.Text.Substring(0, _sender.Text.Length - 1);
                        ctrl.CaretIndex = _sender.Text.Length;
                    }
                }
            }
        }
        private void Tb_TextChangedTxt50(object sender, TextChangedEventArgs e)
        {
            TextBox _sender = (TextBox)sender;
            if (_sender.Text.ToString().Length > 0)
            {
                if (_sender.Text.ToString().Length > 50)
                {
                    TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                    ctrl.Text = _sender.Text.Substring(0, 50);
                    ctrl.CaretIndex = _sender.Text.Length;
                }
            }
        }
        private void Tb_TextChangedTxt255(object sender, TextChangedEventArgs e)
        {
            TextBox _sender = (TextBox)sender;
            if (_sender.Text.ToString().Length > 0)
            {
                if (_sender.Text.ToString().Length > 255)
                {
                    TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                    ctrl.Text = _sender.Text.Substring(0, 255);
                    ctrl.CaretIndex = _sender.Text.Length;
                }
            }
        }
        private void Tb_TextChangedTxtm(object sender, TextChangedEventArgs e)
        {
            TextBox _sender = (TextBox)sender;
            if (_sender.Text.ToString().Length > 8000)
            {
                if (_sender.Text.ToString().Length > 8000)
                {
                    TextBox ctrl = (TextBox)LogicalTreeHelper.FindLogicalNode(_sender, _sender.Name);
                    ctrl.Text = _sender.Text.Substring(0, 8000);
                    ctrl.CaretIndex = _sender.Text.Length;
                }
            }
        }

        private void btnAbbruch_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        public void Clear() {
            grdMain.Children.Clear();
        }
    }
}
