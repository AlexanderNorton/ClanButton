﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Steam.Models.SteamCommunity;
using Steam.Models;

using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;

namespace Clanbutton.Core
{
    public class SteamClient
    {
        public SteamClient()
        {
            SteamUserInterface = new SteamUser("D31F5813BEA5A009B33CF688883F9CD5");
            SteamPlayerInterface = new PlayerService("D31F5813BEA5A009B33CF688883F9CD5");
            SteamAppsInterface = new SteamApps("D31F5813BEA5A009B33CF688883F9CD5");
        }

        public static SteamUser SteamUserInterface { get; set; }
        public static PlayerService SteamPlayerInterface { get; set; }
        public static SteamApps SteamAppsInterface { get; set; }

        // Obtain userId info from API.
        public async Task<PlayerSummaryModel> GetPlayerSummaryAsync(ulong userId)
        {
            var playerSummaryResponse = await SteamUserInterface.GetPlayerSummaryAsync(userId);
            return playerSummaryResponse.Data;
        }

        public async Task<OwnedGamesResultModel> GetPlayerOwnedGamesAsync(ulong userId)
        {
            var playerOwnedGamesResponse = await SteamPlayerInterface.GetOwnedGamesAsync(userId, includeAppInfo:true, includeFreeGames:false);
            return playerOwnedGamesResponse.Data;
        }

        public async Task<IReadOnlyCollection<FriendModel>> GetFriendModels(ulong userId)
        {
            var userFriendsResponse = await SteamUserInterface.GetFriendsListAsync(userId);
            return userFriendsResponse.Data;
        }

        public async Task<RecentlyPlayedGamesResultModel> GetRecentlyPlayed(ulong userId)
        {
            var recentlyPlayedResponse = await SteamPlayerInterface.GetRecentlyPlayedGamesAsync(userId);
            return recentlyPlayedResponse.Data;
        }

        public async Task<IReadOnlyCollection<SteamAppModel>> GetAllSteamGames()
        {
            var steamGames = await SteamAppsInterface.GetAppListAsync();

            return steamGames.Data;
        }

        public async Task<string> GetUserCurrentGame(ulong userId)
        {
            PlayerSummaryModel playerSummary = await GetPlayerSummaryAsync(userId);
            if (playerSummary.PlayingGameName == "")
            {
                return null;
            }
            return playerSummary.PlayingGameName;
        }
    }
}