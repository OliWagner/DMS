using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Linq;
using Vigenere;
using System;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class TabNameDialog : Window
    {
        private EingabeTabelle _eingabeTabelle;
       
        public string TabName
        {
            get { return txtTabName.Text; }
        }

        public TabNameDialog(EingabeTabelle eT)
        {
            _eingabeTabelle = eT;
            InitializeComponent();
            
        }

        #region Eventhandler
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        #endregion

        private void BtnTabSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            //Es dürfen nur Buchstaben und Nummern als Bezeichner verwendet werden
            if (_eingabeTabelle == null)
            {
                e.CanExecute = false; return;
            }
            //Es dürfen nur Buchstaben und Nummern als Bezeichner verwendet werden
            string AllowedChars = @"^[a-zA-Z0-9]+$";

            //Noch testen, ob ein Tabellenname eingegeben wurde + Es dürfen nur Nummer und Ziffern verwendet werden + Es dürfen keine Tabellennamen doppelt sein
            if (txtTabName.Text.Length < 3 || txtTabName.Text.Equals("") || !Regex.IsMatch(txtTabName.Text, AllowedChars) || _eingabeTabelle.alleTabellenNamen.Contains(txtTabName.Text)) { e.CanExecute = false; }
        }

        private void BtnTabSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }


    
}

