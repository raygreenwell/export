namespace threerings.export2.impl {

using System;

public class GenericExportingBaseTypeData : TypedTypeData
{
    public GenericExportingBaseTypeData (Type fullType) : base(fullType)
    {
        // nada
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
