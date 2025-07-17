using System;
using LuckyBlocks.Features.Objects;
using LuckyBlocks.Reflection;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

[Inject]
internal static class IObjectExtensions
{
    [InjectMappedObjectsService]
    private static IMappedObjectsService MappedObjectsService { get; set; }

    public static bool IsValid(this IObject @object)
    {
        return @object is { DestructionInitiated: false, IsRemoved: false, RemovalInitiated: false };
    }

    public static void RemoveDelayed(this IObject @object)
    {
        if (!@object.IsValid())
            return;

        Awaiter.Start(@object.Remove, TimeSpan.Zero);

        // object removal on next update it's cost of free-allocations EventsQueue
        // if it wasn't it, every update call (and other callbacks) allocates list for queue for supporting HookModes
        // OnObjectDestroyed -> Buff or anything calling object removing -> game calls OnObjectDestroyed
        // on same stack frame, so static list Items begin modifying, and exception was thrown
        // same problem in LuckyBlockBrokenNotification
    }

    public static MappedObject ToMappedObject(this IObject @object)
    {
        return MappedObjectsService.ToMapped(@object);
    }
}