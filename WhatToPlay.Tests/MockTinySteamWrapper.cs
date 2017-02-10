using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TinySteamWrapper;
using TinySteamWrapper.Steam;

namespace WhatToPlay.Tests
{
    public class MockTinySteamWrapper
    {
        public static Object CreateJsonApp()
        {
            Type type = typeof(SteamApp).Assembly.GetType("TinySteamWrapper.Steam.JsonApp");
            var instance = type.Assembly.CreateInstance(type.FullName, false);
            return instance;
        }
        public static SteamApp CreateSteamApp()
        {
            var type = typeof(SteamApp);
            var jsonApp = CreateJsonApp();
            var instance = type.Assembly.CreateInstance(type.FullName, false, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { jsonApp }, null, null);
            return (SteamApp)instance;
        }
        public static void SetValue<T>(T instance, string key, object value)
        {
            var prop = instance.GetType().GetProperty(key);
            prop.SetValue(instance, value);
        }
        public static SteamProfile CreateSteamProfile()
        {

            var game1 = CreateSteamApp();
            SetValue(game1, "Name", "Game1");
            SetValue(game1, "ID", 101);

            var game2 = CreateSteamApp();
            SetValue(game2, "Name", "Game2");
            SetValue(game2, "ID", 102);

            var game3 = CreateSteamApp();
            SetValue(game3, "Name", "Game3");
            SetValue(game3, "ID", 103);

            SteamProfile profile = new SteamProfile();

            profile.Avatar = "Avatar1";
            profile.AvatarFull = "AvatarFull1";
            profile.AvatarMedium = "AvatarMedium1";
            profile.CityID = "CityID1";
            profile.CommunityVisibilityState = CommunityVisibilityState.Public;
            profile.CountryCode = "CountryCode1";

            profile.CurrentGame = game2;

            profile.CurrentGameExtraInfo = "CurrentGameExtraInfo1";
            profile.CurrentGameServerIP = "CurrentGameServerIP1";
            profile.CurrentGameServerSteamID = 21;
            profile.CurrentLobbySteamID = 22;

            var profileGames = new ObservableCollection<SteamProfileGame>();
            
            profileGames.Add(new SteamProfileGame(game1, TimeSpan.FromHours(1)));
            profileGames.Add(new SteamProfileGame(game2, TimeSpan.FromHours(2)));
            profileGames.Add(new SteamProfileGame(game3, TimeSpan.FromHours(3)));
            SetValue(profile, "Games", profileGames);

            profile.LastLogOff = new DateTime(2016, 6, 15);
            profile.PersonaName = "PersonaName1";
            profile.PersonaState = PersonaState.Online;
            profile.PersonaStateFlags = 23;
            profile.PrimaryClanID = 24;
            profile.ProfileState = 25;
            profile.ProfileUrl = "ProfileUrl1";
            profile.RealName = "RealName1";
            profile.StateCode = "StateCode1";
            profile.SteamID = 26;
            profile.TimeCreated = new DateTime(2016, 9, 21);
            return profile;
        }
    }
}
