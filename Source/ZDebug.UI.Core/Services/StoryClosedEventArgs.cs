using System;
using ZDebug.Core;

namespace ZDebug.UI.Services
{
    public sealed class StoryClosedEventArgs : EventArgs
    {
        private readonly Story story;

        public StoryClosedEventArgs(Story story)
        {
            this.story = story;
        }

        public Story Story
        {
            get
            {
                return story;
            }
        }
    }
}
