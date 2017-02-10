using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WhatToPlay.View;
using WhatToPlay.ViewModel;

namespace WhatToPlay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            //Disable shutdown when the dialog closes
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var loginView = new LoginView();

            if (loginView.ShowDialog() == true)
            {
                var mainWindow = new MainWindow(((LoginViewModel)loginView.DataContext).Steam);
                //Re-enable normal shutdown mode.
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                Current.MainWindow = mainWindow;
                mainWindow.Show();
            }
            else
            {
                MessageBox.Show("Login Failed.", "Error", MessageBoxButton.OK);
                Current.Shutdown(-1);
            }
        }
    }
}
