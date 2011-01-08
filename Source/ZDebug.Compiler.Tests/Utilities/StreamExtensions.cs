using System;
using System.IO;

namespace ZDebug.Compiler.Tests.Utilities
{
    internal static class StreamExtensions
    {
        public static byte[] ReadFully(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var buffer = new byte[1024];
            var read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    var newBuffer = buffer.Resize(buffer.Length * 2);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }

            return buffer.Resize(read);
        }
    }
}
