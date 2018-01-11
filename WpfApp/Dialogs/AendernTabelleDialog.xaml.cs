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

namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class AendernTabelleDialog : Window
    {
       

        

        public AendernTabelleDialog()
        {
            InitializeComponent();
            
        }

    
     

        #region Eventhandler
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //Wird in Mainindow behandelt, schließt die Applikation
        }
        #endregion
    }
}

