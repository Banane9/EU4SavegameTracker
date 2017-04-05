using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EU4Savegames
{
    /// <summary>
    /// Represents the information about each great power that's stored in the save file.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public sealed class GreatPower
    {
        /// <summary>
        /// Gets the GP's rank.
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Gets the GP's score.
        /// </summary>
        public float Score { get; set; }

        /// <summary>
        /// Gets the GP's country tag.
        /// </summary>
        public string Tag { get; set; }

        public GreatPower(StreamReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("="))
                {
                    var split = line.Split('=');
                    switch (split[0].Trim())
                    {
                        case "rank":
                            Rank = int.Parse(split[1]);
                            break;

                        case "country":
                            Tag = split[1].Trim('"');
                            break;

                        case "value":
                            Score = float.Parse(split[1], CultureInfo.InvariantCulture);
                            break;
                    }
                }

                if (line.Contains("}"))
                    break;
            }
        }

        public GreatPower(int rank, string tag, float score)
        {
            Rank = rank;
            Tag = tag;
            Score = score;
        }

        private GreatPower()
        { }

        public static IEnumerable<GreatPower> ReadGreatPowersFromFile(StreamReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
                if (line.StartsWith("great_powers"))
                    break;

            while ((line = reader.ReadLine()) != null)
                if (line.Contains("original"))
                    yield return new GreatPower(reader);
                else if (line.Contains("}"))
                    yield break;
        }
    }
}