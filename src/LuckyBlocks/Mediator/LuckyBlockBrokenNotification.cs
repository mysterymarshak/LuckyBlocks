using System;
using System.Threading;
using System.Threading.Tasks;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Utils;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Mediator;

internal readonly record struct LuckyBlockBrokenNotification(LuckyBlockBrokenArgs Args) : INotification;

internal class LuckyBlockBrokenNotificationHandler : INotificationHandler<LuckyBlockBrokenNotification>
{
    private readonly ILuckyBlocksService _luckyBlockService;
    private readonly IGame _game;
    private readonly ILogger _logger;

    public LuckyBlockBrokenNotificationHandler(ILuckyBlocksService luckyBlockService, IGame game, ILogger logger)
        => (_luckyBlockService, _game, _logger) = (luckyBlockService, game, logger);

    public ValueTask Handle(LuckyBlockBrokenNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            if (_game.IsGameOver)
            {
                return new ValueTask();
            }

            var args = notification.Args;

            Awaiter.Start(delegate
            {
                _luckyBlockService.OnLuckyBlockBroken(args);

                // it's cost of free-allocations EventsQueue
                // see IObjectExtensions.RemoveDelayed
            }, TimeSpan.Zero);

            _logger.Debug("Lucky block with id '{LuckyBlockId}' was broken", args.LuckyBlockId);
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Unexpected exception in LuckyBlockBrokenNotificationHandler.Handle");
        }

        return new ValueTask();
    }
}