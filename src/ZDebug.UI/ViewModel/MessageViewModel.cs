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

        public bool IsError
        {
            get { return error; }
        }

        public bool IsWarning
        {
            get { return !error; }
        }

        public string Message
        {
            get { return message; }
        }

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
