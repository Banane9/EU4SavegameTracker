using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EU4SavegameInfo.NightbotUpdater
{
    [JsonObject]
    internal sealed class Settings
    {
        [JsonProperty]
        public string AccessToken { get; set; }

        [JsonProperty]
        public DateTime ExpiresAt { get; set; }

        [JsonProperty]
        public string RefreshToken { get; set; }

        [JsonProperty]
        public string SavegamePath { get; set; }

        public void Update(string newPath)
        {
            SavegamePath = newPath;
            Update();
        }

        public void Update()
        {
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(this));
        }

        public void Update(TokenResponse tokenResponse)
        {
            AccessToken = tokenResponse.AccessToken;
            ExpiresAt = tokenResponse.ExpiresAt;
            RefreshToken = tokenResponse.RefreshToken;

            Update();
        }
    }
}