using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EU4Savegames;
using EU4Savegames.Localisation;
using EU4Savegames.Objects;
using Newtonsoft.Json;

namespace EU4SavegameInfo.NightbotUpdater
{
    internal sealed class NightbotUpdater
    {
        private readonly Dictionary<string, string> customCommands = new Dictionary<string, string>();

        private readonly Dictionary<string, string> defaultRequiredCommands = new Dictionary<string, string>
        {
            { "!greatpowers", "Great Powers of the World" },
            { "!highscores", "Top 10 highest scoring countries" },
            //{ "!ideas", "Idea Group Picks" },
            { "!players", "Player Countries" }
        };

        private readonly HttpClient httpClient = new HttpClient();
        private readonly Settings settings;

        public NightbotUpdater(Settings settings)
        {
            this.settings = settings;

            establishDefaultRequiredCommands();
        }

        public async Task<bool> Connect()
        {
            if (settings.AccessToken != null && settings.ExpiresAt > DateTime.Now.AddHours(1))
                return true;

            if (settings.RefreshToken != null && settings.ExpiresAt.AddDays(30) > DateTime.Now)
                if (await refreshAccessToken())
                    return true;

            var browserWindow = new OAuthBrowserWindow();
            browserWindow.ShowDialog();

            var authorizeCode = browserWindow.DataContext as string;
            if (authorizeCode == null || string.IsNullOrWhiteSpace(authorizeCode))
                return false;

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.nightbot.tv/oauth2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", "cac06b4cc0205a7fa9c4f223d42e19e7" },
                    { "client_secret", "74c9cf88e9bdccbb37aa8807e525cf65" },
                    { "code", authorizeCode },
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", "https://banane9.github.io/eu4savegameauthenticate" }
                })
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
                return false;

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

            if (!tokenResponse.Scope.Contains("commands"))
                return false;

            settings.Update(tokenResponse);

            establishDefaultRequiredCommands();

            return true;
        }

        public async Task<bool> GetIsConnected()
        {
            return settings.AccessToken != null && (settings.ExpiresAt > DateTime.Now.AddHours(1) || await Connect());
        }

        public async Task<bool> RevokeAccess()
        {
            if (settings.AccessToken == null)
                return true;

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.nightbot.tv/oauth2/token/revoke")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "token", settings.AccessToken },
                })
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            settings.AccessToken = null;
            settings.RefreshToken = null;
            settings.ExpiresAt = default(DateTime);
            settings.Update();

            return true;
        }

        public async Task Update(EU4Save save)
        {
            var playerCountries = save.GetSavegameObjects<PlayerCountriesObject>().SingleOrDefault();

            if (playerCountries != null)
                await UpdateCommand("!players", buildPlayersResponse(playerCountries));

            var countries = save.GetSavegameObjects<CountriesObject>().SingleOrDefault()?.Countries;

            await Task.Delay(15000);

            if (countries != null)
            {
                await UpdateCommand("!highscores", buildHighscoreResponse(countries));

                await Task.Delay(15000);

                await UpdateCommand("!greatpowers", buildGPResponse(countries));

                //var playerCountry = countries.FirstOrDefault(country => country.IsPlayer);
                //if (playerCountry != null)
                //    await UpdateCommand("!ideas", buildIdeasResponse(playerCountry));
            }
        }

        public async Task<bool> UpdateCommand(string name, string message)
        {
            if (!customCommands.ContainsKey(name))
                return await addCommand(name, message);

            var request = new HttpRequestMessage(HttpMethod.Put, "https://api.nightbot.tv/1/commands/" + customCommands[name])
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "message", message }
                })
            };

            request.Headers.Add("Authorization", "Bearer " + settings.AccessToken);

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            return response.IsSuccessStatusCode;
        }

        private async Task<bool> addCommand(string name, string message)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.nightbot.tv/1/commands")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "name", name },
                    { "message", message },
                    { "coolDown", "30" },
                    { "userLevel", "everyone" }
                })
            };

            request.Headers.Add("Authorization", "Bearer " + settings.AccessToken);

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
                return false;

            var commandResponse = JsonConvert.DeserializeObject<AddCommandResponse>(await response.Content.ReadAsStringAsync());
            customCommands.Add(commandResponse.Command.Name, commandResponse.Command.Id);

            return true;
        }

        private string buildGPResponse(CountriesObject.Country[] countries)
        {
            var i = 0;
            return "Great Powers: " + string.Join(", ", countries.OrderByDescending(country => country.GPScore).Take(8)
                .Select(country => $"{++i}. {TagNames.GetEntry("english", country.Tag)} ({(int)Math.Round(country.GPScore)})"));
        }

        private string buildHighscoreResponse(CountriesObject.Country[] countries)
        {
            var i = 0;
            return "Countries with highest Scores: " + string.Join(", ", countries.OrderByDescending(country => country.Score).Take(10)
                .Select(country => $"{++i}. {TagNames.GetEntry("english", country.Tag)} ({(int)Math.Round(country.Score)})"));
        }

        private string buildIdeasResponse(CountriesObject.Country country)
        {
            if (country.Ideas.Length < 1)
                return "No Idea Groups chosen yet!";

            return "Idea Groups: " + string.Join(", ", country.Ideas
                    .Select(idea => $"{IdeaNames.GetEntry("english", idea.Name)} ({idea.Progress})"));
        }

        private string buildPlayersResponse(PlayerCountriesObject playerCountries)
        {
            return $"Players: {string.Join(", ", playerCountries.Select(kvp => $"{kvp.Key} - {(Regex.IsMatch(kvp.Value, "O\\d{2}") ? "Observer" : TagNames.GetEntry("english", kvp.Value))}"))}";
        }

        private async void establishDefaultRequiredCommands()
        {
            if (!await GetIsConnected())
                return;

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.nightbot.tv/1/commands");
            request.Headers.Add("Authorization", "Bearer " + settings.AccessToken);

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            if (response.IsSuccessStatusCode)
            {
                var commandList = JsonConvert.DeserializeObject<CustomCommandsListResponse>(await response.Content.ReadAsStringAsync());

                foreach (var requiredCommand in defaultRequiredCommands)
                {
                    if (!commandList.Commands.Any(cmd => cmd.Name == requiredCommand.Key))
                        await addCommand(requiredCommand.Key, requiredCommand.Value);
                    else
                        customCommands.Add(requiredCommand.Key, commandList.Commands.Single(cmd => cmd.Name == requiredCommand.Key).Id);
                }
            }
        }

        private async Task<bool> refreshAccessToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.nightbot.tv/oauth2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", "cac06b4cc0205a7fa9c4f223d42e19e7" },
                    { "client_secret", "74c9cf88e9bdccbb37aa8807e525cf65" },
                    { "refresh_token", settings.RefreshToken },
                    { "grant_type", "refresh_token" },
                    { "redirect_uri", "https://banane9.github.io/eu4savegameauthenticate" }
                })
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
                return false;

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

            if (!tokenResponse.Scope.Contains("commands"))
                return false;

            settings.Update(tokenResponse);
            return true;
        }
    }
}