namespace ZDebug.UI.ViewModel;

internal sealed class MessageViewModel : ViewModelBase
{
    public required string Message { get; init; }
    public required bool IsError { get; init; }

    public bool IsWarning => !IsError;

    public static MessageViewModel CreateError(string message)
        => new() { Message = message, IsError = true };

    public static MessageViewModel CreateWarning(string message)
        => new() { Message = message, IsError = false };
}
