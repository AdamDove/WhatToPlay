using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WhatToPlay.Common;
using WhatToPlay.Model;
using WhatToPlay.Properties;

namespace WhatToPlay.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged, ISteamGuardPromptHandler
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Steam _steam = null;
        public Steam Steam { get { return _steam; } }

        public string AuthenticationCode { get; set; }

        private bool _emailAuthenticationRequired = false;
        public bool EmailAuthenticationRequired
        {
            get { return _emailAuthenticationRequired; }
            set
            {
                _emailAuthenticationRequired = value;
                RaisePropertyChangedEvent(nameof(EmailAuthenticationRequired));
            }
        }
        
        private bool _twoFactorAuthenticationRequired = false;
        public bool TwoFactorAuthenticationRequired
        {
            get { return _twoFactorAuthenticationRequired; }
            set
            {
                _twoFactorAuthenticationRequired = value;
                RaisePropertyChangedEvent(nameof(TwoFactorAuthenticationRequired));
            }
        }
       
        public ICommand EmailAuthenticationEntered
        {
            get { return new CommandDelegate(OnEmailAuthencationEntered, true); }
        }
        public ICommand TwoFactorAuthenticationEntered
        {
            get { return new CommandDelegate(OnTwoFactorAuthenticationEntered, true); }
        }
        
        private bool _loginSucceeded = false;
        public bool LoginSucceeded
        {
            get { return _loginSucceeded; }
            set
            {
                _loginSucceeded = value;
                RaisePropertyChangedEvent(nameof(LoginSucceeded));
            }
        }
        private bool _loginInProgress = false;
        public bool LoginInProgress
        {
            get { return _loginInProgress; }
            set
            {
                _loginInProgress = value;
                RaisePropertyChangedEvent(nameof(LoginInProgress));
            }
        }

        private bool _rememberMe = false;
        public bool RememberMe
        {
            get { return _rememberMe; }
            set
            {
                _rememberMe = value;
                RaisePropertyChangedEvent(nameof(RememberMe));
            }
        }
        private String _userName = "";
        public String UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChangedEvent(nameof(UserName));
            }
        }

        public LoginViewModel()
        {
            _steam = new Steam(this);
            _steam.OnLogonSuccess += _steam_OnLogonSuccess;
            _steam.OnLogonFailure += _steam_OnLogonFailure;

            RememberMe = Settings.Default.RememberMe;
            if (RememberMe)
            {
                UserName = Settings.Default.SteamUserName;
            }
        }

        private void _steam_OnLogonFailure(object sender, string message)
        {
            LoginInProgress = false;
            LoginSucceeded = false;
        }

        private void _steam_OnLogonSuccess(object sender, string message)
        {
            LoginInProgress = false;
            LoginSucceeded = true;
        }

        public void Connect(String username, SecurePassword password)
        {
            LoginInProgress = true;
            _steam.Login(username, password, Settings.Default.SteamAPIKey);
        }

        private void OnTwoFactorAuthenticationEntered()
        {
            TwoFactorAuthenticationRequired = false;
        }

        private void OnEmailAuthencationEntered()
        {
            EmailAuthenticationRequired = false;
        }

        public string GetEmailAuthenticationCode()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { EmailAuthenticationRequired = true; }));

            while (EmailAuthenticationRequired)
            {
                Thread.Sleep(250);
            }
            return AuthenticationCode;
        }

        public string GetTwoFactorAuthenticationCode()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { TwoFactorAuthenticationRequired = true; }));

            while (TwoFactorAuthenticationRequired)
            {
                Thread.Sleep(250);
            }
            return AuthenticationCode;
        }
        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
