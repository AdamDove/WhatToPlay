using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TinySteamWrapper;
using WhatToPlay.Model;

namespace WhatToPlay.ViewModel
{
    public class SteamViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Steam m_Steam = null;

        private object m_friendLock = new object();
        private List<Friend> _Friends = new List<Friend>();
        public List<Friend> Friends
        {
            get
            {
                return _Friends;
            }
            set
            {
                _Friends = value;
                RaisePropertyChangedEvent(nameof(Friends));
            }
        }

        private bool _ShouldIncludeThisAccount;
        public bool ShouldIncludeThisAccount
        {
            get { return _ShouldIncludeThisAccount; }
            set
            {
                if (_ShouldIncludeThisAccount != value)
                {
                    _ShouldIncludeThisAccount = value;

                    if (value)
                        addThisAccountToFriendsList();
                    else
                        removeThisAccountFromFriendsList();

                    RaisePropertyChangedEvent(nameof(ShouldIncludeThisAccount));
                }
            }
        }

        private bool _ShouldShowOfflineUsers;
        public bool ShouldShowOfflineUsers
        {
            get { return _ShouldShowOfflineUsers; }
            set
            {
                if (_ShouldShowOfflineUsers != value)
                {
                    _ShouldShowOfflineUsers = value;

                    if (value)
                        addOfflineUsers();
                    else
                        removeOfflineUsers();

                    RaisePropertyChangedEvent(nameof(ShouldShowOfflineUsers));
                }
            }
        }

        private List<SteamGameInfo> _CommonGameList = new List<SteamGameInfo>();
        public List<SteamGameInfo> CommonGameList
        {
            get
            {
                return _CommonGameList;
            }
            set
            {
                _CommonGameList = value.OrderBy(g => g.Name).ToList();
                RaisePropertyChangedEvent(nameof(CommonGameList));
            }
        }

        private List<SteamGameAndMissingPlayerInfo> _CommonGameListMissingOnePlayer = new List<SteamGameAndMissingPlayerInfo>();
        public List<SteamGameAndMissingPlayerInfo> CommonGameListMissingOnePlayer
        {
            get
            {
                return _CommonGameListMissingOnePlayer;
            }
            set
            {
                _CommonGameListMissingOnePlayer = value.OrderBy(g => g.Name).ToList();
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
        
        private void removeOfflineUsers()
        {

        }
        private void addOfflineUsers()
        {

        }
        private void addThisAccountToFriendsList()
        {

        }
        private void removeThisAccountFromFriendsList()
        {

        }

        private void OnFriendListUpdateCallback(object sender, long steamId)
        {
            lock (m_friendLock)
            {
                if (!Friends.Any(f => f.SteamID == steamId))
                {
                    //Friend is new to the list
                    Friend newFriend = new Friend(m_Steam.Friends[steamId]);
                    newFriend.PropertyChanged += FriendOnPropertyChanged;
                    Friends.Add(newFriend);
                    Friends = Friends.OrderBy(f => f.PersonaState == TinySteamWrapper.Steam.PersonaState.Offline).ThenBy(f => f.PersonaName).ToList();
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
        }

        private void FriendOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                UpdateGamesInCommon();
            }
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

        internal void Disconnect()
        {
            if (m_Steam != null)
            {
                m_Steam.Disconnect();
            }
        }
        internal void Shutdown()
        {
            if (m_Steam != null)
            {
                m_Steam.Shutdown();
            }
        }

    }
}
