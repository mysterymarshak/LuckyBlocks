using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Chat;
using LuckyBlocks.Utils;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using SFDGameScriptInterface;

namespace LuckyBlocks.Logs;

internal class ChatSink : ILogEventSink
{
    private readonly IChat _chat;

    public ChatSink(IChat chat)
        => (_chat) = (chat);

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

        _chat.ShowMessage($"[LB-{logEvent.Level.GetShortName()}]: {message}", color);
    }
}

internal static class ChatSinkExtensions
{
    public static LoggerConfiguration Chat(this LoggerSinkConfiguration loggerConfiguration, IChat chat)
    {
        return loggerConfiguration.Sink(new ChatSink(chat));
    }
}