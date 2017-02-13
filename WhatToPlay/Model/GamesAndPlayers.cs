using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinySteamWrapper;
using WhatToPlay.ViewModel;

namespace WhatToPlay.Model
{
    /// <summary>
    /// </summary>
    public class GamesAndPlayers
    {
        // <AppID, SteamApp>
        private Dictionary<long, SteamApp> games = new Dictionary<long, SteamApp>();
        // <SteamID, SteamProfile>
        private Dictionary<long, SteamProfile> players = new Dictionary<long, SteamProfile>();
        // <App.ID, <SteamID's>>
        private Dictionary<long, HashSet<long>> gamePlayers = new Dictionary<long, HashSet<long>>();

        public GamesAndPlayers(List<SteamProfile> Friends)
        {
            foreach (SteamProfile friend in Friends)
            {
                foreach (var game in friend.Games)
                {
                    AddGamePlayer(game.App, friend);
                }
            }
        }

        private void AddGamePlayer(SteamApp game, SteamProfile player)
        {
            //record the player
            if (!players.ContainsKey(player.SteamID))
            {
                players.Add(player.SteamID, player);
            }
            //record the game
            if (!games.ContainsKey(game.ID))
            {
                games.Add(game.ID, game);
            }

            //record the players in the games
            if (!gamePlayers.ContainsKey(game.ID))
            {
                gamePlayers.Add(game.ID, new HashSet<long>());
            }

            //add player to game
            if (!gamePlayers[game.ID].Contains(player.SteamID))
            {
                gamePlayers[game.ID].Add(player.SteamID);
            }
        }

        public List<SteamGameInfo> GetPerfectMatches()
        {
            var subset = GetGames(players.Count);
            return subset.Select(kvp => new SteamGameInfo(games[kvp.Key])).ToList();
        }

        public List<SteamGameAndMissingPlayerInfo> GetOffByOneMatches()
        {
            int playerCount = players.Count - 1;

            var subset = GetGames(playerCount);

            List<SteamGameAndMissingPlayerInfo> result = new List<SteamGameAndMissingPlayerInfo>();

            //keyvaluepair is: gameId, hashset<playerId>
            foreach (KeyValuePair<long, HashSet<long>> gameAndPlayers in subset)
            {
                HashSet<long> gamePlayerIds = gameAndPlayers.Value;
                List<long> allPlayerIds = players.Select(p => p.Key).ToList();
                allPlayerIds.RemoveAll(player => gamePlayerIds.Contains(player));

                //allPlayers is now a collection of the missing players
                if (allPlayerIds.Count == 1)
                {
                    result.Add(new SteamGameAndMissingPlayerInfo(games[gameAndPlayers.Key], players[allPlayerIds[0]]));
                }
                else
                {
                    throw new InvalidOperationException("GetGames should have only returned games with 1 missing player.");
                }
            }
            return result;
        }

        private Dictionary<long, HashSet<long>> GetGames(int playerCount)
        {
            return gamePlayers.Where(keyValuePair => keyValuePair.Value.Count == playerCount).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
