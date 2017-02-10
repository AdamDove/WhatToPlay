using System;
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
using System.Security;

namespace WhatToPlay.ViewModel
{
    public class SteamViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Steam m_Steam = null;

        private object m_friendLock = new object();
        private List<SteamProfile> _Friends = new List<SteamProfile>();
        public List<SteamProfile> Friends
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

        public SteamViewModel()
        {
        }

        public void Initialize(Steam steam)
        {
            this.m_Steam = steam;
            m_Steam.OnFriendListUpdate += OnFriendListUpdateCallback;
        }

        private void OnFriendListUpdateCallback(object sender, long steamId)
        {
            //Skip yourself
            if (steamId == m_Steam.OwnSteamId)
                return;

            if (Friends == null)
                return;

            lock (m_friendLock)
            {
                if (!Friends.Any(f => f.SteamID == steamId))
                {
                    //Friend is new to the list
                    Friends.Add(m_Steam.Friends[steamId]);
                    Console.WriteLine("Adding Friend {0}", m_Steam.Friends[steamId].PersonaName);
                }
                else
                {
                    //Update existing Friend
                    int index = Friends.FindIndex(f => f.SteamID == steamId);
                    Console.WriteLine("Updating Friend {0}", m_Steam.Friends[steamId].PersonaName);
                    Friends[index] = m_Steam.Friends[steamId];
                }
                RaisePropertyChangedEvent(nameof(Friends));
            }
            UpdateGamesInCommon();
        }

        private void UpdateGamesInCommon()
        {
            lock (m_friendLock)
            {
                GamesAndPlayers gamesAndPlayers = new GamesAndPlayers(Friends);
                CommonGameList = gamesAndPlayers.GetPerfectMatches();
                CommonGameListMissingOnePlayer = gamesAndPlayers.GetOffByOneMatches();
            }

        }

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void Shutdown()
        {
            if (m_Steam != null)
            {
                m_Steam.Stop();
            }
        }

    }
}
