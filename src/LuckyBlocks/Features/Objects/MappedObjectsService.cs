using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Extensions;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Objects;

internal interface IMappedObjectsService
{
    MappedObject ToMapped(IObject @object);
    IObject GetActualObject(int uniqueId);
    void UpdateActualObject(int oldUniqueId, IObject? newObject);
    void RemoveMap(int uniqueId);
    bool IsMapped(int uniqueId);
}

internal class MappedObjectsService : IMappedObjectsService
{
    private readonly ILogger _logger;
    private readonly Dictionary<int, ObjectMap> _mappedObjects = new();

    public MappedObjectsService(ILogger logger)
    {
        _logger = logger;
    }

    public MappedObject ToMapped(IObject @object)
    {
        var uniqueId = @object.UniqueId;
        var existingMap = _mappedObjects.Values.FirstOrDefault(map => map.IsFor(uniqueId));
        if (existingMap is not null)
        {
            return existingMap.MappedObject;
        }

        var map = new ObjectMap([uniqueId], new MappedObject(@object)) { ActualObject = @object };
        _mappedObjects.Add(uniqueId, map);
        return map.MappedObject;
    }

    public IObject GetActualObject(int uniqueId)
    {
        return _mappedObjects[uniqueId].ActualObject;
    }

    public MappedObject GetActualMappedObject(int uniqueId)
    {
        return _mappedObjects[uniqueId].MappedObject;
    }

    public void UpdateActualObject(int oldUniqueId, IObject? newObject)
    {
        var map = _mappedObjects[oldUniqueId];

        if (map.ActualObject.IsValid())
        {
            throw new InvalidOperationException("you cannot update the mapped object when old map still valid");
        }

        if (newObject is null)
            return;

        var newUniqueId = newObject.UniqueId;
        map.PopulateIds(newUniqueId);
        map.ActualObject = newObject;
        _mappedObjects.Add(newUniqueId, map);
    }

    public void RemoveMap(int uniqueId)
    {
        var map = _mappedObjects[uniqueId];
        foreach (var objectId in map.ObjectIds)
        {
            _mappedObjects.Remove(objectId);
        }
    }

    public bool IsMapped(int uniqueId)
    {
        return _mappedObjects.ContainsKey(uniqueId);
    }

    private record ObjectMap(List<int> ObjectIds, MappedObject MappedObject)
    {
        public IObject ActualObject { get; set; } = null!;

        public void PopulateIds(int newObjectId)
        {
            ObjectIds.Add(newObjectId);
        }

        public bool IsFor(int uniqueId)
        {
            return ObjectIds.Contains(uniqueId);
        }
    }
}