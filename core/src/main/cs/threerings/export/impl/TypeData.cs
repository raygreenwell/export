namespace threerings.export2.impl {

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using threerings.export; // TEMP
using threerings.trinity.util; // TEMP?

public static class TypeDatas
{
    /** An empty array of types. */
    public static readonly TypeData[] EMPTY_ARRAY = new TypeData[0];

    /** Bootstrap types. */
    public static readonly TypeData[] BOOTSTRAP = new TypeData[] {
            // DO NOT add/remove/reorder without also making the necessary changes for
            // backwards compatability with old exported data.
            ObjectTypeData.INSTANCE, StringTypeData.INSTANCE, BoolTypeData.INSTANCE,
            IntTypeData.INSTANCE, LongTypeData.INSTANCE, ShortTypeData.INSTANCE,
            SbyteTypeData.INSTANCE, CharTypeData.INSTANCE,
            FloatTypeData.INSTANCE, DoubleTypeData.INSTANCE,
            // TODO: nullable primitives? Nullable as a modifier type? ???
            ArrayTypeData.INSTANCE, ListTypeData.INSTANCE, SetTypeData.INSTANCE,
            DictionaryTypeData.INSTANCE, MultisetTypeData.INSTANCE
        };

    public static readonly TypeData[] GENERIC_BOOTSTRAP = BOOTSTRAP
                .Where(t => t.getType().IsGenericType)
                .ToArray();

    /** Indicates that a type is final. */
    public const int IS_FINAL_FLAG = 1 << 0;

    /** Indicates that a type is an inner class. */
    public const int INNER_TYPE_FLAG = 1 << 1; // Unused in C# so far..
}

public abstract class TypeData
{
    virtual
    public bool isFinal ()
    {
        return false; // TODO in subclasses
    }

    virtual
    public bool isValueType ()
    {
        return false;
    }

    abstract
    public Type getType ();

    virtual
    public TypeData[] getTypeArguments ()
    {
        return TypeDatas.EMPTY_ARRAY;
    }

    virtual
    public TypeData getBaseType ()
    {
        return null;
    }

    abstract
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null);

    abstract
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null);
}

public abstract class TypedTypeData : TypeData
{
    public TypedTypeData (Type type)
    {
        _type = type;
    }

    override
    public Type getType ()
    {
        return _type;
    }

    protected readonly Type _type;
}

public class ObjectTypeData : TypeData
{
    public static readonly ObjectTypeData INSTANCE = new ObjectTypeData();

    override
    public Type getType ()
    {
        return typeof(object);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        // TODO: is this correct?
        ctx.writeObject(value, this);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        // TODO: is this correct?
        return ctx.readObject(this);
    }
}

public abstract class FinalTypeData : TypeData
{
    override sealed
    public bool isFinal ()
    {
        return true;
    }
}

public abstract class ValueTypeData : FinalTypeData
{
    override sealed
    public bool isValueType ()
    {
        return true;
    }
}

public class StringTypeData : FinalTypeData
{
    public static readonly StringTypeData INSTANCE = new StringTypeData();

    override
    public Type getType ()
    {
        return typeof(string);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        ctx.writeString((string)value);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        return ctx.readString();
    }
}

public class BoolTypeData : ValueTypeData
{
    public static readonly BoolTypeData INSTANCE = new BoolTypeData();

    override
    public Type getType ()
    {
        return typeof(bool);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        ctx.writeBool((bool)value);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        return ctx.readBool();
    }
}

public class IntTypeData : ValueTypeData
{
    public static readonly IntTypeData INSTANCE = new IntTypeData();

    override
    public Type getType ()
    {
        return typeof(int);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        ctx.writeInt((int)value);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        return ctx.readInt();
    }
}

public class LongTypeData : ValueTypeData
{
    public static readonly LongTypeData INSTANCE = new LongTypeData();

    override
    public Type getType ()
    {
        return typeof(long);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        ctx.writeLong((long)value);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        return ctx.readLong();
    }
}

public class ShortTypeData : ValueTypeData
{
    public static readonly ShortTypeData INSTANCE = new ShortTypeData();

    override
    public Type getType ()
    {
        return typeof(short);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        ctx.writeShort((short)value);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        return ctx.readShort();
    }
}

public class SbyteTypeData : ValueTypeData
{
    public static readonly SbyteTypeData INSTANCE = new SbyteTypeData();

    override
    public Type getType ()
    {
        return typeof(sbyte);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        ctx.writeSbyte((sbyte)value);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        return ctx.readSbyte();
    }
}

public class CharTypeData : ValueTypeData
{
    public static readonly CharTypeData INSTANCE = new CharTypeData();

    override
    public Type getType ()
    {
        return typeof(char);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        ctx.writeChar((char)value);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        return ctx.readChar();
    }
}

public class FloatTypeData : ValueTypeData
{
    public static readonly FloatTypeData INSTANCE = new FloatTypeData();

    override
    public Type getType ()
    {
        return typeof(float);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        ctx.writeFloat((float)value);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        return ctx.readFloat();
    }
}

public class DoubleTypeData : ValueTypeData
{
    public static readonly DoubleTypeData INSTANCE = new DoubleTypeData();

    override
    public Type getType ()
    {
        return typeof(double);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        ctx.writeDouble((double)value);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        return ctx.readDouble();
    }
}

public abstract class EnumerableTypeData : TypeData
{
    protected void writeEntries (
            ExportContext ctx, IEnumerable enumerable, int size, TypeData expectedElementType)
    {
        ctx.writeLength(size);
        foreach (object entry in enumerable) {
            ctx.writeObject(entry, expectedElementType);
        }
    }

    protected void readEntries (
            ImportContext ctx, Action<object> addAction, int size, TypeData expectedElementType)
    {
        for (int ii = 0; ii < size; ii++) {
            addAction(ctx.readObject(expectedElementType));
        }
    }
}

/**
 * List, handles IList.
 */
public class ListTypeData : EnumerableTypeData
{
    public static readonly ListTypeData INSTANCE = new ListTypeData();

    override
    public Type getType ()
    {
        return typeof(List<>);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArgs = null)
    {
        IList list = (IList)value;
        writeEntries(ctx, list, list.Count, typeArgs[0]);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArgs = null)
    {
        int size = ctx.readLength();
        TypeData elementType = typeArgs[0];
        Type listType = getType().MakeGenericType(elementType.getType());
        IList list = (IList)Activator.CreateInstance(listType);
        readEntries(ctx, o => list.Add(o), size, elementType);
        return list;
    }
}

public class SetTypeData : EnumerableTypeData
{
    public static readonly SetTypeData INSTANCE = new SetTypeData();

    override
    public Type getType ()
    {
        return typeof(HashSet<>);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArgs = null)
    {
        // TODO: with a newer version of .NET we could do this better
        IEnumerable en = (IEnumerable)value;
        int size = en.Cast<object>().Count(); // TODO
        writeEntries(ctx, en, size, typeArgs[0]);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArgs = null)
    {
        int size = ctx.readLength();
        TypeData elementType = typeArgs[0];
        Type setType = getType().MakeGenericType(elementType.getType());
        object theSet = Activator.CreateInstance(setType);
        MethodInfo method = setType.GetMethod("Add");
        object[] argArray = new object[1];
        Action<object> addAction = obj => {
                argArray[0] = obj;
                method.Invoke(theSet, argArray);
            };
        readEntries(ctx, addAction, size, elementType);
        return theSet;
    }
}

public class ArrayTypeData : EnumerableTypeData
{
    public static readonly ArrayTypeData INSTANCE = new ArrayTypeData();

    override
    public Type getType ()
    {
        return typeof(Array);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArgs = null)
    {
        Array array = (Array)value;
        writeEntries(ctx, array, array.Length, typeArgs[0]);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArgs = null)
    {
        int size = ctx.readLength();
        TypeData elementType = typeArgs[0];
        Array array = Array.CreateInstance(elementType.getType(), size);
        int idx = 0;
        readEntries(ctx, o => array.SetValue(o, idx++), size, elementType);
        return array;
    }
}

public class DictionaryTypeData : TypeData
{
    public static readonly DictionaryTypeData INSTANCE = new DictionaryTypeData();

    override
    public Type getType ()
    {
        return typeof(Dictionary<, >);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArgs = null)
    {
        TypeData keyType = typeArgs[0];
        TypeData valType = typeArgs[1];
        Type fullType = value.GetType();
        int size = (int)fullType.GetProperty("Count").GetValue(value, null);
        ctx.writeLength(size);
        if (size == 0) {
            return;
        }

        Type entryType = typeof(KeyValuePair<,>).MakeGenericType(
                keyType.getType(), valType.getType());
        Type ienumerableType = typeof(IEnumerable<>).MakeGenericType(entryType);
        IEnumerator en = (IEnumerator)
                ienumerableType.GetMethod("GetEnumerator").Invoke(value, null);
        PropertyInfo getKey = entryType.GetProperty("Key");
        PropertyInfo getVal = entryType.GetProperty("Value");
        while (en.MoveNext()) {
            var entry = en.Current;
            ctx.writeObject(getKey.GetValue(entry, null), keyType);
            ctx.writeObject(getVal.GetValue(entry, null), valType);
        }
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArgs = null)
    {
        int size = ctx.readLength();
        TypeData keyType = typeArgs[0];
        TypeData valType = typeArgs[1];
        Type dicType = getType().MakeGenericType(keyType.getType(), valType.getType());
        IDictionary dic = (IDictionary)Activator.CreateInstance(dicType);
        this.logInfo("Going to be reading in...",
                "dic", dic, "keyType", keyType.getType());
        for (int ii = 0; ii < size; ii++) {
            dic.Add(ctx.readObject(keyType), ctx.readObject(valType));
        }
        return dic;
    }
}

public class MultisetTypeData : TypeData
{
    public static readonly MultisetTypeData INSTANCE = new MultisetTypeData();

    override
    public Type getType ()
    {
        return typeof(HashMultiset<>);
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArgs = null)
    {
        // TODO: we should consider making a non-generic IMuliset interface for easy wildcarding
        TypeData elementType = typeArgs[0];
        Type fullType = value.GetType();
        int size = (int)fullType.GetProperty("ElementCount").GetValue(value, null);
        ctx.writeLength(size);
        if (size == 0) {
            return;
        }

        IEnumerable en = (IEnumerable)fullType.GetMethod("EntrySet").Invoke(value, null);
        PropertyInfo getKey = null, getVal = null;
        foreach (var entry in en) {
            if (getKey == null) {
                Type entryType = entry.GetType();
                getKey = entryType.GetProperty("Key");
                getVal = entryType.GetProperty("Value");
            }
            this.logInfo("Writing value of multiset",
                    "value", getKey.GetValue(entry, null));
            ctx.writeObject(getKey.GetValue(entry, null), elementType);
            ctx.writeLength((int)getVal.GetValue(entry, null));
        }
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArgs = null)
    {
        int size = ctx.readLength();
        TypeData elementType = typeArgs[0];
        Type fullType = typeof(HashMultiset<>).MakeGenericType(elementType.getType());
        object value = Activator.CreateInstance(fullType);
        if (size > 0) {
            MethodInfo method = fullType.GetMethod("Add",
                    new Type[] { elementType.getType(), typeof(int) });
            object[] argArray = new object[2];
            for (int ii = 0; ii < size; ii++) {
                argArray[0] = ctx.readObject(elementType);
                argArray[1] = ctx.readLength();
                method.Invoke(value, argArray);
            }
        }
        return value;
    }
}

public class EnumTypeData : TypedTypeData
{
    public EnumTypeData (Type fullType) : base(fullType)
    {
    }

    override
    public bool isFinal ()
    {
        return true;
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArgs = null)
    {
        string s = Enum.GetName(_type, value);
        if (s == null) {
            throw new ArgumentException("Enum value has no name: " + _type + ": " + value);
        }
        ctx.writeString(s);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArgs = null)
    {
        string s = ctx.readString();
        try {
            return Enum.Parse(_type, s);

        } catch (ArgumentException) {
            ctx.warn("could not find enum constant '" + s + "' in type " + _type);
        }
        return null;
    }
}

/**
 * A TypeData that merely adds type parameters to another TypeData.
 */
public class ParameterizedTypeData : TypedTypeData
{
    /**
     * Construct a ParameterizedTypeData during exporting.
     */
    public ParameterizedTypeData (Type fullType, TypeData baseType, TypeData[] args = null)
        : base(fullType)
    {
        _baseType = baseType;
        _args = args;
    }

    // TODO: isFinal... maybe if the base class if final and all the parameter types are final?

    /**
     * Construct a ParameterizedTypeData during importing.
     */
    public ParameterizedTypeData (TypeData baseType, TypeData[] args = null)
        : this(baseType.getType().MakeGenericType(args.Select(arg => arg.getType()).ToArray()),
                baseType, args)
    { }

    override
    public TypeData[] getTypeArguments ()
    {
        return _args;
    }

    override
    public TypeData getBaseType ()
    {
        return _baseType;
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArgs = null)
    {
        _baseType.writeObject(ctx, value, _args);
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArgs = null)
    {
        return _baseType.readObject(ctx, _args);
    }

    protected readonly TypeData _baseType;

    protected readonly TypeData[] _args;
}

///**
// * A TypeData for reading/writing an Exportable class via reflection.
// */
//public class ExportableTypeData : TypedTypeData
//{
//    public ExportableTypeData (Type fullType) : base(fullType)
//    {
//        object prototype = Activator.CreateInstance(fullType);
//        _fields = getExportableFields(type, prototype).ToArray();
//
//        // TODO : Examine fields... assign them all ids
//        // it's hugely unlikely that a class will have more than 127 fields, so assigning
//        // fieldIds for all fields in a class is probably fine. Urgh... except that when
//        // READING fields we must follow the fieldIds from the stream.
//        // TODO
//    }
//
//    override
//    public bool isSealed ()
//    {
//        return _type.IsSealed;
//    }
//
//    override
//    public void writeObject (ExportContext ctx, object value, TypeData[] typeArgs = null)
//    {
//        // TODO
//    }
//
//    override
//    public object readObject (ImportContext ctx, TypeData[] typeArgs = null)
//    {
//        // TODO
//        return Activator.CreateInstance(_type);
//    }
//
//    protected IEnumerable<FieldData> getExportableFields (Type type, object prototype)
//    {
//        Type baseType = type.BaseType;
//        if (typeof(Exportable).IsAssignableFrom(baseType)) {
//            foreach (FieldData field in getExportableFields(baseType, prototype)) {
//                yield return field;
//            }
//        }
//
//        foreach (FieldInfo field in type.GetFields(FIELD_FLAGS)) {
//            if (field.IsNotSerialized) {
//                continue;
//            }
//            string name;
//            ExportAttribute exportAttr;
//            // see if it's an autoprop field...
//            if (Attribute.IsDefined(field, typeof(CompilerGeneratedAttribute))) {
//                string fieldName = field.Name;
//                int angleStart = fieldName.IndexOf("<");
//                int angleEnd = fieldName.IndexOf(">");
//                if (angleStart != 0 || angleEnd < 0) {
//                    this.logWarning("Did not recognized autoprop name", "fieldName", fieldName);
//                    continue;
//                }
//                name = fieldName.Substring(1, angleEnd - 1); // because angleStart must be 0
//                PropertyInfo prop = type.GetProperty(name, PROPERTY_FLAGS);
//                if (prop == null) {
//                    this.logWarning("Couldn't find property", "name", name);
//                    continue;
//                }
//                exportAttr = (ExportAttribute)
//                        Attribute.GetCustomAttribute(prop, typeof(ExportAttribute));
//                if (exportAttr == null) {
//                    // autoprops are not included by default, they must have the attr
//                    continue;
//                }
//
//            } else {
//                // otherwise it's a regular field; grab the export attr (if present)- ok to be null
//                name = field.Name;
//                exportAttr = (ExportAttribute)
//                        Attribute.GetCustomAttribute(field, typeof(ExportAttribute));
//            }
//            yield return new FieldData(field, exportAttr, name, prototype);
//        }
//    }
//
//    protected class FieldData
//    {
//    }
//}
}
