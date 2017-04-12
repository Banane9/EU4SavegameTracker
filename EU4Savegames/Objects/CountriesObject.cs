using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EU4Savegames.Objects
{
    [SavegameTag("countries")]
    [JsonObject(MemberSerialization.OptOut)]
    public sealed class CountriesObject : SavegameObject
    {
        public Country[] Countries { get; private set; }

        public CountriesObject(StreamReader reader)
            : base(reader)
        {
            var countries = new List<Country>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains('}'))
                    break;

                var split = line.Trim().Split('=');
                if (split[0].Length == 3)
                    countries.Add(new Country(split[0], reader));
            }

            Countries = countries.ToArray();
        }

        [JsonObject(MemberSerialization.OptOut)]
        public sealed class Country
        {
            public float GPScore { get; private set; }
            public float Score { get; private set; }
            public string Tag { get; private set; }

            public Country(string tag, StreamReader reader)
            {
                Tag = tag;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("="))
                    {
                        if (line.Contains('{'))
                        {
                            reader.ReadTillMatchingClosingBrace();
                            continue;
                        }

                        var split = line.Trim().Split('=');
                        switch (split[0])
                        {
                            case "great_power_score":
                                GPScore = float.Parse(split[1], CultureInfo.InvariantCulture);
                                break;

                            case "score":
                                Score = float.Parse(split[1], CultureInfo.InvariantCulture);
                                break;
                        }
                    }

                    if (line.Contains("}"))
                        break;
                }
            }
        }
    }
}