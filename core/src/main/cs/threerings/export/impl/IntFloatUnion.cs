namespace threerings.export2 {

using System.Runtime.InteropServices;

/**
 * A structure to aid converting between int and float.
 * Adapted from code by Jon Skeet.
 */
[StructLayout(LayoutKind.Explicit)]
struct IntFloatUnion
{
    [FieldOffset(0)]
    public int asInt;

    [FieldOffset(0)]
    public float asFloat;

    public IntFloatUnion (int i)
    {
        this.asFloat = 0; // placate compiler
        this.asInt = i;
    }

    public IntFloatUnion (float f)
    {
        this.asInt = 0; // placate compiler
        this.asFloat = f;
    }
}
}
