using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WpfAppDMS
{
    public static class MyCommands
    {
        public static readonly RoutedUICommand Verbinden = new RoutedUICommand
                (
                        "Verbinden",
                        "Verbinden",
                        typeof(MyCommands)
                );

        public static readonly RoutedUICommand Neu = new RoutedUICommand
                       (
                               "Neu",
                               "Neu",
                               typeof(MyCommands)
                       );

        public static readonly RoutedUICommand Loeschen = new RoutedUICommand
                       (
                               "Loeschen",
                               "Loeschen",
                               typeof(MyCommands)
                       );

        public static readonly RoutedUICommand Leeren = new RoutedUICommand
                       (
                               "Leeren",
                               "Leeren",
                               typeof(MyCommands)
                       );

        public static readonly RoutedUICommand SichernDokGruppen = new RoutedUICommand
                       (
                               "SichernDokGruppen",
                               "SichernDokGruppen",
                               typeof(MyCommands)
                       );

        public static readonly RoutedUICommand SichernDokTypen = new RoutedUICommand
                      (
                              "SichernDokTypen",
                              "SichernDokTypen",
                              typeof(MyCommands)
                      );
    }

    public class OkoDokumententyp{
        public int OkoDokumententypId { get; set; }
        public string Bezeichnung { get; set; }
        public string Beschreibung { get; set; }
        public int OkoDokumentengruppenId { get; set; }
    }

    public static class DataGridHelper {
        public static IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }

        public static T DeepClone<T>(T from)
        {
            using (MemoryStream s = new MemoryStream())
            {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize(s, from);
                s.Position = 0;
                object clone = f.Deserialize(s);

                return (T)clone;
            }
        }

        public static DataGrid CloneDataGrid(DataGrid inputVisual)
        {
            DataGrid clonedVisual;
            //Problem seems to be in this method
            //I try to serialize input visual as string
            //But if I inspect this string, it loses all visual markup - has only the data - why ?
            //Hence this functions ends up returning a visually empty datagrid on de-serializing
            string inputVisualAsString = System.Windows.Markup.XamlWriter.Save(inputVisual);
            if (inputVisualAsString == null) return null;

            //Write the string into a memory stream and read it into a new Visual
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(inputVisualAsString.Length))
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(stream))
            {
                sw.Write(inputVisualAsString);
                sw.Flush();
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                //Load from memory stream into a new Visual - On the WPF viewer, the grid looks empty since serialization did not have all the wpf markup.
                clonedVisual = (DataGrid)System.Windows.Markup.XamlReader.Load(stream);
            }

            return clonedVisual;
        }

        
    }
}
