using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
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
    }
}
