namespace threerings.export2 {

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using threerings.trinity.util;

/**
 * Assists {@link Importer} and {@link Exporter} implementations with reading and writing
 * java-compatible type names.
 */
public static class TypeMapper
{
    /**
     * Get the best type to use given one or both of a type name string & a default value.
     * The type string should represent a C# type.
     */
    public static Type getBestType (/* nullable */string typeStr, /* nullable */Type defaultVal)
    {
        if (typeStr == null && defaultVal == null) {
            throw new ArgumentNullException("one arg must be non-null");
        }

        if (defaultVal != null) {
            // if we have a default type, see if it's generic
            string defStr = defaultVal.FullName;
            string baseDefStr = TypeUtil.stripGeneric(defStr);
            if (defStr != baseDefStr) {
                string concDefStr = makeConcreteType(baseDefStr);
                // TODO: this?
//                if (concDefStr == typeStr) {
//                    return defaultVal;
//                }
                if (typeStr != null) {
                    string concTypeStr = makeConcreteType(typeStr);
                    if (concTypeStr != concDefStr) {
                        string concTypeGStr = concTypeStr + defStr.Substring(baseDefStr.Length);
                        Type concTypeG = TypeUtil.getType(concTypeGStr);
                        if (concTypeG != null && defaultVal.IsAssignableFrom(concTypeG)) {
                            return concTypeG;
                        }

                        log().warning("I've got a bad feeling about these types that don't match",
                                "type", typeStr, "default", defaultVal);
                        return defaultVal;
                    }
                }
                if (concDefStr == baseDefStr) {
                    return defaultVal; // the default type is exactly what we want
                }
                // otherwise put the default's generic arguments onto the concrete class
                typeStr = concDefStr + defStr.Substring(baseDefStr.Length);
                Importer.debug("Modify type lookup", "typeStr", typeStr,
                        "concDefStr", concDefStr, "defStr", defStr);
            }
        }
        if (typeStr != null) {
            try {
                Type lookedUp = TypeUtil.getType(typeStr);
                if (lookedUp != null) {
                    return lookedUp;
                }

                log().warning("Type not found", "typeStr", typeStr);

            } catch (Exception e) {
                log().warning("Type not found", "typeStr", typeStr, e);
            }
            // TEMP: IL2CPP bug chasing
            throw new Exception("The buck stops here");
            // END TEMP
        }
        return defaultVal; // may be null
    }

    /**
     * Make a C# collection interface type into its concrete name (generic type parameters omitted).
     */
    public static string makeConcreteType (string name)
    {
        string conc;
        return CONCRETE.TryGetValue(name, out conc) ? conc : name;
    }

    /**
     * Make a type a concrete, applying the generic arguments to the new type...
     */
    public static Type makeConcreteType (Type type)
    {
        if (!type.IsInterface) {
            return type;
        }
        string name = type.FullName;
        int tick = name.IndexOf('`');
        string key = (tick == -1) ? name : name.Substring(0, tick);
        string concname = makeConcreteType(key);
        if (concname == key) {
            return type;
        }
        if (tick != -1) {
            concname += name.Substring(tick);
        }
        return TypeUtil.getType(concname);
    }

    /**
     * Mogrify a type name for importing.
     */
    public static string convertTypeFromJava (string name)
    {
        // check for arrays...
        if (name.StartsWith("[")) {
            // This is pretty basic and we can make this more sophisticated when needed
            if (name[1] == '[') {
                return convertTypeFromJava(name.Substring(1)) + "[]";

            } else if (name[1] == 'L' && name[name.Length - 1] == ';') {
                return convertTypeFromJava(name.Substring(2, name.Length - 3)) + "[]";
            }
            string primEquiv;
            if (JAVA_TO_CS_PRIMITIVES.TryGetValue(name.Substring(1), out primEquiv)) {
                return primEquiv + "[]";

            } else {
                throw new Exception("TODO");
            }
        }

        // return any mapped replacement
        string mogged;
        if (JAVA_TO_CS.TryGetValue(name, out mogged)) {
           return mogged;
        }

        // handle inner classes
        int innerSep = name.IndexOf('$');
        if (innerSep > -1) {
            return convertTypeFromJava(name.Substring(0, innerSep)) +
                    name.Substring(innerSep).Replace('$', '+');
        }

        // Three rings namespace conversions...
        if (name.StartsWith("com.threerings.")) {
            name = name.Substring(4);
        }

        return name;
    }

    /**
     * Mogrify a type name for exporting.
     */
    public static string convertTypeToJava (string name)
    {
        // check for arrays...
        if (name.EndsWith("[]")) {
            // This is pretty basic and we can make this more sophisticated when needed
            name = name.Substring(0, name.Length - 2);
            if (name.EndsWith("[]")) {
                return "[" + convertTypeToJava(name);
            }
            string primtype;
            return (CS_TO_JAVA_PRIMITIVES.TryGetValue(name, out primtype))
                ? "[" + primtype
                : "[L" + convertTypeToJava(name) + ";";
        }

        // see if there is a mapped replacement from the de-genericized type
        name = TypeUtil.stripGeneric(name);
        string mogged;
        if (CS_TO_JAVA.TryGetValue(name, out mogged)) {
           return mogged;
        }

        // handle inner classes
        int innerSep = name.IndexOf('+');
        if (innerSep > -1) {
            return convertTypeToJava(name.Substring(0, innerSep)) +
                    name.Substring(innerSep).Replace('+', '$');
        }

        // Three rings namespace conversions...
        if (name.StartsWith("threerings.")) {
            name = "com." + name;
        }

        return name;
    }

    /**
     * Get the logger for this class.
     */
    private static Logger log ()
    {
        return Logger.getLogger(typeof(TypeMapper));
    }

    /** Maps collection interfaces to their concrete implementations. */
    private static readonly Dictionary<string, string> CONCRETE = new Dictionary<string, string>()
        {
            { "System.Collections.Generic.IList", "System.Collections.Generic.List" },
            { "System.Collections.Generic.ISet", "System.Collections.Generic.HashSet" },
            { "System.Collections.Generic.IDictionary", "System.Collections.Generic.Dictionary" },
            { "threerings.trinity.util.IMultiset", "threerings.trinity.util.HashMultiset" },
        };

    /** Java -> C# */
    private static readonly Dictionary<string, string> JAVA_TO_CS =
        new Dictionary<string, string>()
        { // initializer
            // system & collection types
            { "java.lang.Object", "System.Object" },
            { "java.lang.String", "System.String" },
            { "java.lang.Boolean", "System.Boolean" },
            { "java.lang.Byte", "System.SByte" },
            { "java.lang.Char", "System.Char" },
            { "java.lang.Short", "System.Int16" },
            { "java.lang.Integer", "System.Int32" },
            { "java.lang.Long", "System.Int64" },
            { "java.lang.Float", "System.Single" },
            { "java.lang.Double", "System.Double" },
            { "java.util.List", "System.Collections.Generic.IList" },
            { "java.util.ArrayList", "System.Collections.Generic.List" },
            { "java.util.Set", "System.Collections.Generic.ISet" },
            { "java.util.HashSet", "System.Collections.Generic.HashSet" },
            { "java.util.Map", "System.Collections.Generic.IDictionary" },
            { "java.util.HashMap", "System.Collections.Generic.Dictionary" },
            { "com.google.common.collect.Multiset", "threerings.trinity.util.IMultiset" },
            { "com.google.common.collect.HashMultiset", "threerings.trinity.util.HashMultiset" },

            // unity specific
            { "com.threerings.math.Vector2f", "UnityEngine.Vector2" },
            { "com.threerings.math.Vector3f", "UnityEngine.Vector3" },
            { "com.threerings.math.Quaternion", "UnityEngine.Quaternion" },
            { "com.threerings.opengl.renderer.Color4f", "UnityEngine.Color" },
            { "com.threerings.trinity.gui.config.StyleConfig$FontStyle", "UnityEngine.FontStyle" },
            { "com.threerings.trinity.gui.config.StyleConfig$TextAnchor", "UnityEngine.TextAnchor" },
        };

    /** Java -> C#, primitive types. */
    private static readonly Dictionary<string, string> JAVA_TO_CS_PRIMITIVES =
        new Dictionary<string, string>()
        { // initializer
            { "Z", "System.Boolean" },
            { "B", "System.SByte" },
            { "C", "System.Char" },
            { "S", "System.Int16" },
            { "I", "System.Int32" },
            { "J", "System.Int64" },
            { "F", "System.Single" },
            { "D", "System.Double" },
        };

    /** C# -> Java. */
    private static readonly Dictionary<string, string> CS_TO_JAVA =
        new Dictionary<string, string>(); // Initialized from JAVA_TO_CS

    /** C# -> Java, primitive types. */
    private static readonly Dictionary<string, string> CS_TO_JAVA_PRIMITIVES =
        new Dictionary<string, string>()
        { // initializer
            { "System.Byte", "B" } // allow sending a byte[] back to java as a byte[]
        }; // additional values are initialized from JAVA_TO_CS_PRIMITIVES

    /**
     * Static initializer: populate the CS_TO_JAVA map from JAVA_TO_CS.
     */
    static TypeMapper ()
    {
        foreach (var entry in JAVA_TO_CS) {
            CS_TO_JAVA[entry.Value] = entry.Key;
        }
        foreach (var entry in JAVA_TO_CS_PRIMITIVES) {
            CS_TO_JAVA_PRIMITIVES[entry.Value] = entry.Key;
        }

        // after populating CS_TO_JAVA, put immutable collections into JAVA_TO_CS
        JAVA_TO_CS["com.google.common.collect.ImmutableList"] = "System.Collections.Generic.List";
        JAVA_TO_CS["com.google.common.collect.ImmutableMap"] =
                "System.Collections.Generic.Dictionary";
        JAVA_TO_CS["com.google.common.collect.ImmutableSet"] = "System.Collections.Generic.HashSet";
        JAVA_TO_CS["com.google.common.collect.ImmutableMultiset"] =
                "threerings.trinity.util.HashMultiset";
    }
}
}
