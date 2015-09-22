namespace threerings.export.impl {

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using threerings.trinity.util;

/**
 * A TypeData that reflects upon and writes the fields of an Exportable class.
 * <em>Not used for importing.</em>
 */
public class ExportingReflectiveTypeData : TypedTypeData
{
    public ExportingReflectiveTypeData (Type fullType, ExportContext ctx) : base(fullType)
    {
        if (fullType.IsAbstract || fullType.IsInterface) {
            // TODO: rethink? Use a different kind of TypeData, probably
            _fields = new FieldData[0];
            return;
        }

        object prototype = Activator.CreateInstance(fullType);
        _fields = Reflector.getExportableFields(fullType)
                .Select(entry => new FieldData(ctx, entry.Value, entry.Key, prototype))
                .ToArray();
    }

    override
    public bool isFinal ()
    {
        return _type.IsSealed;
    }

    public int getNextFieldId ()
    {
        return _nextFieldId++;
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArgs = null)
    {
        // make a list of all the fields that have changed, then send only those
        // TODO: Improve this.. Perhaps it writes them all to a new stream, a byte[], so that
        // we can insert the length afterwards... ugh...
        List<FieldData> fields = null;
        foreach (FieldData field in _fields) {
            if (field.hasChanged(value)) {
                (fields ?? (fields = new List<FieldData>())).Add(field);
            }
        }
        if (fields == null) {
            ctx.writeLength(0);
            return;
        }

        ctx.writeLength(fields.Count);
        foreach (FieldData field in fields) {
            field.writeField(ctx, this, value);
        }
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArgs = null)
    {
        throw new Exception();
    }

    protected readonly FieldData[] _fields;

    protected int _nextFieldId;

    /**
     *
     */
    protected class FieldData
    {
        public FieldData (ExportContext ctx, FieldInfo field, string name, object prototype)
        {
            _field = field;
            _name = name;
            _defvalue = field.GetValue(prototype);
            _type = ctx.getTypeData(field.FieldType);
        }

        // TODO: potentially we should check changed and write at the same time, and write to a
        // MemoryStream, so that we don't need to pull value from source twice.

        public bool hasChanged (object source)
        {
            // NOTE: to do! We should do some checking-in here to ensure that each

            // Note: we always write enums because they can't hold null over here and are kinda
            // crappy.
            return _field.FieldType.IsEnum ||
                    !EqualityComparer<object>.Default.Equals(_field.GetValue(source), _defvalue);
        }

        public void writeField (
                ExportContext ctx, ExportingReflectiveTypeData sourceTypeData, object source)
        {
            if (_fieldId == -1) {
                _fieldId = sourceTypeData.getNextFieldId();
//                this.logInfo("Writing new field",
//                        "fieldId", _fieldId,
//                        "name", _name,
//                        "type", _type);
                ctx.writeId(_fieldId);
                ctx.writeString(_name);
                ctx.writeType(_type);

            } else {
                ctx.writeId(_fieldId);
            }
            ctx.writeObject(_field.GetValue(source), _type);
        }

        protected readonly FieldInfo _field;

        protected readonly string _name;

        protected readonly object _defvalue;

        protected readonly TypeData _type;

        protected int _fieldId = -1;
    }
}
}
