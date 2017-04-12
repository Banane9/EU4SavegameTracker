using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EU4Savegames.Objects
{
    public abstract partial class SavegameObject
    {
        private static readonly Type savegameTagType = typeof(SavegameTagAttribute);

        public static string GetSavegameObjectTag(Type type)
        {
            var tagAttribute = type.GetCustomAttributes(savegameTagType, false).SingleOrDefault();

            return ((SavegameTagAttribute)tagAttribute)?.Tag;
        }

        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        protected sealed class SavegameTagAttribute : Attribute
        {
            public string Tag { get; }

            public SavegameTagAttribute(string tag)
            {
                Tag = tag;
            }
        }
    }
}