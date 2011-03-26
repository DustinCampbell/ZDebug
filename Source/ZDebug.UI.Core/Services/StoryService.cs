using System;
using System.ComponentModel.Composition;
using System.IO;
using ZDebug.Core;
using ZDebug.Core.Blorb;

namespace ZDebug.UI.Services
{
    [Export]
    public class StoryService : IService
    {
        private string fileName;
        private Story story;
        private GameInfo gameInfo;

        private void OnStoryOpened(Story story)
        {
            var handler = StoryOpened;
            if (handler != null)
            {
                handler(this, new StoryOpenedEventArgs(story));
            }
        }

        private void OnStoryClosing(Story story)
        {
            var handler = StoryClosing;
            if (handler != null)
            {
                handler(this, new StoryClosingEventArgs(story));
            }
        }

        private void OnStoryClosed(Story story)
        {
            var handler = StoryClosed;
            if (handler != null)
            {
                handler(this, new StoryClosedEventArgs(story));
            }
        }

        public void CloseStory()
        {
            if (!IsStoryOpen)
            {
                return;
            }

            OnStoryClosing(story);

            var oldStory = story;

            fileName = null;
            story = null;
            gameInfo = null;

            OnStoryClosed(oldStory);
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

            this.fileName = fileName;

            OnStoryOpened(story);

            return story;
        }

        public string FileName
        {
            get
            {
                return fileName;
            }
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
        public event EventHandler<StoryClosedEventArgs> StoryClosed;
    }
}
