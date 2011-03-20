using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    public sealed class GameInfoViewModel : ViewModelWithViewBase<Window>
    {
        private GameInfo gameInfo;

        public GameInfoViewModel()
            : base("GameInfoDialogView")
        {
        }

        public void SetGameinfo(GameInfo gameInfo)
        {
            if (gameInfo == null)
            {
                throw new ArgumentNullException("gameInfo");
            }

            this.gameInfo = gameInfo;
            AllPropertiesChanged();
        }

        public string Title
        {
            get { return gameInfo.Title; }
        }

        public string Headline
        {
            get { return gameInfo.Headline; }
        }

        public string Author
        {
            get { return gameInfo.Author; }
        }

        public string FirstPublished
        {
            get { return gameInfo.FirstPublished; }
        }

        public string Description
        {
            get { return gameInfo.Description; }
        }

        public BitmapSource Cover
        {
            get { return gameInfo.Cover; }
        }
    }
}
