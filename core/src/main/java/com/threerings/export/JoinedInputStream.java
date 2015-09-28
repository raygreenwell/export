package com.threerings.export2;

import java.io.IOException;
import java.io.InputStream;
import java.util.Arrays;
import java.util.ArrayList;
import java.util.List;

/**
 * Joins InputStreams together.
 */
public class JoinedInputStream extends InputStream
{
    /**
     * Create a JoinedInputStream from the specified streams.
     */
    public JoinedInputStream (InputStream... streams)
    {
        _streams.addAll(Arrays.asList(streams));
    }

    @Override
    public int read ()
        throws IOException
    {
        for (; !_streams.isEmpty(); closeStream()) {
            int bite = _streams.get(0).read();
            if (bite != -1) {
                return bite;
            }
        }
        return -1;
    }

    @Override
    public int read (byte[] b, int off, int len)
        throws IOException
    {
        for (; !_streams.isEmpty(); closeStream()) {
            int result = _streams.get(0).read(b, off, len);
            if (result != -1) {
                return result;
            }
        }
        return -1;
    }

    @Override
    public void close () throws IOException
    {
        while (!_streams.isEmpty()) {
            closeStream();
        }
        _streams = null;
    }

    /**
     * Close the current stream and remove it.
     */
    protected void closeStream ()
        throws IOException
    {
        _streams.remove(0).close();
    }

    /** The streams. */
    protected List<InputStream> _streams = new ArrayList<>();
}
