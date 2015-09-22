namespace threerings.export.impl {

using System;
using System.Collections.Generic;

using threerings.trinity.util;

public class GenericExportingReflectiveTypeData : ExportingReflectiveTypeData
{
    // TODO: sort out argument order, etc, with superclass
    public GenericExportingReflectiveTypeData (
        ExportContext ctx, Type fullType, TypeData baseType, TypeData[] args)
        : base(fullType, ctx)
    {
        _baseType = baseType;
        _args = args;
    }

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

    protected readonly TypeData _baseType;

    protected readonly TypeData[] _args;
}
}
