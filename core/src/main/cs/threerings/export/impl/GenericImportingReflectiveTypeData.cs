namespace threerings.export.impl {

using System;

public class GenericImportingReflectiveTypeData : ImportingReflectiveTypeData
{
    public GenericImportingReflectiveTypeData (Type fullType, TypeData baseType, TypeData[] args)
            : base(fullType, false) // TODO: isFinal
    {
        // to do?
    }
}
}
