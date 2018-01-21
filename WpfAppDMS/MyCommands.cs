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

}
