using System;
using System.Collections;
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

        public CountriesObject(IEnumerator reader)
            : base(reader)
        {
            var countries = new List<Country>();

            while (reader.MoveNext())
            {
                var line = (string)reader.Current;

                var split = line.Trim().Split('=');
                if (split[0].Length == 3)
                {
                    countries.Add(new Country(split[0], reader));
                    continue;
                }

                if (line.Contains('}'))
                    break;
            }

            Countries = countries.ToArray();
        }

        [JsonObject(MemberSerialization.OptOut)]
        public sealed class Country
        {
            public float GPScore { get; private set; }
            public Idea[] Ideas { get; private set; } = new Idea[0];
            public bool IsPlayer { get; private set; }
            public float Score { get; private set; }
            public string Tag { get; private set; }

            public Country(string tag, IEnumerator reader)
            {
                Tag = tag;

                var openedBraces = 0;
                while (reader.MoveNext())
                {
                    var line = (string)reader.Current;
                    if (line.Contains("}"))
                        if (openedBraces == 0)
                            break;
                        else
                            --openedBraces;

                    if (openedBraces > 0)
                    {
                        if (line.Contains('{'))
                            ++openedBraces;

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

                        case "human":
                            IsPlayer = split[1] == "yes";
                            break;

                        case "active_idea_groups":
                            var ideas = new List<Idea>();
                            while (reader.MoveNext())
                            {
                                line = (string)reader.Current;
                                if (line.Contains('}'))
                                    break;

                                if (line.Split('=').Length == 2)
                                    ideas.Add(new Idea(line));
                            }

                            Ideas = ideas.ToArray();
                            break;
                    }

                    if (line.Contains('{'))
                        ++openedBraces;
                }
            }

            public sealed class Idea
            {
                public string Name { get; private set; }
                public int Progress { get; private set; }

                public Idea(string line)
                {
                    var split = line.Trim().Split('=');

                    Name = split[0];
                    Progress = int.Parse(split[1]);
                }
            }
        }
    }
}