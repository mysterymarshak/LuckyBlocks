using Autofac;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Utils;
using Serilog;

namespace LuckyBlocks.Data.Args;

internal record ImmunityConstructorArgs(
    IPlayerModifiersService PlayerModifiersService,
    IRespawner Respawner,
    ILogger Logger,
    ILifetimeScope LifetimeScope);