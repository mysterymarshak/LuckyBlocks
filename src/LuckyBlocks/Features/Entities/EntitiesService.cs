using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Entities;

internal interface IEntity
{
    int ObjectId { get; }
    void Initialize();
    IEntity Clone();
    void Dispose();
}

internal interface IEntitiesService
{
    void Add<T>(T entity) where T : IEntity;
    void Remove(int objectId);
    List<IEntity> GetAllUnsafe<T>() where T : IEntity;
    bool IsRegistered(IObject @object);
    EntitiesServiceState CloneState();
    void RestoreState(EntitiesServiceState entities);
}

internal class EntitiesService : IEntitiesService
{
    private readonly ILogger _logger;
    private readonly List<IEntity> _entities = [];
    private readonly Dictionary<int, IEntity> _entitiesById = new();
    private readonly Dictionary<Type, List<IEntity>> _entitiesByType = new();

    public EntitiesService(ILogger logger)
    {
        _logger = logger;
    }

    public void Add<T>(T entity) where T : IEntity
    {
        _entities.Add(entity);
        _entitiesById.Add(entity.ObjectId, entity);

        var entityType = entity.GetType();
        if (!_entitiesByType.TryGetValue(entityType, out var entitiesByType))
        {
            entitiesByType = [];
            _entitiesByType.Add(entityType, entitiesByType);
        }

        entitiesByType.Add(entity);
    }

    public void Remove(int objectId)
    {
        var entity = _entitiesById[objectId];
        Remove(entity);
    }

    // use only with reversed for-loop 
    public List<IEntity> GetAllUnsafe<T>() where T : IEntity
    {
        if (!_entitiesByType.TryGetValue(typeof(T), out var entitiesByType))
        {
            entitiesByType = [];
            _entitiesByType.Add(typeof(T), entitiesByType);
        }

        return entitiesByType;
    }

    public bool IsRegistered(IObject @object)
    {
        return _entitiesById.ContainsKey(@object.UniqueId);
    }

    public EntitiesServiceState CloneState()
    {
        var entities = _entities
            .Select(x => x.Clone())
            .ToList();

#if DEBUG
        _logger.Debug("Cloning entities: {Entities}",
            entities.Select(x => $"{x.GetType().Name}: '{x.ObjectId}").ToList());
#endif

        return new EntitiesServiceState(entities);
    }

    public void RestoreState(EntitiesServiceState state)
    {
        var entities = state.Entities;

        foreach (var entity in _entities)
        {
            entity.Dispose();
        }

        _entities.Clear();
        _entitiesById.Clear();
        _entitiesByType.Clear();

        foreach (var entity in entities)
        {
            Add(entity);
            entity.Initialize();
        }

        _logger.Debug("Entities state restored: {Entities}", _entities);
    }

    private void Remove(IEntity entity)
    {
        _entities.Remove(entity);
        _entitiesById.Remove(entity.ObjectId);
        _entitiesByType[entity.GetType()].Remove(entity);
    }
}