namespace ZDebug.UI.ViewModel
{
    internal sealed class MessageViewModel : ViewModelBase
    {
        private readonly string message;
        private readonly bool error;

        private MessageViewModel(string message, bool error)
        {
            this.message = message;
            this.error = error;
        }

        public bool IsError => error;

        public bool IsWarning => !error;

        public string Message => message;

        public static MessageViewModel CreateError(string message)
        {
            return new MessageViewModel(message, error: true);
        }

        public static MessageViewModel CreateWarning(string message)
        {
            return new MessageViewModel(message, error: false);
        }
    }
}
