using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    /// <summary>
    /// Provides utility methods for working with Streams.
    /// </summary>
    public static class StreamExtensions
    {
        const int _defaultBufferSize = 4096;

        /// <summary>
        /// Reads from the current position in the stream to the end and returns a <c>byte[]</c> containing the bytes read.
        /// </summary>
        /// <param name="stream">The stream from which to read.</param>
        /// <returns>The remaining bytes within the stream.</returns>
        public static byte[] ReadToEnd(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return stream.ReadToEndInternal(new byte[stream.GetBufferSize()]);
        }

        /// <summary>
        /// Reads from the current position in the stream to the end and returns a <c>byte[]</c> containing the bytes read.
        /// </summary>
        /// <param name="stream">The stream from which to read.</param>
        /// <param name="bufferSize">The size of the buffer to use for reading.</param>
        /// <returns>The remaining bytes within the stream.</returns>
        public static byte[] ReadToEnd(this Stream stream, int bufferSize)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (bufferSize < 1) throw new ArgumentOutOfRangeException("bufferSize", "bufferSize should be greater than 0");
            return stream.ReadToEndInternal(new byte[bufferSize]);
        }

        /// <summary>
        /// Reads from the current position in the stream to the end and returns a <c>byte[]</c> containing the bytes read.
        /// </summary>
        /// <param name="stream">The stream from which to read.</param>
        /// <param name="buffer">The buffer to use for reading from the stream.</param>
        /// <returns>The remaining bytes within the stream.</returns>
        public static byte[] ReadToEnd(this Stream stream, byte[] buffer)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.Length == 0) throw new ArgumentException("buffer must have a length greater than 0", "buffer");
            return stream.ReadToEndInternal(buffer);
        }

        private static byte[] ReadToEndInternal(this Stream stream, byte[] buffer)
        {
            using (var output = stream.GetMemoryStream())
            {
                while (true)
                {
                    var read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        return output.ToArray();
                    }
                    output.Write(buffer, 0, read);
                }
            }
        }

        /// <summary>
        /// Reads from the current position in the stream to the end and returns a <c>byte[]</c> containing the bytes read.
        /// </summary>
        /// <param name="stream">The stream from which to read.</param>
        /// <returns>A task containing the remaining bytes within the stream.</returns>
        public static Task<byte[]> ReadToEndAsync(this Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            return stream.ReadToEndAsyncInternal(new byte[stream.GetBufferSize()]);
        }

        /// <summary>
        /// Reads from the current position in the stream to the end and returns a <c>byte[]</c> containing the bytes read.
        /// </summary>
        /// <param name="stream">The stream from which to read.</param>
        /// <param name="bufferSize">The size of the buffer to use for reading.</param>
        /// <returns>A task containing the remaining bytes within the stream.</returns>
        public static Task<byte[]> ReadToEndAsync(this Stream stream, int bufferSize)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (bufferSize < 1) throw new ArgumentOutOfRangeException("bufferSize", "bufferSize should be greater than 0");
            return stream.ReadToEndAsyncInternal(new byte[bufferSize]);
        }

        /// <summary>
        /// Reads from the current position in the stream to the end and returns a <c>byte[]</c> containing the bytes read.
        /// </summary>
        /// <param name="stream">The stream from which to read.</param>
        /// <param name="buffer">The buffer to use for reading from the stream.</param>
        /// <returns>A task containing the remaining bytes within the stream.</returns>
        public static Task<byte[]> ReadToEndAsync(this Stream stream, byte[] buffer)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.Length == 0) throw new ArgumentException("buffer must have a length greater than 0", "buffer");
            return stream.ReadToEndAsyncInternal(buffer);
        }

        private static async Task<byte[]> ReadToEndAsyncInternal(this Stream stream, byte[] buffer)
        {
            using (var output = stream.GetMemoryStream())
            {
                while (true)
                {
                    var read = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    if (read <= 0)
                    {
                        return output.ToArray();
                    }
                    output.Write(buffer, 0, read);
                }
            }
        }

        private static MemoryStream GetMemoryStream(this Stream stream)
        {
            if (stream.CanSeek)
            {
                var remainingLength = stream.Length - stream.Position;
                return new MemoryStream((int)remainingLength);
            }
            return new MemoryStream();
        }

        private static int GetBufferSize(this Stream stream)
        {
            if (stream.CanSeek)
            {
                var remainingLength = stream.Length - stream.Position;
                if (remainingLength < _defaultBufferSize)
                {
                    return (int)remainingLength;
                }
            }
            return _defaultBufferSize;
        }
    }
}
