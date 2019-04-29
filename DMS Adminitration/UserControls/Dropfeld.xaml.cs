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

namespace DMS_Adminitration
{
    /// <summary>
    /// Interaktionslogik für CsvDialog.xaml
    /// </summary>
    public partial class Dropfeld : UserControl
    {
        public string[] Data = { };

        public Dropfeld() {
            InitializeComponent();
            //txtDropzone.Text = "Datei Droppen";
            //txtDropzone.FontSize = 18;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            
            if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Data = e.Data.GetData(DataFormats.FileDrop) as string[];
            }            
        }
    }
}

