namespace threerings.export2 {

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Ionic.Zlib;

using threerings.trinity.util;

using threerings.export2.impl;

/**
 * Exports to a compact binary format.
 */
public class BinaryExporter : Exporter
{
    /** Identifies the file type. */
    public const uint MAGIC_NUMBER = 0xFACEAF0E;

    /** The format version. */
    public const byte VERSION = 0;

    /** The compressed format flag. */
    public const int COMPRESSED_FORMAT_FLAG = 1 << 0;

    /**
     * Creates an exporter to write to the specified stream.
     *
     * @param compress if true, compress the output.
     * @param disposeBase whether to Dispose() the underlying stream when we're disposed.
     */
    public BinaryExporter (Stream outStream, bool compress = true, bool disposeBase = true)
    {
        _out = outStream;
        _compress = compress;
        _disposeBase = disposeBase;
    }

    override
    public void writeObject (object obj)
    {
        if (_ctx == null) {
            ExportContext ctx = new ExportContext(_out);

            // write the preamble
            ctx.writeInt(unchecked((int)MAGIC_NUMBER));
            ctx.writeSbyte(unchecked((sbyte)VERSION));
            int flags = 0;
            if (_compress) {
                flags |= COMPRESSED_FORMAT_FLAG;
            }
            Streams.writeVarInt(_out, flags);

            // everything thereafter will be compressed if so requested
            if (_compress) {
                ctx.stream = _out = new ZlibStream(_out, CompressionMode.Compress, !_disposeBase);
            }
            _ctx = ctx;
        }

        _ctx.writeObject(obj, ObjectTypeData.INSTANCE);
    }

    override
    public void Dispose ()
    {
        // Dispose the base *anyway* if compress is true, because
        // - we NEED to Dispose the underlying ZlibStream
        // - if we don't want to disposeBase, we would have constructed the ZlibStream to
        //   not dispose its base, so we know the chain stops there.
        if (_out != null && (_disposeBase || _compress)) {
            _out.Dispose();
        }
    }

    /** The stream that we use for writing data. */
    protected Stream _out;

    /** Whether or not to compress the output. */
    protected readonly bool _compress;

    /** Should we dispose our base stream when we're disposed? */
    protected readonly bool _disposeBase;

    protected ExportContext _ctx;
}
}
