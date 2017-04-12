using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EU4SavegameInfo.NightbotUpdater
{
    [JsonObject]
    internal sealed class AddCommandResponse
    {
        [JsonProperty("command")]
        public CommandResponse Command { get; private set; }

        [JsonProperty("status")]
        public string Status { get; private set; }
    }
}