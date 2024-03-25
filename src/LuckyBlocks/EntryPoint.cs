using System;
using Autofac;
using LuckyBlocks;
using LuckyBlocks.Features.Chat;
using LuckyBlocks.Notifications;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

namespace ㅤ;

public static class ㅤ
{
    public static void ㅤㅤ(IGame game)
    {
        try
        {
            var container = BuildContainer(game);

            var mediator = container.Resolve<IMediator>();
            var notification = new ScriptStartedNotification();
            mediator.Publish(notification);
        }
        catch (Exception exception)
        {
            var chat = new Chat(game);
            chat.ShowMessage($"Exception in lucky blocks entry point: {exception}", ExtendedColors.LightRed);
        }
    }

    private static IContainer BuildContainer(IGame game)
    {
        var builder = new ContainerBuilder();

        builder.RegisterInstance(game);
        builder.RegisterModule<LuckyBlocksModule>();

        return builder.Build();
    }
}