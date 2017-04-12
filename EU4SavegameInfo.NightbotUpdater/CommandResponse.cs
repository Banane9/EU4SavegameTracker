using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EU4SavegameInfo.NightbotUpdater
{
    [JsonObject]
    internal sealed class CommandResponse
    {
        [JsonProperty("coolDown")]
        public int Cooldown { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("userLevel")]
        public string UserLevel { get; set; }
    }
}