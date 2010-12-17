using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using ZDebug.Core.Basics;

namespace ZDebug.Core.Blorb
{
    public sealed class BlorbFile
    {
        private readonly Memory memory;
        private int releaseNumber;

        private static string NameFromId(uint id)
        {
            var chars = new char[4];
            chars[0] = (char)((id >> 24) & 0xff);
            chars[1] = (char)((id >> 16) & 0xff);
            chars[2] = (char)((id >> 8) & 0xff);
            chars[3] = (char)(id & 0xff);

            return new string(chars);
        }

        private static uint MakeId(string name)
        {
            if (name.Length > 4)
            {
                throw new ArgumentException("ID names can contain at most 4 characters.", "name");
            }

            if (name.Length < 4)
            {
                name += new string(' ', 4 - name.Length);
            }

            var chars = name.ToCharArray();
            return ((uint)((byte)chars[0] << 24) | 
                (uint)((byte)chars[1] << 16) | 
                (uint)((byte)chars[2] << 8) | 
                (byte)chars[3]);
        }

        private static readonly uint id_FORM = MakeId("FORM");
        private static readonly uint id_IFRS = MakeId("IFRS");
        private static readonly uint id_RIdx = MakeId("RIdx");
        private static readonly uint id_IFhd = MakeId("IFhd");
        private static readonly uint id_Reso = MakeId("Reso");
        private static readonly uint id_Loop = MakeId("Loop");
        private static readonly uint id_RelN = MakeId("RelN");
        private static readonly uint id_Plte = MakeId("Plte");

        private static readonly uint id_Snd = MakeId("Snd");
        private static readonly uint id_Exec = MakeId("Exec");
        private static readonly uint id_Pict = MakeId("Pict");
        private static readonly uint id_Copyright = MakeId("(c)");
        private static readonly uint id_AUTH = MakeId("AUTH");
        private static readonly uint id_ANNO = MakeId("ANNO");

        private static readonly uint id_ZCOD = MakeId("ZCOD");
        private static readonly uint id_IFmd = MakeId("IFmd");

        private struct ChunkDescriptor
        {
            public uint Type;
            public uint Length;
            public uint Address;
            public uint DataAddress;

            public override string ToString()
            {
                return string.Format("'{0}'; Address={1:x8}; DataAddress={2:x8}; Length={3}", NameFromId(Type), Address, DataAddress, Length);
            }
        }

        private struct ResourceDecriptor
        {
            public uint Usage;
            public uint Number;
            public int ChunkNumber;

            public override string ToString()
            {
                return string.Format("'{0}'; Number={1}; ChunkNumber", NameFromId(Usage), Number, ChunkNumber);
            }
        }

        private struct ZHeader
        {
            public ushort ReleaseNumber;
            public char[] SerialNumber;
            public ushort Checksum;
        }

        private readonly List<ChunkDescriptor> chunks;
        private readonly List<ResourceDecriptor> resources;

        public BlorbFile(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.memory = new Memory(stream);

            var reader = memory.CreateReader(0);

            var dwords = reader.NextDWords(3);

            // First, ensure that this is a valid format
            if (dwords[0] != id_FORM)
            {
                throw new InvalidOperationException();
            }

            if (dwords[2] != id_IFRS)
            {
                throw new InvalidOperationException();
            }

            int totalLength = (int)dwords[1] + 8;

            // Collect all chunks
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
                    chunk.DataAddress = (uint)reader.Address;
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

            // Loop through chunks and collect resources
            this.resources = new List<ResourceDecriptor>();

            foreach (var chunk in chunks)
            {
                if (chunk.Type == id_RIdx)
                {
                    reader.Address = (int)chunk.DataAddress;
                    var numResources = (int)reader.NextDWord();

                    if (chunk.Length < (numResources * 12) + 4)
                    {
                        throw new InvalidOperationException();
                    }

                    for (int i = 0; i < numResources; i++)
                    {
                        var resource = new ResourceDecriptor();
                        resource.Usage = reader.NextDWord();
                        resource.Number = reader.NextDWord();

                        var resourcePos = reader.NextDWord();

                        var chunkIndex = chunks.FindIndex(c => c.Address == resourcePos);
                        if (chunkIndex < 0)
                        {
                            throw new InvalidOperationException();
                        }

                        resource.ChunkNumber = chunkIndex;

                        resources.Add(resource);
                    }
                }
                else if (chunk.Type == id_RelN)
                {
                    reader.Address = (int)chunk.DataAddress;
                    if (chunk.Length < 2)
                    {
                        throw new InvalidOperationException();
                    }

                    releaseNumber = reader.NextWord();
                }
                else if (chunk.Type == id_IFhd)
                {
                    reader.Address = (int)chunk.DataAddress;
                    if (chunk.Length < 3)
                    {
                        throw new InvalidOperationException();
                    }

                    var header = new ZHeader();
                    header.ReleaseNumber = reader.NextWord();
                    header.SerialNumber = new char[6];
                    for (int i = 0; i < 6; i++)
                    {
                        header.SerialNumber[i] = (char)reader.NextByte();
                    }
                    header.Checksum = reader.NextWord();
                }
                else if (chunk.Type == id_Reso)
                {

                }
                else if (chunk.Type == id_Loop)
                {

                }
                else if (chunk.Type == id_Plte)
                {

                }
            }
        }

        private ResourceDecriptor FindExecResource()
        {
            var index = resources.FindIndex(res => res.Usage == id_Exec);

            if (index < 0)
            {
                throw new BlorbFileException("Blorb file does not contain a resource with 'Exec' usage.");
            }

            return resources[index];
        }

        private ChunkDescriptor FindChunkByType(uint type)
        {
            var index = chunks.FindIndex(ch => ch.Type == type);

            if (index < 0)
            {
                throw new BlorbFileException("Blorb file does not contain a chunk of type '" + NameFromId(type) + "'.");
            }

            return chunks[index];
        }

        private byte[] GetChunkData(ChunkDescriptor chunk)
        {
            return memory.ReadBytes((int)chunk.DataAddress, (int)chunk.Length);
        }

        public Story LoadStory()
        {
            var resource = FindExecResource();
            var chunk = chunks[resource.ChunkNumber];

            if (chunk.Type != id_ZCOD)
            {
                throw new BlorbFileException("Blorb file does not contain 'ZCOD' chunk");
            }

            return Story.FromBytes(GetChunkData(chunk));
        }

        public XElement LoadMetadata()
        {
            var chunk = FindChunkByType(id_IFmd);

            using (var stream = new MemoryStream(GetChunkData(chunk)))
            {
                return XElement.Load(stream);
            }
        }

        public int ReleaseNumber
        {
            get { return releaseNumber; }
        }
    }
}
