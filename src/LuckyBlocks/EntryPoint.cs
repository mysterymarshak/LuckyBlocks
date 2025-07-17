using System;
using Autofac;
using LuckyBlocks;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.Triggers;
using LuckyBlocks.Mediator;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

// ReSharper disable once CheckNamespace
namespace ㅤ;

public static class ㅤ
{
    public static Action<TriggerArgs> ㅤㅤ(IGame game)
    {
        Action<TriggerArgs>? triggerCallbackAction = null!;
        try
        {
            var container = BuildContainer(game);

            var triggersService = container.Resolve<ITriggersService>();
            triggerCallbackAction = triggersService.OnTriggerCallback;

            var mediator = container.Resolve<IMediator>();
            var notification = new ScriptStartedNotification();
            mediator.Publish(notification);
        }
        catch (Exception exception)
        {
            var chat = new Chat(game);
            chat.ShowMessage($"Exception in lucky blocks entry point: {exception}", ExtendedColors.LightRed);
        }

        return triggerCallbackAction ?? (_ => { });
    }

    private static IContainer BuildContainer(IGame game)
    {
        var builder = new ContainerBuilder();

        builder.RegisterInstance(game);
        builder.RegisterModule<LuckyBlocksModule>();

        return builder.Build();
    }
}