using ZDebug.UI.Services;

namespace ZDebug.Terp
{
    internal static class Services
    {
        private static readonly StoryService storyService = new StoryService();

        public static StoryService StoryService
        {
            get
            {
                return storyService;
            }
        }
    }
}
