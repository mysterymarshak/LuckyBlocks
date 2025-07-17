using LuckyBlocks.Extensions;
using LuckyBlocks.Utils;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Notifications;

internal class ChatSink : ILogEventSink
{
    private readonly IGame _game;
    private readonly IChat _chat;

    public ChatSink(IGame game, IChat chat)
    {
        _game = game;
        _chat = chat;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();

        if (logEvent.Exception is not null)
        {
            message += $" - {logEvent.Exception}";
        }

        var color = logEvent.Level switch
        {
            LogEventLevel.Information => Color.White,
            LogEventLevel.Warning => Color.Yellow,
            LogEventLevel.Error or LogEventLevel.Fatal => ExtendedColors.LightRed,
            _ => Color.Grey
        };

        _chat.ShowMessage(
            $"[LB-{logEvent.Level.GetShortName()}{(logEvent.Level <= LogEventLevel.Debug ? $"-{_game.TotalElapsedRealTime}" : string.Empty)}]: {message}",
            color);
    }
}

internal static class ChatSinkExtensions
{
    public static LoggerConfiguration Chat(this LoggerSinkConfiguration loggerConfiguration, IGame game, IChat chat)
    {
        return loggerConfiguration.Sink(new ChatSink(game, chat));
    }
}