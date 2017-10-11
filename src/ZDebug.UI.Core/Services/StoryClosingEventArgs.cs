using System;
using ZDebug.Core;

namespace ZDebug.UI.Services
{
    public sealed class StoryClosingEventArgs : EventArgs
    {
        private readonly Story story;

        public StoryClosingEventArgs(Story story)
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
