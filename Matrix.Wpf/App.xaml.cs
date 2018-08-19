using NLog;
using System.Windows;
using System.Windows.Threading;

namespace Matrix.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Logger unhandledLogger = LogManager.GetCurrentClassLogger();

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            unhandledLogger.Error(e.Exception.Message, "Unhandled Error");
            e.Handled = true;
        }
    }
}
