using System;
using System.Collections.Generic;
using System.IO;
using ZDebug.Core.Basics;

namespace ZDebug.Core.Blorb
{
    public sealed class BlorbFile
    {
        private readonly Memory memory;

        private static uint MakeId(char c1, char c2, char c3, char c4)
        {
            return ((uint)((byte)c1 << 24) | (uint)((byte)c2 << 16) | (uint)((byte)c3 << 8) | (byte)c4);
        }

        private static readonly uint id_FORM = MakeId('F', 'O', 'R', 'M');
        private static readonly uint id_IFRS = MakeId('I', 'F', 'R', 'S');
        private static readonly uint id_ZCOD = MakeId('Z', 'C', 'O', 'D');

        private struct ChunkDescriptor
        {
            public uint Type;
            public uint Length;
            public uint Address;
            public uint DataAddress;
        }

        private readonly List<ChunkDescriptor> chunks;

        public BlorbFile(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.memory = new Memory(stream);

            var reader = memory.CreateReader(0);
            var dwords = reader.NextDWords(3);

            if (dwords[0] != id_FORM)
            {
                throw new InvalidOperationException();
            }

            if (dwords[2] != id_IFRS)
            {
                throw new InvalidOperationException();
            }

            int totalLength = (int)dwords[1] + 8;

            this.chunks = new List<ChunkDescriptor>();

            while (reader.Address < totalLength)
            {
                var chunk = new ChunkDescriptor();

                chunk.Address = (uint)reader.Address;

                var type = reader.NextDWord();
                var len = reader.NextDWord();

                chunk.Type = type;
                if (type == id_FORM)
                {
                    chunk.DataAddress = chunk.Address;
                    chunk.Length = len + 8;
                }
                else
                {
                    chunk.DataAddress = chunk.Address + 8;
                    chunk.Length = len;
                }

                chunks.Add(chunk);

                reader.Skip((int)len);
                if ((reader.Address & 1) != 0)
                {
                    reader.Skip(1);
                }

                if (reader.Address > totalLength)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public byte[] GetZCode()
        {
            foreach (var chunk in chunks)
            {
                if (chunk.Type == id_ZCOD)
                {
                    return memory.ReadBytes((int)chunk.DataAddress, (int)chunk.Length);
                }
            }

            throw new InvalidOperationException();
        }
    }
}
