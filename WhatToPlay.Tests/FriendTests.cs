using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinySteamWrapper;
using TinySteamWrapper.Steam;
using WhatToPlay.Model;

namespace WhatToPlay.Tests
{
    [TestFixture]
    public class FriendTests
    {
        [Test]
        public void TestAllProperties()
        {
            SteamProfile profile = MockTinySteamWrapper.CreateSteamProfile();
            Friend friend = new Friend(profile);
            Assert.AreEqual("Avatar1", friend.Avatar);
            Assert.AreEqual("AvatarFull1", friend.AvatarFull);
            Assert.AreEqual("AvatarMedium1", friend.AvatarMedium);
            Assert.AreEqual("CityID1", friend.CityID);
            Assert.AreEqual(CommunityVisibilityState.Public, friend.CommunityVisibilityState);
            Assert.AreEqual("CountryCode1", friend.CountryCode);
            Assert.AreEqual("Game2", friend.CurrentGame.Name);
            Assert.AreEqual(102, friend.CurrentGame.ID);
            Assert.AreEqual("CurrentGameExtraInfo1", friend.CurrentGameExtraInfo);
            Assert.AreEqual("CurrentGameServerIP1", friend.CurrentGameServerIP);
            Assert.AreEqual(21, friend.CurrentGameServerSteamID);
            Assert.AreEqual(22, friend.CurrentLobbySteamID);

            Assert.AreEqual(3, friend.Games.Count);
            var game1 = friend.Games.Single(g => g.TotalPlayTime.TotalHours == 1);
            var game2 = friend.Games.Single(g => g.TotalPlayTime.TotalHours == 2);
            var game3 = friend.Games.Single(g => g.TotalPlayTime.TotalHours == 3);
            Assert.AreEqual("Game1", game1.App.Name);
            Assert.AreEqual(101, game1.App.ID);

            Assert.AreEqual("Game2", game2.App.Name);
            Assert.AreEqual(102, game2.App.ID);

            Assert.AreEqual("Game3", game3.App.Name);
            Assert.AreEqual(103, game3.App.ID);
            
            Assert.AreEqual(new DateTime(2016, 6, 15), friend.LastLogOff);
            Assert.AreEqual("PersonaName1", friend.PersonaName);
            Assert.AreEqual(PersonaState.Online, friend.PersonaState);
            Assert.AreEqual(23, friend.PersonaStateFlags);
            Assert.AreEqual(24, friend.PrimaryClanID);
            Assert.AreEqual(25, friend.ProfileState);
            Assert.AreEqual("ProfileUrl1", friend.ProfileUrl);
            Assert.AreEqual("RealName1", friend.RealName);
            Assert.AreEqual("StateCode1", friend.StateCode);
            Assert.AreEqual(26, friend.SteamID);
            Assert.AreEqual(new DateTime(2016, 9, 21), friend.TimeCreated);

        }
         
    }
}
