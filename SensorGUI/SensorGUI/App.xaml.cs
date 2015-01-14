using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace SensorGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();

            wnd.Title = "Sensor GUI";

            wnd.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show("An unhandled exception has occured: " + e.Exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }
    }
}
