using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMSCommands
{
    public static class MyDMSCommands
    {
        public static readonly RoutedUICommand Verbinden = new RoutedUICommand
                (
                        "Verbinden",
                        "Verbinden",
                        typeof(MyDMSCommands)
                );

        public static readonly RoutedUICommand Neu = new RoutedUICommand
                       (
                               "Neu",
                               "Neu",
                               typeof(MyDMSCommands)
                       );

        public static readonly RoutedUICommand Loeschen = new RoutedUICommand
                       (
                               "Loeschen",
                               "Loeschen",
                               typeof(MyDMSCommands)
                       );

        public static readonly RoutedUICommand Leeren = new RoutedUICommand
                       (
                               "Leeren",
                               "Leeren",
                               typeof(MyDMSCommands)
                       );

        public static readonly RoutedUICommand SichernDokGruppen = new RoutedUICommand
                       (
                               "SichernDokGruppen",
                               "SichernDokGruppen",
                               typeof(MyDMSCommands)
                       );

        public static readonly RoutedUICommand SichernDokTypen = new RoutedUICommand
                      (
                              "SichernDokTypen",
                              "SichernDokTypen",
                              typeof(MyDMSCommands)
                      );
    }

}
