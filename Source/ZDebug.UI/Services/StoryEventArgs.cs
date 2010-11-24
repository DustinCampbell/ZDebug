using System;
using ZDebug.Core;

namespace ZDebug.UI.Services
{
    internal sealed class StoryEventArgs : EventArgs
    {
        public Story Story { get; private set; }

        public StoryEventArgs(Story story)
        {
            this.Story = story;
        }
    }
}
