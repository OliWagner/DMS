﻿using System;
using System.Collections.Generic;
using System.Data;
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

namespace WpfApp
{
    /// <summary>
    /// Interaktionslogik für Tabellendaten.xaml
    /// </summary>
    public partial class Tabellendaten : UserControl
    {
        //string[] arrTxt = new string[5];
        public Tabellendaten()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            DataTable dt = new DataTable();
            //DataGrid füllen
            dgTabelle.ItemsSource = dt.DefaultView;

        }

        public void zeichneTabelle(string tabelle) {
            DataTable dt = new DataTable();

            dt = ((DbConnector)App.Current.Properties["Connector"]).ReadTableData(tabelle);

            //DataGrid füllen
            dgTabelle.ItemsSource = dt.DefaultView;

        }

        private void dgTabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Hier nichts tun, Behandlung in MainWindow
        }

        private void btnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            //Nichts machen, wird in MainWIndow behandelt.
        }

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //DO nothing
        }

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (dgTabelle.Items != null)
            {
                foreach (DataRowView item in dgTabelle.SelectedItems)
                {
                    if (item != null)
                    {
                        e.CanExecute = true;
                        return;
                    }
                }
            }
            e.CanExecute = false;
        }

    }
}
