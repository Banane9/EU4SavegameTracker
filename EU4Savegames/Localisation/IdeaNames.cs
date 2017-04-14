using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EU4Savegames.Localisation
{
    /// <summary>
    /// Contains methods to resolve idea groups => localized names.
    /// </summary>
    public static class IdeaNames
    {
        private static readonly Lazy<Dictionary<string, Dictionary<string, string>>> languages = new Lazy<Dictionary<string, Dictionary<string, string>>>(initialize);

        /// <summary>
        /// Gets the languages for which tag-resolution is available.
        /// </summary>
        public static string[] GetAvailableLanguages()
        {
            return languages.Value.Keys.ToArray();
        }

        /// <summary>
        /// Gets the localized name of the given idea group for the given language.
        /// <para/>
        /// Returns Unknown if the language or idea group was not found.
        /// </summary>
        /// <param name="language">The name of the language.</param>
        /// <param name="idea">The idea group to resolve.</param>
        /// <returns>The name of the idea group in the given language or Unknown.</returns>
        public static string GetEntry(string language, string idea)
        {
            if (languages.Value.ContainsKey(language))
                if (languages.Value[language].ContainsKey(idea))
                    return languages.Value[language][idea];

            return "Unknown";
        }

        private static Dictionary<string, Dictionary<string, string>> initialize()
        {
            var languages = new Dictionary<string, Dictionary<string, string>>();

            var assembly = Assembly.GetExecutingAssembly();
            var ideaFiles = assembly.GetManifestResourceNames()
                                    .Where(name => name.Contains("idea-names"));

            foreach (var ideaFile in ideaFiles)
            {
                var dashIndex = ideaFile.LastIndexOf('-') + 1;
                var length = ideaFile.LastIndexOf('.') - dashIndex;
                var language = ideaFile.Substring(dashIndex, length);

                var reader = new StreamReader(assembly.GetManifestResourceStream(ideaFile));

                languages.Add(language,
                    reader.GetAllLines().Select(line => line.Split(':')).ToDictionary(
                        split => split[0],
                        split => split[1]));

                reader.Close();
            }

            return languages;
        }
    }
}