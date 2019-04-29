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
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace DMS_Adminitration
{
    public class MyEventArgs
    {
        public String Prop1 { get; set; }
    }
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class ScanOrdner : UserControl
    {
        public event EventHandler<MyEventArgs> SomethingChanged;


        public string Ordner { get; set; }
        FileSystemWatcher FSW;
        public string FileName { get; set; }

    public ScanOrdner() {
            InitializeComponent();
        }

        public void zeichneGrid(string ordner) {
            Ordner = ordner;
            if (FSW == null) {
                FSW_Initialisieren();
            }

            grdScanOrdner.Children.Clear();
            grdScanOrdner.RowDefinitions.Clear();
            //Ordnerinhalt auslesen
            System.IO.DirectoryInfo ParentDirectory = new System.IO.DirectoryInfo(Ordner);

            System.IO.FileInfo[] fis = ParentDirectory.GetFiles();
            for (int i = 0; i < fis.Length; ++i)
            {
                Label l = new Label();
                l.Name = "wert" + i;
                l.Width = 300;
                l.Height = 30;
                l.Content = fis[i].Name;
                l.MouseLeftButtonDown += L_MouseLeftButtonDown;
                RowDefinition gridRow = new RowDefinition();
                gridRow.Height = new GridLength(25);
                grdScanOrdner.RowDefinitions.Add(gridRow);
                Grid.SetRow(l, i);

                grdScanOrdner.Children.Add(l);
            }
        }

        private void L_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label _sender = (Label)sender;
            FileName = _sender.Content.ToString();
        }

        private void FSW_Initialisieren()
        {
            // Filesystemwatcher anlegen
            FSW = new FileSystemWatcher();
            // Pfad und Filter festlegen
            FSW.Path = Ordner;
            
            //FSW.Filter = "*.txt";
            // Events definieren
            FSW.Changed += new FileSystemEventHandler(FSW_Changed);
            FSW.Deleted += new FileSystemEventHandler(FSW_Deleted);
            // Filesystemwatcher aktivieren
            FSW.EnableRaisingEvents = true;
            FSW.EndInit();
        }

        private void FSW_Changed(object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                zeichneGrid(Ordner);
            }));
            SomethingChanged?.Invoke(this, new MyEventArgs() { });
        }

        private void FSW_Deleted(object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                zeichneGrid(Ordner);
            }));
            SomethingChanged?.Invoke(this, new MyEventArgs() { });
        }

        private void OnSomethingChanged(object sender, MyEventArgs e)
        {
        }


    }
}

