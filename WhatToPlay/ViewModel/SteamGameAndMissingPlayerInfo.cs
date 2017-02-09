using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinySteamWrapper;

namespace WhatToPlay.ViewModel
{
    public class SteamGameAndMissingPlayerInfo :SteamGameInfo
    {
        public SteamGameAndMissingPlayerInfo(SteamProfileGame game, SteamProfile missingPlayer) : base(game)
        {
            SteamProfile = missingPlayer;
        }
        public SteamGameAndMissingPlayerInfo(SteamApp game, SteamProfile missingPlayer) : base(game)
        {
            SteamProfile = missingPlayer;
        }

        public SteamProfile SteamProfile { get; set; }
        public long MissingPlayerID { get { return SteamProfile.SteamID; } }
        public string MissingPlayerName { get { return SteamProfile.PersonaName; } }
        public string MissingPlayerProfileURL { get { return string.Format("http://cdn4.steampowered.com/v/gfx/apps/{0}/header.jpg", SteamProfile.ProfileUrl); } }
        public string MissingPlayerAvatarURL { get { return SteamProfile.Avatar; } }
    }
}
