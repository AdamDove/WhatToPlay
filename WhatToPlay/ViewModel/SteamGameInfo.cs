using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinySteamWrapper;

namespace WhatToPlay.ViewModel
{
    public class SteamGameInfo
    {
        public SteamGameInfo(SteamProfileGame game)
        {
            SteamApp = game.App;
        }
        public SteamGameInfo(SteamApp game)
        {
            SteamApp = game;
        }

        public SteamApp SteamApp { get; set; }
        public long ID { get { return SteamApp.ID; } }
        public string Name { get { return SteamApp.Name; } }
        public string HeaderURL { get { return string.Format("http://cdn4.steampowered.com/v/gfx/apps/{0}/header.jpg", SteamApp.ID); } }
        public string StoreImageURL { get { return string.Format("http://cdn.akamai.steamstatic.com/steam/apps/{0}/capsule_184x69.jpg", SteamApp.ID); } }
    }
}
