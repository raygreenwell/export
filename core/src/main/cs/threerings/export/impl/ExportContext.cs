namespace threerings.export2.impl {

using System;
using System.Collections.Generic;
using System.IO;

using MiscUtil.IO;

using threerings.export; // TODO
using threerings.trinity.util;

// TODO: stop using eout to write stuff
// Also include the base stream?
public class ExportContext
{
    public readonly EndianBinaryWriter eout;

    public ExportContext (EndianBinaryWriter eout)
    {
        this.eout = eout;
        _stream = eout.BaseStream;

        // populate the bootstrap types
        foreach (TypeData type in TypeDatas.BOOTSTRAP) {
            _typeIds[type] = _nextTypeId++;
            _types[type.getType()] = type;
        }
    }

    // let's assume primitives aren't allowed in here... yeah?

    public void writeObject (object value, TypeData expectedType)
    {
        if (expectedType.isValueType()) {
            writeValue(value, expectedType);
            return;
        }

        // null is objectId 0, but we can't store that in our map...
        if (value == null) {
            writeId(0);
            return;
        }

        // intern strings
        if (value is string) {
            value = string.Intern((string)value); // TODO: use our own interning Dictionary
        }

        // see if we've seen it before
        int objectId;
        if (_objectIds.TryGetValue(value, out objectId)) {
            writeId(objectId);
            return;
        }

        // assign and write the new id
        _objectIds[value] = objectId = _nextObjectId++;
        writeId(objectId);
        // TODO: if writing a new objectId, we can potentially piggy-back some data on this id

        // and then write the value
        writeValue(value, expectedType);
    }

    /**
     * Convenience to write a string to the stream.
     */
    public void writeString (string value)
    {
        Streams.writeVarString(_stream, value);
    }

    public void writeId (int id)
    {
        Streams.writeVarInt(_stream, id);
    }

    public void writeLength (int len)
    {
        Streams.writeVarInt(_stream, len);
    }

    /**
     * Write a <em>non-null</em> value.
     */
    protected void writeValue (object value, TypeData expectedType)
    {
        TypeData typeData;
        if (expectedType.isFinal()) {
            typeData = expectedType;

        } else {
            typeData = getTypeData(value.GetType());
            writeType(typeData);
        }
        typeData.writeObject(this, value); // no type args are passed
    }

    public void writeType (TypeData type)
    {
        int typeId;
        if (_typeIds.TryGetValue(type, out typeId)) {
            writeId(typeId);
            return;
        }

        typeId = _nextTypeId++;
        _typeIds[type] = typeId;

        int typeKind;
        int flags = 0;
        int args = 0;

        // TODO
        if (type is ParameterizedTypeData) {
            typeKind = 2;
            // TODO: Sending the number of args for a built-in type is not necessary!
            args = type.getTypeArguments().Length;

        } else if (type is GenericExportingBaseTypeData) {
            typeKind = 3;
            args = type.getType().GetGenericArguments().Length;

        } else if (type is GenericExportingReflectiveTypeData) {
            typeKind = 1;

        } else {
            if (type.isFinal()) {
                flags |= TypeDatas.IS_FINAL_FLAG;
            }
            typeKind = 0;
        }

        // TODO: remove magic numbers
        int id = typeId + typeKind + (flags << 2) + (args << 3);

        writeId(id);

        switch (typeKind) {
        default:
            writeString(TypeMapper.convertTypeToJava(type.getType().FullName));
            break;

        case 3:
            writeString(TypeMapper.convertTypeToJava(type.getType().FullName));
            break;

        case 1: // fall through
        case 2:
            writeType(type.getBaseType());
            foreach (TypeData argType in type.getTypeArguments()) {
                writeType(argType);
            }
            break;
        }
    }

    protected enum TypeType
    {
        /** A regular class that communicates no hierarchy information.
         * Followed by: name.
         */
        // 0
        REGULAR,

        /** Regular class with generic parameters. ConfigReference<ActorConfig> for example.
         * Followed by: type of base, type of args... */
        // 1
        REGULAR_GENERIC,

        /** Named extension of a generic parameterized type. */
        // 2
        BUILTIN_GENERIC,

        /**
         * A new definition of a base type. Purely sends a name and the expected number of
         * generic arguments.
         */
        // 3
        BASE_TYPE,

        /** Fully parameterizes a generic definition.
         * Followed by: type of base, followed by the types of the args. */
        // TODO: The ArgumentMap case.
        // TO-Friggin'-DO
    }

    public TypeData getTypeData (Type type)
    {
        this.logInfo("getTypeData(" + type + ")");

        TypeData typeData;
        if (_types.TryGetValue(type, out typeData)) {
            return typeData;
        }

        // figure out the TypeData for this type
        /*if (type.IsAbstract || type.IsInterface) {
            typeData = ObjectTypeData.INSTANCE;
            // Can't do this: we need to send real types so that a List<SOMEINTERFACE> works.

        } else*/ if (type.IsArray) {
            TypeData[] elementType = new TypeData[] { getTypeData(type.GetElementType()) };
            typeData = new ParameterizedTypeData(type, ArrayTypeData.INSTANCE, elementType);

        } else if (type.IsGenericType && !type.ContainsGenericParameters) {
            TypeData baseType = getTypeData(type.GetGenericTypeDefinition());
            if (baseType is GenericExportingBaseTypeData) {
                this.logInfo("Full name: " + type.FullName);
                typeData = new GenericExportingReflectiveTypeData(this, type, baseType,
                        getTypeData(type.GetGenericArguments()));
                foreach (Type t in type.GetGenericArguments()) {
                    this.logInfo("type arg: " + t);
                }
            } else {
                typeData = new ParameterizedTypeData(type, baseType,
                        getTypeData(type.GetGenericArguments()));
            }

        } else if (type.IsGenericType && !isBuiltinGenericType(type)) {
            typeData = new GenericExportingBaseTypeData(type);

        } else if (typeof(Exportable).IsAssignableFrom(type)) {
            typeData = new ExportingReflectiveTypeData(type, this);

        } else if (type == typeof(IList<>)) {
            this.logInfo("Plain old list!");
            return ListTypeData.INSTANCE;

        } else {
            TypeData toMap;
            if (findCollectionType(type, typeof(IList<>), ListTypeData.INSTANCE,
                            out toMap, out typeData) ||
                    findCollectionType(type, typeof(IDictionary<,>), DictionaryTypeData.INSTANCE,
                            out toMap, out typeData) ||
                    findCollectionType(type, typeof(IMultiset<>), MultisetTypeData.INSTANCE,
                            out toMap, out typeData)) {
                if (toMap != null) {
                    _types[type] = toMap;
                }
                return typeData;
            }
        }

        if (typeData == null) {
            throw new System.Exception("Could not figure out typeData [type=" + type + "]");
        }

        _types[type] = typeData;
        return typeData;
    }

    /**
     * Turn a Type[] into a TypeData[].
     */
    protected TypeData[] getTypeData (Type[] types)
    {
        TypeData[] datas = new TypeData[types.Length];
        for (int ii = 0, nn = types.Length; ii < nn; ii++) {
            datas[ii] = getTypeData(types[ii]);
        }
        return datas;
    }

    protected bool isBuiltinGenericType (Type type)
    {
        if (!typeof(Exportable).IsAssignableFrom(type)) {
            foreach (TypeData t in TypeDatas.GENERIC_BOOTSTRAP) {
                if (TypeUtil.inheritsOrImplements(type, t.getType())) {
                    return true;
                }
            }
        }
        return false;
    }

    protected bool findCollectionType (
            Type type, Type collType, TypeData instance, out TypeData typeData, out TypeData toMap)
    {
        if (TypeUtil.inheritsOrImplements(type, collType)) {
            foreach (Type t in type.GetInterfaces()) {
                if (TypeUtil.inheritsOrImplements(t, collType)) {
                    if (t.ContainsGenericParameters) {
                        toMap = null;
                        typeData = instance;
                        return true;

                    } else {
                        typeData = new ParameterizedTypeData(type, instance,
                                getTypeData(t.GetGenericArguments()));
                        toMap = typeData;
                        return true;
                    }
                }
            }
        }
        typeData = null;
        toMap = null;
        return false;
    }

    protected Stream _stream;

    protected IDictionary<object, int> _objectIds =
            new Dictionary<object, int>(new IdentityComparer<object>());

    protected int _nextObjectId = 1; // because null is zero

    protected IDictionary<TypeData, int> _typeIds =
            new Dictionary<TypeData, int>(new IdentityComparer<TypeData>());

    protected IDictionary<Type, TypeData> _types = new Dictionary<Type, TypeData>();

    protected int _nextTypeId = 0;
}
}
