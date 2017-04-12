using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EU4SavegameInfo.NightbotUpdater
{
    [JsonObject]
    internal sealed class CustomCommandsListResponse
    {
        [JsonProperty("commands")]
        public CommandResponse[] Commands { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("_total")]
        public int Total { get; set; }
    }
}