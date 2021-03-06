﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TinySteamWrapper;
using SteamKit2;
using WhatToPlay.Properties;
using System.Threading;
using System.Security;
using WhatToPlay.ViewModel;

namespace WhatToPlay.Model
{
    public delegate void NotificationEventHandler(object sender, string message);
    public delegate void FriendAddedHandler(object sender, long steamId);

    public class Steam
    {
        private SteamClient m_steamClient;
        private CallbackManager m_callbackManager;

        private SteamUser m_steamUser;
        private string m_TwoFactorAuth;
        private string m_AuthCode;
        private string m_SteamUserName;
        private SecurePassword m_SteamPassword;
        private SteamFriends m_steamFriends;
        private ISteamGuardPromptHandler m_steamGuardPromptHandler;

        private bool m_isRunning = false;

        private Thread m_steamKitThread;

        #region Events

        public event NotificationEventHandler OnLogonFailure;
        public event NotificationEventHandler OnLogonSuccess;
        public event NotificationEventHandler OnLoggedOff;
        public event NotificationEventHandler OnConnectionFailure;
        public event NotificationEventHandler OnConnectionSuccess;
        public event NotificationEventHandler OnDisconnected;
        public event NotificationEventHandler OnNewPersonaState;
        public event FriendAddedHandler OnFriendListUpdate;
        public event NotificationEventHandler OnFriendRequestAccepted;

        #endregion Events

        #region Properties

        public Dictionary<long, SteamProfile> Friends { get; private set; }
        public long OwnSteamId { get { return (long)m_steamClient.SteamID.ConvertToUInt64(); } }

        #endregion Properties

        public Steam(ISteamGuardPromptHandler steamGuardPromptHandler)
        {
            m_steamGuardPromptHandler = steamGuardPromptHandler;

            //Set up SteamKit
            m_steamClient = new SteamClient();
            m_callbackManager = new CallbackManager(m_steamClient);
            m_steamUser = m_steamClient.GetHandler<SteamUser>();
            m_steamFriends = m_steamClient.GetHandler<SteamFriends>();

            m_callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnSteamClientConnected);
            m_callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnSteamClientDisconnected);
            m_callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnSteamUserLoggedOn);
            m_callbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnSteamUserLoggedOff);
            m_callbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnSteamUserAccountInfo);
            m_callbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnSteamFriendsList);
            m_callbackManager.Subscribe<SteamFriends.PersonaStateCallback>(OnSteamFriendsPersonaChange);
            m_callbackManager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);
            m_callbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnSteamMachineAuth);

            Friends = new Dictionary<long, SteamProfile>();
        }

        public void Login(string steamUserName, SecurePassword steamPassword, string steamWebAPIKey)
        {
            //Check inputs for null (notify if incorrect outside of constructor)
            if (String.IsNullOrEmpty(steamUserName))
                throw new ArgumentNullException("steamUserName");
            if (steamPassword == null)
                throw new ArgumentNullException("steamPassword");
            if (String.IsNullOrEmpty(steamWebAPIKey))
                throw new ArgumentNullException("steamWebAPIKey");

            m_SteamUserName = steamUserName;
            m_SteamPassword = steamPassword;

            //Set the Steam Web API Key as we only need to do this once.
            //This doesn't check whether the key is valid though.
            SteamManager.SteamAPIKey = steamWebAPIKey;

            // Connect to Steam
            m_isRunning = true;
            //Create a thread to manage callbacks
            m_steamKitThread = new Thread(ManageCallbacks);
            m_steamKitThread.IsBackground = true;

            //Start the Callback Manager thread
            m_steamKitThread.Start();
            m_steamClient.Connect();
        }

        ManualResetEvent waitForLastCallback = new ManualResetEvent(false);

        public void Disconnect()
        {
            waitForLastCallback.Reset();
            m_steamClient.Disconnect();
            waitForLastCallback.WaitOne(TimeSpan.FromSeconds(3));//give ManageCallbacks() 1 second to process the disconnect event.
            m_isRunning = false;
        }

        public void Shutdown()
        {
            Disconnect();
            if (m_steamKitThread != null && !m_steamKitThread.Join(TimeSpan.FromSeconds(2))) // give it 2 seconds to finish gracefully.
            {
                m_steamKitThread.Abort(); // if it hasn't finished gracefully, blow it away.
            }
        }

        /// <summary>
        ///   Blocking call to manage SteamKit CallbackManager.  Blocks while m_isRunning is true.
        /// </summary>
        private void ManageCallbacks()
        {
            while (m_isRunning)
            {
                m_callbackManager.RunWaitAllCallbacks(Settings.Default.SteamCallbackManagerPeriod);
                waitForLastCallback.Set(); //let stop() know i've run my last callbacks.
            }
        }

        private void OnSteamMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            int fileSize;
            byte[] sentryHash;
            Settings.Default.SentryFileLocation = callback.FileName;
            Settings.Default.Save();
            using (var fs = File.Open(Settings.Default.SentryFileLocation, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = new SHA1CryptoServiceProvider())
                {
                    sentryHash = sha.ComputeHash(fs);
                }
            }

            // inform the steam servers that we're accepting this sentry file
            m_steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = Settings.Default.SentryFileLocation,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });
        }

        private void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            OnFriendRequestAccepted?.Invoke(this, callback.PersonaName);
        }

        private void OnSteamFriendsPersonaChange(SteamFriends.PersonaStateCallback callback)
        {
            new Task(() =>
            {
                GetSteamProfile(callback.FriendID.ConvertToUInt64());
            }).Start();
        }

        private async void GetSteamProfile(ulong steamId)
        {
            long id = (long)steamId;
            SteamProfile friendProfile = await SteamManager.GetSteamProfileByID(id);

            if (friendProfile != null)
            {
                await SteamManager.LoadGamesForProfile(friendProfile);

                lock (Friends)
                {
                    Friends[friendProfile.SteamID] = friendProfile;
                }
                OnFriendListUpdate?.Invoke(this, friendProfile.SteamID);
            }
        }

        private void OnSteamFriendsList(SteamFriends.FriendsListCallback callback)
        {
            //List of friends received, accept all friend requests
            foreach (var friend in callback.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    m_steamFriends.AddFriend(friend.SteamID);
                }
            }
        }

        private void OnSteamUserAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            ChangePersonaState(EPersonaState.Online);
        }

        private void ChangePersonaState(EPersonaState personaState)
        {
            m_steamFriends.SetPersonaState(personaState);
            OnNewPersonaState?.Invoke(this, personaState.ToString());
        }

        private void OnSteamUserLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            OnLoggedOff?.Invoke(this, string.Format("Logged off of Steam: {0}", callback.Result));
        }

        private void OnSteamUserLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.AccountLogonDenied)
            {
                //Account is Steam Guarded, but doesn't need 2FA, this is a blocking call, when it's done, we have completed this.
                RequestEmailAuthenticationCode();
                //now disconnect
                Disconnect();
                //and reconnect with the needed info.
                Login(m_SteamUserName, m_SteamPassword, SteamManager.SteamAPIKey);
            }
            else if (callback.Result == EResult.AccountLoginDeniedNeedTwoFactor)
            {
                //Account is Steam Guarded, and uses 2FA, this is a blocking call, when it's done, we have completed this.
                RequestTwoFactorAuthenticationCode();
                //now disconnect
                Disconnect();
                //and reconnect with the needed info.
                Login(m_SteamUserName, m_SteamPassword, SteamManager.SteamAPIKey);
            }
            else if (callback.Result != EResult.OK)
            {
                OnLogonFailure?.Invoke(this, string.Format("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult));

                Disconnect();
            }
            else
            {
                OnLogonSuccess?.Invoke(this, string.Format("Successfully logged on!"));
            }
        }

        private void RequestEmailAuthenticationCode()
        {
            m_AuthCode = m_steamGuardPromptHandler?.GetEmailAuthenticationCode();
        }

        private void RequestTwoFactorAuthenticationCode()
        {
            m_TwoFactorAuth = m_steamGuardPromptHandler?.GetEmailAuthenticationCode();
        }

        private void OnSteamClientDisconnected(SteamClient.DisconnectedCallback callback)
        {
            OnDisconnected?.Invoke(this, "Disconnected from Steam.");
        }

        private void OnSteamClientConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                OnConnectionFailure?.Invoke(this, string.Format("Unable to connect to Steam: {0}", callback.Result));

                Disconnect();
                return;
            }

            OnConnectionSuccess?.Invoke(this, string.Format("Connected to Steam! Logging in '{0}'...", m_SteamUserName));

            byte[] sentryHash = null;
            if (File.Exists(Settings.Default.SentryFileLocation))
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes(Settings.Default.SentryFileLocation);
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            m_steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = m_SteamUserName,
                Password = m_SteamPassword.ToPlainText(),

                // this value will be null (which is the default) for our first logon attempt
                AuthCode = m_AuthCode,

                // if the account is using 2-factor auth, we'll provide the two factor code instead
                // this will also be null on our first logon attempt
                TwoFactorCode = m_TwoFactorAuth,

                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            });
        }
    }
}
