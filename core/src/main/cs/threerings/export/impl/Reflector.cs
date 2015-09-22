namespace threerings.export.impl {

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

using threerings.trinity.util;

using threerings.export;

public static class Reflector
{
    // TODO
    // For handling shadowed variables, we probably want to "chain" fields with the same name.
    // When writing, we only write out the first one? When reading, we try to fill-in all
    // of the fields where the read object fits?
    // Kookoo!

    public static Dictionary<string, FieldInfo> getExportableFields (Type type)
    {
        Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();
        getExportableFields(type, fields);
        return fields;
    }

    private static void getExportableFields (Type type, Dictionary<string, FieldInfo> infos)
    {
        Type baseType = type.BaseType;
        if (typeof(Exportable).IsAssignableFrom(baseType)) {
            getExportableFields(baseType, infos);
        }

        foreach (FieldInfo field in type.GetFields(FIELD_FLAGS)) {
            if (field.IsNotSerialized) {
                continue;
            }
            string name;
            ExportAttribute exportAttr;
            // see if it's an autoprop field...
            if (Attribute.IsDefined(field, typeof(CompilerGeneratedAttribute))) {
                string fieldName = field.Name;
                int angleStart = fieldName.IndexOf("<");
                int angleEnd = fieldName.IndexOf(">");
                if (angleStart != 0 || angleEnd < 0) {
                    "".logWarning("Did not recognized autoprop name", "fieldName", fieldName);
                    continue;
                }
                name = fieldName.Substring(1, angleEnd - 1); // because angleStart must be 0
                PropertyInfo prop = type.GetProperty(name, PROPERTY_FLAGS);
                if (prop == null) {
                    "".logWarning("Couldn't find property", "name", name);
                    continue;
                }
                exportAttr = (ExportAttribute)
                        Attribute.GetCustomAttribute(prop, typeof(ExportAttribute));
                if (exportAttr == null) {
                    // autoprops are not included by default, they must have the attr
                    continue;
                }

            } else {
                // otherwise it's a regular field; grab the export attr (if present)- ok to be null
                name = field.Name;
                exportAttr = (ExportAttribute)
                        Attribute.GetCustomAttribute(field, typeof(ExportAttribute));
            }

            // Use the name of the specified in the attribute, if any
            if (exportAttr != null && exportAttr.name != null) {
                name = exportAttr.name;

            } else if (name[0] == '_') {
                // otherwise strip off any leading underscore.
                name = name.Substring(1);
            }

            infos.Add(name, field); // throws if there is a duplicate name // TODO
        }
    }

    /** The binding flags we use to search for fields. */
    private const BindingFlags FIELD_FLAGS =
            BindingFlags.DeclaredOnly | // exclude fields from supertypes (we selectively include)
            BindingFlags.Instance |     // exclude static fields
            BindingFlags.Public | BindingFlags.NonPublic; // public OR nonpublic

    /** The binding flags we use to search for properties. */
    private const BindingFlags PROPERTY_FLAGS = FIELD_FLAGS;
}
}
