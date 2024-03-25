﻿using System;
using System.Threading;
using LuckyBlocks.Reflection;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils;

[Inject]
internal static class Awaiter
{
    [InjectLogger]
    private static ILogger Logger { get; set; }

    public static void Start(Action callback, TimeSpan interval)
    {
        Events.UpdateCallback.Start(delegate
        {
            try
            {
                callback();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "unexpected exception in lucky blocks awaiter callback");
            }
        }, (uint)interval.TotalMilliseconds, 1);
    }
    
    public static void Start(Action<float> callback, TimeSpan interval)
    {
        Events.UpdateCallback.Start(delegate (float e)
        {
            try
            {
                callback(e);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "unexpected exception in lucky blocks awaiter callback");
            }
        }, (uint)interval.TotalMilliseconds, 1);
    }

    public static void Start(Action callback, TimeSpan interval, CancellationToken ct)
    {
        Events.UpdateCallback.Start(delegate
        {
            if (ct.IsCancellationRequested)
                return;

            try
            {
                callback();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "unexpected exception in lucky blocks awaiter callback");
            }
        }, (uint)interval.TotalMilliseconds, 1);
    }
}