using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TinySteamWrapper;
using WhatToPlay.Model;
using WhatToPlay.ViewModel;

namespace WhatToPlay.Tests
{
    [TestFixture]
    public class GamesAndPlayersTests
    {
     
        [Test]
        public void PerfectMatchesTest()
        {
            List<SteamProfile> Friends = new List<SteamProfile>();

            SteamApp app1 = MockTinySteamWrapper.CreateSteamApp();
            MockTinySteamWrapper.SetValue(app1, "Name", "Game1");
            MockTinySteamWrapper.SetValue(app1, "ID", 101);
            SteamApp app2 = MockTinySteamWrapper.CreateSteamApp();
            MockTinySteamWrapper.SetValue(app2, "Name", "Game2");
            MockTinySteamWrapper.SetValue(app2, "ID", 102);
            SteamApp app3 = MockTinySteamWrapper.CreateSteamApp();
            MockTinySteamWrapper.SetValue(app3, "Name", "Game3");
            MockTinySteamWrapper.SetValue(app3, "ID", 103);

            SteamProfileGame game1 = new SteamProfileGame(app1, TimeSpan.FromHours(1));
            SteamProfileGame game2 = new SteamProfileGame(app2, TimeSpan.FromHours(1));
            SteamProfileGame gameCommon = new SteamProfileGame(app3, TimeSpan.FromHours(1));

            SteamProfile friend1 = new SteamProfile() { SteamID = 1, Avatar = "AvatarUrl1", PersonaState = TinySteamWrapper.Steam.PersonaState.Online };
            friend1.Games.Add(game1);
            friend1.Games.Add(gameCommon);
            Friends.Add(friend1);

            SteamProfile friend2 = new SteamProfile() { SteamID = 2, Avatar = "AvatarUrl2", PersonaState = TinySteamWrapper.Steam.PersonaState.Online };
            friend2.Games.Add(game2);
            friend2.Games.Add(gameCommon);
            Friends.Add(friend2);

            GamesAndPlayers gamesAndPlayers = new GamesAndPlayers(Friends);
            var match = gamesAndPlayers.GetPerfectMatches();
            Assert.AreEqual(1, match.Count);
            Assert.AreEqual(gameCommon.App, match[0].SteamApp);

        }

        [Test]
        public void OffByOneMatchesTest()
        {
            List<SteamProfile> Friends = new List<SteamProfile>();

            SteamApp app1 = MockTinySteamWrapper.CreateSteamApp();
            MockTinySteamWrapper.SetValue(app1, "Name", "Game1");
            MockTinySteamWrapper.SetValue(app1, "ID", 101);
            SteamApp app3 = MockTinySteamWrapper.CreateSteamApp();
            MockTinySteamWrapper.SetValue(app3, "Name", "Game3");
            MockTinySteamWrapper.SetValue(app3, "ID", 103);

            SteamProfileGame game1 = new SteamProfileGame(app1, TimeSpan.FromHours(1));
            SteamProfileGame gameCommon = new SteamProfileGame(app3, TimeSpan.FromHours(1));

            SteamProfile friend1 = new SteamProfile() { SteamID = 1, Avatar = "AvatarUrl1", PersonaState = TinySteamWrapper.Steam.PersonaState.Online };
            friend1.Games.Add(game1);
            friend1.Games.Add(gameCommon);
            Friends.Add(friend1);

            SteamProfile friend2 = new SteamProfile() { SteamID = 2, Avatar = "AvatarUrl2", PersonaState = TinySteamWrapper.Steam.PersonaState.Online };
            friend2.Games.Add(gameCommon);
            Friends.Add(friend2);

            GamesAndPlayers gamesAndPlayers = new GamesAndPlayers(Friends);
            var match = gamesAndPlayers.GetOffByOneMatches();
            Assert.AreEqual(1, match.Count);
            Assert.AreEqual(game1.App, match[0].SteamApp);

        }
        [Test]
        public void OnlyIncludeOnlinePlayers()
        {
            List<SteamProfile> Friends = new List<SteamProfile>();

            SteamApp app1 = MockTinySteamWrapper.CreateSteamApp();
            MockTinySteamWrapper.SetValue(app1, "Name", "Game1");
            MockTinySteamWrapper.SetValue(app1, "ID", 101);
            SteamApp app2 = MockTinySteamWrapper.CreateSteamApp();
            MockTinySteamWrapper.SetValue(app2, "Name", "Game2");
            MockTinySteamWrapper.SetValue(app2, "ID", 102);
            SteamApp app3 = MockTinySteamWrapper.CreateSteamApp();
            MockTinySteamWrapper.SetValue(app3, "Name", "Game3");
            MockTinySteamWrapper.SetValue(app3, "ID", 103);
            SteamApp appCommon = MockTinySteamWrapper.CreateSteamApp();
            MockTinySteamWrapper.SetValue(appCommon, "Name", "GameCommon");
            MockTinySteamWrapper.SetValue(appCommon, "ID", 999);

            SteamProfileGame game1 = new SteamProfileGame(app1, TimeSpan.FromHours(1));
            SteamProfileGame game2 = new SteamProfileGame(app2, TimeSpan.FromHours(1));
            SteamProfileGame game3 = new SteamProfileGame(app3, TimeSpan.FromHours(1));
            SteamProfileGame gameCommon = new SteamProfileGame(appCommon, TimeSpan.FromHours(1));
            {
                SteamProfile friend1 = new SteamProfile() { SteamID = 1, Avatar = "AvatarUrl1", PersonaState = TinySteamWrapper.Steam.PersonaState.Online };
                friend1.Games.Add(game1);
                friend1.Games.Add(gameCommon);
                Friends.Add(friend1);
            }
            {
                //Friend 2 does not have the common game, or the unique games game1, or game2.
                SteamProfile friend2 = new SteamProfile() { SteamID = 2, Avatar = "AvatarUrl2", PersonaState = TinySteamWrapper.Steam.PersonaState.Offline };
                friend2.Games.Add(game2);
                Friends.Add(friend2);
            }
            {
                SteamProfile friend3 = new SteamProfile() { SteamID = 3, Avatar = "AvatarUrl3", PersonaState = TinySteamWrapper.Steam.PersonaState.Online };
                friend3.Games.Add(game3);
                friend3.Games.Add(gameCommon);
                Friends.Add(friend3);
            }
            GamesAndPlayers gamesAndPlayers = new GamesAndPlayers(Friends);

            var match = gamesAndPlayers.GetPerfectMatches();
            Assert.AreEqual(1, match.Count);
            Assert.AreEqual(gameCommon.App, match[0].SteamApp);

            var similar = gamesAndPlayers.GetOffByOneMatches();
            Assert.AreEqual(2, similar.Count);
            Assert.IsNotNull(similar.Find(s => s.ID == game1.App.ID));
            Assert.IsNotNull(similar.Find(s => s.ID == game3.App.ID));

        }
    }
}
