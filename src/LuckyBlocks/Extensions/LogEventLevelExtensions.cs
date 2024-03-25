using Serilog.Events;

namespace LuckyBlocks.Extensions;

internal static class LogEventLevelExtensions
{
    public static string GetShortName(this LogEventLevel logEventLevel) => logEventLevel switch
    {
        LogEventLevel.Verbose => "VB",
        LogEventLevel.Debug => "DBG",
        LogEventLevel.Information => "INF",
        LogEventLevel.Warning => "WARN",
        LogEventLevel.Error => "ERR",
        LogEventLevel.Fatal => "FAT",
        _ => "???"
    };
}