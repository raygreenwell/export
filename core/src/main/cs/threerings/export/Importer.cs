namespace threerings.export {

using System;
using System.Collections.Generic;

using threerings.trinity.util;

/**
 * Writes exportable objects.
 */
public abstract class Importer
    : IDisposable
{
    [System.Diagnostics.Conditional("DEBUG")]
    public static void debug (object msg, params object[] args)
    {
        Logger.getLogger(typeof(Importer)).info(msg, args);
    }

    /**
     * Read the next object off the underlying stream.
     */
    public abstract object readObject ();

    public abstract T readObject<T> ();

    // from IDisposable
    abstract
    public void Dispose ();
}
}
