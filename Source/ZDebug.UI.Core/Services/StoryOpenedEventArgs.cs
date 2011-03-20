using System;
using ZDebug.Core;

namespace ZDebug.UI.Services
{
    public sealed class StoryOpenedEventArgs : EventArgs
    {
        private readonly Story story;

        public StoryOpenedEventArgs(Story story)
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
