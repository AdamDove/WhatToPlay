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
using System.Windows.Shapes;
using WhatToPlay.Properties;
using WhatToPlay.ViewModel;

namespace WhatToPlay.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private LoginViewModel ViewModel
        {
            get
            {
                return (LoginViewModel)DataContext;
            }
        }

        public LoginView()
        {
            InitializeComponent();
            ViewModel.PropertyChanged += LoginView_PropertyChanged;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            bool rememberMe = cbxRememberMe.IsChecked.HasValue ? cbxRememberMe.IsChecked.Value : false;
            SecurePassword password = new SecurePassword(txtPassword.SecurePassword, rememberMe);
            Login(rememberMe, password);
        }
        private void Login(bool rememberMe, SecurePassword password)
        {
            string username = txtUserName.Text;

            if (rememberMe)
            {
                Settings.Default.SteamUserName = username;
                Settings.Default.RememberMe = true;
                Settings.Default.Save();
            }
            else
            {
                Settings.Default.SteamUserName = "";
                Settings.Default.RememberMe = false;
                Settings.Default.Save();
            }
            ViewModel.Connect(username, password);
        }

        private void LoginView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LoginSucceeded")
            {
                if (ViewModel.LoginSucceeded)
                {
                    DialogResult = true;
                }
            }
            if (e.PropertyName == "LoginInProgress")
            {
                if (ViewModel.LoginInProgress)
                {
                    txtUserName.IsEnabled = false;
                    txtPassword.IsEnabled = false;
                    cbxRememberMe.IsEnabled = false;
                    btnLogin.IsEnabled = false;
                }
                else
                {
                    txtUserName.IsEnabled = true;
                    txtPassword.IsEnabled = true;
                    cbxRememberMe.IsEnabled = true;
                    btnLogin.IsEnabled = true;
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
