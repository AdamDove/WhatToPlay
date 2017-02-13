using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
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

        #region EmailAuthentication
        ManualResetEvent waitForEmailAuthentication = new ManualResetEvent(false);
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
        public ICommand EmailAuthenticationEntered
        {
            get { return new CommandDelegate(OnEmailAuthencationEntered, true); }
        }
        #endregion

        #region TwoFactorAuthentication
        ManualResetEvent waitFor2FactorAuthentication = new ManualResetEvent(false);
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
        public ICommand TwoFactorAuthenticationEntered
        {
            get { return new CommandDelegate(OnTwoFactorAuthenticationEntered, true); }
        }
        #endregion

        #region Login
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

        private String _errorMessage = "";
        public String ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisePropertyChangedEvent(nameof(ErrorMessage));
            }
        }
        #endregion

        #region Saved Settings
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
        private SecurePassword _securePassword;
        public SecurePassword SecurePassword
        {
            get { return _securePassword; }
            set
            {
                _securePassword = value;
            }
        }
        #endregion

        public LoginViewModel()
        {
            _steam = new Steam(this);
            _steam.OnLogonSuccess += _steam_OnLogonSuccess;
            _steam.OnLogonFailure += _steam_OnLogonFailure;
            _steam.OnLoggedOff += _steam_OnLoggedOff;

            RememberMe = Settings.Default.RememberMe;
            if (RememberMe)
            {
                UserName = Settings.Default.SteamUserName;
                SecurePassword = SecurePassword.Load();
            }
        }

        #region Event Handlers

        private void _steam_OnLoggedOff(object sender, string message)
        {
            LoginInProgress = false;
            LoginSucceeded = false;
            ErrorMessage = message;
        }

        private void _steam_OnLogonFailure(object sender, string message)
        {
            LoginInProgress = false;
            LoginSucceeded = false;
            ErrorMessage = message;
        }

        private void _steam_OnLogonSuccess(object sender, string message)
        {
            LoginInProgress = false;
            LoginSucceeded = true;
            ErrorMessage = "";
        }

        #endregion

        #region UI Event Handlers

        public void Connect()
        {
            if (RememberMe)
            {
                Connect(UserName, SecurePassword);
            }
        }
        public void Connect(String username, SecurePassword password)
        {
            LoginInProgress = true;
            ErrorMessage = "";
            _steam.Login(username, password, Settings.Default.SteamAPIKey);
        }

        private void OnTwoFactorAuthenticationEntered()
        {
            TwoFactorAuthenticationRequired = false;
            waitFor2FactorAuthentication.Set(); //unblock this
        }

        private void OnEmailAuthencationEntered()
        {
            EmailAuthenticationRequired = false;
            waitForEmailAuthentication.Set(); //unblock this 
        }

        #endregion

        #region ISteamGuardPromptHandler

        public string GetEmailAuthenticationCode()
        {
            waitForEmailAuthentication.Reset(); // this will now block until '.Set()' is called.
            Application.Current.Dispatcher.Invoke(new Action(() => { EmailAuthenticationRequired = true;}));
            waitForEmailAuthentication.WaitOne();
            return AuthenticationCode;
        }

        public string GetTwoFactorAuthenticationCode()
        {
            waitFor2FactorAuthentication.Reset();
            Application.Current.Dispatcher.Invoke(new Action(() => { TwoFactorAuthenticationRequired = true; }));
            waitFor2FactorAuthentication.WaitOne();
            return AuthenticationCode;
        }

        #endregion

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
