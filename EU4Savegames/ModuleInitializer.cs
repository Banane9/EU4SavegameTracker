using System;
using System.Reflection;
using EU4Savegames.Objects;
using System.Linq;
using System.IO;
using System.Collections;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    private static readonly Type savegameObjectType = typeof(SavegameObject);
    private static readonly Type streamReaderType = typeof(IEnumerator);

    /// <summary>
    /// Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes)
        {
            if (!savegameObjectType.IsAssignableFrom(type))
                continue;

            var constructor = type.GetConstructors().SingleOrDefault(ctr =>
            {
                var parameters = ctr.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == streamReaderType;
            });

            var tag = SavegameObject.GetSavegameObjectTag(type);

            if (constructor == null || tag == null)
                continue;

            SavegameObject.AddTagToObjectReader(tag, reader => (SavegameObject)constructor.Invoke(new object[] { reader }));
        }
    }
}