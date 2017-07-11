using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EU4Savegames.Objects
{
    [SavegameTag("players_countries")]
    public sealed class PlayerCountriesObject : SavegameObject, IEnumerable<KeyValuePair<string, string>>
    {
        private static readonly char[] trimChars = new[] { '\t', ' ', '"' };
        private readonly Dictionary<string, string> players = new Dictionary<string, string>();

        public string this[string player]
        {
            get
            {
                if (player.Contains(player))
                    return players[player];

                return null;
            }
        }

        public PlayerCountriesObject(IEnumerator reader)
            : base(reader)
        {
            string name = null;
            while (reader.MoveNext())
            {
                var line = (string)reader.Current;

                if (line.Contains('}'))
                    break;

                if (name == null)
                    name = line.Trim(trimChars);
                else
                {
                    players.Add(name, line.Trim(trimChars));
                    name = null;
                }
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string>>)players).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string>>)players).GetEnumerator();
        }
    }
}