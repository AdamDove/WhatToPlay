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

        private Steam m_Steam = null;
        public Steam Steam { get { return m_Steam; } }
        private bool _twoFactorAuthenticationRequired = false;
        private bool _emailAuthenticationRequired = false;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool EmailAuthenticationRequired
        {
            get { return _emailAuthenticationRequired; }
            set
            {
                _emailAuthenticationRequired = value;
                RaisePropertyChangedEvent(nameof(EmailAuthenticationRequired));
            }
        }
        public bool TwoFactorAuthenticationRequired
        {
            get { return _twoFactorAuthenticationRequired; }
            set
            {
                _twoFactorAuthenticationRequired = value;
                RaisePropertyChangedEvent(nameof(TwoFactorAuthenticationRequired));
            }
        }
        public string AuthenticationCode { get; set; }
        public ICommand EmailAuthenticationEntered
        {
            get { return new CommandDelegate(OnEmailAuthencationEntered, true); }
        }
        public ICommand TwoFactorAuthenticationEntered
        {
            get { return new CommandDelegate(OnTwoFactorAuthenticationEntered, true); }
        }
        public bool LoginRequired { get; private set; }
        public bool RememberMe { get; set; }

        public void Login()
        {
            if (Settings.Default.RememberMe)
            {
                String username = Settings.Default.SteamUserName;
                SecurePassword password = new SecurePassword();
                password.Load();
                Connect(username, password);
            }
            else
            {
                LoginRequired = true;
            }
        }
        public void Connect(String username, SecurePassword password)
        {
            m_Steam = new Steam(username, password, Settings.Default.SteamAPIKey, this);
            m_Steam.Start();
            LoginRequired = false;
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
    }
}
