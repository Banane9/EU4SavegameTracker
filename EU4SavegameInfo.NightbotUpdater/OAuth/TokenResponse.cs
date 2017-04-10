using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EU4SavegameInfo.NightbotUpdater.OAuth
{
    [JsonObject]
    internal sealed class TokenResponse
    {
        private readonly DateTime creation = DateTime.Now;

        [JsonProperty("access_token")]
        public string AccessToken { get; private set; }

        [JsonIgnore]
        public DateTime ExpiresAt => creation.AddSeconds(ExpiresIn);

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; private set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; private set; }

        [JsonProperty("scope")]
        public string Scope { get; private set; }

        [JsonProperty("token_type")]
        public string TokenType { get; private set; }
    }
}