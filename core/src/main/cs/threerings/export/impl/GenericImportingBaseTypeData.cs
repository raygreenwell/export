namespace threerings.export2.impl {

using System;

public class GenericImportingBaseType : TypedTypeData
{
    // TODO: move to method?
    public readonly string name;

    // TODO: move to method?
    public readonly int args;

    public GenericImportingBaseType (string name, int args) : base(null)
    {
        this.name = name;
        this.args = args;
    }

    override
    public void writeObject (ExportContext ctx, object value, TypeData[] typeArguments = null)
    {
        throw new Exception("Nothing should call this");
    }

    override
    public object readObject (ImportContext ctx, TypeData[] typeArguments = null)
    {
        throw new Exception("Nothing should call this");
    }
}
}
