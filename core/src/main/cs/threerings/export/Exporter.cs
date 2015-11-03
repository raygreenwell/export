namespace threerings.export2 {

using System;
using System.Collections.Generic;

using threerings.trinity.util;

/**
 * Writes exportable objects.
 */
public abstract class Exporter
    : IDisposable
{
    /**
     * Writes the object to the underlying stream.
     */
    abstract
    public void writeObject (object obj);

    // from IDisposable
    abstract
    public void Dispose ();

    /**
     * Gets the actual class of the specified value, performing slight modifications for
     * certain types.
     */
    // TODO
    protected static Type getType (object value)
    {
        // nothing special yet here on the C# side, as the TypeMapper will convert to Java types
        return value.GetType();
    }

    /**
     * Debugging logging.
     */
    [System.Diagnostics.Conditional("DEBUG")]
    public static void debug (object msg, params object[] args)
    {
        Logger.getLogger(typeof(Exporter)).info(msg, args);
    }
}
}
