using System.Windows;

namespace WeSignSetup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            // Application is running
            // Process command line args
            bool isOptionsMode = false;
            bool isUninstallMode = false;
            bool isCleanInstalltion = e.Args.Length == 0;
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "--options")
                {
                    isOptionsMode = true;
                }
                else if(e.Args[i] == "--uninstall")
                {
                    isUninstallMode = true;
                }
            }

            MainWindow mainWindow = new MainWindow(isOptionsMode, isUninstallMode, isCleanInstalltion);
            mainWindow.Show();
        }
    }
}
