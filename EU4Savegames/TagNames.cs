using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EU4Savegames
{
    /// <summary>
    /// Contains methods to resolve tags => localized names.
    /// </summary>
    public static class TagNames
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
        /// Gets the localized name of the given tag for the given language.
        /// <para/>
        /// Returns Unknown if the language or tag was not found.
        /// </summary>
        /// <param name="language">The name of the language.</param>
        /// <param name="tag">The tag to resolve.</param>
        /// <returns>The name of the tag in the given language or Unknown.</returns>
        public static string GetEntry(string language, string tag)
        {
            if (languages.Value.ContainsKey(language))
                if (languages.Value[language].ContainsKey(tag))
                    return languages.Value[language][tag];

            return "Unknown";
        }

        private static Dictionary<string, Dictionary<string, string>> initialize()
        {
            var languages = new Dictionary<string, Dictionary<string, string>>();

            var assembly = Assembly.GetExecutingAssembly();
            var tagFiles = assembly.GetManifestResourceNames()
                                    .Where(name => name.Contains("tag-names"));

            foreach (var tagFile in tagFiles)
            {
                var dashIndex = tagFile.LastIndexOf('-') + 1;
                var length = dashIndex - tagFile.LastIndexOf('.');
                var language = tagFile.Substring(dashIndex, length);

                var reader = new StreamReader(assembly.GetManifestResourceStream(tagFile));

                languages.Add(language,
                    reader.GetAllLines().Select(line => line.Split(':')).ToDictionary(
                        split => split[0],
                        split => split[1]));
            }

            return languages;
        }
    }
}