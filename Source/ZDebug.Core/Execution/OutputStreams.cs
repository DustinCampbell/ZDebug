using System;
namespace ZDebug.Core.Execution
{
    public sealed partial class OutputStreams
    {
        private readonly Story story;
        private readonly Tuple<bool, IOutputStream>[] streams;

        internal OutputStreams(Story story)
        {
            this.story = story;
            this.streams = new Tuple<bool, IOutputStream>[4];

            this.streams[0] = Tuple.Create(false, EmptyStream.Instance);
            this.streams[1] = Tuple.Create(true, EmptyStream.Instance);
            this.streams[2] = Tuple.Create(false, EmptyStream.Instance);
            this.streams[3] = Tuple.Create(false, EmptyStream.Instance);
        }

        public void RegisterTranscript(IOutputStream stream)
        {
            if (streams[0].Item2 == EmptyStream.Instance)
            {
                streams[0] = Tuple.Create(streams[0].Item1, stream);
            }
        }

        public void RegisterScreen(IOutputStream stream)
        {
            if (streams[1].Item2 == EmptyStream.Instance)
            {
                streams[1] = Tuple.Create(streams[1].Item1, stream);
            }
        }

        public void Print(string text)
        {
            for (int i = 0; i < 4; i++)
            {
                var pair = streams[i];
                if (pair.Item1)
                {
                    pair.Item2.Print(text);
                }
            }
        }

        public void Print(char ch)
        {
            for (int i = 0; i < 4; i++)
            {
                var pair = streams[i];
                if (pair.Item1)
                {
                    pair.Item2.Print(ch);
                }
            }
        }
    }
}
