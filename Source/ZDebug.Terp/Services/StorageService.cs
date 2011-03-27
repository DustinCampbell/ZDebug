using System.ComponentModel.Composition;
using System.Xml.Linq;
using ZDebug.Core;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.Services
{
    [Export]
    internal class StorageService : IService
    {
        private readonly IPersistable storyPersistence;
        private readonly IPersistable gameScriptPersistence;

        [ImportingConstructor]
        private StorageService(
            StoryService storyService,
            GameScriptService gameScriptService)
        {
            this.storyPersistence = storyService;
            storyService.StoryOpened += StoryService_StoryOpened;
            storyService.StoryClosing += StoryService_StoryClosing;
            this.gameScriptPersistence = gameScriptService;
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            LoadSettings(e.Story);
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            SaveSettings(e.Story);
        }

        private void LoadSettings(Story story)
        {
            var xml = Storage.RestoreStorySettings(story);

            gameScriptPersistence.Load(xml);
        }

        private void SaveSettings(Story story)
        {
            var xml =
                new XElement("settings",
                    storyPersistence.Store(),
                    gameScriptPersistence.Store());

            Storage.SaveStorySettings(story, xml);
        }
    }
}
