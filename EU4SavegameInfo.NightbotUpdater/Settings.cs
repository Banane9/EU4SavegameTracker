using System;
using System.Collections.Generic;
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
    }
}