namespace threerings.export2.impl {

using System;
using System.Collections.Generic;
using System.Reflection;

using threerings.trinity.util;

// TODO: I don't like the "ifs" in here during importing.
// I'd rather have a separate "ImportingUnknownTypeData" but I'll split that up later...
public class ImportingReflectiveTypeData : TypedTypeData
{
    public ImportingReflectiveTypeData (Type fullType, bool isFinal) : base(fullType)
    {
        _fieldInfo = (fullType == null)
                ? null
                : Reflector.getExportableFields(fullType);
        _isFinal = isFinal;
    }

    override
    public bool isFinal ()
    {
        return _isFinal; // we do what the stream says
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArgs = null)
    {
        throw new Exception();
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArgs = null)
    {
        object value = (_type == null) ? null : Activator.CreateInstance(_type);
        int fieldCount = ctx.readLength();
        this.logInfo("Reading " + fieldCount + " fields...");
        for (int ii = 0; ii < fieldCount; ii++) {
            int fieldId = ctx.readId();
            FieldData field;
            if (fieldId < _fields.Count) {
                field = _fields[fieldId];

            } else {
                // ASSERT the fieldId
                // TODO
                if (fieldId != _fields.Count) {
                    this.logWarning("Unexpected field id!",
                            "expected", _fields.Count,
                            "got", fieldId);
                    throw new Exception("Unexpected field id!");
                }

                // new field
                string name = ctx.readString();
                TypeData type = ctx.readType();
                FieldInfo fieldInfo;
                if (_fieldInfo != null) {
                    _fieldInfo.TryGetValue(name, out fieldInfo);
                    if (_fieldInfo == null) {
                        ctx.warn("Importing class no longer has field: " + name);
                    }
                } else {
                    fieldInfo = null;
                }
                // fieldInfo can now be null
                field = new FieldData(type, fieldInfo);
                _fields.Add(field);
            }
            field.readField(ctx, value);
        }
        return value;
    }

    protected readonly List<FieldData> _fields = new List<FieldData>();

    protected readonly Dictionary<string, FieldInfo> _fieldInfo;

    protected readonly bool _isFinal;

    protected class FieldData
    {
        public FieldData (TypeData type, FieldInfo field)
        {
            _type = type;
            _field = field;
            // TODO: resolve that the fieldInfo matches the TypeData coming from the stream.
            // For example we could have something with [Export(box=true)] and so the types don't
            // match....
        }

        public void readField (ImportContext ctx, object target)
        {
            object value = ctx.readObject(_type);
            if (_field != null) {
                ctx.setField(_field, target, value);
            }
        }

        protected readonly TypeData _type;

        protected readonly FieldInfo _field;
    }
}
}
