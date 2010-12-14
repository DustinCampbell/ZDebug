using System;
using System.Collections.Generic;
using ZDebug.Core.Basics;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        private class OutputStreams
        {
            private readonly Story story;

            private bool screenActive;
            private IOutputStream screenStream;

            private bool transcriptActive;
            private IOutputStream transcriptStream;

            private readonly Stack<MemoryOutputStream> memoryStreams;

            internal OutputStreams(Story story)
            {
                this.story = story;
                this.memoryStreams = new Stack<MemoryOutputStream>();

                screenActive = true;
                screenStream = NullScreen.Instance;
                transcriptStream = NullOutputStream.Instance;
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
            }

            public void DeselectTranscriptStream()
            {
                transcriptActive = false;
            }

            public void SelectMemoryStream(Memory memory, int address)
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
                    if (screenActive)
                    {
                        screenStream.Print(text);
                    }

                    if (transcriptActive)
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
                    if (screenActive)
                    {
                        screenStream.Print(ch);
                    }

                    if (transcriptActive)
                    {
                        transcriptStream.Print(ch);
                    }
                }
            }
        }
    }
}