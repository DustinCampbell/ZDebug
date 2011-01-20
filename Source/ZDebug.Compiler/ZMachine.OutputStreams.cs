using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core.Execution;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler
{
    public sealed partial class ZMachine
    {
        internal class OutputStreams
        {
            private readonly byte[] memory;

            private bool screenActive;
            private IOutputStream screenStream;

            private bool transcriptActive;
            private IOutputStream transcriptStream;

            private readonly Stack<MemoryOutputStream> memoryStreams;

            internal OutputStreams(byte[] memory)
            {
                this.memory = memory;
                this.memoryStreams = new Stack<MemoryOutputStream>();

                screenActive = true;
                screenStream = null;
                transcriptStream = null;
            }

            public void RegisterTranscript(IOutputStream stream)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("stream");
                }

                transcriptStream = stream;
            }

            public void RegisterScreen(IOutputStream stream)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("stream");
                }

                screenStream = stream;
            }

            public void SelectScreenStream()
            {
                screenActive = true;
            }

            public void DeselectScreenStream()
            {
                screenActive = false;
            }

            public void SelectTranscriptStream()
            {
                transcriptActive = true;

                // set transcript flag as demanded by standard 1.0
                var flags2 = memory.ReadWord(0x10);
                flags2 |= 0x01;
                memory.WriteWord(0x10, flags2);
            }

            public void DeselectTranscriptStream()
            {
                transcriptActive = false;

                // clear transcript flag as demanded by standard 1.0
                var flags2 = memory.ReadWord(0x10);
                flags2 &= unchecked((ushort)(~0x01));
                memory.WriteWord(0x10, flags2);
            }

            public void SelectMemoryStream(int address)
            {
                if (memoryStreams.Count == 16)
                {
                    throw new InvalidOperationException("Cannot create more than 16 memory output streams.");
                }

                memoryStreams.Push(new MemoryOutputStream(memory, address));
            }

            public void DeselectMemoryStream()
            {
                memoryStreams.Pop();
            }

            public void Print(string text)
            {
                if (memoryStreams.Count > 0)
                {
                    // If stream 3 is active, only print to that
                    memoryStreams.Peek().Print(text);
                }
                else
                {
                    if (screenActive && screenStream != null)
                    {
                        screenStream.Print(text);
                    }

                    if (transcriptActive && transcriptStream != null)
                    {
                        transcriptStream.Print(text);
                    }
                }
            }

            public void Print(char ch)
            {
                if (memoryStreams.Count > 0)
                {
                    // If stream 3 is active, only print to that
                    memoryStreams.Peek().Print(ch);
                }
                else
                {
                    if (screenActive && screenStream != null)
                    {
                        screenStream.Print(ch);
                    }

                    if (transcriptActive && transcriptStream != null)
                    {
                        transcriptStream.Print(ch);
                    }
                }
            }
        }
    }
}
