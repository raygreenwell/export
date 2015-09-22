namespace threerings.export2.impl {

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using threerings.export; // TEMP
using threerings.trinity.util;

public class ImportContext
{
    /** Public access to the stream... */
    public Stream stream;

    public ImportContext (Stream inStream)
    {
        stream = inStream;
    }

    // TODO: this will change into something whereby the warning types are enumerated
    // so that they can be suppressed, logged, or throwing.
    public void warn (string msg, params object[] args)
    {
        // TODO
        this.logWarning("Warning importing: " + msg, args);
    }

    public object readObject (TypeData expectedType)
    {
        if (expectedType.isValueType()) {
            return readValue(expectedType);
        }

        int objectId = readId();
        // see if it's one we've already seen
        if (objectId < _objects.Count) {
            return _objects[objectId];
        }

        // verify that objectId is as expected
        if (objectId != _objects.Count) {
            // TODO
            throw new System.Exception("Precondition failed: read bad id.");
        }

        // TODO: we need to pass the objectId to the TypeData so that they can call back to a
        // new method on this class for setting the typedata into the slot properly.
        // I tried to do this (Sep 10) but it causes fucking unity to crash, crash, crash.
        // It could even log that I did it after the fact, and that line would log but then
        // unity would crash.
        // So: hooray for a shitty life. I already fixed this problem, but now it needs to
        // remain broken until I can fix it again...
        _objects.Add(null);
        object value = readValue(expectedType);
        _objects[objectId] = value;
        return value;
    }

    public bool readBool ()
    {
        return (stream.ReadByte() != 0);
    }

    public sbyte readSbyte ()
    {
        return (sbyte)stream.ReadByte();
    }

    public char readChar ()
    {
        return (char)((stream.ReadByte() << 8) | stream.ReadByte());
    }

    public short readShort ()
    {
        return (short)((stream.ReadByte() << 8) | stream.ReadByte());
    }

    public int readInt ()
    {
        return (stream.ReadByte() << 24) |
                (stream.ReadByte() << 16) |
                (stream.ReadByte() << 8) |
                stream.ReadByte();
    }

    public long readLong ()
    {
        return (((long)stream.ReadByte()) << 56) |
                (((long)stream.ReadByte()) << 48) |
                (((long)stream.ReadByte()) << 40) |
                (((long)stream.ReadByte()) << 32) |
                (((long)stream.ReadByte()) << 24) |
                (((long)stream.ReadByte()) << 16) |
                (((long)stream.ReadByte()) << 8) |
                ((long)stream.ReadByte());
    }

    public float readFloat ()
    {
        return new IntFloatUnion(readInt()).asFloat;
    }

    public double readDouble ()
    {
        return BitConverter.Int64BitsToDouble(readLong());
    }

    public string readString ()
    {
        return Streams.readVarString(stream);
    }

    public int readId ()
    {
        return Streams.readVarInt(stream);
    }

    public int readLength ()
    {
        return Streams.readVarInt(stream);
    }

    /**
     * Attempt to set the value into the specified field of the target object.
     */
    public void setField (FieldInfo field, object target, object value)
    {
        try {
            field.SetValue(target, value);
        } catch (Exception e) {
            // TODO: coaxing
            warn("Unable to cram value into field",
                    "target type", target.GetType(),
                    "field name", field.Name,
                    "field type", field.FieldType,
                    "value type", (value == null) ? (object)"<null>" : (object)value.GetType());
        }
    }

    /**
     * Read a non-null value.
     */
    protected object readValue (TypeData expectedType)
    {
        TypeData type = expectedType.isFinal() ? expectedType : readType();
        return type.readObject(this); // no type args are passed
    }

    public TypeData readType ()
    {
        int typeId = readId();
        int nextTypeId = _types.Count;
        if (typeId < nextTypeId) {
            return _types[typeId];
        }

        // add the placeholder for this new type
        _types.Add(null);

        int flagsAndInfo = typeId - nextTypeId;
        // TODO: remove magic numbers
        string name;
        TypeData baseType;
        TypeData[] typeArgs;
        int typeKind = flagsAndInfo & 0x3;
        int flags = (flagsAndInfo >> 2) & 0x1;
        int args = (flagsAndInfo >> 3);

        TypeData type;
        switch (typeKind) {
        default:
            name = TypeMapper.convertTypeFromJava(readString());
            Type systemType = TypeUtil.getType(name);
            // TODO: different for unkown types? Right now IRTD handles both.
            if (systemType == null) {
                warn("Unknown type will be dropped: " + name);
            }
            bool isFinal = (flags & TypeDatas.IS_FINAL_FLAG) != 0;
            type = new ImportingReflectiveTypeData(systemType, isFinal);
            break;

        case 1:
            baseType = readType();
            // TODO: Assert that baseType is correct type
            GenericImportingBaseType giBase = (GenericImportingBaseType)baseType;
            args = giBase.args;
            typeArgs = new TypeData[args];
            for (int ii = 0; ii < args; ii++) {
                typeArgs[ii] = readType();
            }
            Type sysType = TypeUtil.getType(giBase.name + "`" + giBase.args)
                    .MakeGenericType(typeArgs.Select(t => t.getType()).ToArray());
            type = new GenericImportingReflectiveTypeData(sysType, baseType, typeArgs);
            break;

        case 3:
            name = TypeMapper.convertTypeFromJava(readString());
            type = new GenericImportingBaseType(name, args);
            break;

        case 2:
            baseType = readType();
            typeArgs = new TypeData[args];
            for (int ii = 0; ii < args; ii++) {
                typeArgs[ii] = readType();
            }
//            this.logInfo("Read type",
//                    "baseType", baseType.getType());
            type = new ParameterizedTypeData(baseType, typeArgs);
            break;
        }

        // now that we've created the type, replace the placeholder slot
        _types[nextTypeId] = type;
        return type;
    }

    protected List<TypeData> _types = new List<TypeData>(TypeDatas.BOOTSTRAP);

    protected List<object> _objects = new List<object>() { null }; // initialize with null at 0.
}
}
