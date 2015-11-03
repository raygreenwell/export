namespace threerings.export2 {

using System;
using System.Collections.Generic;

using threerings.trinity.util;

/**
 * Writes exportable objects.
 */
public abstract class Importer
    : IDisposable
{
    /**
     * Read the next object off the underlying stream.
     */
    abstract
    public object readObject ();

    // TODO
    abstract
    public T readObject<T> ();

    // from IDisposable
    abstract
    public void Dispose ();

    /**
     * Debug logging.
     */
    [System.Diagnostics.Conditional("DEBUG")]
    public static void debug (object msg, params object[] args)
    {
        Logger.getLogger(typeof(Importer)).info(msg, args);
    }

}
}
