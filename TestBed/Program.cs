using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

using SteamKit2;
using TinySteamWrapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhatToPlay.Model;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TestApp test = new TestApp();
        }

        public class FriendInfo
        {
            public FriendInfo(long id)
            {
                new Task(async () =>
                {
                    Profile = await SteamManager.GetSteamProfileByID(id);
                    await SteamManager.LoadGamesForProfile(Profile);

                }).Start();
            }
            public SteamProfile Profile;
        }

        public class TestApp
        {
            SteamClient steamClient;
            CallbackManager manager;

            SteamUser steamUser;
            SteamFriends steamFriends;

            bool isRunning;

            string user, pass;
            string authCode, twoFactorAuth;

            public TestApp()
            {
                Console.Write("Game ID: ");
                string id = Console.ReadLine();
                GameInformation gameInformation = new GameInformation("", long.Parse(id));
                gameInformation.RetrieveStoreInformation();
                foreach(string tag in gameInformation.Tags)
                {
                    Console.WriteLine("{0}", tag);
                }


                Console.Write("Enter API Key: ");
                SteamManager.SteamAPIKey = Console.ReadLine();


                Console.Write("Enter UserName: ");
                string userName = Console.ReadLine();
                Console.Write("Enter Password: ");
                string password = Console.ReadLine();

                // save our logon details
                user = userName;
                pass = password;


                // create our steamclient instance
                steamClient = new SteamClient();
                // create the callback manager which will route callbacks to function calls
                manager = new CallbackManager(steamClient);

                // get the steamuser handler, which is used for logging on after successfully connecting
                steamUser = steamClient.GetHandler<SteamUser>();
                // get the steam friends handler, which is used for interacting with friends on the network after logging on
                steamFriends = steamClient.GetHandler<SteamFriends>();

                // register a few callbacks we're interested in
                // these are registered upon creation to a callback manager, which will then route the callbacks
                // to the functions specified
                manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
                manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

                manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
                manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

                // we use the following callbacks for friends related activities
                manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
                manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
                manager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);
                manager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);

                // this callback is triggered when the steam servers wish for the client to store the sentry file
                manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

                isRunning = true;

                Console.WriteLine("Connecting to Steam...");

                // initiate the connection
                steamClient.Connect();

                // create our callback handling loop
                while (isRunning)
                {
                    // in order for the callbacks to get routed, they need to be handled by the manager
                    manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                }
            }

            void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
            {
                // someone accepted our friend request, or we accepted one
                Console.WriteLine("{0} is now a friend", callback.PersonaName);
            }

            void OnFriendsList(SteamFriends.FriendsListCallback callback)
            {
                // at this point, the client has received it's friends list

                int friendCount = steamFriends.GetFriendCount();

                Console.WriteLine("We have {0} friends", friendCount);

                for (int x = 0; x < friendCount; x++)
                {
                    // steamids identify objects that exist on the steam network, such as friends, as an example
                    SteamID steamIdFriend = steamFriends.GetFriendByIndex(x);
                    string friendPersonaName = steamFriends.GetFriendPersonaName(steamIdFriend);
                    string friendRender = steamIdFriend.Render();

                    // we'll just display the STEAM_ rendered version
                    Console.WriteLine("Friend: {0} - {1}", friendPersonaName, friendRender);
                }

                // we can also iterate over our friendslist to accept or decline any pending invites

                foreach (var friend in callback.FriendList)
                {
                    if (friend.Relationship == EFriendRelationship.RequestRecipient)
                    {
                        // this user has added us, let's add him back
                        Console.WriteLine("Friend Request: {0}", friend.SteamID);
                        steamFriends.AddFriend(friend.SteamID);
                        Console.WriteLine("Accepting Friend request.");
                    }
                }
            }

            void OnAccountInfo(SteamUser.AccountInfoCallback callback)
            {
                // before being able to interact with friends, you must wait for the account info callback
                // this callback is posted shortly after a successful logon

                // at this point, we can go online on friends, so lets do that
                steamFriends.SetPersonaState(EPersonaState.LookingToPlay);
            }

            void OnPersonaState(SteamFriends.PersonaStateCallback callback)
            {
                // this callback is received when the persona state (friend information) of a friend changes

                // for this sample we'll simply display the names of the friends
                Console.WriteLine("State change: {0} - {1}", callback.Name, callback.State);

                new Task(() =>
                {
                    Test(callback.FriendID.ConvertToUInt64(), callback.Name);
                }).Start();

            }

            private async void Test(ulong id, string name)
            {
                SteamProfile test = await SteamManager.GetSteamProfileByID((long)id);
                await SteamManager.LoadGamesForProfile(test);
                Console.WriteLine("User {0} Game List:", test.PersonaName);
                foreach (SteamProfileGame game in test.Games)
                {
                    Console.WriteLine("     {0}", game.App.Name);
                }
            }

            void OnConnected(SteamClient.ConnectedCallback callback)
            {
                if (callback.Result != EResult.OK)
                {
                    Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);

                    isRunning = false;
                    return;
                }

                Console.WriteLine("Connected to Steam! Logging in '{0}'...", user);

                byte[] sentryHash = null;
                if (File.Exists("sentry.bin"))
                {
                    // if we have a saved sentry file, read and sha-1 hash it
                    byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                    sentryHash = CryptoHelper.SHAHash(sentryFile);
                }

                steamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = user,
                    Password = pass,

                    // in this sample, we pass in an additional authcode
                    // this value will be null (which is the default) for our first logon attempt
                    AuthCode = authCode,

                    // if the account is using 2-factor auth, we'll provide the two factor code instead
                    // this will also be null on our first logon attempt
                    TwoFactorCode = twoFactorAuth,

                    // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                    // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                    SentryFileHash = sentryHash,
                });
            }

            void OnDisconnected(SteamClient.DisconnectedCallback callback)
            {
                // after recieving an AccountLogonDenied, we'll be disconnected from steam
                // so after we read an authcode from the user, we need to reconnect to begin the logon flow again

                Console.WriteLine("Disconnected from Steam, reconnecting in 5...");

                Thread.Sleep(TimeSpan.FromSeconds(5));

                steamClient.Connect();
            }

            void OnLoggedOn(SteamUser.LoggedOnCallback callback)
            {
                bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
                bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

                if (isSteamGuard || is2FA)
                {
                    Console.WriteLine("This account is SteamGuard protected!");

                    if (is2FA)
                    {
                        Console.Write("Please enter your 2 factor auth code from your authenticator app: ");
                        twoFactorAuth = Console.ReadLine();
                    }
                    else
                    {
                        Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                        authCode = Console.ReadLine();
                    }

                    return;
                }

                if (callback.Result != EResult.OK)
                {
                    Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                    isRunning = false;
                    return;
                }

                Console.WriteLine("Successfully logged on!");
            }

            void OnLoggedOff(SteamUser.LoggedOffCallback callback)
            {
                Console.WriteLine("Logged off of Steam: {0}", callback.Result);
            }

            void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
            {
                Console.WriteLine("Updating sentryfile...");

                // write out our sentry file
                // ideally we'd want to write to the filename specified in the callback
                // but then this sample would require more code to find the correct sentry file to read during logon
                // for the sake of simplicity, we'll just use "sentry.bin"

                int fileSize;
                byte[] sentryHash;
                using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
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
                steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
                {
                    JobID = callback.JobID,

                    FileName = callback.FileName,

                    BytesWritten = callback.BytesToWrite,
                    FileSize = fileSize,
                    Offset = callback.Offset,

                    Result = EResult.OK,
                    LastError = 0,

                    OneTimePassword = callback.OneTimePassword,

                    SentryFileHash = sentryHash,
                });

                Console.WriteLine("Done!");
            }
        }
    }
}
