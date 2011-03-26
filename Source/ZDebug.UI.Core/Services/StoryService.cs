using System;
using System.ComponentModel.Composition;
using System.IO;
using ZDebug.Core;
using ZDebug.Core.Blorb;

namespace ZDebug.UI.Services
{
    [Export]
    public class StoryService
    {
        private Story story;
        private GameInfo gameInfo;

        private void OnStoryOpened()
        {
            var handler = StoryOpened;
            if (handler != null)
            {
                handler(this, new StoryOpenedEventArgs(story));
            }
        }

        private void OnStoryClosing()
        {
            var handler = StoryClosing;
            if (handler != null)
            {
                handler(this, new StoryClosingEventArgs(story));
            }
        }

        public void CloseStory()
        {
            if (!IsStoryOpen)
            {
                return;
            }

            OnStoryClosing();

            story = null;
            gameInfo = null;
        }

        public Story OpenStory(string fileName)
        {
            CloseStory();

            if (Path.GetExtension(fileName) == ".zblorb")
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var blorb = new BlorbFile(stream);
                    gameInfo = new GameInfo(blorb);
                    story = blorb.LoadStory();
                }
            }
            else
            {
                story = Story.FromBytes(File.ReadAllBytes(fileName));
            }

            OnStoryOpened();

            return story;
        }

        public Story Story
        {
            get
            {
                return story;
            }
        }

        public bool IsStoryOpen
        {
            get
            {
                return story != null;
            }
        }

        public GameInfo GameInfo
        {
            get
            {
                return gameInfo;
            }
        }

        public bool HasGameInfo
        {
            get
            {
                return gameInfo != null;
            }
        }

        public event EventHandler<StoryOpenedEventArgs> StoryOpened;
        public event EventHandler<StoryClosingEventArgs> StoryClosing;
    }
}
