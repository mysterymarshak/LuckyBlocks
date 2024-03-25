using System.Collections.Generic;
using Autofac;
using LuckyBlocks.Entities;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Repositories;

internal interface ILuckyBlocksRepository
{
    LuckyBlock CreateLuckyBlock(IObjectSupplyCrate supplyCrate);
    bool IsLuckyBlockExists(int supplyCrateId);
    void RemoveLuckyBlock(int id);
}

internal class LuckyBlocksRepository : ILuckyBlocksRepository
{
    private readonly IMediator _mediator;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly Dictionary<int, LuckyBlock> _luckyBlocks;
    private readonly Dictionary<int, ILifetimeScope> _lifetimeScopes;

    public LuckyBlocksRepository(IMediator mediator, ILifetimeScope lifetimeScope)
        => (_mediator, _lifetimeScope, _luckyBlocks, _lifetimeScopes) = (mediator, lifetimeScope, new(), new());
    
    public LuckyBlock CreateLuckyBlock(IObjectSupplyCrate supplyCrate)
    {
        var scope = _lifetimeScope.BeginLifetimeScope();
        var extendedEvents = scope.Resolve<IExtendedEvents>();
        
        var luckyBlock = new LuckyBlock(supplyCrate, _mediator, extendedEvents);
        luckyBlock.Init();
        
        _luckyBlocks.Add(supplyCrate.UniqueId, luckyBlock);
        _lifetimeScopes.Add(supplyCrate.UniqueId, scope);

        return luckyBlock;
    }

    public bool IsLuckyBlockExists(int supplyCrateId)
    {
        return _luckyBlocks.ContainsKey(supplyCrateId);
    }

    public void RemoveLuckyBlock(int id)
    {
        _luckyBlocks.Remove(id);
        
        var lifetimeScope = _lifetimeScopes[id];
        lifetimeScope.Dispose();
        _lifetimeScopes.Remove(id);
    }
}