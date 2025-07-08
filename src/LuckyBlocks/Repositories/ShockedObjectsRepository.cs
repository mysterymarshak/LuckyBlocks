using System.Collections.Generic;
using LuckyBlocks.Entities;
using SFDGameScriptInterface;

namespace LuckyBlocks.Repositories;

internal interface IShockedObjectsRepository
{
    void AddShockedObject(ShockedObject shockedObject);
    bool IsShockedObject(IObject @object);
    void RemoveShockedObject(int objectId, int index);
    IReadOnlyList<ShockedObject> GetShockedObjects();
}

internal class ShockedObjectsRepository : IShockedObjectsRepository
{
    private readonly Dictionary<int, ShockedObject> _objects = new();
    private readonly List<ShockedObject> _shockedObjects = [];

    public void AddShockedObject(ShockedObject shockedObject)
    {
        var objectId = shockedObject.ObjectId;
        _objects.Add(objectId, shockedObject);
        _shockedObjects.Add(shockedObject);
    }

    public bool IsShockedObject(IObject @object)
    {
        return _objects.ContainsKey(@object.UniqueId);
    }

    public void RemoveShockedObject(int objectId, int index)
    {
        _objects.Remove(objectId);
        _shockedObjects.RemoveAt(index);
    }

    public IReadOnlyList<ShockedObject> GetShockedObjects() => _shockedObjects;
}