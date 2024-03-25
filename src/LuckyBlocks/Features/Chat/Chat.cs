using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Chat;

internal interface IChat
{
    void ShowMessage(string message);
    void ShowMessage(string message, Color color);
    void ShowMessage(string message, Color color, int userId);
}

internal class Chat : IChat
{
    private const int MAX_MESSAGE_LENGTH = 400;

    private readonly IGame _game;

    public Chat(IGame game)
        => (_game) = (game);

    public void ShowMessage(string message) => ShowMessage(message, Color.Grey);

    public void ShowMessage(string message, Color color)
    {
        var chunks = message.Chunk(MAX_MESSAGE_LENGTH);

        foreach (var chunk in chunks)
        {
            ShowMessageToAll(string.Join(string.Empty, chunk), color);
        }
    }

    public void ShowMessage(string message, Color color, int userId)
    {
        var chunks = message.Chunk(MAX_MESSAGE_LENGTH);

        foreach (var chunk in chunks)
        {
            ShowMessageToUser(string.Join(string.Empty, chunk), color, userId);
        }
    }

    private void ShowMessageToAll(string message, Color color) => _game.ShowChatMessage(message, color);

    private void ShowMessageToUser(string message, Color color, int userId)
    {
        var user = _game.GetActiveUser(userId);

#if !DEBUG
        if (user?.IsBot != false)
            return;
#endif

        _game.ShowChatMessage(message, color, userId);
    }
}