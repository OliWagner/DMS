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
    public partial class OrdnerAnzeigen : UserControl
    {
       
        public string Pfad { get; set; }
        public bool HasChanged { get; set; }

        public void Start(string pfad) {
            if (pfad != null)
            {
                if (!pfad.Equals(Pfad)) {
                    HasChanged = true; 
                }
                Pfad = pfad;
                lblOrdner.Content = "Scanordner: " + Pfad;
            }
            else {
                lblOrdner.Content = "Noch kein Ablageordner ausgewählt.";
            }
            lblOrdner.FontSize = 16;
        }

        public OrdnerAnzeigen() {
            InitializeComponent();
            HasChanged = false;
            Pfad = "";
        }
        
    }
}

