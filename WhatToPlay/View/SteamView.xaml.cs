using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TinySteamWrapper;
using WhatToPlay.Properties;
using WhatToPlay.ViewModel;

namespace WhatToPlay.View
{
    /// <summary>
    /// Interaction logic for SteamView.xaml
    /// </summary>
    public partial class SteamView : UserControl
    {
        public SteamView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SteamViewModel model = (DataContext as SteamViewModel);
            
            bool rememberMe = cbxRememberMe.IsChecked.HasValue ? cbxRememberMe.IsChecked.Value : false;
            string username = txtUserName.Text;

            SecurePassword password = new SecurePassword(PasswordBox.SecurePassword, rememberMe);
            if (rememberMe)
            {
                Settings.Default.SteamUserName = username;
                Settings.Default.RememberMe = true;
            }
            else
            {
                Settings.Default.SteamUserName = "";
                Settings.Default.RememberMe = false;
            }
            model.Connect(username, password);
        }
    }
}
