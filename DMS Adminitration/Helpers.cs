using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DMS_Adminitration
{
    public static class DataGridHelper
    {
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

    public static class Helpers
    {
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
    }
}
