namespace threerings.export2 {

using System;
using System.IO;
using System.Text;

using threerings.trinity.util;

/**
 * Export-related stream utilities.
 */
public static class Streams
{
    /**
     * Return the next Stream, or null if we've reached the end of the stream.
     * This adds a level of safety in that the returned Stream does not need to actually be
     * read from at all in order to get the next stream from the source. This is accomplished
     * by pre-reading all the bytes and returning a wrapper around the byte[].
     */
    public static Stream input (Stream source)
    {
        int length = readVarInt(source);
        if (length == -1) {
            return null;
        }
        // slurp that many bytes into the array
        return new MemoryStream(readBytes(source, length));
    }

    /**
     * Return a new Stream that will be appended to 'dest' when it is closed,
     * with a varlong length prefix.
     * Trying to read from this stream is undefined. Don't do it.
     */
    public static Stream output (Stream dest)
    {
        return new MetaOutputStream(dest);
    }

    /**
     * Write a little-endian "varlong" to the specified stream.
     *
     * Each byte is used to encode 7 bits of data and a continuation bit if more is coming,
     * which will use between 1 and 9 bytes to write out any value between 0 and Long.MAX_VALUE.
     */
    public static void writeVarLong (Stream outStream, long value)
    {
        if (value < 0) {
            throw new ArgumentOutOfRangeException("value");
        }
        while (true) {
            byte bite = (byte)(value & 0x7f);
            value >>= 7;
            if (value == 0) {
                outStream.WriteByte(bite); // write the byte and exit
                return;
            }
            outStream.WriteByte((byte)(bite | 0x80)); // write the byte with the continuation flag
        }
    }

    /**
     * Read a little-endian "varlong" from the specified stream.
     *
     * @return the value read off the stream, or -1 if we're at the end of the stream.
     * @throws IOException if there's an error reading.
     */
    public static long readVarLong (Stream inStream)
    {
        long ret = 0;
        for (int shift = 0; shift < 63; shift += 7) {
            int bite = inStream.ReadByte();
            if (bite == -1) {
                if (shift == 0) {
                    return -1; // expected: we're at the end of the stream
                }
                break; // throw InvalidData
            }
            ret |= ((long)(bite & 0x7f)) << shift;
            if ((bite & 0x80) == 0) {
                if (shift > 0 && ((bite & 0x7f) == 0)) {
                    break; // detect invalid extra 0-padding; throw InvalidData
                }
                return ret;
            }
        }
        // this was a InvalidDataException, but unity doesn't support that
        throw new IOException("Invalid length prefix");
    }

    /**
     * Write a positive int to the specified stream, encoded little-endian, variable length.
     */
    public static void writeVarInt (Stream outStream, int value)
    {
        writeVarLong(outStream, value);
    }

    /**
     * Read the next varlong off the stream, but freak out if it's bigger than Integer.MAX_VALUE.
     */
    public static int readVarInt (Stream inStream)
    {
        checked {
            return (int)readVarLong(inStream);
        }
    }

    /**
     * Write a String to the specified stream, UTF-8 encoded and prefixed by a varint length.
     */
    public static void writeVarString (Stream outStream, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writeVarInt(outStream, bytes.Length);
        outStream.Write(bytes, 0, bytes.Length);
    }

    /**
     * Read a String from the specified stream, UTF-8 encoded and prefixed by a varint length.
     */
    public static string readVarString (Stream inStream)
    {
        int length = readVarInt(inStream);
        byte[] buf = readBytes(inStream, length);
        return Encoding.UTF8.GetString(buf, 0, length);
    }

    /**
     * Utility to read bytes into an array.
     */
    public static byte[] readBytes (Stream inStream, int size)
    {
        byte[] buf = new byte[size];
        for (int index = 0; index < size; ) {
            int read = inStream.Read(buf, index, size - index);
            if (read == 0) {
                throw new EndOfStreamException();
            }
            index += read;
        }
        return buf;
    }

    /**
     * A MemoryStream that writes to the specified destination stream when disposed, prefixing
     * with a little-endian varlong.
     */
    private class MetaOutputStream : MemoryStream
    {
        public MetaOutputStream (Stream dest) : base()
        {
            if (dest == null) {
                throw new ArgumentNullException();
            }
            _dest = dest;
        }

        // this is not the IDisposable method, but Streams have Dispose() call Dispose(bool).
        override
        protected void Dispose (bool disposing)
        {
            if (_dest != null) {
                int len = (int)Length;
                writeVarLong(_dest, len);
                _dest.Write(GetBuffer(), 0, len); // CopyTo(_dest); // TODO .NET 4.0
                _dest.Flush();
                _dest = null;
            }
            base.Dispose(disposing);
        }

        /** Our dest stream. */
        protected Stream _dest;
    }
}
}
