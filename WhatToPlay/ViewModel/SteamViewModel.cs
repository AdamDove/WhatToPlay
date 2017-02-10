﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using TinySteamWrapper;
using WhatToPlay.Model;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using WhatToPlay.Common;
using System.Collections.Generic;
using WhatToPlay.Properties;
using System.Reflection;

namespace WhatToPlay.ViewModel
{
    public class SteamViewModel : INotifyPropertyChanged, ISteamGuardPromptHandler
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Steam m_Steam;

        private bool _twoFactorAuthenticationRequired = false;
        private bool _emailAuthenticationRequired = false;

       
        private object m_friendLock = new object();
        private List<Friend> _Friends = new List<Friend>();
        public List<Friend> Friends
        {
            get { return _Friends; }
            set
            {
                _Friends = value;
                RaisePropertyChangedEvent(nameof(Friends));
            }
        }

        private List<SteamGameInfo> _CommonGameList = new List<SteamGameInfo>();
        public List<SteamGameInfo> CommonGameList
        {
            get { return _CommonGameList; }
            set
            {
                _CommonGameList = value;
                RaisePropertyChangedEvent(nameof(CommonGameList));
            }
        } 
        private List<SteamGameAndMissingPlayerInfo> _CommonGameListMissingOnePlayer = new List<SteamGameAndMissingPlayerInfo>();
        public List<SteamGameAndMissingPlayerInfo> CommonGameListMissingOnePlayer
        {
            get { return _CommonGameListMissingOnePlayer; }
            set
            {
                _CommonGameListMissingOnePlayer = value;
                RaisePropertyChangedEvent(nameof(CommonGameListMissingOnePlayer));
            }
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
        public string UserName { get; set; }
        public string Password { get; set; }

        private void OnTwoFactorAuthenticationEntered()
        {
            TwoFactorAuthenticationRequired = false;
        }

        private void OnEmailAuthencationEntered()
        {
            EmailAuthenticationRequired = false;
        }

        public SteamViewModel()
        {
            /* Add your info here and uncomment this section on the first run only.
            Settings.Default.SteamUserName = "YourNameHere";
            Settings.Default.SteamPassword = "YourPasswordHere";
            Settings.Default.SteamAPIKey = "YouGetTheIdea";
            Settings.Default.Save();
            */

            //Yes I am aware that the following line won't work unless you manually set the Settings.  I'll add a prompt for this later.
            m_Steam = new Steam(Settings.Default.SteamUserName, Settings.Default.SteamPassword, Settings.Default.SteamAPIKey, this);
            m_Steam.OnFriendListUpdate += OnFriendListUpdateCallback;

            m_Steam.Start();
        }

        private void OnFriendListUpdateCallback(object sender, long steamId)
        {
            //Skip yourself
            if (Friends == null)
                return;

            lock (m_friendLock)
            {
                if (!Friends.Any(f => f.SteamID == steamId))
                {
                    //Friend is new to the list
                    Friends.Add(new Friend(m_Steam.Friends[steamId]));
                    Console.WriteLine("Adding Friend {0}", m_Steam.Friends[steamId].PersonaName);
                }
                else
                {
                    //Update existing Friend
                    int index = Friends.FindIndex(f => f.SteamID == steamId);
                    Console.WriteLine("Updating Friend {0}", m_Steam.Friends[steamId].PersonaName);
                    Friends[index] = new Friend(m_Steam.Friends[steamId]);
                }
                RaisePropertyChangedEvent(nameof(Friends));
            }
            UpdateGamesInCommon();
        }

        private void UpdateGamesInCommon()
        {
            lock (m_friendLock)
            {
                GamesAndPlayers gamesAndPlayers = new GamesAndPlayers(Friends.Where(f => f.IsSelected).Select(f => (SteamProfile)f).ToList());
                CommonGameList = gamesAndPlayers.GetPerfectMatches();
                CommonGameListMissingOnePlayer = gamesAndPlayers.GetOffByOneMatches();
            }

        }

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string GetEmailAuthenticationCode()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {EmailAuthenticationRequired = true;}));

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

        internal void Shutdown()
        {
            m_Steam.Stop();
        }

    }
}
